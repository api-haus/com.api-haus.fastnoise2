using System.IO;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace FastNoise2.Tests
{
	using Bindings;

	public class BindingTests
	{
		[Test]
		public void GenerateBitmapTestSafe()
		{
			using var cellular = new FastNoise("CellularDistance");
			cellular.Set("ReturnType", "Index0Add1");
			cellular.Set("DistanceIndex0", 2);

			using var fractal = new FastNoise("FractalFBm");
			fractal.Set("Source", new FastNoise("Simplex"));
			fractal.Set("Gain", 0.3f);
			fractal.Set("Lacunarity", 0.6f);

			using var addDim = new FastNoise("AddDimension");
			addDim.Set("Source", cellular);
			addDim.Set("NewDimensionPosition", 0.5f);
			// or
			// addDim.Set("NewDimensionPosition", new FastNoise("Perlin"));

			using var maxSmooth = new FastNoise("MaxSmooth");
			maxSmooth.Set("LHS", fractal);
			maxSmooth.Set("RHS", addDim);

			Debug.Log("SIMD Level " + maxSmooth.GetSIMDLevel());

			GenerateBitmap(maxSmooth, "testMetadata");

			// Dunes
			using var nodeTree = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==");

			// Encoded node trees can be invalid and return null
			if (nodeTree != FastNoise.Invalid)
			{
				GenerateBitmap(nodeTree, "testENT");
			}
		}

		static void GenerateBitmap(FastNoise fastNoise, string filename, ushort size = 512)
		{
			using (var writer = new BinaryWriter(File.Open(filename + ".bmp", FileMode.Create)))
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
				for (var i = 0; i < 256; i++)
				{
					writer.Write((byte)i);
					writer.Write((byte)i);
					writer.Write((byte)i);
				}

				// Image data
				var noiseData = new float[size * size];
				var minMax = fastNoise.GenUniformGrid2D(noiseData, 0, 0, size, size, 0.02f, 1337);

				var scale = 255.0f / (minMax.max - minMax.min);

				foreach (var noise in noiseData)
				{
					//Scale noise to 0 - 255
					var noiseI = (int)math.round((noise - minMax.min) * scale);

					writer.Write((byte)math.clamp(noiseI, 0, 255));
				}
			}
		}
	}
}
