using System;
using System.IO;
using FastNoise2.Bindings;
using FastNoise2.Jobs;
using FastNoise2.NativeTexture;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Tests
{
	public class ValueBoundsTests
	{
		[Test]
		public void GenerateNoiseWithManualBoundsTracking()
		{
			// Create noise generator
			using FastNoise nodeTree = FastNoise.FromEncodedNodeTree(
				"DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA=="
			);

			// Create output texture
			Texture2D texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			int width = texture.width;
			int height = texture.height;

			// Create native texture from Unity texture
			NativeTexture2D<float> nativeTexture = new NativeTexture2D<float>(texture);

			// Create a separate bounds reference to track min/max values
			NativeReference<ValueBounds> boundsRef = new NativeReference<ValueBounds>(
				Allocator.TempJob
			);

			// Reset bounds before generation
			boundsRef.Reset();

			float frequency = 0.01f;
			int seed = 1337;

			// Generate noise directly into the texture with built-in bounds tracking
			nodeTree.GenUniformGrid2D(
				nativeTexture,
				boundsRef,
				0,
				0, // start position
				width,
				height, // size
				frequency,
				seed
			);

			// Check bounds are tracked properly
			ValueBounds bounds = boundsRef.Value;
			Assert.That(bounds.Min, Is.LessThan(0));
			Assert.That(bounds.Max, Is.GreaterThan(0));

			// Precalculate normalization values
			boundsRef.PrecalculateScale();

			// Normalize the texture using our bounds
			NativeArray<float> textureData = nativeTexture.AsArray();
			for (int i = 0; i < textureData.Length; i++)
			{
				textureData[i] = boundsRef.NormalizeValue(textureData[i]);
			}

			// Check normalization is working correctly
			foreach (float value in textureData)
			{
				Assert.That(value, Is.GreaterThanOrEqualTo(0));
				Assert.That(value, Is.LessThanOrEqualTo(1));
			}

			// Apply the native texture data back to the Unity texture
			nativeTexture.ApplyTo(texture);

			File.WriteAllBytes("noiseWithManualBounds.png", texture.EncodeToPNG());

			// Dispose resources
			nativeTexture.Dispose();
			boundsRef.Dispose();
			UnityEngine.Object.DestroyImmediate(texture);
		}

		[Test]
		public void GenerateNoiseWithCustomBounds()
		{
			// Create noise generator
			using FastNoise nodeTree = FastNoise.FromEncodedNodeTree(
				"DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA=="
			);

			// Create output texture
			Texture2D texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			int width = texture.width;
			int height = texture.height;

			// Create native texture from Unity texture
			NativeTexture2D<float> nativeTexture = new NativeTexture2D<float>(texture);

			// Create a temporary bounds reference for capturing actual noise bounds
			NativeReference<ValueBounds> tmpBoundsRef = new NativeReference<ValueBounds>(
				Allocator.TempJob
			);
			tmpBoundsRef.Reset();

			float frequency = 0.01f;
			int seed = 1337;

			// Generate noise with the temporary bounds reference to track actual bounds
			nodeTree.GenUniformGrid2D(
				nativeTexture,
				tmpBoundsRef,
				0,
				0, // start position
				width,
				height, // size
				frequency,
				seed
			);

			// Get the actual bounds
			float actualMin = tmpBoundsRef.Value.Min;
			float actualMax = tmpBoundsRef.Value.Max;
			Debug.Log($"Actual noise range: Min={actualMin}, Max={actualMax}");

			// Create the custom bounds reference with our custom values
			NativeReference<ValueBounds> customBoundsRef = new NativeReference<ValueBounds>(
				Allocator.TempJob
			);

			// Set explicit custom bounds
			float minValue = -0.5f;
			float maxValue = 0.5f;

			ValueBounds bounds = customBoundsRef.Value;
			bounds.Min = minValue;
			bounds.Max = maxValue;
			customBoundsRef.Value = bounds;

			// Precalculate normalization values from custom bounds
			customBoundsRef.PrecalculateScale();

			// Normalize with custom bounds
			NativeArray<float> textureData = nativeTexture.AsArray();
			for (int i = 0; i < textureData.Length; i++)
			{
				textureData[i] = customBoundsRef.NormalizeValue(textureData[i]);
			}

			Debug.Log($"Custom bounds: Min={minValue}, Max={maxValue}");

			// Apply the native texture data back to the Unity texture
			nativeTexture.ApplyTo(texture);

			File.WriteAllBytes("noiseWithCustomBounds.png", texture.EncodeToPNG());

			// Dispose resources
			nativeTexture.Dispose();
			tmpBoundsRef.Dispose();
			customBoundsRef.Dispose();
			UnityEngine.Object.DestroyImmediate(texture);
		}

		[Test]
		public void GenerateNoiseWithJobSystem()
		{
			// Create noise generator
			using FastNoise nodeTree = FastNoise.FromEncodedNodeTree(
				"DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA=="
			);

			// Create output texture
			Texture2D texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			int width = texture.width;
			int height = texture.height;

			// Create a native texture with its own memory
			NativeTexture2D<float> nativeTexture = new NativeTexture2D<float>(
				new int2(width, height),
				Allocator.TempJob
			);

			// Create bounds reference
			NativeReference<ValueBounds> boundsRef =
				NativeTextureNormalizeJobExt.CreateBoundsReference(Allocator.TempJob);

			float frequency = 0.01f;
			int seed = 1337;

			// Generate noise directly into the texture with built-in bounds tracking
			nodeTree.GenUniformGrid2D(
				nativeTexture,
				boundsRef,
				0,
				0,
				width,
				height,
				frequency,
				seed
			);

			// Schedule normalization using job system
			JobHandle jobHandle = default;
			jobHandle = nativeTexture.ScheduleNormalize(boundsRef, jobHandle);

			// Wait for jobs to complete
			jobHandle.Complete();

			// Check that bounds were tracked and normalization was applied
			ValueBounds bounds = boundsRef.Value;
			Debug.Log($"Noise bounds: Min={bounds.Min}, Max={bounds.Max}");

			// Check that all values in the texture are normalized [0, 1]
			NativeArray<float> textureData = nativeTexture.AsArray();
			foreach (float value in textureData)
			{
				Assert.That(value, Is.GreaterThanOrEqualTo(0));
				Assert.That(value, Is.LessThanOrEqualTo(1));
			}

			// Apply the native texture data back to the Unity texture
			nativeTexture.ApplyTo(texture);

			File.WriteAllBytes("noiseWithJobSystem.png", texture.EncodeToPNG());

			// Dispose resources
			nativeTexture.Dispose();
			boundsRef.Dispose();
			UnityEngine.Object.DestroyImmediate(texture);
		}
	}
}
