using System;
using System.IO;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace FastNoise2.Tests
{
	using Bindings;
	using NativeTexture;

	public class NativeTexture3DTests
	{
		[Test]
		public void CreateNativeTexture3D_FromTexture2D()
		{
			// Create a Texture2D that we'll treat as a 3D texture
			int size = 32;
			int depth = 16;
			int3 resolution = new(size, size, depth);
			Texture2D texture = new(size, size * depth, TextureFormat.RFloat, false);

			// Create the NativeTexture3D wrapper
			using NativeTexture3D<float> texture3D = new(
				texture,
				resolution
			);

			// Verify the texture dimensions
			Assert.AreEqual(size, texture3D.Width);
			Assert.AreEqual(size, texture3D.Height);
			Assert.AreEqual(depth, texture3D.Depth);
			Assert.AreEqual(size * size * depth, texture3D.Length);
			Assert.AreEqual(size * size, texture3D.widthXHeight);
			Assert.IsTrue(texture3D.IsCreated);
			Assert.IsTrue(texture3D.IsUnityTexture2DPointer);

			// Clean up
			UnityEngine.Object.DestroyImmediate(texture);
		}

		[Test]
		public void CreateNativeTexture3D_WithAllocator()
		{
			// Create a standalone NativeTexture3D
			int3 resolution = new(32, 32, 16);
			using NativeTexture3D<float> texture3D = new(
				resolution,
				Allocator.TempJob
			);

			// Verify the texture dimensions
			Assert.AreEqual(32, texture3D.Width);
			Assert.AreEqual(32, texture3D.Height);
			Assert.AreEqual(16, texture3D.Depth);
			Assert.AreEqual(32 * 32 * 16, texture3D.Length);
			Assert.AreEqual(32 * 32, texture3D.widthXHeight);
			Assert.IsTrue(texture3D.IsCreated);
			Assert.IsFalse(texture3D.IsUnityTexture2DPointer);
		}

		[Test]
		public void NativeTexture3D_DataAccessTests()
		{
			int3 resolution = new(4, 4, 4);
			NativeTexture3D<float> texture3D = new(
				resolution,
				Allocator.TempJob
			);

			try
			{
				// Fill with test values
				for (int z = 0; z < resolution.z; z++)
				for (int y = 0; y < resolution.y; y++)
				for (int x = 0; x < resolution.x; x++)
				{
					// Create a unique value for each position: x + y*width + z*width*height
					float value = x + (y * resolution.x) + (z * resolution.x * resolution.y);
					texture3D[new int3(x, y, z)] = value;
				}

				// Verify values with 3D coordinates
				for (int z = 0; z < resolution.z; z++)
				for (int y = 0; y < resolution.y; y++)
				for (int x = 0; x < resolution.x; x++)
				{
					float expectedValue =
						x + (y * resolution.x) + (z * resolution.x * resolution.y);
					float actualValue = texture3D[new int3(x, y, z)];
					Assert.AreEqual(expectedValue, actualValue, 0.0001f);

					// Also test ReadPixel method
					actualValue = texture3D.ReadPixel(new int3(x, y, z));
					Assert.AreEqual(expectedValue, actualValue, 0.0001f);
				}

				// Verify values with linear indices
				for (int i = 0; i < texture3D.Length; i++)
				{
					int x = i % resolution.x;
					int y = i / resolution.x % resolution.y;
					int z = i / (resolution.x * resolution.y);

					float expectedValue = x + (y * resolution.x) + (z * resolution.x * resolution.y);
					float actualValue = texture3D[i];
					Assert.AreEqual(expectedValue, actualValue, 0.0001f);

					// Test linear index ReadPixel with coordinate output
					actualValue = texture3D.ReadPixel(i, out int3 coord);
					Assert.AreEqual(expectedValue, actualValue, 0.0001f);
					Assert.AreEqual(new int3(x, y, z), coord);
				}
			}
			finally
			{
				// Ensure cleanup
				texture3D.Dispose();
			}
		}

		[Test]
		public void NativeTexture3D_ApplyToTexture2D()
		{
			// Create a 3D texture
			int3 resolution = new(16, 16, 8);
			NativeTexture3D<float> texture3D = new(
				resolution,
				Allocator.TempJob
			);
			Texture2D texture = null;

			try
			{
				// Fill with gradient values
				for (int z = 0; z < resolution.z; z++)
				for (int y = 0; y < resolution.y; y++)
				for (int x = 0; x < resolution.x; x++)
				{
					// Create a normalized gradient [0-1]
					float value =
						((float)x / resolution.x)
						+ ((float)y / resolution.y)
						+ ((float)z / resolution.z);
					value /= 3.0f; // Normalize to [0-1]
					texture3D[new int3(x, y, z)] = value;
				}

				// Create a Texture2D to apply the data to
				texture = new Texture2D(
					resolution.x,
					resolution.y * resolution.z,
					TextureFormat.RFloat,
					false
				);

				// Apply the 3D texture data to the 2D texture
				texture3D.ApplyTo(texture);

				// Verify by sampling directly from the texture
				// This would be more of an integration test in a real environment
				// For unit testing, we're just checking that ApplyTo doesn't throw exceptions
				Assert.DoesNotThrow(() => texture3D.ApplyTo(texture));
			}
			finally
			{
				// Clean up
				texture3D.Dispose();
				if (texture != null)
					UnityEngine.Object.DestroyImmediate(texture);
			}
		}

		[Test]
		public void NativeTexture3D_AsArray()
		{
			int3 resolution = new(4, 4, 4);
			NativeTexture3D<float> texture3D = new(
				resolution,
				Allocator.TempJob
			);

			try
			{
				// Fill with test values
				for (int i = 0; i < texture3D.Length; i++) texture3D[i] = i;

				// Get array view
				NativeArray<float> array = texture3D.AsArray();

				// Verify values match
				Assert.AreEqual(texture3D.Length, array.Length);
				for (int i = 0; i < texture3D.Length; i++)
				{
					Assert.AreEqual(i, array[i]);

					// Test that it's truly a view by modifying the array
					array[i] = i * 2;
				}

				// Verify changes are reflected in the texture
				for (int i = 0; i < texture3D.Length; i++) Assert.AreEqual(i * 2, texture3D[i]);
			}
			finally
			{
				// Ensure cleanup
				texture3D.Dispose();
			}
		}

		[Test]
		public void NativeTexture3D_ReadOnlyView()
		{
			int3 resolution = new(4, 4, 4);
			NativeTexture3D<float> texture3D = new(
				resolution,
				Allocator.TempJob
			);

			try
			{
				// Fill with test values
				for (int i = 0; i < texture3D.Length; i++) texture3D[i] = i;

				// Create read-only view
				NativeTexture3D<float>.ReadOnly readOnlyView = texture3D.AsReadOnly();

				// Verify read access works
				for (int i = 0; i < readOnlyView.Length; i++) Assert.AreEqual(i, readOnlyView[i]);

				// Verify write access is prevented (should throw exception)
				Assert.Throws<NotSupportedException>(() => readOnlyView[0] = 100);
			}
			finally
			{
				// Ensure cleanup
				texture3D.Dispose();
			}
		}

		// Job system integration test
		private struct NativeTexture3DJobTest : IJobParallelFor
		{
			public NativeTexture3D<float> texture;

			public void Execute(int index) =>
				// Modify each voxel based on its index
				texture[index] = index * 2;
		}

		[Test]
		public void NativeTexture3D_JobSystemIntegration()
		{
			int3 resolution = new(16, 16, 16);
			NativeTexture3D<float> texture3D = new(
				resolution,
				Allocator.TempJob
			);

			try
			{
				// Schedule a job to fill the texture
				NativeTexture3DJobTest job = new()
					{ texture = texture3D };

				JobHandle handle = job.Schedule(texture3D.Length, 32);
				handle.Complete();

				// Verify job results
				for (int i = 0; i < texture3D.Length; i++) Assert.AreEqual(i * 2, texture3D[i]);
			}
			finally
			{
				// Ensure resource cleanup
				texture3D.Dispose();
			}
		}

		// Test job that uses AsDeferredJobArray
		private struct NativeTexture3DDeferredJobTest : IJobParallelFor
		{
			[ReadOnly]
			public NativeArray<float> input;

			public NativeArray<float> output;

			public void Execute(int index) => output[index] = input[index] * 3;
		}

		[Test]
		public void NativeTexture3D_AsDeferredJobArray()
		{
			int3 resolution = new(8, 8, 8);
			NativeTexture3D<float> inputTexture = new(
				resolution,
				Allocator.TempJob
			);
			NativeTexture3D<float> outputTexture = new(
				resolution,
				Allocator.TempJob
			);

			try
			{
				// Initialize input texture
				for (int i = 0; i < inputTexture.Length; i++) inputTexture[i] = i;

				// Create a job that reads from input and writes to output
				NativeTexture3DDeferredJobTest job = new()
				{
					input = inputTexture.AsDeferredJobArray(),
					output = outputTexture.AsDeferredJobArray(),
				};

				JobHandle handle = job.Schedule(inputTexture.Length, 32);
				handle.Complete();

				// Verify job results
				for (int i = 0; i < outputTexture.Length; i++) Assert.AreEqual(i * 3, outputTexture[i]);
			}
			finally
			{
				// Ensure resource cleanup
				inputTexture.Dispose();
				outputTexture.Dispose();
			}
		}

		[Test]
		public void NativeTexture3D_UnsafePointerAccess()
		{
			int3 resolution = new(4, 4, 4);
			NativeTexture3D<float> texture3D = new(
				resolution,
				Allocator.TempJob
			);

			try
			{
				// Fill with test values
				for (int i = 0; i < texture3D.Length; i++) texture3D[i] = i;

				// Test GetUnsafePtr and GetUnsafeReadOnlyPtr
				unsafe
				{
					// Get pointers
					float* writePtr = (float*)texture3D.GetUnsafePtr();
					float* readPtr = (float*)texture3D.GetUnsafeReadOnlyPtr();

					// Modify via write pointer
					for (int i = 0; i < texture3D.Length; i++) writePtr[i] = i * 4;

					// Verify via read pointer
					for (int i = 0; i < texture3D.Length; i++) Assert.AreEqual(i * 4, readPtr[i]);

					// Verify the changes are reflected in the texture
					for (int i = 0; i < texture3D.Length; i++) Assert.AreEqual(i * 4, texture3D[i]);
				}
			}
			finally
			{
				// Ensure cleanup
				texture3D.Dispose();
			}
		}

		[Test]
		public void NoiseIntoNativeTexture3D()
		{
			using FastNoise nodeTree = FastNoise.FromEncodedNodeTree(
				"DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA=="
			);

			int3 resolution = new(32, 32, 32);
			NativeTexture3D<float> noiseTexture3D = new(
				resolution,
				Allocator.TempJob
			);
			Texture2D texture = null;

			try
			{
				// Create a Texture2D for visualization (slice visualization)
				texture = new Texture2D(resolution.x, resolution.y, TextureFormat.RFloat, false);

				// Generate 3D noise (using 3D coordinates)
				for (int z = 0; z < resolution.z; z++)
				for (int y = 0; y < resolution.y; y++)
				for (int x = 0; x < resolution.x; x++)
				{
					float nx = x * 0.05f;
					float ny = y * 0.05f;
					float nz = z * 0.05f;

					// Generate a 3D noise value at this coordinate
					float noiseValue = nodeTree.GenSingle3D(nx, ny, nz, 1337);
					noiseTexture3D[new int3(x, y, z)] = noiseValue;
				}

				// Verify a middle slice of the 3D texture
				int sliceZ = resolution.z / 2;

				// Extract a slice from our 3D texture and apply to the 2D texture
				NativeArray<float> textureData = texture.GetRawTextureData<float>();

				for (int y = 0; y < resolution.y; y++)
				for (int x = 0; x < resolution.x; x++)
				{
					int index2D = x + (y * resolution.x);
					textureData[index2D] = noiseTexture3D[new int3(x, y, sliceZ)];
				}

				texture.Apply();

				// In a real test environment, you might save the texture for visual verification
				// File.WriteAllBytes("tex3DNoise_slice.png", texture.EncodeToPNG());

				// For unit test, we just verify that some values are within expected range for noise
				bool hasValues = false;
				for (int i = 0; i < noiseTexture3D.Length; i++)
				{
					float value = noiseTexture3D[i];
					// Basic noise value range check
					Assert.IsTrue(value >= -1.0f && value <= 1.0f);

					// Check that we don't just have all zeros
					if (Mathf.Abs(value) > 0.01f) hasValues = true;
				}

				Assert.IsTrue(hasValues, "Noise texture should contain non-zero values");
			}
			finally
			{
				// Clean up
				noiseTexture3D.Dispose();
				if (texture != null)
					UnityEngine.Object.DestroyImmediate(texture);
			}
		}

		[Test]
		public void NativeTexture3D_Dispose_JobHandle()
		{
			int3 resolution = new(16, 16, 16);
			NativeTexture3D<float> texture3D = new(
				resolution,
				Allocator.TempJob
			);

			// Fill with values
			for (int i = 0; i < texture3D.Length; i++) texture3D[i] = i;

			// Schedule a job
			NativeTexture3DJobTest job = new()
				{ texture = texture3D };

			JobHandle jobHandle = job.Schedule(texture3D.Length, 32);

			// Schedule disposal after job completes
			JobHandle disposeHandle = texture3D.Dispose(jobHandle);

			// Complete all work
			disposeHandle.Complete();

			// No way to assert disposal worked correctly in a unit test
			// In practice, if the resource was not properly disposed,
			// the domain reload safety system would report errors
		}
	}
}
