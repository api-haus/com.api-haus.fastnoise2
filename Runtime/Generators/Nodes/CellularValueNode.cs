using FastNoise2.Bindings;

namespace FastNoise2.Generators
{
	/// <summary>
	/// Builder for CellularValue noise. Immutable: each <c>With*</c> returns a new instance.
	/// </summary>
	public class CellularValueNode : NoiseNode
	{
		readonly DistanceFunction m_DistFunc;
		readonly int m_ValueIndex;
		readonly Hybrid m_GridJitter, m_SizeJitter, m_MinkowskiP;

		internal CellularValueNode(DistanceFunction distFunc, int valueIndex,
			Hybrid gridJitter, Hybrid sizeJitter, Hybrid minkowskiP)
			: base(() =>
			{
				FastNoise fn = new("CellularValue");
				fn.Set("DistanceFunction", distFunc.ToMetadataString());
				fn.Set("ValueIndex", valueIndex);
				gridJitter.Apply(fn, "GridJitter");
				sizeJitter.Apply(fn, "SizeJitter");
				minkowskiP.Apply(fn, "MinkowskiP");
				return fn;
			})
		{
			m_DistFunc = distFunc;
			m_ValueIndex = valueIndex;
			m_GridJitter = gridJitter;
			m_SizeJitter = sizeJitter;
			m_MinkowskiP = minkowskiP;
		}

		public CellularValueNode WithDistanceFunction(DistanceFunction value) =>
			new(value, m_ValueIndex, m_GridJitter, m_SizeJitter, m_MinkowskiP);

		public CellularValueNode WithValueIndex(int value) =>
			new(m_DistFunc, value, m_GridJitter, m_SizeJitter, m_MinkowskiP);

		public CellularValueNode WithGridJitter(Hybrid value) =>
			new(m_DistFunc, m_ValueIndex, value, m_SizeJitter, m_MinkowskiP);

		public CellularValueNode WithSizeJitter(Hybrid value) =>
			new(m_DistFunc, m_ValueIndex, m_GridJitter, value, m_MinkowskiP);

		public CellularValueNode WithMinkowskiP(Hybrid value) =>
			new(m_DistFunc, m_ValueIndex, m_GridJitter, m_SizeJitter, value);
	}
}
