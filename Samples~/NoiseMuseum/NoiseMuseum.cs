using FastNoise2.Bindings;
using FastNoise2.Generators;
using UnityEngine;
using static FastNoise2.Generators.Noise;
using static Unity.Mathematics.math;

namespace FastNoise2.Samples
{
	/// <summary>
	/// Generates a grid of noise textures showcasing the full breadth of the FastNoise2 builder API.
	/// Each "exhibit" is a static method returning a NoiseNode, demonstrating a different API feature.
	/// </summary>
	public class NoiseMuseum : MonoBehaviour
	{
		[Header("Display")]
		[SerializeField] int m_TileResolution = 128;
		[SerializeField] float m_Frequency = 0.02f;
		[SerializeField] int m_Seed = 1337;

		Texture2D m_Texture;
		Renderer m_Renderer;

		static (string label, NoiseNode node)[] BuildExhibits()
		{
			return new (string, NoiseNode)[]
			{
				// --- 1. Basics: raw generator factory methods ---
				("Perlin", BasicPerlin()),
				("Simplex", BasicSimplex()),
				("Value", BasicValue()),

				// --- 2. Fractals: .Fbm() vs .Ridged() on the same source ---
				("Perlin FBM", FractalFbm()),
				("Perlin Ridged", FractalRidged()),

				// --- 3. Operators: all five arithmetic operators ---
				("Add (P+S)", OpAdd()),
				("Sub (P-C)", OpSubtract()),
				("Mul (*0.5)", OpMultiply()),
				("Div (/2)", OpDivide()),
				("Mod (%0.5)", OpModulo()),
				("Negate (-P)", OpNegate()),

				// --- 4. Hybrid params: constant vs noise-driven ---
				("FBM gain=0.5", HybridConstant()),
				("FBM gain=Simplex", HybridNoiseDriven()),

				// --- 5. Blend functions ---
				("Min(P,S)", BlendMin()),
				("Max(P,S)", BlendMax()),
				("MinSmooth", BlendMinSmooth()),
				("Fade(P,S)", BlendFade()),

				// --- 6. Domain warp ---
				("DWarp Gradient", DomainWarpGradient()),
				("DWarp Simplex", DomainWarpSimplex()),

				// --- 7. Domain transforms ---
				("DomainScale", TransformScale()),
				("DomainAxisScale", TransformAxisScale()),
				("DomainRotate", TransformRotate()),
				("DomainOffset", TransformOffset()),

				// --- 8. Modifiers ---
				("Terrace", ModTerrace()),
				("PingPong", ModPingPong()),
				("Abs", ModAbs()),
				("Remap", ModRemap()),
				("PowInt(3)", ModPowInt()),
				("PowFloat(0.5)", ModPowFloat()),

				// --- 9. Cellular variants ---
				("Cell Dist", CellDistance()),
				("Cell Dist Manhattan", CellDistanceManhattan()),
				("Cell Value", CellValue()),
				("Cell Lookup", CellLookup()),

				// --- 10. Specialized nodes ---
				("Gradient", SpecialGradient()),
				("DistToPoint", SpecialDistanceToPoint()),
				("Checkerboard", SpecialCheckerboard()),
				("SineWave", SpecialSineWave()),

				// --- 11. Composition: everything combined ---
				("Composition", Composition()),
			};
		}

		// =====================================================================
		// 1. Basics — raw factory methods
		// =====================================================================

		static NoiseNode BasicPerlin() => Perlin();
		static NoiseNode BasicSimplex() => Simplex();
		static NoiseNode BasicValue() => Value();

		// =====================================================================
		// 2. Fractals — .Fbm() and .Ridged() on the same Perlin source
		// =====================================================================

		static NoiseNode FractalFbm() => Perlin().Fbm(octaves: 5);
		static NoiseNode FractalRidged() => Perlin().Ridged(octaves: 5);

		// =====================================================================
		// 3. Operators — all five arithmetic operators + unary negate
		// =====================================================================

		// Add: blend two noise sources equally
		static NoiseNode OpAdd() => Perlin() * 0.5f + Simplex() * 0.5f;

