using System.IO;
using NativeTexture;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Tests
{
	using Bindings;
	using Jobs;

	public class JobSystemTests
	{
		[Test]
		public void NativeTexture2DFromTexture2D()
		{
			FastNoise nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> nt = new(texture);

			// Create bounds reference for tracking min/max values
			NativeReference<ValueBounds> boundsRef = NativeTextureNormalizeJobExt.CreateBoundsReference(
				Allocator.TempJob
			);

			JobHandle noiseJobDependency = default;

			// Generate noise with bounds tracking
			noiseJobDependency = new GenUniformGrid2DJob
			{
				texture = nt,
				noise = nodeTree,
				boundsRef = boundsRef,
				offset = 0,
				frequency = .02f,
				seed = 1337,
			}.Schedule(noiseJobDependency);

			// Complete the noise generation job before scheduling normalization
			noiseJobDependency.Complete();

			// Now schedule normalization with a fresh dependency
			JobHandle normalizeDependency = default;
			normalizeDependency = nt.ScheduleNormalize(boundsRef, normalizeDependency);
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
			FastNoise nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

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
			FastNoise nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

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
			// Create noise generator
			FastNoise nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

			// Create output texture
			Texture2D texture = new(512, 512, TextureFormat.RFloat, false);
			NativeTexture2D<float> nt = new(texture);

			// Create bounds reference for tracking
			NativeReference<ValueBounds> boundsRef = new(Allocator.TempJob);

			// Initial job dependency
			JobHandle dependency = default;

			// Instead of using GenUniformGrid2DJob directly, use the extension method
			// This properly handles dependencies between the jobs
			dependency = nodeTree.GenUniformGrid2D(
				nt,
				boundsRef,
				1337, // seed
				new int2(0, 0), // start position
				0.02f, // frequency
				dependency
			); // pass the dependency chain

			// Chain the normalization job after the noise generation
			// The normalization job will automatically wait for noise generation to complete
			dependency = nt.ScheduleNormalize(boundsRef, dependency);

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
