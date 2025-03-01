using System;
using Unity.Collections;
using UnityEngine;

namespace FastNoise2.Authoring.NoiseGraph
{
	using Bindings;

	/// <summary>
	/// Serialized as base64 String in Editor.
	/// Launches NoiseTool and hunts for Copied Values.
	/// </summary>
	[Serializable]
	public class FastNoiseGraph
	{
		public FixedString64Bytes Fixed64 => new(encodedGraph);
		public FixedString128Bytes Fixed256 => new(encodedGraph);
		public FixedString512Bytes Fixed512 => new(encodedGraph);
		public FixedString4096Bytes Fixed4096 => new(encodedGraph);

		[SerializeField]
		string encodedGraph = "DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA==";

		public string EncodedValue => encodedGraph;

		internal void SetValue(string graph)
		{
			encodedGraph = graph;
		}

		public static implicit operator string(FastNoiseGraph graph)
		{
			return graph.EncodedValue;
		}

		public static implicit operator FastNoiseGraph(string encodedGraph)
		{
			return new FastNoiseGraph { encodedGraph = encodedGraph };
		}

		public override string ToString()
		{
			return EncodedValue;
		}
	}

	public static class FastNoiseGraphExt
	{
		public static FastNoise Instantiate(this FastNoiseGraph graph)
		{
			Debug.Assert(
				!string.IsNullOrWhiteSpace(graph.EncodedValue),
				nameof(graph.EncodedValue)
			);
			var noise = FastNoise.FromEncodedNodeTree(graph.EncodedValue);

			return noise;
		}
	}
}
