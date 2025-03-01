using Unity.Collections;
using Unity.Jobs;

namespace FastNoise2.Jobs
{
	using NativeTexture;

	/// <summary>
	/// Extension methods for normalizing textures via jobs.
	/// </summary>
	public static class NativeTextureNormalizeJobExt
	{
		/// <summary>
		/// Creates a bounds reference for texture normalization.
		/// </summary>
		public static NativeReference<ValueBounds> CreateBoundsReference(Allocator allocator) =>
			new NativeReference<ValueBounds>(allocator);

		/// <summary>
		/// Schedules texture normalization as a job.
		/// </summary>
		public static JobHandle ScheduleNormalize(
			this NativeTexture2D<float> noiseDataNoiseOut,
			NativeReference<ValueBounds> boundsRef,
			JobHandle dependency = default
		)
		{
			dependency = PrecalculateScaleJob.Schedule(boundsRef, dependency);
			return NormalizeTextureJob.Schedule(noiseDataNoiseOut, boundsRef, dependency);
		}

		/// <summary>
		/// Schedules texture normalization as a job.
		/// </summary>
		public static JobHandle ScheduleNormalize(
			this NativeTexture3D<float> noiseDataNoiseOut,
			NativeReference<ValueBounds> boundsRef,
			JobHandle dependency = default
		)
		{
			dependency = PrecalculateScaleJob.Schedule(boundsRef, dependency);
			return NormalizeTextureJob.Schedule(noiseDataNoiseOut, boundsRef, dependency);
		}
	}
}
