using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine;
using UnityEngine.UIElements;

namespace FastNoise2.Editor.GraphEditor
{
	class FN2TexturePreviewPart : BaseModelViewPart
	{
		public static readonly string ussClassName = "fn2-texture-preview-part";
		const int PreviewSize = 128;
		const long TickIntervalMs = 50; // 20 Hz

		// --- Shared round-robin state ---
		static readonly List<FN2TexturePreviewPart> s_Parts = new();
		static int s_NextIndex;
		static VisualElement s_TickHost;
		static IVisualElementScheduledItem s_TickLoop;

		// --- Instance ---
		VisualElement m_Root;
		Image m_Image;
		Texture2D m_CachedTexture;

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

			m_Image = new Image();
			m_Image.AddToClassList("fn2-preview-image");
			m_Root.Add(m_Image);

			parent.Add(m_Root);

			s_Parts.Add(this);
			EnsureTickLoop();

			m_Root.RegisterCallback<DetachFromPanelEvent>(OnDetach);
		}

		void OnDetach(DetachFromPanelEvent _)
		{
			s_Parts.Remove(this);
		}

		public override void UpdateUIFromModel(UpdateFromModelVisitor visitor)
		{
			if (FN2BridgeCallbacks.RenderNodePreview == null)
				return;

			EnsureTickLoop();
		}

		// --- Tick loop: render one node per tick, round-robin ---
		void EnsureTickLoop()
		{
			if (s_TickHost?.panel != null)
				return;

			s_TickHost = m_Root;
			s_TickLoop = m_Root.schedule.Execute(ProcessNextNode).Every(TickIntervalMs);
		}

		static void ProcessNextNode()
		{
			if (s_Parts.Count == 0 || FN2BridgeCallbacks.RenderNodePreview == null)
				return;

			var index = s_NextIndex % s_Parts.Count;
			s_NextIndex = index + 1;

			var part = s_Parts[index];
			if (part.m_Root?.panel != null)
				part.RenderPreview();
		}

		void RenderPreview()
		{
			if (FN2BridgeCallbacks.RenderNodePreview == null)
				return;

			var nodeModel = m_Model as IUserNodeModelImp;
			if (nodeModel == null)
				return;

			var newTexture = FN2BridgeCallbacks.RenderNodePreview(
				nodeModel.Node, PreviewSize, PreviewSize);

			if (m_CachedTexture != null && m_CachedTexture != newTexture)
				Object.DestroyImmediate(m_CachedTexture);

			m_CachedTexture = newTexture;
			m_Image.image = m_CachedTexture;
		}
	}
}