		// Subtract: perlin minus cellular creates carved-out look
		static NoiseNode OpSubtract() => Perlin() - CellularDistance();

		// Multiply: scale noise amplitude
		static NoiseNode OpMultiply() => Perlin().Fbm(octaves: 5) * 0.5f;

		// Divide: halve noise range
		static NoiseNode OpDivide() => Perlin().Fbm(octaves: 5) / 2f;

		// Modulo: creates repeating bands
		static NoiseNode OpModulo() => Perlin().Fbm(octaves: 5) % 0.5f;

		// Unary negate: inverts noise
		static NoiseNode OpNegate() => -Perlin().Fbm(octaves: 5);

		// =====================================================================
		// 4. Hybrid params — constant float vs noise-driven NoiseNode
		// =====================================================================

		// Constant gain: uniform fractal detail everywhere
		static NoiseNode HybridConstant() => Perlin().Fbm(gain: 0.5f, octaves: 5);

		// Noise-driven gain: detail varies spatially based on Simplex
		static NoiseNode HybridNoiseDriven() => Perlin().Fbm(gain: Simplex(50f), octaves: 5);

		// =====================================================================
		// 5. Blend — Min, Max, MinSmooth, Fade between two sources
		// =====================================================================

		static NoiseNode BlendMin() => Perlin().Min(Simplex());
		static NoiseNode BlendMax() => Perlin().Max(Simplex());
		static NoiseNode BlendMinSmooth() => Perlin().MinSmooth(Simplex(), smoothness: 0.2f);
		static NoiseNode BlendFade() => Perlin().Fade(Simplex(), fade: 0.5f);

		// =====================================================================
		// 6. Domain warp — distort input coordinates with gradient/simplex warp
		// =====================================================================

		static NoiseNode DomainWarpGradient() => Perlin().DomainWarpGradient(warpAmplitude: 50f);
		static NoiseNode DomainWarpSimplex() => Perlin().DomainWarpSimplex(warpAmplitude: 50f);

		// =====================================================================
		// 7. Domain transforms — scale, axis-scale, rotate, offset
		// =====================================================================

		// Uniform scaling: zoom in by 2x
		static NoiseNode TransformScale() => Perlin().DomainScale(2f);

		// Per-axis scaling: stretch horizontally
		static NoiseNode TransformAxisScale() => Perlin().DomainAxisScale(x: 2f, y: 0.5f);

		// Rotation: 45-degree yaw rotation
		static NoiseNode TransformRotate() => Perlin().DomainRotate(yaw: 45f);

		// Offset: shift noise origin
		static NoiseNode TransformOffset() => Perlin().DomainOffset(x: 100f, y: 100f);

		// =====================================================================
		// 8. Modifiers — shape the output curve
		// =====================================================================

		// Terrace: creates plateau steps
		static NoiseNode ModTerrace() => Perlin().Fbm(octaves: 5).Terrace(6f, smoothness: 0.1f);

		// PingPong: folds values back and forth
		static NoiseNode ModPingPong() => Perlin().Fbm(octaves: 5).PingPong(strength: 2f);

		// Abs: absolute value creates ridge-like features
		static NoiseNode ModAbs() => Perlin().Fbm(octaves: 5).Abs();

		// Remap: shift output range from [-1,1] to [0,1]
		static NoiseNode ModRemap() => Perlin().Remap(fromMin: -1f, fromMax: 1f, toMin: 0f, toMax: 1f);

		// PowInt: raises to integer power (sharpens contrast)
		static NoiseNode ModPowInt() => Perlin().PowInt(3);

		// PowFloat: raises to float power (sqrt = expand low values)
		static NoiseNode ModPowFloat() => Perlin().Abs().PowFloat(0.5f);

		// =====================================================================
		// 9. Cellular — different return types, distance functions, lookup
		// =====================================================================

		// Default cellular distance (Euclidean, Index0)
		static NoiseNode CellDistance() =>
			CellularDistance()
				.WithReturnType(CellularReturnType.Index0);

		// Manhattan distance function: diamond-shaped cells
		static NoiseNode CellDistanceManhattan() =>
			CellularDistance()
				.WithDistanceFunction(DistanceFunction.Manhattan)
				.WithReturnType(CellularReturnType.Index0Sub1);

