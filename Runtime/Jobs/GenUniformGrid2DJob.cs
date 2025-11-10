using NativeTexture;

namespace FastNoise2.Jobs
{
	using Bindings;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;

	[BurstCompile]
	public struct GenUniformGrid2DJob : IJob
	{
		public NativeTexture2D<float> texture;
		public FastNoise noise;
		public NativeReference<ValueBounds> boundsRef;

		public int seed;
		public int2 offset;
		public float frequency;

		public readonly void Execute() =>
			noise.GenUniformGrid2D(
				texture,
				boundsRef,
				offset.x,
				offset.y,
				texture.Width,
				texture.Height,
				frequency,
				seed
			);
	}
}
