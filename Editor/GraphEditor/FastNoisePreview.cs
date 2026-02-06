using FastNoise2.Bindings;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Static utility for rendering 2D noise preview textures from encoded node trees.
	/// </summary>
	static class FastNoisePreview
	{
		public static Texture2D RenderPreview(string encoded, int width, int height)
		{
#if FN2_USER_SIGNED
			if (string.IsNullOrEmpty(encoded))
				return null;

			FastNoise noise = FastNoise.FromEncodedNodeTree(encoded);
			if (!noise.IsCreated)
				return null;

			try
			{
				float[] noiseData = new float[width * height];
				noise.GenUniformGrid2D(noiseData, 0f, 0f, width, height, 0.02f, 0.02f, 1337);

				var texture = new Texture2D(width, height, TextureFormat.R8, false)
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp
				};

				var pixels = new Color32[width * height];
				for (int i = 0; i < noiseData.Length; i++)
				{
					byte v = (byte)(Mathf.Clamp01(noiseData[i] * 0.5f + 0.5f) * 255f);
					pixels[i] = new Color32(v, v, v, 255);
				}

				texture.SetPixels32(pixels);
				texture.Apply(false, false);
				return texture;
			}
			finally
			{
				noise.Dispose();
			}
#else
			return null;
#endif
		}
	}
}
