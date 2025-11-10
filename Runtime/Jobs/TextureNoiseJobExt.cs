using NativeTexture;

namespace FastNoise2.Jobs
{
	using Bindings;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;

	public static class TextureNoiseJobExt
	{
		public static JobHandle GenUniformGrid2D(
			this FastNoise noise,
			NativeTexture2D<float> texture,
			NativeReference<ValueBounds> boundsRef,
			int seed,
			int2 start,
			float frequency,
			JobHandle dependency = default
		)
		{
			dependency = new GenUniformGrid2DJob
			{
				seed = seed,
				noise = noise,
				offset = start,
				texture = texture,
				boundsRef = boundsRef,
				frequency = frequency,
			}.Schedule(dependency);
			return dependency;
		}

		public static JobHandle GenTileableGrid2D(
			this FastNoise noise,
			NativeTexture2D<float> texture,
			NativeReference<ValueBounds> boundsRef,
			int seed,
			float frequency,
			JobHandle dependency = default
		)
		{
			dependency = new GenTileable2DJob
			{
				seed = seed,
				noise = noise,
				texture = texture,
				boundsRef = boundsRef,
				frequency = frequency,
			}.Schedule(dependency);
			return dependency;
		}

		public static JobHandle GenUniformGrid3D(
			this FastNoise noise,
			NativeTexture3D<float> texture,
			NativeReference<ValueBounds> boundsRef,
			int seed,
			int3 start,
			float frequency,
			JobHandle dependency = default
		)
		{
			dependency = new GenUniformGrid3DJob
			{
				seed = seed,
				noise = noise,
				offset = start,
				texture = texture,
				boundsRef = boundsRef,
				frequency = frequency,
			}.Schedule(dependency);
			return dependency;
		}
	}
}
