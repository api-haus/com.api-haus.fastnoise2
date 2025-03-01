namespace FastNoise2.NativeTexture
{
   using System.Runtime.CompilerServices;
   using Unity.Collections;
   using Unity.Mathematics;

   /// <summary>
   /// This extension adds usage of NativeReference of ValueBounds&lt;float&gt; as MinMax Bounds.
   /// </summary>
   public static class NativeReferenceBoundsExtension
   {
	  /// <summary>
	  /// Called during normalization.
	  /// </summary>
	  [MethodImpl(MethodImplOptions.AggressiveInlining)]
	  public static float NormalizeValue(this NativeReference<ValueBounds<float>> bounds, float value)
	  {
		 (float min, float scale) = (bounds.Value.Min, bounds.Value.Scale);

		 return (value - min) * scale;
	  }

	  /// <summary>
	  /// Called before normalization.
	  /// </summary>
	  [MethodImpl(MethodImplOptions.AggressiveInlining)]
	  public static void PrecalculateScale(this NativeReference<ValueBounds<float>> bounds)
	  {
		 (float min, float max) = (bounds.Value.Min, bounds.Value.Max);
		 float scale = math.rcp(max - min);

		 bounds.Value = new ValueBounds<float>(min, max, scale);
	  }

	  /// <summary>
	  /// Called before noise generation.
	  /// </summary>
	  [MethodImpl(MethodImplOptions.AggressiveInlining)]
	  public static void Reset(this NativeReference<ValueBounds<float>> bounds) =>
		  bounds.Value = new(float.PositiveInfinity, float.NegativeInfinity);
   }
}
