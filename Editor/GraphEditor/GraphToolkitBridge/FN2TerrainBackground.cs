using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine;
using UnityEngine.UIElements;

namespace FastNoise2.Editor.GraphEditor
{
	class FN2TerrainBackground : VisualElement
	{
		const int TextureSize = 512;
		const double DebounceDelaySec = 0.5;

		readonly Image m_Image;
		readonly Debounce m_Debounce;
		RenderTexture m_RenderTexture;
		Texture2D m_Heightmap;
		string m_LastEncoded;

		public FN2TerrainBackground()
		{
			AddToClassList("fn2-terrain-background");
			pickingMode = PickingMode.Ignore;

			m_Image = new Image();
			m_Image.AddToClassList("fn2-terrain-background__image");
			m_Image.pickingMode = PickingMode.Ignore;
			Add(m_Image);

			m_Debounce = new Debounce(UpdateTerrain, DebounceDelaySec);

			RegisterCallback<AttachToPanelEvent>(OnAttach);
			RegisterCallback<DetachFromPanelEvent>(OnDetach);
		}

		void OnAttach(AttachToPanelEvent evt)
		{
			if (m_RenderTexture == null)
			{
				m_RenderTexture = new RenderTexture(TextureSize, TextureSize, 0, RenderTextureFormat.ARGB32)
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp
				};
				m_RenderTexture.Create();
			}

			m_Image.image = m_RenderTexture;
			m_LastEncoded = null;
			FN2EditorUpdate.Register(m_Debounce);
			FN2EditorUpdate.GraphChanged += OnGraphChanged;
			m_Debounce.Signal();
		}

		void OnDetach(DetachFromPanelEvent evt)
		{
			FN2EditorUpdate.GraphChanged -= OnGraphChanged;
			FN2EditorUpdate.Unregister(m_Debounce);

			if (m_RenderTexture != null)
			{
				m_RenderTexture.Release();
				Object.DestroyImmediate(m_RenderTexture);
				m_RenderTexture = null;
			}

			if (m_Heightmap != null)
			{
				Object.DestroyImmediate(m_Heightmap);
				m_Heightmap = null;
			}

			m_Image.image = null;
		}

		void OnGraphChanged()
		{
			m_Debounce.Signal();
		}

		void UpdateTerrain()
		{
			if (FN2BridgeCallbacks.CompileFullGraph == null
				|| FN2BridgeCallbacks.GenerateTerrainHeightmap == null
				|| FN2BridgeCallbacks.BlitTerrain == null)
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
			RenderTerrain(encoded);
		}

		void RenderTerrain(string encoded)
		{
			if (string.IsNullOrEmpty(encoded))
			{
				if (m_Heightmap != null)
				{
					Object.DestroyImmediate(m_Heightmap);
					m_Heightmap = null;
				}
				// Clear the render texture to transparent
				var prev = RenderTexture.active;
				RenderTexture.active = m_RenderTexture;
				GL.Clear(true, true, Color.clear);
				RenderTexture.active = prev;
				return;
			}

			var newHeightmap = FN2BridgeCallbacks.GenerateTerrainHeightmap(encoded);
			if (newHeightmap == null)
				return;

			if (m_Heightmap != null && m_Heightmap != newHeightmap)
				Object.DestroyImmediate(m_Heightmap);

			m_Heightmap = newHeightmap;
			FN2BridgeCallbacks.BlitTerrain(m_Heightmap, m_RenderTexture);
		}
	}
}
