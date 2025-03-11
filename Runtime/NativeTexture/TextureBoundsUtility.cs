namespace FastNoise2.NativeTexture
{
	using System;
	using Bindings;
	using Jobs;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;

	/// <summary>
	/// Utility methods for working with texture value bounds and normalization.
	/// </summary>
	public static class TextureBoundsUtility
	{
		/// <summary>
		/// Creates a new value bounds reference initialized to extreme values for tracking.
		/// </summary>
		/// <param name="allocator">The allocator to use for the reference.</param>
		/// <returns>A NativeReference containing initialized value bounds.</returns>
		public static NativeReference<ValueBounds> CreateBoundsReference(Allocator allocator)
		{
			NativeReference<ValueBounds> boundsRef = new(allocator);
			boundsRef.Reset();
			return boundsRef;
		}

		/// <summary>
		/// Normalizes a texture based on its value bounds.
		/// </summary>
		/// <param name="texture">The texture to normalize.</param>
		/// <param name="boundsRef">The value bounds to use for normalization.</param>
		/// <param name="dependency">Any job dependencies.</param>
		/// <returns>A JobHandle for the scheduled normalization job.</returns>
		public static JobHandle NormalizeTexture(
			NativeTexture2D<float> texture,
			NativeReference<ValueBounds> boundsRef,
			JobHandle dependency = default
		) =>
			texture.ScheduleNormalize(boundsRef, dependency);

		/// <summary>
		/// Sets explicit bounds values.
		/// </summary>
		/// <param name="boundsRef">The bounds reference to modify.</param>
		/// <param name="min">The minimum value to set.</param>
		/// <param name="max">The maximum value to set.</param>
		public static void SetExplicitBounds(
			NativeReference<ValueBounds> boundsRef,
			float min,
			float max
		)
		{
			ValueBounds bounds = boundsRef.Value;
			bounds.Min = min;
			bounds.Max = max;
			boundsRef.Value = bounds;
		}

		/// <summary>
		/// Generates normalized noise directly into a texture.
		/// </summary>
		/// <param name="noise">The FastNoise generator to use.</param>
		/// <param name="texture">The texture to fill.</param>
		/// <param name="boundsRef">The bounds reference to use.</param>
		/// <param name="frequency">The noise frequency.</param>
		/// <param name="seed">The noise seed.</param>
		/// <param name="dependency">Any job dependencies.</param>
		/// <returns>A JobHandle for the scheduled jobs.</returns>
		public static JobHandle GenerateNormalizedNoise(
			FastNoise noise,
			NativeTexture2D<float> texture,
			NativeReference<ValueBounds> boundsRef,
			float frequency,
			int seed,
			JobHandle dependency = default
		)
		{
			// Complete the dependency since we need to use the CPU for noise generation
			dependency.Complete();

			// Generate noise directly into the texture using FastNoise's built-in bounds tracking
			// (The bounds are automatically reset inside this method)
			noise.GenUniformGrid2D(
				texture,
				boundsRef,
				0,
				0,
				texture.Width,
				texture.Height,
				frequency,
				seed
			);

			// Schedule the normalization
			return NormalizeTexture(texture, boundsRef, dependency);
		}
	}
}
