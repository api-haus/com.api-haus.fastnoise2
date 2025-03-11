namespace FastNoise2.NativeTexture
{
	using System;
	using System.Diagnostics;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Mathematics;

	/// <summary>
	/// Contains unsafe methods for working with NativeTexture instances.
	/// Similar to Unity's NativeArrayUnsafeUtility.
	/// </summary>
	public static class NativeTextureUnsafeUtility
	{
		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		private static void CheckConvertArguments<T>(int2 resolution)
			where T : unmanaged
		{
			if (resolution.x < 0 || resolution.y < 0)
				throw new ArgumentOutOfRangeException(
					"resolution",
					"Resolution dimensions must be >= 0"
				);

			// Verify T is unmanaged
			if (!UnsafeUtility.IsUnmanaged<T>())
				throw new InvalidOperationException(
					$"{typeof(T)} used in NativeTexture2D<{typeof(T)}> must be unmanaged (contain no managed types)."
				);
		}

		/// <summary>
		/// Converts existing data to a NativeTexture2D instance.
		/// </summary>
		/// <typeparam name="T">The type of elements in the texture.</typeparam>
		/// <param name="dataPointer">Pointer to the existing data.</param>
		/// <param name="resolution">The resolution (width, height) of the texture.</param>
		/// <param name="allocator">The allocator that was used to create the memory pointed to, or Allocator.None if memory is not owned by this container.</param>
		/// <returns>A NativeTexture2D that references the provided data.</returns>
		public static unsafe NativeTexture2D<T> ConvertExistingDataToNativeTexture2D<T>(
			void* dataPointer,
			int2 resolution,
			Allocator allocator = Allocator.None
		)
			where T : unmanaged
		{
			CheckConvertArguments<T>(resolution);

			// Create a default struct
			NativeTexture2D<T> result = default;

			// Initialize the fields directly
			result.m_Buffer = dataPointer;
			result.Resolution = resolution;
			result.m_Length = resolution.x * resolution.y;
			result.m_AllocatorLabel = allocator;
			result.m_MinIndex = 0;
			result.m_MaxIndex = result.m_Length - 1;
			result.texturePtr = IntPtr.Zero;

			// Initialize safety handle
			result.m_Safety = AtomicSafetyHandle.Create();

			// Set static safety ID
			if (NativeTexture2D<T>.s_staticSafetyId.Data == 0)
				NativeTexture2D<T>.s_staticSafetyId.Data = AtomicSafetyHandle.NewStaticSafetyId<
					NativeTexture2D<T>
				>();
			AtomicSafetyHandle.SetStaticSafetyId(
				ref result.m_Safety,
				NativeTexture2D<T>.s_staticSafetyId.Data
			);

			return result;
		}

		/// <summary>
		/// Gets an unsafe pointer to the underlying texture data.
		/// </summary>
		/// <typeparam name="T">The type of elements in the texture.</typeparam>
		/// <param name="texture">The texture to get the pointer from.</param>
		/// <returns>An unsafe pointer to the texture data.</returns>
		public static unsafe void* GetUnsafePtr<T>(this NativeTexture2D<T> texture)
			where T : unmanaged
		{
			AtomicSafetyHandle.CheckWriteAndThrow(texture.m_Safety);
			return texture.m_Buffer;
		}

		/// <summary>
		/// Gets an unsafe read-only pointer to the underlying texture data.
		/// </summary>
		/// <typeparam name="T">The type of elements in the texture.</typeparam>
		/// <param name="texture">The texture to get the pointer from.</param>
		/// <returns>An unsafe read-only pointer to the texture data.</returns>
		public static unsafe void* GetUnsafeReadOnlyPtr<T>(this NativeTexture2D<T> texture)
			where T : unmanaged
		{
			AtomicSafetyHandle.CheckReadAndThrow(texture.m_Safety);
			return texture.m_Buffer;
		}

		/// <summary>
		/// Gets an unsafe read-only pointer to the underlying texture data.
		/// </summary>
		/// <typeparam name="T">The type of elements in the texture.</typeparam>
		/// <param name="texture">The read-only texture view to get the pointer from.</param>
		/// <returns>An unsafe read-only pointer to the texture data.</returns>
		public static unsafe void* GetUnsafeReadOnlyPtr<T>(this NativeTexture2D<T>.ReadOnly texture)
			where T : unmanaged
		{
			AtomicSafetyHandle.CheckReadAndThrow(texture.safety);
			return texture.buffer;
		}

		/// <summary>
		/// Gets an unsafe buffer pointer without safety checks.
		/// </summary>
		/// <typeparam name="T">The type of elements in the texture.</typeparam>
		/// <param name="texture">The texture to get the pointer from.</param>
		/// <returns>An unsafe pointer to the texture data without any safety checks.</returns>
		public static unsafe void* GetUnsafeBufferPointerWithoutChecks<T>(
			NativeTexture2D<T> texture
		)
			where T : unmanaged =>
			texture.m_Buffer;
	}
}
