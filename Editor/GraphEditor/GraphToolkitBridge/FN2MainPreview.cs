using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine;
using UnityEngine.UIElements;


namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Floating Main Preview element positioned at the bottom-right of the GraphView.
	/// Compiles the full graph (or a preview override node's subtree) and renders
	/// via <see cref="FN2PreviewWidget"/> in either flat texture or heightfield mode.
	/// Supports scroll-wheel zoom, middle/right-button pan, and left-button orbit (3D).
	/// </summary>
	class FN2MainPreview : VisualElement
	{
		const int PreviewSize = 128;
		const double DebounceDelaySec = 0.2;
		const float MinFrequency = 0.001f;
		const float MaxFrequency = 0.2f;
		const float ScrollMultiplier = 1.1f;
		const float OrbitSpeed = 0.01f;
		const float OrbitPitchMin = 0.3f;
		const float OrbitPitchMax = 1.1f;

		enum DragMode { None, Orbit, Pan }

		readonly Label m_Title;
		readonly Button m_ModeToggle;
		readonly FN2PreviewWidget m_Widget;
		readonly Debounce m_Debounce;
		string m_LastEncoded;

		DragMode m_DragMode;
		int m_DragButton = -1;
		Vector2 m_LastPointerPos;
		Graph m_LoadedGraph;

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

			// Pan (middle/right drag) and orbit (left drag, 3D only) on the widget
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
			m_LoadedGraph = null;
			FN2EditorUpdate.Register(m_Debounce);
			FN2EditorUpdate.GraphChanged += OnGraphChanged;
			m_Debounce.Signal();
		}

		void OnDetach(DetachFromPanelEvent evt)
		{
			SaveState();
			FN2EditorUpdate.GraphChanged -= OnGraphChanged;
			FN2EditorUpdate.Unregister(m_Debounce);
			m_LoadedGraph = null;
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
			SaveState();
		}

		static string GetModeLabel()
		{
			return FN2PreviewWidget.Mode == FN2PreviewWidget.PreviewMode.Texture ? "2D" : "3D";
		}

		void OnPointerDown(PointerDownEvent evt)
		{
			if (m_DragMode != DragMode.None)
				return;

			if (evt.button == 0 && FN2PreviewWidget.Mode == FN2PreviewWidget.PreviewMode.Heightfield)
				m_DragMode = DragMode.Orbit;
			else if (evt.button == 1 || evt.button == 2)
				m_DragMode = DragMode.Pan;
			else
				return;

			m_DragButton = evt.button;
			m_LastPointerPos = evt.position;
			m_Widget.CapturePointer(evt.pointerId);
			evt.StopPropagation();
		}

		void OnPointerMove(PointerMoveEvent evt)
		{
			if (m_DragMode == DragMode.None)
				return;

			Vector2 delta = (Vector2)evt.position - m_LastPointerPos;
			m_LastPointerPos = evt.position;

			if (m_DragMode == DragMode.Pan)
			{
				Vector2 screenDelta = new Vector2(delta.x, -delta.y);

				if (FN2PreviewWidget.Mode == FN2PreviewWidget.PreviewMode.Heightfield)
				{
					float yaw = FN2BridgeCallbacks.CameraYaw;
					float c = Mathf.Cos(yaw);
					float s = Mathf.Sin(yaw);
					screenDelta = new Vector2(
						screenDelta.x * c - screenDelta.y * s,
						screenDelta.x * s + screenDelta.y * c);
				}

				FN2BridgeCallbacks.PanOffset -= screenDelta * FN2BridgeCallbacks.PreviewFrequency;
				m_Widget.SetPanOffset();
			}
			else
			{
				FN2BridgeCallbacks.CameraYaw += delta.x * OrbitSpeed;
				FN2BridgeCallbacks.CameraPitch = Mathf.Clamp(
					FN2BridgeCallbacks.CameraPitch - delta.y * OrbitSpeed, OrbitPitchMin, OrbitPitchMax);
				m_Widget.UpdateOrbit();
			}

			evt.StopPropagation();
		}

		void OnPointerUp(PointerUpEvent evt)
		{
			if (m_DragMode == DragMode.None || evt.button != m_DragButton)
				return;

			m_DragMode = DragMode.None;
			m_DragButton = -1;
			m_Widget.ReleasePointer(evt.pointerId);
			evt.StopPropagation();
			SaveState();
		}

		void OnWheel(WheelEvent evt)
		{
			float factor = evt.delta.y > 0 ? ScrollMultiplier : 1f / ScrollMultiplier;
			float oldFreq = FN2BridgeCallbacks.PreviewFrequency;
			float newFreq = Mathf.Clamp(oldFreq * factor, MinFrequency, MaxFrequency);

			// Zoom toward cursor: keep the noise-space point under the cursor fixed.
			// Grid maps screenX directly, screenY is inverted (texture bottom = screen bottom).
			Vector2 localPos = m_Widget.WorldToLocal(evt.mousePosition);
			localPos.x = Mathf.Clamp(localPos.x, 0, PreviewSize);
			localPos.y = Mathf.Clamp(localPos.y, 0, PreviewSize);
			Vector2 cursorGrid = new Vector2(localPos.x, PreviewSize - localPos.y);

			FN2BridgeCallbacks.PanOffset += cursorGrid * (oldFreq - newFreq);
			FN2BridgeCallbacks.PreviewFrequency = newFreq;
			m_Widget.SetEncoded(m_LastEncoded, newFreq);

			FN2EditorUpdate.NotifyGraphChanged();
			evt.StopPropagation();
			SaveState();
		}

		Graph GetGraph()
		{
			var graphView = GetFirstAncestorOfType<GraphView>();
			if (graphView == null)
				return null;

			var graphModel = graphView.GraphModel as GraphModelImp;
			var graph = graphModel?.Graph;
			if (graph == null || FN2BridgeCallbacks.IsFN2Graph == null
				|| !FN2BridgeCallbacks.IsFN2Graph(graph))
				return null;

			return graph;
		}

		void SaveState()
		{
			if (m_LoadedGraph == null)
				return;

			FN2BridgeCallbacks.PreviewModeValue = (int)FN2PreviewWidget.Mode;
			FN2BridgeCallbacks.SavePreviewState?.Invoke(m_LoadedGraph);
		}

		void UpdatePreview()
		{
			if (FN2BridgeCallbacks.CompileFullGraph == null
				|| FN2BridgeCallbacks.RenderPreviewWithFrequency == null)
				return;

			var graph = GetGraph();
			if (graph == null)
				return;

			// Load persisted preview state when first accessing a graph
			if (m_LoadedGraph != graph)
			{
				m_LoadedGraph = graph;
				FN2BridgeCallbacks.LoadPreviewState?.Invoke(graph);
				FN2PreviewWidget.Mode = (FN2PreviewWidget.PreviewMode)FN2BridgeCallbacks.PreviewModeValue;
				m_ModeToggle.text = GetModeLabel();
			}

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
