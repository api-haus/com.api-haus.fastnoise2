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
	/// Native Texture2D Wrapper.
	/// </summary>
	[DebuggerDisplay("Length = {RawTextureData.m_Length}")]
	public struct NativeTexture2D<T> : INativeTexture<int2, T>, INativeDisposable where T : unmanaged
	{
		#region Properties

		[field: NativeDisableContainerSafetyRestriction]
		public NativeReference<ValueBounds<T>> BoundsRef { get; }

		internal NativeArray<T> RawTextureData;

		[ReadOnly] [NativeDisableUnsafePtrRestriction]
		readonly IntPtr texturePtr;

		public readonly int Width => Resolution.x;
		public readonly int Height => Resolution.y;
		public readonly bool IsCreated => RawTextureData.IsCreated;
		public readonly bool IsUnityTexture2DPointer => texturePtr == IntPtr.Zero;

		#endregion

		#region Constructors

		/// <summary>
		/// Create NativeTexture2D From Texture2D.
		/// </summary>
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
		/// Create NativeTexture2D.
		/// </summary>
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

		public int2 Resolution { get; }
		public ValueBounds<T> Bounds => BoundsRef.Value;

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData.Length;
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => RawTextureData[index];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => RawTextureData[index] = value;
		}

		public T this[int2 coord]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this[coord.ToIndex(Width)];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this[coord.ToIndex(Width)] = value;
		}

		public T ReadPixel(int2 pixelCoord) =>
			this[pixelCoord.ToIndex(Width)];

		public T ReadPixel(int pixelIndex, out int2 coord) =>
			this[coord = pixelIndex.ToCoord(Width)];

		public NativeArray<T> AsNativeArray() => RawTextureData;

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

		public unsafe void* GetUnsafePtr() => RawTextureData.GetUnsafePtr();

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (!IsUnityTexture2DPointer && RawTextureData.IsCreated)
				RawTextureData.Dispose();
			if (BoundsRef.IsCreated)
				BoundsRef.Dispose();
		}

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
