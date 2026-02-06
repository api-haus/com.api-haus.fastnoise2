using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine;
using UnityEngine.UIElements;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Floating Main Preview element positioned at the bottom-right of the GraphView.
	/// Compiles the full graph from the output node and renders a noise preview texture.
	/// Supports scroll-wheel zoom to adjust noise frequency.
	/// </summary>
	class FN2MainPreview : VisualElement
	{
		const int PreviewSize = 128;
		const long UpdateIntervalMs = 500;
		const float MinFrequency = 0.001f;
		const float MaxFrequency = 0.2f;
		const float ScrollMultiplier = 1.1f;

		readonly Image m_Image;
		string m_LastEncoded;
		Texture2D m_CachedTexture;

		public FN2MainPreview()
		{
			AddToClassList("fn2-main-preview");

			// Header
			var header = new VisualElement();
			header.AddToClassList("fn2-main-preview__header");

			var title = new Label("Main Preview");
			title.AddToClassList("fn2-main-preview__title");
			header.Add(title);

			Add(header);

			// Preview image
			m_Image = new Image();
			m_Image.AddToClassList("fn2-main-preview__image");
			Add(m_Image);

			// Scroll-wheel zoom on the entire element
			RegisterCallback<WheelEvent>(OnWheel);

			// Schedule periodic updates
			schedule.Execute(UpdatePreview).Every(UpdateIntervalMs);
		}

		void OnWheel(WheelEvent evt)
		{
			float factor = evt.delta.y > 0 ? ScrollMultiplier : 1f / ScrollMultiplier;
			FN2BridgeCallbacks.PreviewFrequency = Mathf.Clamp(
				FN2BridgeCallbacks.PreviewFrequency * factor, MinFrequency, MaxFrequency);

			// Force re-render at new frequency
			RenderFromEncoded(m_LastEncoded);

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

			string encoded = FN2BridgeCallbacks.CompileFullGraph(graph);

			if (encoded == m_LastEncoded)
				return;

			m_LastEncoded = encoded;
			RenderFromEncoded(encoded);
		}

		void RenderFromEncoded(string encoded)
		{
			if (string.IsNullOrEmpty(encoded))
			{
				if (m_CachedTexture != null)
				{
					Object.DestroyImmediate(m_CachedTexture);
					m_CachedTexture = null;
					m_Image.image = null;
				}
				return;
			}

			if (FN2BridgeCallbacks.RenderPreviewWithFrequency == null)
				return;

			var newTexture = FN2BridgeCallbacks.RenderPreviewWithFrequency(
				encoded, PreviewSize, PreviewSize, FN2BridgeCallbacks.PreviewFrequency);

			if (m_CachedTexture != null && m_CachedTexture != newTexture)
				Object.DestroyImmediate(m_CachedTexture);

			m_CachedTexture = newTexture;
			m_Image.image = m_CachedTexture;
		}
	}
}
