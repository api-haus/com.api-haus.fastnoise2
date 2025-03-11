using System.IO;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Tests
{
	using Bindings;
	using NativeTexture;

	public class NativeTextureTests
	{
		[Test]
		public void NoiseIntoNativeTexture2D()
		{
			using FastNoise nodeTree = FastNoise.FromEncodedNodeTree(
				"DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA=="
			);

			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> noiseTexture2D = new(
				512,
				Allocator.TempJob
			);

			// Create a bounds reference for tracking min/max values
			using NativeReference<ValueBounds> boundsRef = new(
				Allocator.Temp
			);

			// Generate noise directly into the native texture with built-in bounds tracking
			nodeTree.GenUniformGrid2D(
				noiseTexture2D,
				boundsRef,
				0,
				0,
				noiseTexture2D.Width,
				noiseTexture2D.Height,
				0.02f,
				1337
			);

			// Log the bounds for verification
			Debug.Log($"Noise bounds: Min={boundsRef.Value.Min}, Max={boundsRef.Value.Max}");

			noiseTexture2D.ApplyTo(texture);

			File.WriteAllBytes("texNative.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
		}

		[Test]
		public void NoiseIntoNativeTexture2DZeroCopy()
		{
			using FastNoise nodeTree = FastNoise.FromEncodedNodeTree(
				"DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA=="
			);

			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> noiseTexture2D = new(texture);

			// Create a bounds reference for tracking min/max values
			using NativeReference<ValueBounds> boundsRef = new(
				Allocator.Temp
			);

			// Generate noise directly into the native texture with built-in bounds tracking
			nodeTree.GenUniformGrid2D(
				noiseTexture2D,
				boundsRef,
				0,
				0,
				noiseTexture2D.Width,
				noiseTexture2D.Height,
				0.02f,
				1337
			);

			// Log the bounds for verification
			Debug.Log($"Noise bounds: Min={boundsRef.Value.Min}, Max={boundsRef.Value.Max}");

			noiseTexture2D.ApplyTo(texture);

			File.WriteAllBytes("texNativeZeroCopy.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
		}

		[Test]
		public void NoiseIntoNativeTexture2DWithBounds()
		{
			using FastNoise nodeTree = FastNoise.FromEncodedNodeTree(
				"DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA=="
			);

			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> noiseTexture2D = new(texture);

			// Create a bounds reference to track min/max values
			NativeReference<ValueBounds> boundsRef = new(
				Allocator.Temp
			);

			// Generate noise directly into the texture with built-in bounds tracking
			nodeTree.GenUniformGrid2D(
				noiseTexture2D,
				boundsRef,
				0,
				0,
				noiseTexture2D.Width,
				noiseTexture2D.Height,
				0.02f,
				1337
			);

			// Output the bounds
			Debug.Log($"Noise bounds: Min={boundsRef.Value.Min}, Max={boundsRef.Value.Max}");

			// Precalculate normalization parameters
			boundsRef.PrecalculateScale();

			// Normalize the texture using our bounds
			NativeArray<float> textureData = noiseTexture2D.AsArray();
			for (int i = 0; i < textureData.Length; i++) textureData[i] = boundsRef.NormalizeValue(textureData[i]);

			noiseTexture2D.ApplyTo(texture);

			File.WriteAllBytes("texNativeZeroCopyWithBounds.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
			boundsRef.Dispose();
		}

		[Test]
		public void NoiseIntoNativeTexture2DWithCustomBounds()
		{
			using FastNoise nodeTree = FastNoise.FromEncodedNodeTree(
				"DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA=="
			);

			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> noiseTexture2D = new(texture);

			// First, generate noise directly and track the actual bounds
			NativeReference<ValueBounds> actualBoundsRef = new(
				Allocator.Temp
			);

			// Generate noise with the built-in bounds tracking
			nodeTree.GenUniformGrid2D(
				noiseTexture2D,
				actualBoundsRef,
				0,
				0,
				noiseTexture2D.Width,
				noiseTexture2D.Height,
				0.02f,
				1337
			);

			// Output the actual bounds
			Debug.Log(
				$"Actual noise bounds: Min={actualBoundsRef.Value.Min}, Max={actualBoundsRef.Value.Max}"
			);

			// Create a bounds reference with custom min/max values
			NativeReference<ValueBounds> customBoundsRef = new(
				Allocator.Temp
			);

			// Set custom bounds values
			ValueBounds bounds = customBoundsRef.Value;
			bounds.Min = -0.5f; // Custom minimum
			bounds.Max = 0.5f; // Custom maximum
			customBoundsRef.Value = bounds;

			// Precalculate normalization with custom bounds
			customBoundsRef.PrecalculateScale();

			// Normalize with custom bounds (will clamp values outside the specified range)
			NativeArray<float> textureData = noiseTexture2D.AsArray();
			for (int i = 0; i < textureData.Length; i++)
				textureData[i] = customBoundsRef.NormalizeValue(textureData[i]);

			noiseTexture2D.ApplyTo(texture);

			File.WriteAllBytes("texNativeZeroCopyWithCustomBounds.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
			actualBoundsRef.Dispose();
			customBoundsRef.Dispose();
		}
	}
}
