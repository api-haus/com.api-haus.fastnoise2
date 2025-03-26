namespace FastNoise2.Jobs
{
	using NativeTexture;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;

	[BurstCompile]
	public struct NormalizeTextureJob : IJobParallelFor
	{
		[NativeMatchesParallelForLength]
		private NativeArray<float> m_Texture;

		[ReadOnly]
		private NativeReference<ValueBounds> m_BoundsRef;

		[BurstCompile]
		public void Execute(int i) => m_Texture[i] = m_BoundsRef.NormalizeValue(m_Texture[i]);

		public static JobHandle Schedule(
			NativeTexture2D<float> tex,
			NativeReference<ValueBounds> boundsRef,
			JobHandle dependency = default
		) =>
			new NormalizeTextureJob
			{
				m_Texture = tex.AsDeferredJobArray(),
				m_BoundsRef = boundsRef,
			}.Schedule(tex.Length, 64, dependency);

		public static JobHandle Schedule(
			NativeTexture3D<float> tex,
			NativeReference<ValueBounds> boundsRef,
			JobHandle dependency = default
		) =>
			new NormalizeTextureJob
			{
				m_Texture = tex.AsDeferredJobArray(),
				m_BoundsRef = boundsRef,
			}.Schedule(tex.Length, 64, dependency);
	}

	[BurstCompile]
	public struct PrecalculateScaleJob : IJob
	{
		public NativeReference<ValueBounds> bounds;

		public readonly void Execute() => bounds.PrecalculateScale();

		public static JobHandle Schedule(
			NativeReference<ValueBounds> boundsRef,
			JobHandle dependency = default
		) => new PrecalculateScaleJob { bounds = boundsRef }.Schedule(dependency);
	}
}
