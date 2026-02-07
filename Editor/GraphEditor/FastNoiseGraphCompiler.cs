using System;
using System.Collections.Generic;
using System.Linq;
using FastNoise2.Generators;
using Unity.GraphToolkit.Editor;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Walks backward from the output node to build a <see cref="NodeDescriptor"/> tree,
	/// then encodes it to an FN2 base64 string.
	/// </summary>
	static class FastNoiseGraphCompiler
	{
		public static string Compile(FastNoiseEditorGraph graph)
		{
			return Compile(graph, out _);
		}

		public static string Compile(FastNoiseEditorGraph graph, out string error)
		{
			error = null;

			var outputNode = graph.GetNodes()
				.OfType<FastNoiseOutputNode>()
				.FirstOrDefault();

			if (outputNode == null)
			{
				error = "No output node found.";
				return null;
			}

			var sourcePort = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);
			if (sourcePort == null || !sourcePort.isConnected)
			{
				error = "Output node's Source port is not connected.";
				return null;
			}

			var visited = new Dictionary<FN2EditorNode, NodeDescriptor>();
			var descriptor = CompileFromPort(sourcePort.firstConnectedPort, visited);
			if (descriptor == null)
			{
				error = "Failed to compile connected node.";
				return null;
			}

			return NodeEncoder.Encode(descriptor);
		}

		internal static NodeDescriptor CompileFromPort(IPort outputPort,
			Dictionary<FN2EditorNode, NodeDescriptor> visited)
		{
			var ownerNode = GraphToolkitBridge.GetNodeFromPort(outputPort) as FN2EditorNode;
			if (ownerNode == null)
				return null;

			if (visited.TryGetValue(ownerNode, out var existing))
				return existing;

			return CompileEditorNode(ownerNode, visited);
		}

		internal static NodeDescriptor CompileEditorNode(FN2EditorNode node,
			Dictionary<FN2EditorNode, NodeDescriptor> visited)
		{
			FN2NodeDef def;
			try { def = FN2NodeRegistry.GetNodeDef(node.NodeTypeName); }
			catch { return null; }

			var variables = new Dictionary<string, int>();
			var nodeLookups = new Dictionary<string, NodeDescriptor>();
			var hybrids = new Dictionary<string, HybridValue>();

			foreach (var member in def.Members)
			{
				switch (member.Type)
				{
					case FN2MemberType.Float:
					{
						string optionId = FN2EditorNode.VarPrefix + member.LookupKey;
						bool hasStored = node.variableValues.TryGetInt(optionId, out int storedRaw);
						var option = node.GetNodeOptionByName(optionId);
						if (option != null && option.TryGetValue<float>(out float fval))
						{
							int bits = BitConverter.SingleToInt32Bits(fval);
							if (hasStored || bits != 0)
								variables[member.LookupKey] = bits;
						}
						else if (hasStored)
						{
							variables[member.LookupKey] = storedRaw;
						}
						break;
					}
					case FN2MemberType.Int:
					case FN2MemberType.Enum:
					{
						string optionId = FN2EditorNode.VarPrefix + member.LookupKey;
						bool hasStored = node.variableValues.TryGetInt(optionId, out int storedRaw);
						var option = node.GetNodeOptionByName(optionId);
						if (option != null && option.TryGetValue<int>(out int ival))
						{
							if (hasStored || ival != 0)
								variables[member.LookupKey] = ival;
						}
						else if (hasStored)
						{
							variables[member.LookupKey] = storedRaw;
						}
						break;
					}
					case FN2MemberType.NodeLookup:
					{
						string portId = FN2EditorNode.NodeLookupPrefix + member.LookupKey;
						var port = node.GetInputPortByName(portId);
						if (port != null && port.isConnected)
						{
							var child = CompileFromPort(port.firstConnectedPort, visited);
							if (child != null)
								nodeLookups[member.LookupKey] = child;
						}
						break;
					}
					case FN2MemberType.Hybrid:
					{
						string portId = FN2EditorNode.HybridPortPrefix + member.LookupKey;
						var port = node.GetInputPortByName(portId);
						if (port != null && port.isConnected)
						{
							var child = CompileFromPort(port.firstConnectedPort, visited);
							if (child != null)
								hybrids[member.LookupKey] = new HybridValue(child);
						}
						else
						{
							string optionId = FN2EditorNode.HybridValuePrefix + member.LookupKey;
							bool hasStored = node.hybridDefaults.TryGetFloat(optionId, out float storedFloat);
							var option = node.GetNodeOptionByName(optionId);
							if (option != null && option.TryGetValue<float>(out float hval))
							{
								int bits = BitConverter.SingleToInt32Bits(hval);
								if (hasStored || bits != 0)
									hybrids[member.LookupKey] = new HybridValue(hval);
							}
							else if (hasStored)
							{
								hybrids[member.LookupKey] = new HybridValue(storedFloat);
							}
						}
						break;
					}
				}
			}

			var descriptor = new NodeDescriptor(node.NodeTypeName, variables, nodeLookups, hybrids);
			visited[node] = descriptor;
			return descriptor;
		}
	}
}
