using FastNoise2.Bindings;

namespace FastNoise2.Generators
{
	/// <summary>
	/// Builder for CellularDistance noise. Immutable: each <c>With*</c> returns a new instance.
	/// </summary>
	public class CellularDistanceNode : NoiseNode
	{
		readonly DistanceFunction m_DistFunc;
		readonly CellularReturnType m_ReturnType;
		readonly int m_DistIdx0, m_DistIdx1;
		readonly Hybrid m_GridJitter, m_SizeJitter, m_MinkowskiP;

		internal CellularDistanceNode(DistanceFunction distFunc,
			CellularReturnType returnType, int distIdx0, int distIdx1,
			Hybrid gridJitter, Hybrid sizeJitter, Hybrid minkowskiP)
			: base(() =>
			{
				FastNoise fn = new("CellularDistance");
				fn.Set("DistanceFunction", distFunc.ToMetadataString());
				fn.Set("ReturnType", returnType.ToMetadataString());
				fn.Set("DistanceIndex0", distIdx0);
				fn.Set("DistanceIndex1", distIdx1);
				gridJitter.Apply(fn, "GridJitter");
				sizeJitter.Apply(fn, "SizeJitter");
				minkowskiP.Apply(fn, "MinkowskiP");
				return fn;
			})
		{
			m_DistFunc = distFunc;
			m_ReturnType = returnType;
			m_DistIdx0 = distIdx0;
			m_DistIdx1 = distIdx1;
			m_GridJitter = gridJitter;
			m_SizeJitter = sizeJitter;
			m_MinkowskiP = minkowskiP;
		}

		public CellularDistanceNode WithDistanceFunction(DistanceFunction value) =>
			new(value, m_ReturnType, m_DistIdx0, m_DistIdx1,
				m_GridJitter, m_SizeJitter, m_MinkowskiP);

		public CellularDistanceNode WithReturnType(CellularReturnType value) =>
			new(m_DistFunc, value, m_DistIdx0, m_DistIdx1,
				m_GridJitter, m_SizeJitter, m_MinkowskiP);

		public CellularDistanceNode WithDistanceIndex0(int value) =>
			new(m_DistFunc, m_ReturnType, value, m_DistIdx1,
				m_GridJitter, m_SizeJitter, m_MinkowskiP);

		public CellularDistanceNode WithDistanceIndex1(int value) =>
			new(m_DistFunc, m_ReturnType, m_DistIdx0, value,
				m_GridJitter, m_SizeJitter, m_MinkowskiP);

		public CellularDistanceNode WithGridJitter(Hybrid value) =>
			new(m_DistFunc, m_ReturnType, m_DistIdx0, m_DistIdx1,
				value, m_SizeJitter, m_MinkowskiP);

		public CellularDistanceNode WithSizeJitter(Hybrid value) =>
			new(m_DistFunc, m_ReturnType, m_DistIdx0, m_DistIdx1,
				m_GridJitter, value, m_MinkowskiP);

		public CellularDistanceNode WithMinkowskiP(Hybrid value) =>
			new(m_DistFunc, m_ReturnType, m_DistIdx0, m_DistIdx1,
				m_GridJitter, m_SizeJitter, value);
	}
}
