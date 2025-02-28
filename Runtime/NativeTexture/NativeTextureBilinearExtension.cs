namespace FastNoise2.NativeTexture
{
	using System.Runtime.CompilerServices;
	using Unity.Mathematics;

	/// <summary>
	/// Provides extension methods for reading pixel values from NativeTexture2D with bilinear interpolation support.
	/// </summary>
	public static class NativeTextureBilinearExtension
	{
		/// <summary>
		/// Reads the pixel value at the specified floating-point coordinate by flooring the coordinate to the nearest integer pixel.
		/// </summary>
		/// <param name="tex2D">The NativeTexture2D instance to read from.</param>
		/// <param name="pixelCoord">The floating-point coordinate of the pixel.</param>
		/// <returns>The pixel value at the floored coordinate.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ReadPixel(this NativeTexture2D<float> tex2D, float2 pixelCoord) =>
			tex2D[(int2)math.floor(pixelCoord)];

		/// <summary>
		/// Reads the pixel value at the specified normalized floating-point coordinate using bilinear interpolation.
		/// </summary>
		/// <param name="tex2D">The NativeTexture2D instance to read from.</param>
		/// <param name="pixelCoord">The normalized floating-point coordinate (range [0,1]) of the pixel.</param>
		/// <returns>The interpolated pixel value.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ReadPixelBilinear(this NativeTexture2D<float> tex2D, float2 pixelCoord)
		{
			// Scale normalized coordinates to texture resolution
			pixelCoord *= tex2D.Resolution;
			pixelCoord = math.clamp(pixelCoord, 0, tex2D.Resolution - 1);

			// Calculate floor and ceil coordinates for interpolation
			var pixelFloorCeil = new int4(
				(int2)math.floor(pixelCoord),
				(int2)math.ceil(pixelCoord)
			);

			// Compute interpolation ratios
			var ratio = pixelCoord - pixelFloorCeil.xy;

			// Retrieve pixel values at surrounding coordinates
			float f1 = tex2D[pixelFloorCeil.xy];
			float f2 = tex2D[pixelFloorCeil.xw];
			float f3 = tex2D[pixelFloorCeil.zy];
			float f4 = tex2D[pixelFloorCeil.zw];

			// Perform bilinear interpolation
			float f12 = f1 + ((f2 - f1) * ratio.x);
			float f34 = f3 + ((f4 - f3) * ratio.x);
			float result = f12 + ((f34 - f12) * ratio.y);

			return result;
		}
	}
}
