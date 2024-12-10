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
	/// Native Texture2D Wrapper as 3D.
	/// </summary>
	[DebuggerDisplay("Length = {RawTextureData.m_Length}")]
	public struct NativeTexture3D<T> : INativeTexture<int3, T>, IDisposable where T : unmanaged
	{
		#region Properties

		[field: NativeDisableContainerSafetyRestriction]
		public NativeReference<ValueBounds<T>> BoundsRef { get; }

		internal NativeArray<T> RawTextureData;

		[ReadOnly] [NativeDisableUnsafePtrRestriction]
		readonly IntPtr texturePtr;

		public readonly int Width => Resolution.x;
		public readonly int Height => Resolution.y;
		public readonly int Depth => Resolution.z;
		public readonly int WidthXHeight;
		public readonly bool IsCreated => RawTextureData.IsCreated;
		public readonly bool IsUnityTexture2DPointer => texturePtr == IntPtr.Zero;

		#endregion

		#region Constructors

		/// <summary>
		/// Create NativeTexture2D From Texture2D.
		/// </summary>
		public NativeTexture3D(Texture2D texture, int3 resolution, Allocator boundsAllocator)
		{
			Resolution = resolution;
			WidthXHeight = texture.width * texture.height;
			texturePtr = texture.GetNativeTexturePtr();
			RawTextureData = texture.GetRawTextureData<T>();
			BoundsRef = new NativeReference<ValueBounds<T>>(
				new ValueBounds<T>(),
				boundsAllocator
			);
		}

		/// <summary>
		/// Create NativeTexture3D With Allocator.
		/// </summary>
		public NativeTexture3D(int3 resolution, Allocator allocator)
		{
			Resolution = resolution;
			WidthXHeight = resolution.x * resolution.y;
			texturePtr = IntPtr.Zero;
			RawTextureData = new NativeArray<T>(resolution.x * resolution.y * resolution.z, allocator);
			BoundsRef = new NativeReference<ValueBounds<T>>(
				new ValueBounds<T>(),
				allocator
			);
		}

		#endregion

		#region INativeTexture API

		public int3 Resolution { get; }
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

		public T this[int3 coord]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => this[coord.ToIndex(WidthXHeight, Width)];
			[WriteAccessRequired, MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => this[coord.ToIndex(WidthXHeight, Width)] = value;
		}

		public T ReadPixel(int3 pixelCoord) =>
			this[pixelCoord];

		public T ReadPixel(int index, out int3 coord) =>
			this[coord = index.ToCoord(WidthXHeight, Width)];

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
			public NativeTexture3D<T> Texture;

			public void Execute() => Texture.Dispose();
		}

		public JobHandle Dispose(JobHandle inputDeps) => new DisposeJob
		{
			Texture = this, //
		}.Schedule(inputDeps);

		#endregion
	}
}
