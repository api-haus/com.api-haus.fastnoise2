using FastNoise2.Bindings;

namespace FastNoise2.Generators
{
	using System;

	/// <summary>
	/// A noise node produced by domain warp methods. Only domain warp nodes
	/// can be chained with fractal domain warp operations.
	/// </summary>
	public class DomainWarpNode : NoiseNode
	{
		internal DomainWarpNode(Func<FastNoise> buildFunc) : base(buildFunc)
		{
		}

		public NoiseNode DomainWarpProgressive(Hybrid gain = default,
			Hybrid weightedStrength = default, int octaves = 3, float lacunarity = 2f)
		{
			DomainWarpNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("DomainWarpFractalProgressive");
				fn.Set("DomainWarpSource", source.Build());
				gain.Apply(fn, "Gain");
				weightedStrength.Apply(fn, "WeightedStrength");
				fn.Set("Octaves", octaves);
				fn.Set("Lacunarity", lacunarity);
				return fn;
			});
		}

		public NoiseNode DomainWarpIndependent(Hybrid gain = default,
			Hybrid weightedStrength = default, int octaves = 3, float lacunarity = 2f)
		{
			DomainWarpNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("DomainWarpFractalIndependent");
				fn.Set("DomainWarpSource", source.Build());
				gain.Apply(fn, "Gain");
				weightedStrength.Apply(fn, "WeightedStrength");
				fn.Set("Octaves", octaves);
				fn.Set("Lacunarity", lacunarity);
				return fn;
			});
		}
	}
}
