using System.IO;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Tests
{
	using Bindings;
	using Jobs;
	using NativeTexture;

	public class JobSystemTests
	{
		[Test]
		public void NativeTexture2DFromTexture2D()
		{
			TestGuards.RequireFastNoiseNative();
			FastNoise nodeTree = TestGuards.CreateTestNoiseOrSkip();

			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> nt = new(texture);

			// Create bounds reference for tracking min/max values
			NativeReference<ValueBounds> boundsRef = NativeTextureNormalizeJobExt.CreateBoundsReference(
				Allocator.TempJob
			);

			// Generate noise on managing thread (avoid calling native from Burst job)
			nodeTree.GenUniformGrid2D(nt, boundsRef, 0, 0, nt.Width, nt.Height, 0.02f, 1337);

			// Now schedule normalization with a fresh dependency
			JobHandle normalizeDependency = nt.ScheduleNormalize(boundsRef, default);
			normalizeDependency.Complete();

			Debug.Log($"Noise bounds: Min={boundsRef.Value.Min}, Max={boundsRef.Value.Max}");

			nt.ApplyTo(texture);

			File.WriteAllBytes("texJobSystem.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
			nodeTree.Dispose();
			nt.Dispose();
			boundsRef.Dispose();
		}

		[Test]
		public void NativeTexture2DWithValueBounds()
		{
			TestGuards.RequireFastNoiseNative();
			FastNoise nodeTree = TestGuards.CreateTestNoiseOrSkip();

			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> nt = new(texture);

			// Create a bounds reference to track min/max values during noise generation
			NativeReference<ValueBounds> boundsRef = new(Allocator.TempJob);

			// Generate noise with built-in bounds tracking
			nodeTree.GenUniformGrid2D(nt, boundsRef, 0, 0, nt.Width, nt.Height, 0.02f, 1337);

			// Now normalize based on the tracked bounds
			JobHandle dependency = default;
			dependency = nt.ScheduleNormalize(boundsRef, dependency);

			dependency.Complete();

			nt.ApplyTo(texture);

			Debug.Log($"Noise bounds: Min={boundsRef.Value.Min}, Max={boundsRef.Value.Max}");

			File.WriteAllBytes("texJobSystemWithBounds.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
			nodeTree.Dispose();
			nt.Dispose();
			boundsRef.Dispose();
		}

		[Test]
		public void TextureBoundsUtilityTest()
		{
			TestGuards.RequireFastNoiseNative();
			FastNoise nodeTree = TestGuards.CreateTestNoiseOrSkip();

			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> nt = new(new int2(512, 512), Allocator.TempJob);

			// Create a bounds reference using our extension method
			NativeReference<ValueBounds> boundsRef = NativeTextureNormalizeJobExt.CreateBoundsReference(
				Allocator.TempJob
			);

			// Generate noise directly with built-in bounds tracking
			nodeTree.GenUniformGrid2D(nt, boundsRef, 0, 0, nt.Width, nt.Height, 0.02f, 1337);

			// Schedule normalization only (bounds already tracked by FastNoise2)
			JobHandle dependency = default;
			dependency = nt.ScheduleNormalize(boundsRef, dependency);

			dependency.Complete();

			Debug.Log($"Generated noise bounds: Min={boundsRef.Value.Min}, Max={boundsRef.Value.Max}");

			nt.ApplyTo(texture);

			File.WriteAllBytes("texUtilityBounds.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
			nodeTree.Dispose();
			nt.Dispose();
			boundsRef.Dispose();
		}

		[Test]
		public void ChainedNoiseAndNormalize()
		{
			TestGuards.RequireFastNoiseNative();
			// Create noise generator
			FastNoise nodeTree = TestGuards.CreateTestNoiseOrSkip();

			// Create output texture
			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> nt = new(texture);

			// Create bounds reference for tracking
			NativeReference<ValueBounds> boundsRef = new(Allocator.TempJob);

			// Initial job dependency
			JobHandle dependency = default;

			// Generate noise on managing thread for stability with native plugin
			nodeTree.GenUniformGrid2D(nt, boundsRef, 0, 0, nt.Width, nt.Height, 0.02f, 1337);

			// Chain the normalization job after the noise generation
			dependency = nt.ScheduleNormalize(boundsRef, default);

			// Complete the final job only after all jobs are scheduled
			dependency.Complete();

			// Output the results
			Debug.Log($"Chained noise bounds: Min={boundsRef.Value.Min}, Max={boundsRef.Value.Max}");

			nt.ApplyTo(texture);

			File.WriteAllBytes("texChainedJobs.png", texture.EncodeToPNG());

			// Cleanup
			Object.DestroyImmediate(texture);
			nodeTree.Dispose();
			nt.Dispose();
			boundsRef.Dispose();
		}
	}
}
