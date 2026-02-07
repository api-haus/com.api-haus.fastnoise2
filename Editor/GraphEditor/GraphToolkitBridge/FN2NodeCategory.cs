using System.Collections.Generic;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	public static class FN2NodeCategory
	{
		static readonly Dictionary<string, Color> k_GroupColors = new()
		{
			{ "Basic Generators", new Color(0.30f, 0.55f, 0.52f) },
			{ "Coherent Noise", new Color(0.32f, 0.46f, 0.63f) },
			{ "Blends", new Color(0.42f, 0.58f, 0.38f) },
			{ "Fractal", new Color(0.52f, 0.40f, 0.62f) },
			{ "Domain Warp", new Color(0.65f, 0.48f, 0.32f) },
			{ "Modifiers", new Color(0.60f, 0.38f, 0.42f) },
			{ "Domain Modifiers", new Color(0.58f, 0.54f, 0.34f) },
		};

		static readonly Color k_BaseColor = new(0.157f, 0.157f, 0.157f); // #282828

		static Texture2D s_SheenTexture;

		public static string[] GetSynonyms(string nodeTypeName)
		{
			return null;
		}

		public static bool TryGetCategoryPath(string nodeTypeName, out string categoryPath)
		{
			categoryPath = FN2BridgeCallbacks.GetNodeCategoryPath?.Invoke(nodeTypeName);
			return categoryPath != null;
		}

		public static bool TryGetCategoryColor(string nodeTypeName, out Color color)
		{
			color = default;
			if (FN2BridgeCallbacks.GetNodeCategoryColor == null)
				return false;

			color = FN2BridgeCallbacks.GetNodeCategoryColor(nodeTypeName);
			return color.a > 0;
		}

		public static Color GetGroupColor(string groupName)
		{
			if (groupName != null && k_GroupColors.TryGetValue(groupName, out var groupColor))
				return Color.Lerp(k_BaseColor, groupColor, 0.25f);
			return default;
		}

		public static Texture2D SheenTexture
		{
			get
			{
				if (s_SheenTexture == null)
				{
					const int height = 32;
					s_SheenTexture = new Texture2D(1, height, TextureFormat.RGBA32, false)
					{
						wrapMode = TextureWrapMode.Clamp,
						filterMode = FilterMode.Bilinear,
						hideFlags = HideFlags.HideAndDontSave,
					};

					var pixels = new Color[height];
					for (int i = 0; i < height; i++)
					{
						// t=1 at top, t=0 at bottom
						float t = (height - 1 - i) / (float)(height - 1);
						// Quadratic falloff: peak ~12% at top, fading to 0
						float alpha = 0.12f * t * t;
						pixels[i] = new Color(1f, 1f, 1f, alpha);
					}

					s_SheenTexture.SetPixels(pixels);
					s_SheenTexture.Apply(false, true);
				}

				return s_SheenTexture;
			}
		}
	}
}
