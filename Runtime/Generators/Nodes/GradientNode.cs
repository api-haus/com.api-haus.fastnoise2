using FastNoise2.Bindings;

namespace FastNoise2.Generators
{
	/// <summary>
	/// Builder for Gradient (position output) noise with per-axis multipliers and offsets.
	/// Immutable: each <c>With*</c> method returns a new instance.
	/// </summary>
	public class GradientNode : NoiseNode
	{
		readonly float m_Mx, m_My, m_Mz, m_Mw;
		readonly float m_Ox, m_Oy, m_Oz, m_Ow;

		internal GradientNode(float mx, float my, float mz, float mw,
			float ox, float oy, float oz, float ow)
			: base(() =>
			{
				FastNoise fn = new("Gradient");
				fn.Set("MultiplierX", mx);
				fn.Set("MultiplierY", my);
				fn.Set("MultiplierZ", mz);
				fn.Set("MultiplierW", mw);
				fn.Set("OffsetX", ox);
				fn.Set("OffsetY", oy);
				fn.Set("OffsetZ", oz);
				fn.Set("OffsetW", ow);
				return fn;
			})
		{
			m_Mx = mx;
			m_My = my;
			m_Mz = mz;
			m_Mw = mw;
			m_Ox = ox;
			m_Oy = oy;
			m_Oz = oz;
			m_Ow = ow;
		}

		public GradientNode WithMultipliers(float x, float y, float z = 0f, float w = 0f) =>
			new(x, y, z, w, m_Ox, m_Oy, m_Oz, m_Ow);

		public GradientNode WithMultiplierX(float value) =>
			new(value, m_My, m_Mz, m_Mw, m_Ox, m_Oy, m_Oz, m_Ow);

		public GradientNode WithMultiplierY(float value) =>
			new(m_Mx, value, m_Mz, m_Mw, m_Ox, m_Oy, m_Oz, m_Ow);

		public GradientNode WithMultiplierZ(float value) =>
			new(m_Mx, m_My, value, m_Mw, m_Ox, m_Oy, m_Oz, m_Ow);

		public GradientNode WithMultiplierW(float value) =>
			new(m_Mx, m_My, m_Mz, value, m_Ox, m_Oy, m_Oz, m_Ow);

		public GradientNode WithOffsets(float x, float y, float z = 0f, float w = 0f) =>
			new(m_Mx, m_My, m_Mz, m_Mw, x, y, z, w);

		public GradientNode WithOffsetX(float value) =>
			new(m_Mx, m_My, m_Mz, m_Mw, value, m_Oy, m_Oz, m_Ow);

		public GradientNode WithOffsetY(float value) =>
			new(m_Mx, m_My, m_Mz, m_Mw, m_Ox, value, m_Oz, m_Ow);

		public GradientNode WithOffsetZ(float value) =>
			new(m_Mx, m_My, m_Mz, m_Mw, m_Ox, m_Oy, value, m_Ow);

		public GradientNode WithOffsetW(float value) =>
			new(m_Mx, m_My, m_Mz, m_Mw, m_Ox, m_Oy, m_Oz, value);
	}
}
