using System.IO;
using FastNoise2.Runtime.Bindings;
using FastNoise2.Runtime.NativeTexture;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Tests
{
	public class NativeTextureTests
	{
		[Test]
		public void NoiseIntoNativeTexture2D()
		{
			using var nodeTree = FastNoise.FromEncodedNodeTree("Bg@AJZD/w==");

			var texture = new Texture2D(512, 512, TextureFormat.RFloat, false);
			using var noiseTexture2D = new NativeTexture2D<float>(new int2(512, 512), Allocator.TempJob);

			nodeTree.GenUniformGrid2D(
				noiseTexture2D,
				0, 0,
				noiseTexture2D.Width, noiseTexture2D.Height,
				0.02f, 1337);

			noiseTexture2D.ApplyTo(texture);

			File.WriteAllBytes("texNative.png", texture.EncodeToPNG());
			Object.DestroyImmediate(texture);
		}
	}
}
