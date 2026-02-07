using UnityEngine;
using UnityEngine.UIElements;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Reusable dual-mode preview widget that can display noise as either
	/// a flat grayscale texture or a raymarched heightfield.
	/// </summary>
	class FN2PreviewWidget : VisualElement
	{
		public enum PreviewMode { Texture, Heightfield }

		/// <summary>
		/// Global mode shared across all widget instances.
		/// Toggled by the mode button in the Main Preview header.
		/// </summary>
		public static PreviewMode Mode = PreviewMode.Texture;

		static RenderTexture s_SharedRT;

		readonly Image m_Image;
		readonly int m_PreviewSize;

		string m_LastEncoded;
		float m_LastFrequency;
		PreviewMode m_LastMode;

		Texture2D m_CachedTexture;
		Texture2D m_CachedHeightmap;

		public int PreviewSize => m_PreviewSize;

		public FN2PreviewWidget(int previewSize)
		{
			m_PreviewSize = previewSize;

			AddToClassList("fn2-preview-widget");

			m_Image = new Image();
			m_Image.AddToClassList("fn2-preview-widget__image");
			m_Image.style.width = previewSize;
			m_Image.style.height = previewSize;
			Add(m_Image);

			RegisterCallback<DetachFromPanelEvent>(OnDetach);
		}

		void OnDetach(DetachFromPanelEvent evt)
		{
			CleanupTextures();
			m_LastEncoded = null;
		}

		/// <summary>
		/// Feed the widget an encoded noise tree. It renders based on the current mode.
		/// </summary>
		public void SetEncoded(string encoded, float frequency)
		{
			if (encoded == m_LastEncoded && Mathf.Approximately(frequency, m_LastFrequency) && Mode == m_LastMode)
				return;

			m_LastEncoded = encoded;
			m_LastFrequency = frequency;
			m_LastMode = Mode;

			Render(encoded, frequency);
		}

		/// <summary>
		/// Force re-render at current encoded/frequency (e.g., after mode toggle).
		/// </summary>
		public void Refresh()
		{
			m_LastMode = Mode;
			Render(m_LastEncoded, m_LastFrequency);
		}

		void Render(string encoded, float frequency)
		{
			if (string.IsNullOrEmpty(encoded))
			{
				CleanupTextures();
				m_Image.image = null;
				return;
			}

			if (Mode == PreviewMode.Texture)
				RenderFlat(encoded, frequency);
			else
				RenderHeightfield(encoded, frequency);
		}

		void RenderFlat(string encoded, float frequency)
		{
			if (FN2BridgeCallbacks.RenderPreviewWithFrequency == null)
				return;

			var newTexture = FN2BridgeCallbacks.RenderPreviewWithFrequency(
				encoded, m_PreviewSize, m_PreviewSize, frequency);

			if (m_CachedTexture != null && m_CachedTexture != newTexture)
				Object.DestroyImmediate(m_CachedTexture);

			m_CachedTexture = newTexture;
			m_Image.image = m_CachedTexture;
		}

		void RenderHeightfield(string encoded, float frequency)
		{
			if (FN2BridgeCallbacks.GenerateHeightmapWithFrequency == null
				|| FN2BridgeCallbacks.BlitTerrain == null)
				return;

			var newHeightmap = FN2BridgeCallbacks.GenerateHeightmapWithFrequency(
				encoded, m_PreviewSize, m_PreviewSize, frequency);

			if (newHeightmap == null)
				return;

			if (m_CachedHeightmap != null && m_CachedHeightmap != newHeightmap)
				Object.DestroyImmediate(m_CachedHeightmap);
			m_CachedHeightmap = newHeightmap;

			EnsureSharedRT(m_PreviewSize);
			FN2BridgeCallbacks.BlitTerrain(m_CachedHeightmap, s_SharedRT);

			// Read back from RT into a Texture2D for display
			if (m_CachedTexture == null
				|| m_CachedTexture.width != m_PreviewSize
				|| m_CachedTexture.height != m_PreviewSize
				|| m_CachedTexture.format != TextureFormat.RGBA32)
			{
				if (m_CachedTexture != null)
					Object.DestroyImmediate(m_CachedTexture);
				m_CachedTexture = new Texture2D(m_PreviewSize, m_PreviewSize, TextureFormat.RGBA32, false)
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp
				};
			}

			var prevRT = UnityEngine.RenderTexture.active;
			UnityEngine.RenderTexture.active = s_SharedRT;
			m_CachedTexture.ReadPixels(new Rect(0, 0, m_PreviewSize, m_PreviewSize), 0, 0, false);
			m_CachedTexture.Apply(false, false);
			UnityEngine.RenderTexture.active = prevRT;

			m_Image.image = m_CachedTexture;
		}

		static void EnsureSharedRT(int size)
		{
			if (s_SharedRT != null && s_SharedRT.width >= size && s_SharedRT.height >= size)
				return;

			if (s_SharedRT != null)
			{
				s_SharedRT.Release();
				Object.DestroyImmediate(s_SharedRT);
			}

			s_SharedRT = new UnityEngine.RenderTexture(size, size, 0, RenderTextureFormat.ARGB32)
			{
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};
			s_SharedRT.Create();
		}

		void CleanupTextures()
		{
			if (m_CachedTexture != null)
			{
				Object.DestroyImmediate(m_CachedTexture);
				m_CachedTexture = null;
			}

			if (m_CachedHeightmap != null)
			{
				Object.DestroyImmediate(m_CachedHeightmap);
				m_CachedHeightmap = null;
			}
		}
	}
}
