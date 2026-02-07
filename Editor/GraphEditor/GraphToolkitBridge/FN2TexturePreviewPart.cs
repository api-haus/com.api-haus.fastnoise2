using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine.UIElements;

namespace FastNoise2.Editor.GraphEditor
{
	class FN2TexturePreviewPart : BaseModelViewPart
	{
		public static readonly string ussClassName = "fn2-texture-preview-part";
		const int PreviewSize = 128;
		const double TickIntervalSec = 0.016; // ~60 Hz

		// --- Shared round-robin state ---
		static readonly List<FN2TexturePreviewPart> s_Parts = new();
		static Throttle s_RoundRobinThrottle;

		// --- Dirty-driven rendering state ---
		static int s_ChangeGeneration;
		static int s_ScanGeneration;
		static int s_ScanIndex;
		static float s_LastFrequency;
		static UnityEngine.Vector2 s_LastPanOffset;
		static float s_LastYaw;
		static float s_LastPitch;
		static FN2PreviewWidget.PreviewMode s_LastMode;

		// --- Instance ---
		VisualElement m_Root;
		FN2PreviewWidget m_Widget;
		bool m_Dirty = true;

		public static FN2TexturePreviewPart Create(string name, Model model,
			ChildView ownerElement, string parentClassName)
		{
			return new FN2TexturePreviewPart(name, model, ownerElement, parentClassName);
		}

		FN2TexturePreviewPart(string name, Model model, ChildView ownerElement,
			string parentClassName)
			: base(name, model, ownerElement, parentClassName) { }

		public override VisualElement Root => m_Root;

		protected override void BuildUI(VisualElement parent)
		{
			m_Root = new VisualElement();
			m_Root.AddToClassList(ussClassName);

			m_Widget = new FN2PreviewWidget(PreviewSize);
			m_Root.Add(m_Widget);

			m_Root.RegisterCallback<AttachToPanelEvent>(OnAttach);
			m_Root.RegisterCallback<DetachFromPanelEvent>(OnDetach);

			parent.Add(m_Root);
		}

		void OnAttach(AttachToPanelEvent _)
		{
			if (!s_Parts.Contains(this))
				s_Parts.Add(this);
			m_Dirty = true;
			s_ChangeGeneration++;
			EnsureTickLoop();
			FN2EditorUpdate.PreviewsInvalidated += OnPreviewsInvalidated;
			FN2EditorUpdate.NotifyGraphChanged();
		}

		void OnDetach(DetachFromPanelEvent _)
		{
			s_Parts.Remove(this);
			FN2EditorUpdate.PreviewsInvalidated -= OnPreviewsInvalidated;
			FN2EditorUpdate.NotifyGraphChanged();
			if (s_Parts.Count == 0 && s_RoundRobinThrottle != null)
			{
				FN2EditorUpdate.Unregister(s_RoundRobinThrottle);
				s_RoundRobinThrottle = null;
			}
		}

		public override void UpdateUIFromModel(UpdateFromModelVisitor visitor)
		{
			if (FN2BridgeCallbacks.CompileNodeSubtree == null)
				return;

			m_Dirty = true;
			s_ChangeGeneration++;
			EnsureTickLoop();
			FN2EditorUpdate.NotifyGraphChanged();
		}

		static void EnsureTickLoop()
		{
			if (s_RoundRobinThrottle != null)
				return;

			s_RoundRobinThrottle = new Throttle(ProcessNextNode, TickIntervalSec);
			FN2EditorUpdate.Register(s_RoundRobinThrottle);
		}

		static void ProcessNextNode()
		{
			if (s_Parts.Count == 0 || FN2BridgeCallbacks.CompileNodeSubtree == null)
				return;

			// Phase 1: Detect global setting changes (frequency / mode / camera / pan / orbit)
			float currentFrequency = FN2BridgeCallbacks.PreviewFrequency;
			var currentPan = FN2BridgeCallbacks.PanOffset;
			float currentYaw = FN2BridgeCallbacks.CameraYaw;
			float currentPitch = FN2BridgeCallbacks.CameraPitch;
			var currentMode = FN2PreviewWidget.Mode;

			if (currentFrequency != s_LastFrequency
				|| currentPan != s_LastPanOffset || currentYaw != s_LastYaw
				|| currentPitch != s_LastPitch || currentMode != s_LastMode)
			{
				s_LastFrequency = currentFrequency;
				s_LastPanOffset = currentPan;
				s_LastYaw = currentYaw;
				s_LastPitch = currentPitch;
				s_LastMode = currentMode;
				for (int i = 0; i < s_Parts.Count; i++)
					s_Parts[i].m_Dirty = true;
				s_ChangeGeneration++;
				return; // let dirty rendering pick them up starting next tick
			}

			// Phase 2: Priority — render first dirty node
			for (int i = 0; i < s_Parts.Count; i++)
			{
				var part = s_Parts[i];
				if (part.m_Dirty && part.m_Root?.panel != null)
				{
					part.m_Dirty = false;
					part.RenderPreview();
					return;
				}
			}

			// Phase 3: Background scan — one node per tick
			if (s_ScanGeneration < s_ChangeGeneration)
			{
				if (s_ScanIndex >= s_Parts.Count)
				{
					// Scan complete for this generation
					s_ScanGeneration = s_ChangeGeneration;
					s_ScanIndex = 0;
					return;
				}

				var part = s_Parts[s_ScanIndex];
				s_ScanIndex++;

				if (part.m_Root?.panel != null)
					part.RenderPreview();

				return;
			}

			// Phase 4: Idle — nothing to do
		}

		void OnPreviewsInvalidated()
		{
			m_Dirty = true;
			s_ChangeGeneration++;
			m_Widget?.Invalidate();
			EnsureTickLoop();
		}

		void RenderPreview()
		{
			var nodeModel = m_Model as IUserNodeModelImp;
			if (nodeModel == null)
				return;

			string encoded = FN2BridgeCallbacks.CompileNodeSubtree(nodeModel.Node);
			if (string.IsNullOrEmpty(encoded))
				return;

			m_Widget.SetEncoded(encoded, FN2BridgeCallbacks.PreviewFrequency);
		}
	}
}
