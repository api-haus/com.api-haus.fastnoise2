using System.IO;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Tests
{
	using System;
	using Bindings;
	using NativeTexture;

	public static class TestGuards
	{
		public static void RequireFastNoiseNative()
		{
			try
			{
				// Touch a trivial native call to force load
				_ = FastNoise.FromEncodedNodeTree("BgQ=");
			}
			catch (DllNotFoundException e)
			{
				Assert.Ignore($"FastNoise native plugin blocked: {e.Message}");
			}
			catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
			{
				Assert.Ignore($"FastNoise native plugin blocked: {e.InnerException.Message}");
			}
		}

		public static FastNoise CreateTestNoiseOrSkip()
		{
			try
			{
				FastNoise node = FastNoise.FromEncodedNodeTree("BgQ=");
				if (node == FastNoise.Invalid)
				{
					Assert.Ignore(
						"FastNoise encoded node tree is unsupported by the current native library."
					);
				}
				return node;
			}
			catch (DllNotFoundException e)
			{
				Assert.Ignore($"FastNoise native plugin blocked: {e.Message}");
				throw;
			}
			catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
			{
				Assert.Ignore($"FastNoise native plugin blocked: {e.InnerException.Message}");
				throw;
			}
		}
	}

	public class BindingTests
	{
		[Test]
		public void GenerateBitmapTestSafe()
		{
			TestGuards.RequireFastNoiseNative();
			// Use encoded node tree with the updated native library
			using FastNoise nodeTree = TestGuards.CreateTestNoiseOrSkip();

			if (nodeTree != FastNoise.Invalid)
				GenerateBitmap(nodeTree, "testENT");
		}

		[Test]
		public void GenerateBitmapWithValueBounds()
		{
			TestGuards.RequireFastNoiseNative();
			using FastNoise nodeTree = TestGuards.CreateTestNoiseOrSkip();

			// Encoded node trees can be invalid and return null
			if (nodeTree != FastNoise.Invalid)
				GenerateBitmapWithTrackedBounds(nodeTree, "testWithValueBounds");
		}

		private static void GenerateBitmap(FastNoise fastNoise, string filename, ushort size = 512)
		{
			using (BinaryWriter writer = new(File.Open(filename + ".bmp", FileMode.Create)))
			{
				const uint imageDataOffset = 14u + 12u + (256u * 3u);

				// File header (14)
				writer.Write('B');
				writer.Write('M');
				writer.Write(imageDataOffset + (uint)(size * size)); // file size
				writer.Write(0); // reserved
				writer.Write(imageDataOffset); // image data offset
				// Bmp Info Header (12)
				writer.Write(12u); // size of header
				writer.Write(size); // width
				writer.Write(size); // height
				writer.Write((ushort)1); // color planes
				writer.Write((ushort)8); // bit depth
				// Colour map
				for (int i = 0; i < 256; i++)
				{
					writer.Write((byte)i);
					writer.Write((byte)i);
					writer.Write((byte)i);
				}

				// Image data
				float[] noiseData = new float[size * size];
				FastNoise.OutputMinMax minMax = fastNoise.GenUniformGrid2D(
					noiseData,
					0,
					0,
					size,
					size,
					0.02f,
					1337
				);

				float scale = 255.0f / (minMax.max - minMax.min);

				foreach (float noise in noiseData)
				{
					//Scale noise to 0 - 255
					int noiseI = (int)math.round((noise - minMax.min) * scale);

					writer.Write((byte)math.clamp(noiseI, 0, 255));
				}
			}
		}

		private static void GenerateBitmapWithTrackedBounds(
			FastNoise fastNoise,
			string filename,
			ushort size = 512
		)
		{
			using (BinaryWriter writer = new(File.Open(filename + ".bmp", FileMode.Create)))
			{
				const uint imageDataOffset = 14u + 12u + (256u * 3u);

				// File header (14)
				writer.Write('B');
				writer.Write('M');
				writer.Write(imageDataOffset + (uint)(size * size)); // file size
				writer.Write(0); // reserved
				writer.Write(imageDataOffset); // image data offset
				// Bmp Info Header (12)
				writer.Write(12u); // size of header
				writer.Write(size); // width
				writer.Write(size); // height
				writer.Write((ushort)1); // color planes
				writer.Write((ushort)8); // bit depth
				// Colour map
				for (int i = 0; i < 256; i++)
				{
					writer.Write((byte)i);
					writer.Write((byte)i);
					writer.Write((byte)i);
				}

				// Image data - use NativeTexture2D and ValueBounds
				NativeTexture2D<float> noiseTexture = new(new int2(size, size), Allocator.Temp);

				// Create a bounds reference to track min/max values
				using (NativeReference<ValueBounds> boundsRef = new(Allocator.Temp))
				{
					// Reset bounds before generation
					boundsRef.Reset();

					// Generate noise with bounds tracking
					fastNoise.GenUniformGrid2D(noiseTexture, boundsRef, 0, 0, size, size, 0.02f, 1337);

					// Precalculate normalization values
					boundsRef.PrecalculateScale();

					// Get the min/max values
					float min = boundsRef.Value.Min;
					float max = boundsRef.Value.Max;

					Debug.Log($"Noise bounds: Min={min}, Max={max}");

					// Scale factor for 0-255 range
					float scale = 255.0f / (max - min);

					// Get native array and process
					NativeArray<float> noiseData = noiseTexture.AsArray();

					foreach (float noise in noiseData)
					{
						//Scale noise to 0 - 255
						int noiseI = (int)math.round((noise - min) * scale);

						writer.Write((byte)math.clamp(noiseI, 0, 255));
					}
				}

				// Dispose the texture
				noiseTexture.Dispose();
			}
		}
	}
}
