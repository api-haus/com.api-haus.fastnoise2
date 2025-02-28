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
	/// A native wrapper for Unity's Texture2D that provides efficient memory management and direct texture data access.
	/// Implements INativeTexture for 2D textures and INativeDisposable for proper resource cleanup.
	/// </summary>
	/// <typeparam name="T">The unmanaged type of the texture data (e.g., float).</typeparam>
	[DebuggerDisplay("Length = {RawTextureData.m_Length}")]
	public struct NativeTexture2D<T> : INativeTexture<int2, T>, INativeDisposable where T : unmanaged
	{
		/// <summary>
		/// Gets a reference to the bounds containing minimum, maximum, and scale values of the texture data.
		/// This reference is used for normalization operations.
		/// </summary>
		[field: NativeDisableContainerSafetyRestriction]
		public NativeReference<ValueBounds<T>> BoundsRef { get; }

		internal NativeArray<T> RawTextureData;

		[ReadOnly] [NativeDisableUnsafePtrRestriction]
		readonly IntPtr texturePtr;

		/// <summary>
		/// Gets the width of the texture in pixels.
		/// </summary>
		public readonly int Width => Resolution.x;

		/// <summary>
		/// Gets the height of the texture in pixels.
		/// </summary>
		public readonly int Height => Resolution.y;

		/// <summary>
		/// Gets whether the native texture has been created and initialized.
		/// </summary>
		public readonly bool IsCreated => RawTextureData.IsCreated;

		/// <summary>
		/// Gets whether the texture data points directly to a Unity Texture2D native pointer.
		/// Returns true if the texture is not using a Unity native pointer.
		/// </summary>
		public readonly bool IsUnityTexture2DPointer => texturePtr == IntPtr.Zero;

		#region Constructors

		/// <summary>
		/// Creates a NativeTexture2D from an existing Unity Texture2D.
		/// </summary>
		/// <param name="texture">The source Unity Texture2D to wrap.</param>
		/// <param name="boundsAllocator">The allocator to use for the bounds data.</param>
		public NativeTexture2D(Texture2D texture, Allocator boundsAllocator)
		{
			Resolution = new int2(texture.width, texture.height);
			texturePtr = texture.GetNativeTexturePtr();
			RawTextureData = texture.GetRawTextureData<T>();
			BoundsRef = new NativeReference<ValueBounds<T>>(
				new ValueBounds<T>(),
				boundsAllocator
			);
		}

		/// <summary>
		/// Creates a new NativeTexture2D with the specified resolution.
		/// </summary>
		/// <param name="resolution">The dimensions of the texture (width, height).</param>
		/// <param name="allocator">The allocator to use for the texture and bounds data.</param>
		public NativeTexture2D(int2 resolution, Allocator allocator)
		{
			Resolution = resolution;
			texturePtr = IntPtr.Zero;
			RawTextureData = new NativeArray<T>(resolution.x * resolution.y, allocator);
			BoundsRef = new NativeReference<ValueBounds<T>>(
				new ValueBounds<T>(),
				allocator
			);
		}

		#endregion

		#region INativeTexture API

		/// <summary>
		/// Gets the resolution (width, height) of the texture.
		/// </summary>
		public int2 Resolution { get; }

		/// <summary>
		/// Gets the bounds containing minimum, maximum, and scale values of the texture data.
		/// </summary>
		public ValueBounds<T> Bounds => BoundsRef.Value;

		/// <summary>
		/// Gets the total number of pixels in the texture.
		/// </summary>
		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData.Length;
		}

		/// <summary>
		/// Gets or sets the texture value at the specified linear pixel index.
		/// </summary>
		/// <param name="index">The linear index of the pixel.</param>
		/// <returns>The value at the specified index.</returns>
		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData[index];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => RawTextureData[index] = value;
		}

		/// <summary>
		/// Gets or sets the texture value at the specified 2D coordinate.
		/// </summary>
		/// <param name="coord">The 2D coordinate (x, y) of the pixel.</param>
		/// <returns>The value at the specified coordinate.</returns>
		public T this[int2 coord]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this[coord.ToIndex(Width)];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this[coord.ToIndex(Width)] = value;
		}

		/// <summary>
		/// Reads the texture value at the specified 2D coordinate.
		/// </summary>
		/// <param name="pixelCoord">The 2D coordinate of the pixel.</param>
		/// <returns>The value at the specified coordinate.</returns>
		public T ReadPixel(int2 pixelCoord) =>
			this[pixelCoord.ToIndex(Width)];

		/// <summary>
		/// Reads the texture value at the specified linear index and outputs the corresponding 2D coordinate.
		/// </summary>
		/// <param name="pixelIndex">The linear index of the pixel.</param>
		/// <param name="coord">Outputs the 2D coordinate corresponding to the linear index.</param>
		/// <returns>The value at the specified index.</returns>
		public T ReadPixel(int pixelIndex, out int2 coord) =>
			this[coord = pixelIndex.ToCoord(Width)];

		/// <summary>
		/// Returns the underlying texture data as a NativeArray.
		/// </summary>
		/// <returns>A NativeArray containing the texture data.</returns>
		public NativeArray<T> AsNativeArray() => RawTextureData;

		/// <summary>
		/// Applies the native texture data to a Unity Texture2D object.
		/// If the texture pointers don't match, copies the data to the target texture.
		/// </summary>
		/// <param name="texture">The Texture2D object to apply data to.</param>
		/// <param name="updateMipmaps">Whether to update mipmaps after applying data.</param>
		/// <returns>The updated Texture2D object.</returns>
		[BurstDiscard]
		public Texture2D ApplyTo(Texture2D texture, bool updateMipmaps = false)
		{
			if (texture.GetNativeTexturePtr() != texturePtr)
			{
				var textureData = RawTextureData;
				var writeableTextureMemory = texture.GetRawTextureData<T>();

				textureData.CopyTo(writeableTextureMemory);
			}

			texture.Apply(updateMipmaps);
			return texture;
		}

		/// <summary>
		/// Gets an unsafe pointer to the underlying texture data.
		/// </summary>
		/// <returns>An unsafe pointer to the texture data.</returns>
		public unsafe void* GetUnsafePtr() => RawTextureData.GetUnsafePtr();

		#endregion

		#region IDisposable

		/// <summary>
		/// Disposes of the native texture resources, including the raw texture data and bounds reference if they were created.
		/// </summary>
		public void Dispose()
		{
			if (!IsUnityTexture2DPointer && RawTextureData.IsCreated)
				RawTextureData.Dispose();
			if (BoundsRef.IsCreated)
				BoundsRef.Dispose();
		}

		/// <summary>
		/// Schedules the disposal of native texture resources as a job.
		/// </summary>
		/// <param name="inputDeps">The JobHandle for any dependent jobs.</param>
		/// <returns>A JobHandle for the scheduled dispose job.</returns>
		[BurstCompile]
		struct DisposeJob : IJob
		{
			public NativeTexture2D<T> Texture;

			public void Execute() => Texture.Dispose();
		}

		public JobHandle Dispose(JobHandle inputDeps) => new DisposeJob
		{
			Texture = this, //
		}.Schedule(inputDeps);

		#endregion
	}
}
