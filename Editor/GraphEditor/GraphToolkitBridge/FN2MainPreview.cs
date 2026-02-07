using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Mathematics.math;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Floating Main Preview element positioned at the bottom-right of the GraphView.
	/// Compiles the full graph (or a preview override node's subtree) and renders
	/// via <see cref="FN2PreviewWidget"/> in either flat texture or heightfield mode.
	/// Supports scroll-wheel zoom, left-button pan, and hover orbit (3D).
	/// </summary>
	class FN2MainPreview : VisualElement
	{
		const int PreviewSize = 128;
		const double DebounceDelaySec = 0.2;
		const float MinFrequency = 0.001f;
		const float MaxFrequency = 0.2f;
		const float ScrollMultiplier = 1.1f;
		const float MinCamDist = 0.3f;
		const float MaxCamDist = 3.0f;
		const float OrbitYawRange = 0.8f;
		const float OrbitPitchMin = 0.3f;
		const float OrbitPitchMax = 1.1f;

		readonly Label m_Title;
		readonly Button m_ModeToggle;
		readonly FN2PreviewWidget m_Widget;
		readonly Debounce m_Debounce;
		string m_LastEncoded;

		bool m_IsPanning;
		Vector2 m_LastPointerPos;

		public FN2MainPreview()
		{
			AddToClassList("fn2-main-preview");

			// Header
			var header = new VisualElement();
			header.AddToClassList("fn2-main-preview__header");

			m_Title = new Label("Main Preview");
			m_Title.AddToClassList("fn2-main-preview__title");
			header.Add(m_Title);

			m_ModeToggle = new Button(OnModeToggle);
			m_ModeToggle.AddToClassList("fn2-main-preview__mode-toggle");
			m_ModeToggle.text = GetModeLabel();
			m_ModeToggle.tooltip = "Toggle flat texture / heightfield preview";
			header.Add(m_ModeToggle);

			Add(header);

			// Preview widget
			m_Widget = new FN2PreviewWidget(PreviewSize);
			Add(m_Widget);

			// Scroll-wheel zoom on the entire element
			RegisterCallback<WheelEvent>(OnWheel);

			// Pan (left-button drag) and orbit (hover, 3D only) on the widget
			m_Widget.RegisterCallback<PointerDownEvent>(OnPointerDown);
			m_Widget.RegisterCallback<PointerMoveEvent>(OnPointerMove);
			m_Widget.RegisterCallback<PointerUpEvent>(OnPointerUp);

			m_Debounce = new Debounce(UpdatePreview, DebounceDelaySec);

			RegisterCallback<AttachToPanelEvent>(OnAttach);
			RegisterCallback<DetachFromPanelEvent>(OnDetach);
		}

		void OnAttach(AttachToPanelEvent evt)
		{
			m_LastEncoded = null;
			FN2EditorUpdate.Register(m_Debounce);
			FN2EditorUpdate.GraphChanged += OnGraphChanged;
			m_Debounce.Signal();
		}

		void OnDetach(DetachFromPanelEvent evt)
		{
			FN2EditorUpdate.GraphChanged -= OnGraphChanged;
			FN2EditorUpdate.Unregister(m_Debounce);
		}

		void OnGraphChanged()
		{
			m_Debounce.Signal();
		}

		void OnModeToggle()
		{
			FN2PreviewWidget.Mode = FN2PreviewWidget.Mode == FN2PreviewWidget.PreviewMode.Texture
				? FN2PreviewWidget.PreviewMode.Heightfield
				: FN2PreviewWidget.PreviewMode.Texture;

			m_ModeToggle.text = GetModeLabel();
			m_Widget.Refresh();
			FN2EditorUpdate.NotifyGraphChanged();
		}

		static string GetModeLabel()
		{
			return FN2PreviewWidget.Mode == FN2PreviewWidget.PreviewMode.Texture ? "2D" : "3D";
		}

		void OnPointerDown(PointerDownEvent evt)
		{
			if (evt.button != 0)
				return;

			m_IsPanning = true;
			m_LastPointerPos = evt.position;
			m_Widget.CapturePointer(evt.pointerId);
			evt.StopPropagation();
		}

		void OnPointerMove(PointerMoveEvent evt)
		{
			if (m_IsPanning)
			{
				Vector2 delta = (Vector2)evt.position - m_LastPointerPos;
				m_LastPointerPos = evt.position;

				FN2BridgeCallbacks.PanOffset -= new Vector2(delta.x, -delta.y) * FN2BridgeCallbacks.PreviewFrequency;
				m_Widget.SetPanOffset();
				evt.StopPropagation();
			}
			else if (FN2PreviewWidget.Mode == FN2PreviewWidget.PreviewMode.Heightfield)
			{
				Vector2 localPos = m_Widget.WorldToLocal(evt.position);
				float normalizedX = saturate(localPos.x / PreviewSize);
				float normalizedY = saturate(localPos.y / PreviewSize);

				FN2BridgeCallbacks.CameraYaw = (normalizedX - 0.5f) * 2f * OrbitYawRange;
				FN2BridgeCallbacks.CameraPitch = lerp(OrbitPitchMax, OrbitPitchMin, normalizedY);
				m_Widget.UpdateOrbit();
				evt.StopPropagation();
			}
		}

		void OnPointerUp(PointerUpEvent evt)
		{
			if (evt.button != 0 || !m_IsPanning)
				return;

			m_IsPanning = false;
			m_Widget.ReleasePointer(evt.pointerId);
			evt.StopPropagation();
		}

		void OnWheel(WheelEvent evt)
		{
			float factor = evt.delta.y > 0 ? ScrollMultiplier : 1f / ScrollMultiplier;

			FN2BridgeCallbacks.PreviewFrequency = clamp(
				FN2BridgeCallbacks.PreviewFrequency * factor, MinFrequency, MaxFrequency);
			m_Widget.SetEncoded(m_LastEncoded, FN2BridgeCallbacks.PreviewFrequency);

			FN2EditorUpdate.NotifyGraphChanged();
			evt.StopPropagation();
		}

		void UpdatePreview()
		{
			if (FN2BridgeCallbacks.CompileFullGraph == null
				|| FN2BridgeCallbacks.RenderPreviewWithFrequency == null)
				return;

			var graphView = GetFirstAncestorOfType<GraphView>();
			if (graphView == null)
				return;

			var graphModel = graphView.GraphModel as GraphModelImp;
			var graph = graphModel?.Graph;
			if (graph == null || FN2BridgeCallbacks.IsFN2Graph == null
				|| !FN2BridgeCallbacks.IsFN2Graph(graph))
				return;

			string encoded;
			string title;

			if (FN2BridgeCallbacks.PreviewOverrideNode != null
				&& FN2BridgeCallbacks.CompileNodeSubtree != null)
			{
				encoded = FN2BridgeCallbacks.CompileNodeSubtree(FN2BridgeCallbacks.PreviewOverrideNode);
				string nodeName = FN2BridgeCallbacks.GetNodeTypeName?.Invoke(FN2BridgeCallbacks.PreviewOverrideNode);
				title = "Preview: " + (nodeName ?? "Node");
			}
			else
			{
				encoded = FN2BridgeCallbacks.CompileFullGraph(graph);
				title = "Main Preview";
			}

			m_Title.text = title;

			if (encoded == m_LastEncoded)
				return;

			m_LastEncoded = encoded;
			m_Widget.SetEncoded(encoded, FN2BridgeCallbacks.PreviewFrequency);
		}
	}
}
