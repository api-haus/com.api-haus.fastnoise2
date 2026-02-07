using System.Collections.Generic;
using FastNoise2.Generators;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Compiles a <see cref="NodeDescriptor"/> tree from any FN2 editor node
	/// (not just the output node) and encodes it to a base64 string.
	/// Used by the texture preview system to render noise at any point in the graph.
	/// </summary>
	static class FastNoiseSubtreeCompiler
	{
		/// <summary>
		/// Compile the subtree rooted at <paramref name="node"/> into a base64 FN2 string.
		/// Returns null on failure.
		/// </summary>
		public static string CompileSubtree(FN2EditorNode node)
		{
			var visited = new Dictionary<FN2EditorNode, NodeDescriptor>();
			var descriptor = FastNoiseGraphCompiler.CompileEditorNode(node, visited);
			if (descriptor == null)
				return null;

			return NodeEncoder.Encode(descriptor);
		}
	}
}
