using System.IO;
using NUnit.Framework;
using Unity.Collections;
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
			using var nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

			var texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			using var noiseTexture2D = new NativeTexture2D<float>(512, Allocator.TempJob);

			nodeTree.GenUniformGrid2D(
				noiseTexture2D,
				0, 0,
				noiseTexture2D.Width, noiseTexture2D.Height,
				0.02f, 1337);

			noiseTexture2D.ApplyTo(texture);

			File.WriteAllBytes("texNative.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
		}

		[Test]
		public void NoiseIntoNativeTexture2DZeroCopy()
		{
			using var nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

			var texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			using var noiseTexture2D = new NativeTexture2D<float>(texture, Allocator.TempJob);

			nodeTree.GenUniformGrid2D(
				noiseTexture2D,
				0, 0,
				noiseTexture2D.Width, noiseTexture2D.Height,
				0.02f, 1337);

			noiseTexture2D.ApplyTo(texture);

			File.WriteAllBytes("texNativeZeroCopy.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
		}
	}
}
