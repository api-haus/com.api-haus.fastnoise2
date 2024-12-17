namespace FastNoise2.Jobs
{
	using Bindings;
	using NativeTexture;
	using Unity.Burst;
	using Unity.Jobs;

	[BurstCompile]
	public struct GenTileable2DJob : IJob
	{
		public NativeTexture2D<float> Texture;
		public FastNoise Noise;

		public int Seed;
		public float Frequency;

		public void Execute() =>
			Noise.GenTileable2D(
				Texture,
				Texture.Width, Texture.Height,
				Frequency, Seed);
	}
}
