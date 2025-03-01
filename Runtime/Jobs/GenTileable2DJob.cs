namespace FastNoise2.Jobs
{
	using Bindings;
	using NativeTexture;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;

	[BurstCompile]
	public struct GenTileable2DJob : IJob
	{
		public NativeTexture2D<float> texture;
		public FastNoise noise;
		public NativeReference<ValueBounds> boundsRef;

		public int seed;
		public float frequency;

		public readonly void Execute() =>
			noise.GenTileable2D(
				texture,
				boundsRef,
				texture.Width, texture.Height,
				frequency, seed);
	}
}
