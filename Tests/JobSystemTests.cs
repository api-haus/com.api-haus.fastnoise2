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
			var nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

			var texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			var nt = new NativeTexture2D<float>(texture, Allocator.TempJob);

			JobHandle dependency = default;

			dependency = new GenUniformGrid2DJob
			{
				Texture = nt,
				Noise = nodeTree,
				Offset = 0,
				Frequency = .02f,
				Seed = 1337,
			}.Schedule(dependency);

			dependency = nt.ScheduleNormalize(dependency);

			dependency.Complete();

			nt.ApplyTo(texture);

			File.WriteAllBytes("texJobSystem.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
			nodeTree.Dispose();
			nt.Dispose();
		}
	}
}
