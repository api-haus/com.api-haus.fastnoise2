namespace FastNoise2.Jobs
{
	using Bindings;
	using NativeTexture;
	using Unity.Burst;
	using Unity.Jobs;
	using Unity.Mathematics;

	[BurstCompile]
	public struct GenUniformGrid3DJob : IJob
	{
		public NativeTexture3D<float> Texture;
		public FastNoise Noise;

		public int Seed;
		public int3 Offset;
		public float Frequency;

		public void Execute() =>
			Noise.GenUniformGrid3D(
				Texture,
				Offset.x, Offset.y, Offset.z,
				Texture.Width, Texture.Height, Texture.Depth,
				Frequency, Seed);
	}
}
