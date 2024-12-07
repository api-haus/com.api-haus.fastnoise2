using System.IO;
using FastNoise2.Runtime.Bindings;
using FastNoise2.Runtime.Jobs._2D;
using FastNoise2.Runtime.NativeTexture;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Tests
{
	public class JobSystemTests
	{
		[Test]
		public void NativeTexture2DFromTexture2D()
		{
			var nodeTree = FastNoise.FromEncodedNodeTree("Bg@AJZD/w==");

			var texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			var nt = new NativeTexture2D<float>(new int2(texture.width, texture.height), texture, Allocator.Persistent);

			JobHandle dependency = default;

			dependency = new GenUniformGrid2DJob
			{
				Texture = nt,
				Noise = nodeTree,
				Offset = 0,
				Frequency = .02f,
				Seed = 1337,
			}.Schedule(dependency);

			dependency = NormalizeTexture2DJob.Schedule(nt, dependency);

			dependency.Complete();

			nt.ApplyTo(texture);

			File.WriteAllBytes("texJobSystem.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
			nodeTree.Dispose();
			nt.Dispose();
		}
	}
}
