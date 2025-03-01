namespace FastNoise2.Jobs
{
	using Bindings;
	using NativeTexture;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;

	[BurstCompile]
	public struct GenUniformGrid3DJob : IJob
	{
		public NativeTexture3D<float> texture;
		public FastNoise noise;
		public NativeReference<ValueBounds> boundsRef;

		public int seed;
		public int3 offset;
		public float frequency;

		public readonly void Execute() =>
			noise.GenUniformGrid3D(
				texture,
				boundsRef,
				offset.x,
				offset.y,
				offset.z,
				texture.Width,
				texture.Height,
				texture.Depth,
				frequency,
				seed
			);
	}
}
