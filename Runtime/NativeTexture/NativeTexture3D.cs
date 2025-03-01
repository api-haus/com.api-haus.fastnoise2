namespace FastNoise2.NativeTexture
{
   using System;
   using System.Diagnostics;
   using System.Runtime.CompilerServices;
   using Unity.Burst;
   using Unity.Collections;
   using Unity.Collections.LowLevel.Unsafe;
   using Unity.Jobs;
   using Unity.Mathematics;
   using UnityEngine;

   /// <summary>
   /// A native wrapper for Unity's Texture2D that provides 3D texture functionality with efficient memory management and direct data access.
   /// Implements INativeTexture for 3D textures and IDisposable for proper resource cleanup.
   /// </summary>
   /// <typeparam name="T">The unmanaged type of the texture data (e.g., float).</typeparam>
   [DebuggerDisplay("Length = {RawTextureData.m_Length}")]
   public struct NativeTexture3D<T> : INativeTexture<int3, T>, IDisposable where T : unmanaged
   {
	  /// <summary>
	  /// Gets a reference to the bounds containing minimum, maximum, and scale values of the texture data.
	  /// This reference is used for normalization operations.
	  /// </summary>
	  public NativeReference<ValueBounds<T>> BoundsRef { get; }

	  internal NativeArray<T> rawTextureData;

	  [ReadOnly]
	  [NativeDisableUnsafePtrRestriction]
	  readonly IntPtr m_TexturePtr;

	  /// <summary>
	  /// Gets the width (X dimension) of the 3D texture in pixels.
	  /// </summary>
	  public readonly int Width => Resolution.x;

	  /// <summary>
	  /// Gets the height (Y dimension) of the 3D texture in pixels.
	  /// </summary>
	  public readonly int Height => Resolution.y;

	  /// <summary>
	  /// Gets the depth (Z dimension) of the 3D texture in pixels.
	  /// </summary>
	  public readonly int Depth => Resolution.z;

	  /// <summary>
	  /// Gets the product of width and height dimensions, used for coordinate calculations.
	  /// </summary>
	  public readonly int widthXHeight;

	  /// <summary>
	  /// Gets whether the native texture has been created and initialized.
	  /// </summary>
	  public readonly bool IsCreated => rawTextureData.IsCreated;

	  /// <summary>
	  /// Gets whether the texture data points directly to a Unity Texture2D native pointer.
	  /// Returns true if the texture is not using a Unity native pointer.
	  /// </summary>
	  public readonly bool IsUnityTexture2DPointer => m_TexturePtr == IntPtr.Zero;

	  #region Constructors

	  /// <summary>
	  /// Creates a NativeTexture3D from an existing Unity Texture2D, treating it as a 3D texture with specified resolution.
	  /// </summary>
	  /// <param name="texture">The source Unity Texture2D to wrap.</param>
	  /// <param name="resolution">The 3D dimensions (width, height, depth) of the texture.</param>
	  /// <param name="boundsAllocator">The allocator to use for the bounds data.</param>
	  public NativeTexture3D(Texture2D texture, int3 resolution, Allocator boundsAllocator)
	  {
		 Resolution = resolution;
		 widthXHeight = texture.width * texture.height;
		 m_TexturePtr = texture.GetNativeTexturePtr();
		 rawTextureData = texture.GetRawTextureData<T>();
		 BoundsRef = new NativeReference<ValueBounds<T>>(
			 new ValueBounds<T>(),
			 boundsAllocator
		 );
	  }

	  /// <summary>
	  /// Creates a new NativeTexture3D with the specified 3D resolution.
	  /// </summary>
	  /// <param name="resolution">The 3D dimensions (width, height, depth) of the texture.</param>
	  /// <param name="allocator">The allocator to use for the texture and bounds data.</param>
	  public NativeTexture3D(int3 resolution, Allocator allocator)
	  {
		 Resolution = resolution;
		 widthXHeight = resolution.x * resolution.y;
		 m_TexturePtr = IntPtr.Zero;
		 rawTextureData = new NativeArray<T>(resolution.x * resolution.y * resolution.z, allocator);
		 BoundsRef = new NativeReference<ValueBounds<T>>(
			 new ValueBounds<T>(),
			 allocator
		 );
	  }

	  #endregion

	  #region INativeTexture API

	  /// <summary>
	  /// Gets the 3D resolution (width, height, depth) of the texture.
	  /// </summary>
	  public int3 Resolution { get; }

	  /// <summary>
	  /// Gets the bounds containing minimum, maximum, and scale values of the texture data.
	  /// </summary>
	  public readonly ValueBounds<T> Bounds => BoundsRef.Value;

	  /// <summary>
	  /// Gets the total number of voxels in the 3D texture.
	  /// </summary>
	  public int Length
	  {
		 [MethodImpl(MethodImplOptions.AggressiveInlining)]
		 get => rawTextureData.Length;
	  }

	  /// <summary>
	  /// Gets or sets the texture value at the specified linear voxel index.
	  /// </summary>
	  /// <param name="index">The linear index of the voxel.</param>
	  /// <returns>The value at the specified index.</returns>
	  public T this[int index]
	  {
		 [MethodImpl(MethodImplOptions.AggressiveInlining)]
		 get => rawTextureData[index];
		 [WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
		 set => rawTextureData[index] = value;
	  }

	  /// <summary>
	  /// Gets or sets the texture value at the specified 3D coordinate.
	  /// </summary>
	  /// <param name="coord">The 3D coordinate (x, y, z) of the voxel.</param>
	  /// <returns>The value at the specified coordinate.</returns>
	  public T this[int3 coord]
	  {
		 [MethodImpl(MethodImplOptions.AggressiveInlining)]
		 get => this[coord.ToIndex(widthXHeight, Width)];
		 [WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
		 set => this[coord.ToIndex(widthXHeight, Width)] = value;
	  }

	  /// <summary>
	  /// Reads the texture value at the specified 3D coordinate.
	  /// </summary>
	  /// <param name="pixelCoord">The 3D coordinate of the voxel.</param>
	  /// <returns>The value at the specified coordinate.</returns>
	  public T ReadPixel(int3 pixelCoord) =>
		  this[pixelCoord];

	  /// <summary>
	  /// Reads the texture value at the specified linear index and outputs the corresponding 3D coordinate.
	  /// </summary>
	  /// <param name="index">The linear index of the voxel.</param>
	  /// <param name="coord">Outputs the 3D coordinate corresponding to the linear index.</param>
	  /// <returns>The value at the specified index.</returns>
	  public T ReadPixel(int index, out int3 coord) =>
		  this[coord = index.ToCoord(widthXHeight, Width)];

	  /// <summary>
	  /// Returns the underlying texture data as a NativeArray.
	  /// </summary>
	  /// <returns>A NativeArray containing the texture data.</returns>
	  public readonly NativeArray<T> AsNativeArray() => rawTextureData;

	  /// <summary>
	  /// Applies the native 3D texture data to a Unity Texture2D object.
	  /// If the texture pointers don't match, copies the data to the target texture.
	  /// </summary>
	  /// <param name="texture">The Texture2D object to apply data to.</param>
	  /// <param name="updateMipmaps">Whether to update mipmaps after applying data.</param>
	  /// <returns>The updated Texture2D object.</returns>
	  [BurstDiscard]
	  public readonly Texture2D ApplyTo(Texture2D texture, bool updateMipmaps = false)
	  {
		 if (texture.GetNativeTexturePtr() != m_TexturePtr)
		 {
			NativeArray<T> textureData = rawTextureData;
			NativeArray<T> writeableTextureMemory = texture.GetRawTextureData<T>();

			textureData.CopyTo(writeableTextureMemory);
		 }

		 texture.Apply(updateMipmaps);
		 return texture;
	  }

	  /// <summary>
	  /// Gets an unsafe pointer to the underlying texture data.
	  /// </summary>
	  /// <returns>An unsafe pointer to the texture data.</returns>
	  public readonly unsafe void* GetUnsafePtr() => rawTextureData.GetUnsafePtr();

	  #endregion

	  #region IDisposable

	  /// <summary>
	  /// Disposes of the native texture resources, including the raw texture data and bounds reference if they were created.
	  /// </summary>
	  public void Dispose()
	  {
		 if (!IsUnityTexture2DPointer && rawTextureData.IsCreated)
			rawTextureData.Dispose();
		 if (BoundsRef.IsCreated)
			BoundsRef.Dispose();
	  }

	  [BurstCompile]
	  struct DisposeJob : IJob
	  {
		 public NativeTexture3D<T> texture;

		 public void Execute() => texture.Dispose();
	  }

	  /// <summary>
	  /// Schedules the disposal of native texture resources as a job.
	  /// </summary>
	  /// <param name="inputDeps">The JobHandle for any dependent jobs.</param>
	  /// <returns>A JobHandle for the scheduled dispose job.</returns>
	  public JobHandle Dispose(JobHandle inputDeps) => new DisposeJob
	  {
		 texture = this, //
	  }.Schedule(inputDeps);

	  #endregion
   }
}
