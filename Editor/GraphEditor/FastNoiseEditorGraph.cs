using System;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	[Graph("fn2graph")]
	[Serializable]
	public class FastNoiseEditorGraph : Graph
	{
		[MenuItem("Assets/Create/FastNoise2/Noise Graph")]
		static void CreateNoiseGraphAsset()
		{
			GraphDatabase.PromptInProjectBrowserToCreateNewAsset<FastNoiseEditorGraph>("New Noise Graph");
		}

		public override void OnGraphChanged(GraphLogger graphLogger)
		{
			int outputCount = GetNodes().OfType<FastNoiseOutputNode>().Count();
			if (outputCount == 0)
				graphLogger.LogWarning("Graph has no Output node.");
			else if (outputCount > 1)
				graphLogger.LogWarning("Graph has multiple Output nodes. Only one is allowed.");

			foreach (var node in GetNodes())
			{
				if (node is FastNoiseEditorNode editorNode)
					editorNode.ValidateConnections(graphLogger);
			}
		}
	}
}