		// Cellular value: flat color per cell
		static NoiseNode CellValue() =>
			CellularValue()
				.WithDistanceFunction(DistanceFunction.Euclidean);

		// Cellular lookup: uses another noise to color cells
		static NoiseNode CellLookup() =>
			CellularLookup(Simplex(20f))
				.WithDistanceFunction(DistanceFunction.Euclidean);

		// =====================================================================
		// 10. Specialized nodes — Gradient, DistanceToPoint, Checkerboard, SineWave
		// =====================================================================

		// Gradient with per-axis multipliers
		static NoiseNode SpecialGradient() =>
			Gradient().WithMultipliers(x: 1f, y: 0.5f);

		// Distance to a specific point
		static NoiseNode SpecialDistanceToPoint() =>
			DistanceToPoint().WithPoint(0.5f, 0.5f);

		// Checkerboard pattern
		static NoiseNode SpecialCheckerboard() => Checkerboard(10f);

		// Sine wave
		static NoiseNode SpecialSineWave() => SineWave(20f);

		// =====================================================================
		// 11. Composition — one big expression combining multiple API features
		// =====================================================================

		static NoiseNode Composition()
		{
			// Terrain-like: warped ridged fractal blended with cellular,
			// terraced and remapped to [0,1]
			var ridged = Perlin()
				.Ridged(gain: Simplex(50f), octaves: 5)
				.DomainWarpGradient(warpAmplitude: 30f);

			var cells = CellularDistance()
				.WithReturnType(CellularReturnType.Index0Sub1)
				.WithDistanceFunction(DistanceFunction.Hybrid);

			return ridged.MinSmooth(cells, smoothness: 0.3f)
				.Terrace(8f, smoothness: 0.05f)
				.Remap(fromMin: -1f, fromMax: 1f, toMin: 0f, toMax: 1f);
		}

		// =====================================================================
		// Rendering
		// =====================================================================

		void OnEnable()
		{
			var exhibits = BuildExhibits();
			int count = exhibits.Length;

			// Calculate grid dimensions
			int cols = Mathf.CeilToInt(Mathf.Sqrt(count));
			int rows = Mathf.CeilToInt((float)count / cols);

			int texWidth = cols * m_TileResolution;
			int texHeight = rows * m_TileResolution;

			m_Texture = new Texture2D(texWidth, texHeight, TextureFormat.R8, false)
			{
				filterMode = FilterMode.Bilinear,
				wrapMode = TextureWrapMode.Clamp
			};

			var pixels = new Color32[texWidth * texHeight];
			var buffer = new float[m_TileResolution * m_TileResolution];

			for (int i = 0; i < count; i++)
			{
				var (label, node) = exhibits[i];

				using FastNoise fn = node.Build();
				if (!fn.IsCreated)
				{
					Debug.LogWarning($"NoiseMuseum: failed to build exhibit '{label}'");
					continue;
				}

				fn.GenUniformGrid2D(buffer, 0f, 0f,
					m_TileResolution, m_TileResolution, m_Frequency, m_Frequency, m_Seed);

				int col = i % cols;
				int row = rows - 1 - i / cols; // flip Y so first exhibit is top-left

				int baseX = col * m_TileResolution;
				int baseY = row * m_TileResolution;

				for (int py = 0; py < m_TileResolution; py++)
				{
					for (int px = 0; px < m_TileResolution; px++)
					{
						float val = buffer[py * m_TileResolution + px];
						byte v = (byte)(saturate(val * 0.5f + 0.5f) * 255f);
						pixels[(baseY + py) * texWidth + baseX + px] = new Color32(v, v, v, 255);
					}
				}
			}

			m_Texture.SetPixels32(pixels);
			m_Texture.Apply(false);

			m_Renderer = GetComponent<Renderer>();
			if (m_Renderer != null)
				m_Renderer.material.mainTexture = m_Texture;
		}

		void OnDisable()
		{
			if (m_Texture != null)
				Destroy(m_Texture);

			m_Texture = null;
		}
	}
}
