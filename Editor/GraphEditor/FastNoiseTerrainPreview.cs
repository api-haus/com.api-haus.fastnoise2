using FastNoise2.Bindings;
using UnityEditor;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	static class FastNoiseTerrainPreview
	{
		const int DefaultHeightmapSize = 512;
		const float DefaultFrequency = 0.01f;
		const string ShaderPath = "Packages/com.auburn.fastnoise2/Editor/GraphEditor/Shaders/FN2TerrainRaymarch.shader";

		static Material s_Material;

		public static Texture2D GenerateHeightmap(string encoded)
		{
			return GenerateHeightmap(encoded, DefaultHeightmapSize, DefaultHeightmapSize, DefaultFrequency);
		}

		public static Texture2D GenerateHeightmap(string encoded, int width, int height, float frequency)
		{
#if FN2_USER_SIGNED
			if (string.IsNullOrEmpty(encoded))
				return null;

			FastNoise noise = FastNoise.FromEncodedNodeTree(encoded);
			if (!noise.IsCreated)
				return null;

			try
			{
				float[] data = new float[width * height];
				var pan = FN2BridgeCallbacks.PanOffset;
				var minMax = noise.GenUniformGrid2D(data, pan.x, pan.y, width, height, frequency, frequency, 1337);

				var texture = new Texture2D(width, height, TextureFormat.RFloat, false)
				{
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp
				};

				// Normalize to actual data range so peaks are consistent across zoom levels
				float range = minMax.max - minMax.min;
				if (range < 1e-6f)
				{
					for (int i = 0; i < data.Length; i++)
						data[i] = 0.5f;
				}
				else
				{
					float invRange = 1f / range;
					for (int i = 0; i < data.Length; i++)
						data[i] = (data[i] - minMax.min) * invRange;
				}

				texture.SetPixelData(data, 0);
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

		public static void BlitTerrain(Texture2D heightmap, RenderTexture target)
		{
			if (heightmap == null || target == null)
				return;

			if (s_Material == null)
			{
				var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
				if (shader == null)
					return;
				s_Material = new Material(shader);
			}

			s_Material.SetTexture("_HeightMap", heightmap);
			s_Material.SetFloat("_HeightScale", FN2BridgeCallbacks.HeightScale);
			s_Material.SetFloat("_CamYaw", FN2BridgeCallbacks.CameraYaw);
			s_Material.SetFloat("_CamPitch", FN2BridgeCallbacks.CameraPitch);
			Graphics.Blit(null, target, s_Material);
		}
	}
}
