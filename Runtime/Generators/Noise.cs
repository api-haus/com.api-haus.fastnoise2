using FastNoise2.Bindings;

namespace FastNoise2.Generators
{
	/// <summary>
	/// Static factory for creating noise generator nodes.
	/// </summary>
	public static class Noise
	{
		#region Basic

		public static NoiseNode Constant(float value = 1f) =>
			new(() =>
			{
				FastNoise fn = new("Constant");
				fn.Set("Value", value);
				return fn;
			});

		public static NoiseNode White(int seedOffset = 0) =>
			new(() =>
			{
				FastNoise fn = new("White");
				if (seedOffset != 0)
					fn.Set("SeedOffset", seedOffset);
				return fn;
			});

		public static NoiseNode Checkerboard(float featureScale = 100f) =>
			new(() =>
			{
				FastNoise fn = new("Checkerboard");
				fn.Set("FeatureScale", featureScale);
				return fn;
			});

		public static NoiseNode SineWave(float featureScale = 100f) =>
			new(() =>
			{
				FastNoise fn = new("SineWave");
				fn.Set("FeatureScale", featureScale);
				return fn;
			});

		public static GradientNode Gradient() => new(1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

		public static DistanceToPointNode DistanceToPoint() =>
			new(DistanceFunction.Euclidean, 0f, 0f, 0f, 0f, 0f);

		#endregion

		#region Coherent Noise

		public static NoiseNode Simplex(float featureScale = 100f, int seedOffset = 0) =>
			new(() =>
			{
				FastNoise fn = new("Simplex");
				fn.Set("FeatureScale", featureScale);
				if (seedOffset != 0)
					fn.Set("SeedOffset", seedOffset);
				return fn;
			});

		public static NoiseNode SuperSimplex(float featureScale = 100f, int seedOffset = 0) =>
			new(() =>
			{
				FastNoise fn = new("SuperSimplex");
				fn.Set("FeatureScale", featureScale);
				if (seedOffset != 0)
					fn.Set("SeedOffset", seedOffset);
				return fn;
			});

		public static NoiseNode Perlin(float featureScale = 100f, int seedOffset = 0) =>
			new(() =>
			{
				FastNoise fn = new("Perlin");
				fn.Set("FeatureScale", featureScale);
				if (seedOffset != 0)
					fn.Set("SeedOffset", seedOffset);
				return fn;
			});

		public static NoiseNode Value(float featureScale = 100f, int seedOffset = 0) =>
			new(() =>
			{
				FastNoise fn = new("Value");
				fn.Set("FeatureScale", featureScale);
				if (seedOffset != 0)
					fn.Set("SeedOffset", seedOffset);
				return fn;
			});

		#endregion

		#region Cellular

		public static CellularValueNode CellularValue() =>
			new(DistanceFunction.Euclidean, 0, 0f, 0f, 0f);

		public static CellularDistanceNode CellularDistance() =>
			new(DistanceFunction.Euclidean, CellularReturnType.Index0, 0, 1, 0f, 0f, 0f);

		public static CellularLookupNode CellularLookup(NoiseNode lookup) =>
			new(lookup, DistanceFunction.Euclidean, 0f, 0f, 0f);

		#endregion
	}
}
