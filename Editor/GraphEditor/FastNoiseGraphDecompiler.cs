using System.Collections.Generic;
using FastNoise2.Generators;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Decompiles an FN2 encoded string into visual graph nodes and wires
	/// using <see cref="GraphToolkitBridge"/> for programmatic graph mutation.
	/// </summary>
	static class FastNoiseGraphDecompiler
	{
		const float NodeSpacingX = 300f;
		const float NodeSpacingY = 180f;

		static string NormalizeLookupKey(string key) => key.Replace(" ", "").ToLower();

		struct CreatedNode
		{
			public FN2EditorNode EditorNode;
			public INode GraphNode;
		}

		public static bool Decompile(string encoded, FastNoiseEditorGraph graph)
		{
			if (string.IsNullOrEmpty(encoded))
				return false;

			NodeDescriptor root = NodeDecoder.Decode(encoded);
			if (root == null)
				return false;

			var nodeMap = new Dictionary<NodeDescriptor, CreatedNode>();
			int nodeIndex = 0;

			// Recursively create noise nodes (depth-first, children before parents)
			CreateNodesRecursive(root, graph, nodeMap, ref nodeIndex, 0);

			// Create the output node to the right of everything
			var outputNode = new FastNoiseOutputNode();
			GraphToolkitBridge.CreateNode(graph, outputNode,
				new Vector2((nodeIndex + 1) * NodeSpacingX, 0f));

			// Wire root node's output to the output node's input
			if (nodeMap.TryGetValue(root, out var rootCreated))
			{
				var fromPort = rootCreated.EditorNode.GetOutputPortByName(
					FN2EditorNode.OutputPortName);
				var toPort = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);
				if (fromPort != null && toPort != null)
					GraphToolkitBridge.CreateWire(graph, toPort, fromPort);
			}

			return true;
		}

		static FN2EditorNode CreateNodeByTypeName(string nodeTypeName)
		{
			var node = new FN2GenericNode();
			node.SetNodeTypeName(nodeTypeName);
			return node;
		}

		static void CreateNodesRecursive(NodeDescriptor descriptor, FastNoiseEditorGraph graph,
			Dictionary<NodeDescriptor, CreatedNode> nodeMap, ref int nodeIndex, int depth)
		{
			if (nodeMap.ContainsKey(descriptor))
				return;

			// Create child nodes first (depth-first)
			foreach (var kv in descriptor.NodeLookups)
				CreateNodesRecursive(kv.Value, graph, nodeMap, ref nodeIndex, depth + 1);

			foreach (var kv in descriptor.Hybrids)
			{
				if (kv.Value.IsNode)
					CreateNodesRecursive(kv.Value.NodeValue, graph, nodeMap, ref nodeIndex, depth + 1);
			}

			// Create this node
			var editorNode = CreateNodeByTypeName(descriptor.NodeName);
			if (editorNode == null)
				return;

			PopulateVariables(editorNode, descriptor);
			PopulateHybrids(editorNode, descriptor);

			float posX = nodeIndex * NodeSpacingX;
			float posY = depth * NodeSpacingY;
			var inode = GraphToolkitBridge.CreateNode(graph, editorNode,
				new Vector2(posX, posY));

			nodeMap[descriptor] = new CreatedNode
			{
				EditorNode = editorNode,
				GraphNode = inode
			};
			nodeIndex++;

			// Wire children to this node
			WireChildren(descriptor, editorNode, graph, nodeMap);
		}

		static void PopulateVariables(FN2EditorNode node, NodeDescriptor descriptor)
		{
			foreach (var kv in descriptor.Variables)
			{
				string lookupKey = NormalizeLookupKey(kv.Key);
				string optionId = FN2EditorNode.VarPrefix + lookupKey;
				node.variableValues.SetInt(optionId, kv.Value);
			}
		}

		static void PopulateHybrids(FN2EditorNode node, NodeDescriptor descriptor)
		{
			foreach (var kv in descriptor.Hybrids)
			{
				if (kv.Value.IsNode)
					continue;

				string lookupKey = NormalizeLookupKey(kv.Key);
				string optionId = FN2EditorNode.HybridValuePrefix + lookupKey;
				node.hybridDefaults.SetFloat(optionId, kv.Value.FloatValue);
			}
		}

		static void WireChildren(NodeDescriptor descriptor, FN2EditorNode parentNode,
			FastNoiseEditorGraph graph, Dictionary<NodeDescriptor, CreatedNode> nodeMap)
		{
			foreach (var kv in descriptor.NodeLookups)
			{
				if (!nodeMap.TryGetValue(kv.Value, out var childCreated))
					continue;

				string lookupKey = NormalizeLookupKey(kv.Key);
				string portId = FN2EditorNode.NodeLookupPrefix + lookupKey;
				var toPort = parentNode.GetInputPortByName(portId);
				var fromPort = childCreated.EditorNode.GetOutputPortByName(
					FN2EditorNode.OutputPortName);

				if (toPort != null && fromPort != null)
					GraphToolkitBridge.CreateWire(graph, toPort, fromPort);
			}

			foreach (var kv in descriptor.Hybrids)
			{
				if (!kv.Value.IsNode)
					continue;

				if (!nodeMap.TryGetValue(kv.Value.NodeValue, out var childCreated))
					continue;

				string lookupKey = NormalizeLookupKey(kv.Key);
				string portId = FN2EditorNode.HybridPortPrefix + lookupKey;
				var toPort = parentNode.GetInputPortByName(portId);
				var fromPort = childCreated.EditorNode.GetOutputPortByName(
					FN2EditorNode.OutputPortName);

				if (toPort != null && fromPort != null)
					GraphToolkitBridge.CreateWire(graph, toPort, fromPort);
			}
		}
	}
}
