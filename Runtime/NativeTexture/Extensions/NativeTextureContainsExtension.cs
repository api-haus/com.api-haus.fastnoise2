using FastNoise2.NativeTexture.Utilities;
using Unity.Mathematics;

namespace FastNoise2.NativeTexture.Extensions
{
	public static class NativeTextureContainsExtension
	{
		public static bool Contains<T>(this NativeTexture2D<T> t, int2 coord)
			where T : unmanaged
		{
			int i = coord.ToIndex(t.Width);
			return i >= 0 && i < t.Length;
		}
	}
}
