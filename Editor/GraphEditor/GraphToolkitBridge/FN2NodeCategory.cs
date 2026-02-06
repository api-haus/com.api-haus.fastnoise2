using System.Collections.Generic;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	public enum FN2Category
	{
		BasicGenerators,
		CoherentNoise,
		Blends,
		Fractal,
		DomainWarp,
		Modifiers,
		DomainModifiers,
	}

	public static class FN2NodeCategory
	{
		static readonly Dictionary<string, FN2Category> k_NodeCategories = new()
		{
			// Basic Generators
			{ "Constant", FN2Category.BasicGenerators },
			{ "White", FN2Category.BasicGenerators },
			{ "Checkerboard", FN2Category.BasicGenerators },
			{ "SineWave", FN2Category.BasicGenerators },
			{ "Gradient", FN2Category.BasicGenerators },
			{ "DistanceToPoint", FN2Category.BasicGenerators },

			// Coherent Noise
			{ "Simplex", FN2Category.CoherentNoise },
			{ "SuperSimplex", FN2Category.CoherentNoise },
			{ "Perlin", FN2Category.CoherentNoise },
			{ "Value", FN2Category.CoherentNoise },
			{ "CellularValue", FN2Category.CoherentNoise },
			{ "CellularDistance", FN2Category.CoherentNoise },
			{ "CellularLookup", FN2Category.CoherentNoise },

			// Blends
			{ "Add", FN2Category.Blends },
			{ "Subtract", FN2Category.Blends },
			{ "Multiply", FN2Category.Blends },
			{ "Divide", FN2Category.Blends },
			{ "Min", FN2Category.Blends },
			{ "Max", FN2Category.Blends },
			{ "MinSmooth", FN2Category.Blends },
			{ "MaxSmooth", FN2Category.Blends },
			{ "Fade", FN2Category.Blends },
			{ "PowFloat", FN2Category.Blends },
			{ "PowInt", FN2Category.Blends },
			{ "Modulus", FN2Category.Blends },

			// Fractal
			{ "FractalFBm", FN2Category.Fractal },
			{ "FractalRidged", FN2Category.Fractal },

			// Domain Warp
			{ "DomainWarpSimplex", FN2Category.DomainWarp },
			{ "DomainWarpSuperSimplex", FN2Category.DomainWarp },
			{ "DomainWarpGradient", FN2Category.DomainWarp },
			{ "DomainWarpFractalProgressive", FN2Category.DomainWarp },
			{ "DomainWarpFractalIndependent", FN2Category.DomainWarp },

			// Modifiers
			{ "PingPong", FN2Category.Modifiers },
			{ "Abs", FN2Category.Modifiers },
			{ "SignedSquareRoot", FN2Category.Modifiers },
			{ "SeedOffset", FN2Category.Modifiers },
			{ "Remap", FN2Category.Modifiers },
			{ "ConvertRGBA8", FN2Category.Modifiers },
			{ "Terrace", FN2Category.Modifiers },
			{ "GeneratorCache", FN2Category.Modifiers },

			// Domain Modifiers
			{ "DomainScale", FN2Category.DomainModifiers },
			{ "DomainOffset", FN2Category.DomainModifiers },
			{ "DomainRotate", FN2Category.DomainModifiers },
			{ "DomainAxisScale", FN2Category.DomainModifiers },
			{ "AddDimension", FN2Category.DomainModifiers },
			{ "RemoveDimension", FN2Category.DomainModifiers },
			{ "DomainRotatePlane", FN2Category.DomainModifiers },
		};

		static readonly Dictionary<FN2Category, Color> k_CategoryColors = new()
		{
			{ FN2Category.BasicGenerators, new Color(0.30f, 0.55f, 0.52f) },
			{ FN2Category.CoherentNoise, new Color(0.32f, 0.46f, 0.63f) },
			{ FN2Category.Blends, new Color(0.42f, 0.58f, 0.38f) },
			{ FN2Category.Fractal, new Color(0.52f, 0.40f, 0.62f) },
			{ FN2Category.DomainWarp, new Color(0.65f, 0.48f, 0.32f) },
			{ FN2Category.Modifiers, new Color(0.60f, 0.38f, 0.42f) },
			{ FN2Category.DomainModifiers, new Color(0.58f, 0.54f, 0.34f) },
		};

		static readonly Dictionary<FN2Category, string> k_CategoryNames = new()
		{
			{ FN2Category.BasicGenerators, "Basic Generators" },
			{ FN2Category.CoherentNoise, "Coherent Noise" },
			{ FN2Category.Blends, "Blends" },
			{ FN2Category.Fractal, "Fractal" },
			{ FN2Category.DomainWarp, "Domain Warp" },
			{ FN2Category.Modifiers, "Modifiers" },
			{ FN2Category.DomainModifiers, "Domain Modifiers" },
		};

		static readonly Dictionary<string, string[]> k_NodeSynonyms = new()
		{
			// Basic Generators
			{ "Constant", new[] { "flat", "uniform", "fixed" } },
			{ "White", new[] { "random", "static", "white noise" } },
			{ "Checkerboard", new[] { "checker", "grid", "tiling" } },
			{ "SineWave", new[] { "sine", "sin", "wave", "oscillate", "periodic" } },
			{ "Gradient", new[] { "ramp", "slope", "linear" } },
			{ "DistanceToPoint", new[] { "distance", "radial", "point", "euclidean" } },

			// Coherent Noise
			{ "Simplex", new[] { "simplex noise", "snoise" } },
			{ "SuperSimplex", new[] { "open simplex" } },
			{ "Perlin", new[] { "perlin noise", "classic noise" } },
			{ "Value", new[] { "value noise", "interpolated" } },
			{ "CellularValue", new[] { "voronoi", "worley", "cell value" } },
			{ "CellularDistance", new[] { "voronoi distance", "worley distance", "cell distance" } },
			{ "CellularLookup", new[] { "voronoi lookup", "worley lookup", "cell lookup" } },

			// Blends
			{ "Add", new[] { "sum", "plus", "combine" } },
			{ "Subtract", new[] { "minus", "difference" } },
			{ "Multiply", new[] { "mul", "times", "scale" } },
			{ "Divide", new[] { "div", "ratio" } },
			{ "Min", new[] { "minimum", "floor", "clamp low", "darkest" } },
			{ "Max", new[] { "maximum", "ceil", "clamp high", "brightest" } },
			{ "MinSmooth", new[] { "smooth minimum", "soft min", "smin" } },
			{ "MaxSmooth", new[] { "smooth maximum", "soft max", "smax" } },
			{ "Fade", new[] { "lerp", "mix", "blend", "interpolate" } },
			{ "PowFloat", new[] { "power", "exponent", "pow" } },
			{ "PowInt", new[] { "power int", "exponent int", "square", "cube" } },
			{ "Modulus", new[] { "mod", "remainder", "fmod", "wrap" } },

			// Fractal
			{ "FractalFBm", new[] { "fbm", "fractional brownian", "octaves", "lacunarity" } },
			{ "FractalRidged", new[] { "ridged", "ridged multi", "turbulence", "billowy" } },

			// Domain Warp
			{ "DomainWarpSimplex", new[] { "warp simplex", "distort simplex" } },
			{ "DomainWarpSuperSimplex", new[] { "warp open simplex", "distort super simplex" } },
			{ "DomainWarpGradient", new[] { "warp gradient", "distort gradient" } },
			{ "DomainWarpFractalProgressive", new[] { "warp fractal progressive", "distort fractal" } },
			{ "DomainWarpFractalIndependent", new[] { "warp fractal independent", "distort independent" } },

			// Modifiers
			{ "PingPong", new[] { "ping pong", "bounce", "triangle wave", "zigzag" } },
			{ "Abs", new[] { "absolute", "absolute value" } },
			{ "SignedSquareRoot", new[] { "sqrt", "square root", "signed sqrt" } },
			{ "SeedOffset", new[] { "seed", "variation", "seed shift" } },
			{ "Remap", new[] { "range", "rescale", "map range", "normalize" } },
			{ "ConvertRGBA8", new[] { "rgba", "color", "8bit", "quantize" } },
			{ "Terrace", new[] { "step", "staircase", "posterize", "quantize levels" } },
			{ "GeneratorCache", new[] { "cache", "memoize", "store" } },

			// Domain Modifiers
			{ "DomainScale", new[] { "frequency", "zoom", "scale domain" } },
			{ "DomainOffset", new[] { "translate", "shift", "move domain" } },
			{ "DomainRotate", new[] { "rotate", "spin", "twist" } },
			{ "DomainAxisScale", new[] { "stretch", "squash", "anisotropic", "axis scale" } },
			{ "AddDimension", new[] { "add dim", "extra dimension", "4d" } },
			{ "RemoveDimension", new[] { "remove dim", "reduce dimension", "slice" } },
			{ "DomainRotatePlane", new[] { "rotate plane", "2d rotate", "planar rotation" } },
		};

		static readonly Color k_BaseColor = new(0.157f, 0.157f, 0.157f); // #282828

		static Texture2D s_SheenTexture;

		public static string[] GetSynonyms(string nodeTypeName)
		{
			if (nodeTypeName != null && k_NodeSynonyms.TryGetValue(nodeTypeName, out var synonyms))
				return synonyms;
			return null;
		}

		public static bool TryGetCategoryPath(string nodeTypeName, out string categoryPath)
		{
			if (nodeTypeName != null
				&& k_NodeCategories.TryGetValue(nodeTypeName, out var category)
				&& k_CategoryNames.TryGetValue(category, out categoryPath))
			{
				return true;
			}

			categoryPath = null;
			return false;
		}

		public static bool TryGetCategoryColor(string nodeTypeName, out Color color)
		{
			if (nodeTypeName != null
				&& k_NodeCategories.TryGetValue(nodeTypeName, out var category)
				&& k_CategoryColors.TryGetValue(category, out var categoryColor))
			{
				color = Color.Lerp(k_BaseColor, categoryColor, 0.25f);
				return true;
			}

			color = default;
			return false;
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
