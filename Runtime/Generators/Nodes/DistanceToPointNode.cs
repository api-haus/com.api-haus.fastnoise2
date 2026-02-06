using FastNoise2.Bindings;

namespace FastNoise2.Generators
{
	/// <summary>
	/// Builder for DistanceToPoint noise with configurable distance function and target point.
	/// Immutable: each <c>With*</c> method returns a new instance.
	/// </summary>
	public class DistanceToPointNode : NoiseNode
	{
		readonly DistanceFunction m_DistFunc;
		readonly float m_Px, m_Py, m_Pz, m_Pw;
		readonly float m_MinkowskiP;

		internal DistanceToPointNode(DistanceFunction distFunc,
			float px, float py, float pz, float pw, float minkowskiP = 0f)
			: base(() =>
			{
				FastNoise fn = new("DistanceToPoint");
				fn.Set("DistanceFunction", distFunc.ToMetadataString());
				fn.Set("PointX", px);
				fn.Set("PointY", py);
				fn.Set("PointZ", pz);
				fn.Set("PointW", pw);
				if (distFunc == DistanceFunction.Minkowski)
					fn.Set("MinkowskiP", minkowskiP);
				return fn;
			})
		{
			m_DistFunc = distFunc;
			m_Px = px;
			m_Py = py;
			m_Pz = pz;
			m_Pw = pw;
			m_MinkowskiP = minkowskiP;
		}

		public DistanceToPointNode WithDistanceFunction(DistanceFunction value) =>
			new(value, m_Px, m_Py, m_Pz, m_Pw, m_MinkowskiP);

		public DistanceToPointNode WithPoint(float x, float y, float z = 0f, float w = 0f) =>
			new(m_DistFunc, x, y, z, w, m_MinkowskiP);

		public DistanceToPointNode WithPointX(float value) =>
			new(m_DistFunc, value, m_Py, m_Pz, m_Pw, m_MinkowskiP);

		public DistanceToPointNode WithPointY(float value) =>
			new(m_DistFunc, m_Px, value, m_Pz, m_Pw, m_MinkowskiP);

		public DistanceToPointNode WithPointZ(float value) =>
			new(m_DistFunc, m_Px, m_Py, value, m_Pw, m_MinkowskiP);

		public DistanceToPointNode WithPointW(float value) =>
			new(m_DistFunc, m_Px, m_Py, m_Pz, value, m_MinkowskiP);

		public DistanceToPointNode WithMinkowskiP(float value) =>
			new(m_DistFunc, m_Px, m_Py, m_Pz, m_Pw, value);
	}
}
