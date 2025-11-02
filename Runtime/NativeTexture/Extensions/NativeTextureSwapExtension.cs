using Unity.Mathematics;

namespace FastNoise2.NativeTexture.Extensions
{
	public static class NativeTextureSwapExtension
	{
		public static void Swap<T>(this NativeTexture2D<T> t, int i1, int i2)
			where T : unmanaged => (t[i1], t[i2]) = (t[i2], t[i1]);

		public static void Swap<T>(this NativeTexture2D<T> t, int2 i1, int2 i2)
			where T : unmanaged => (t[i1], t[i2]) = (t[i2], t[i1]);
	}
}
