namespace FastNoise2.Jobs
{
	using Bindings;
	using NativeTexture;
	using Unity.Jobs;
	using Unity.Mathematics;

	public static class TextureNoiseJobExt
	{
		public static JobHandle GenUniformGrid2D(
			this FastNoise noise,
			NativeTexture2D<float> texture,
			int seed, int2 start, float frequency,
			JobHandle dependency = default)
		{
			dependency = new GenUniformGrid2DJob
			{
				Seed = seed,
				Noise = noise,
				Offset = start,
				Texture = texture,
				Frequency = frequency,
			}.Schedule(dependency);
			return dependency;
		}

		public static JobHandle GenUniformGrid2DNormalized(
			this FastNoise noise,
			NativeTexture2D<float> texture,
			int seed, int2 start, float frequency,
			JobHandle dependency = default)
		{
			dependency = GenUniformGrid2D(noise, texture, seed, start, frequency, dependency);
			dependency = texture.ScheduleNormalize(dependency);

			return dependency;
		}

		public static JobHandle GenTileableGrid2D(
			this FastNoise noise,
			NativeTexture2D<float> texture,
			int seed, float frequency,
			JobHandle dependency = default)
		{
			dependency = new GenTileable2DJob
			{
				Seed = seed, Noise = noise, Texture = texture, Frequency = frequency,
			}.Schedule(dependency);
			return dependency;
		}

		public static JobHandle GenTileableGrid2DNormalized(
			this FastNoise noise,
			NativeTexture2D<float> texture,
			int seed, float frequency,
			JobHandle dependency = default)
		{
			dependency = GenTileableGrid2D(noise, texture, seed, frequency, dependency);
			dependency = texture.ScheduleNormalize(dependency);

			return dependency;
		}

		public static JobHandle GenUniformGrid3D(
			this FastNoise noise,
			NativeTexture3D<float> texture,
			int seed, int3 start, float frequency,
			JobHandle dependency = default)
		{
			dependency = new GenUniformGrid3DJob
			{
				Seed = seed,
				Noise = noise,
				Offset = start,
				Texture = texture,
				Frequency = frequency,
			}.Schedule(dependency);
			return dependency;
		}

		public static JobHandle GenUniformGrid3DNormalized(
			this FastNoise noise,
			NativeTexture3D<float> texture,
			int seed, int3 start, float frequency,
			JobHandle dependency = default)
		{
			dependency = GenUniformGrid3D(noise, texture, seed, start, frequency, dependency);
			dependency = texture.ScheduleNormalize(dependency);

			return dependency;
		}
	}
}
