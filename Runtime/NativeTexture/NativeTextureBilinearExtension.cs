namespace FastNoise2.NativeTexture
{
	using System.Runtime.CompilerServices;
	using Unity.Mathematics;

	public static class NativeTextureBilinearExtension
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ReadPixel(this NativeTexture2D<float> tex2D, float2 pixelCoord) =>
			tex2D[(int2)math.floor(pixelCoord)];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ReadPixelBilinear(this NativeTexture2D<float> tex2D, float2 pixelCoord)
		{
			pixelCoord *= tex2D.Resolution;
			pixelCoord = math.clamp(pixelCoord, 0, tex2D.Resolution - 1);

			// Swizzled thingy
			var pixelFloorCeil = new int4(
				(int2)math.floor(pixelCoord),
				(int2)math.ceil(pixelCoord)
			);

			var ratio = pixelCoord - pixelFloorCeil.xy;

			float f1 = tex2D[pixelFloorCeil.xy];
			float f2 = tex2D[pixelFloorCeil.xw];
			float f3 = tex2D[pixelFloorCeil.zy];
			float f4 = tex2D[pixelFloorCeil.zw];

			float f12 = f1 + ((f2 - f1) * ratio.x);
			float f34 = f3 + ((f4 - f3) * ratio.x);
			float result = f12 + ((f34 - f12) * ratio.y);

			return result;
		}
	}
}
