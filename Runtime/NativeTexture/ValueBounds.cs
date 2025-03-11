namespace FastNoise2.NativeTexture
{
	using System;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Mathematics;

	/// <summary>
	/// Structure that tracks the minimum and maximum values of a float data set.
	/// Used for normalizing values between 0 and 1.
	/// </summary>
	public struct ValueBounds
	{
		public float Min;
		public float Max;
		public float NormalizationScale;
		public float NormalizationOffset;

		/// <summary>
		/// Creates a new ValueBounds structure with the specified min and max values.
		/// </summary>
		public ValueBounds(float min, float max)
		{
			Min = min;
			Max = max;
			NormalizationScale = 0f;
			NormalizationOffset = 0f;
		}

		/// <summary>
		/// Resets the bounds to their extreme values (for capturing new min/max).
		/// </summary>
		public void Reset()
		{
			Min = float.MaxValue;
			Max = float.MinValue;
			NormalizationScale = 0f;
			NormalizationOffset = 0f;
		}

		/// <summary>
		/// Updates the bounds if the provided value exceeds current min or max.
		/// </summary>
		public void UpdateBounds(float value)
		{
			if (value < Min)
				Min = value;

			if (value > Max)
				Max = value;
		}

		/// <summary>
		/// Precalculates the scale and offset for normalization.
		/// </summary>
		public void PrecalculateScale()
		{
			float range = Max - Min;
			NormalizationScale = range != 0f ? 1f / range : 0f;
			NormalizationOffset = Min;
		}

		/// <summary>
		/// Normalizes a value to the range [0, 1] based on the calculated bounds.
		/// </summary>
		public float NormalizeValue(float value) => (value - NormalizationOffset) * NormalizationScale;
	}

	/// <summary>
	/// Extension methods for working with ValueBounds in NativeReferences.
	/// </summary>
	public static class ValueBoundsExtensions
	{
		/// <summary>
		/// Resets the bounds in a NativeReference.
		/// </summary>
		public static void Reset(this NativeReference<ValueBounds> boundsRef)
		{
			ValueBounds bounds = boundsRef.Value;
			bounds.Reset();
			boundsRef.Value = bounds;
		}

		/// <summary>
		/// Updates bounds in a NativeReference with a new value.
		/// </summary>
		public static void UpdateBounds(this NativeReference<ValueBounds> boundsRef, float value)
		{
			ValueBounds bounds = boundsRef.Value;
			bounds.UpdateBounds(value);
			boundsRef.Value = bounds;
		}

		/// <summary>
		/// Precalculates normalization scale and offset for a bounds reference.
		/// </summary>
		public static void PrecalculateScale(this NativeReference<ValueBounds> boundsRef)
		{
			ValueBounds bounds = boundsRef.Value;
			bounds.PrecalculateScale();
			boundsRef.Value = bounds;
		}

		/// <summary>
		/// Normalizes a value using the bounds from a NativeReference.
		/// </summary>
		public static float NormalizeValue(this NativeReference<ValueBounds> boundsRef, float value) =>
			boundsRef.Value.NormalizeValue(value);
	}
}
