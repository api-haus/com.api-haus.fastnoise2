namespace FastNoise2.Jobs
{
	using NativeTexture;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;

	[BurstCompile]
	public struct NormalizeTextureJob : IJobParallelFor
	{
		[NativeMatchesParallelForLength] NativeArray<float> texture;

		[ReadOnly] NativeReference<ValueBounds<float>> boundsRef;

		[BurstCompile]
		public void Execute(int i) =>
			texture[i] = boundsRef.NormalizeValue(texture[i]);

		public static JobHandle Schedule(NativeTexture2D<float> tex, JobHandle dependency = default) =>
			new NormalizeTextureJob
			{
				texture = tex.RawTextureData, //
				boundsRef = tex.BoundsRef,
			}.Schedule(tex.Length, tex.Width, dependency);

		public static JobHandle Schedule(NativeTexture3D<float> tex, JobHandle dependency = default) =>
			new NormalizeTextureJob
			{
				texture = tex.RawTextureData, //
				boundsRef = tex.BoundsRef,
			}.Schedule(tex.Length, tex.Width, dependency);
	}

	[BurstCompile]
	public struct PrecalculateScaleJob : IJob
	{
		public NativeReference<ValueBounds<float>> Bounds;

		public void Execute() => Bounds.PrecalculateScale();

		public static JobHandle Schedule(NativeReference<ValueBounds<float>> boundsRef,
			JobHandle dependency = default) =>
			new PrecalculateScaleJob { Bounds = boundsRef }.Schedule(dependency);
	}

	[BurstCompile]
	struct ResetBoundsJob : IJob
	{
		[WriteOnly] public NativeReference<ValueBounds<float>> Bounds;

		public void Execute() => Bounds.Reset();

		public static JobHandle Schedule(NativeReference<ValueBounds<float>> boundsRef,
			JobHandle dependency = default) =>
			new ResetBoundsJob { Bounds = boundsRef }.Schedule(dependency);
	}
}
