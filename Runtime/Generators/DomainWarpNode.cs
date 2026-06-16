using System.Collections.Generic;

namespace FastNoise2.Generators
{
	/// <summary>
	/// A noise node produced by domain warp methods. Only domain warp nodes
	/// can be chained with fractal domain warp operations.
	/// </summary>
	public class DomainWarpNode : NoiseNode
	{
		internal DomainWarpNode(NodeDescriptor descriptor) : base(descriptor)
		{
		}

		public NoiseNode DomainWarpProgressive(Hybrid gain = default,
			Hybrid weightedStrength = default, int octaves = 3, float lacunarity = 2f)
		{
			Dictionary<string, int> vars = new()
			{
				{ "Octaves", octaves },
				{ "Lacunarity", Bits(lacunarity) }
			};
			Dictionary<string, NodeDescriptor> nodes = new()
			{
				{ "DomainWarpSource", m_Descriptor }
			};
			Dictionary<string, HybridValue> hybrids = new();
			gain.AddTo(hybrids, "Gain");
			weightedStrength.AddTo(hybrids, "WeightedStrength");
			return new NoiseNode(new NodeDescriptor("DomainWarpFractalProgressive",
				vars, nodes, hybrids));
		}

		public NoiseNode DomainWarpIndependent(Hybrid gain = default,
			Hybrid weightedStrength = default, int octaves = 3, float lacunarity = 2f)
		{
			Dictionary<string, int> vars = new()
			{
				{ "Octaves", octaves },
				{ "Lacunarity", Bits(lacunarity) }
			};
			Dictionary<string, NodeDescriptor> nodes = new()
			{
				{ "DomainWarpSource", m_Descriptor }
			};
			Dictionary<string, HybridValue> hybrids = new();
			gain.AddTo(hybrids, "Gain");
			weightedStrength.AddTo(hybrids, "WeightedStrength");
			return new NoiseNode(new NodeDescriptor("DomainWarpFractalIndependent",
				vars, nodes, hybrids));
		}
	}
}
