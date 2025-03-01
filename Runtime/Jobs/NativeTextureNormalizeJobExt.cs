namespace FastNoise2.Jobs
{
   using NativeTexture;
   using Unity.Jobs;

   public static class NativeTextureNormalizeJobExt
   {
	  public static JobHandle ScheduleNormalize(
		  this NativeTexture2D<float> noiseDataNoiseOut,
		  JobHandle dependency)
	  {
		 dependency = PrecalculateScaleJob.Schedule(noiseDataNoiseOut.BoundsRef, dependency);
		 dependency = NormalizeTextureJob.Schedule(noiseDataNoiseOut, dependency);
		 dependency = ResetBoundsJob.Schedule(noiseDataNoiseOut.BoundsRef, dependency);

		 return dependency;
	  }

	  public static JobHandle ScheduleNormalize(
		  this NativeTexture3D<float> noiseDataNoiseOut,
		  JobHandle dependency)
	  {
		 dependency = PrecalculateScaleJob.Schedule(noiseDataNoiseOut.BoundsRef, dependency);
		 dependency = NormalizeTextureJob.Schedule(noiseDataNoiseOut, dependency);
		 dependency = ResetBoundsJob.Schedule(noiseDataNoiseOut.BoundsRef, dependency);

		 return dependency;
	  }
   }
}
