using FastNoise2.Bindings;

namespace FastNoise2.Generators
{
	/// <summary>
	/// Builder for CellularLookup noise. Immutable: each <c>With*</c> returns a new instance.
	/// </summary>
	public class CellularLookupNode : NoiseNode
	{
		readonly NoiseNode m_Lookup;
		readonly DistanceFunction m_DistFunc;
		readonly Hybrid m_GridJitter, m_SizeJitter, m_MinkowskiP;

		internal CellularLookupNode(NoiseNode lookup, DistanceFunction distFunc,
			Hybrid gridJitter, Hybrid sizeJitter, Hybrid minkowskiP)
			: base(() =>
			{
				FastNoise fn = new("CellularLookup");
				fn.Set("Lookup", lookup.Build());
				fn.Set("DistanceFunction", distFunc.ToMetadataString());
				gridJitter.Apply(fn, "GridJitter");
				sizeJitter.Apply(fn, "SizeJitter");
				minkowskiP.Apply(fn, "MinkowskiP");
				return fn;
			})
		{
			m_Lookup = lookup;
			m_DistFunc = distFunc;
			m_GridJitter = gridJitter;
			m_SizeJitter = sizeJitter;
			m_MinkowskiP = minkowskiP;
		}

		public CellularLookupNode WithDistanceFunction(DistanceFunction value) =>
			new(m_Lookup, value, m_GridJitter, m_SizeJitter, m_MinkowskiP);

		public CellularLookupNode WithGridJitter(Hybrid value) =>
			new(m_Lookup, m_DistFunc, value, m_SizeJitter, m_MinkowskiP);

		public CellularLookupNode WithSizeJitter(Hybrid value) =>
			new(m_Lookup, m_DistFunc, m_GridJitter, value, m_MinkowskiP);

		public CellularLookupNode WithMinkowskiP(Hybrid value) =>
			new(m_Lookup, m_DistFunc, m_GridJitter, m_SizeJitter, value);
	}
}
