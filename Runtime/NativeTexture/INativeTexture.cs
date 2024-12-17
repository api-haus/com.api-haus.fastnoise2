namespace FastNoise2.NativeTexture
{
	using Unity.Collections;
	using UnityEngine;

	public interface INativeTexture<TCoord, TValue> where TValue : unmanaged
	{
		TCoord Resolution { get; }

		ValueBounds<TValue> Bounds { get; }

		NativeReference<ValueBounds<TValue>> BoundsRef { get; }

		TValue this[TCoord coord] { get; set; }

		TValue this[int pixelIndex] { get; set; }

		TValue ReadPixel(TCoord pixelCoord);

		TValue ReadPixel(int index, out TCoord coord);

		bool IsCreated { get; }

		bool IsUnityTexture2DPointer { get; }

		int Length { get; }

		NativeArray<TValue> AsNativeArray();

		Texture2D ApplyTo(Texture2D texture, bool updateMipmaps = false);

		unsafe void* GetUnsafePtr();
	}
}
