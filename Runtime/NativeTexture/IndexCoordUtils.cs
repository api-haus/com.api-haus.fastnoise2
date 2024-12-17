namespace FastNoise2.NativeTexture
{
	using System.Runtime.CompilerServices;
	using Unity.Mathematics;

	public static class IndexCoordUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int3 ToCoord(this int pixelIndex, int widthXHeight, int width)
		{
			int z = pixelIndex / widthXHeight;
			int remainderAfterZ = pixelIndex % widthXHeight;
			int y = remainderAfterZ / width;
			int x = remainderAfterZ % width;

			return new int3(
				x, y, z
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int2 ToCoord(this int pixelIndex, int width)
		{
			int y = pixelIndex / width;
			int x = pixelIndex % width;

			return new int2(
				x, y
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToIndex(this int3 id, int widthXHeight, int width) =>
			(id.z * widthXHeight) + (id.y * width) + id.x;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToIndex(this int2 id, int width) =>
			(id.y * width) + id.x;
	}
}
