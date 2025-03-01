namespace FastNoise2.NativeTexture
{
   using Unity.Collections;
   using UnityEngine;

   /// <summary>
   /// Represents a native texture interface for handling texture data efficiently using Unity's native collections.
   /// </summary>
   /// <typeparam name="TCoord">The coordinate type used for indexing texture data (e.g., int2, int3).</typeparam>
   /// <typeparam name="TValue">The unmanaged value type stored in the texture (e.g., float).</typeparam>
   public interface INativeTexture<TCoord, TValue> where TValue : unmanaged
   {
	  /// <summary>
	  /// Gets the resolution of the texture.
	  /// </summary>
	  TCoord Resolution { get; }

	  /// <summary>
	  /// Gets the bounds containing minimum, maximum, and scale values of the texture data.
	  /// </summary>
	  ValueBounds<TValue> Bounds { get; }

	  /// <summary>
	  /// Gets a reference to the bounds used for normalization and other operations.
	  /// </summary>
	  NativeReference<ValueBounds<TValue>> BoundsRef { get; }

	  /// <summary>
	  /// Gets or sets the texture value at the specified coordinate.
	  /// </summary>
	  /// <param name="coord">The coordinate of the pixel.</param>
	  /// <returns>The value at the specified coordinate.</returns>
	  TValue this[TCoord coord] { get; set; }

	  /// <summary>
	  /// Gets or sets the texture value at the specified linear pixel index.
	  /// </summary>
	  /// <param name="pixelIndex">The linear index of the pixel.</param>
	  /// <returns>The value at the specified index.</returns>
	  TValue this[int pixelIndex] { get; set; }

	  /// <summary>
	  /// Reads the texture value at the specified coordinate.
	  /// </summary>
	  /// <param name="pixelCoord">The coordinate of the pixel.</param>
	  /// <returns>The value at the specified coordinate.</returns>
	  TValue ReadPixel(TCoord pixelCoord);

	  /// <summary>
	  /// Reads the texture value at the specified linear index and outputs the corresponding coordinate.
	  /// </summary>
	  /// <param name="index">The linear index of the pixel.</param>
	  /// <param name="coord">Outputs the coordinate corresponding to the linear index.</param>
	  /// <returns>The value at the specified index.</returns>
	  TValue ReadPixel(int index, out TCoord coord);

	  /// <summary>
	  /// Indicates whether the native texture has been created and initialized.
	  /// </summary>
	  bool IsCreated { get; }

	  /// <summary>
	  /// Indicates whether the texture data points directly to a Unity Texture2D native pointer.
	  /// </summary>
	  bool IsUnityTexture2DPointer { get; }

	  /// <summary>
	  /// Gets the total number of elements in the texture.
	  /// </summary>
	  int Length { get; }

	  /// <summary>
	  /// Returns the underlying texture data as a NativeArray.
	  /// </summary>
	  /// <returns>A NativeArray containing the texture data.</returns>
	  NativeArray<TValue> AsNativeArray();

	  /// <summary>
	  /// Applies the native texture data to a Unity Texture2D object.
	  /// </summary>
	  /// <param name="texture">The Texture2D object to apply data to.</param>
	  /// <param name="updateMipmaps">Whether to update mipmaps after applying data.</param>
	  /// <returns>The updated Texture2D object.</returns>
	  Texture2D ApplyTo(Texture2D texture, bool updateMipmaps = false);

	  /// <summary>
	  /// Gets an unsafe pointer to the underlying texture data.
	  /// </summary>
	  /// <returns>An unsafe pointer to the texture data.</returns>
	  unsafe void* GetUnsafePtr();
   }
}
