using System;
using System.Collections.Generic;
using System.Linq;
using FastNoise2.Bindings;
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

			var visited = new Dictionary<FastNoiseEditorNode, NodeDescriptor>();
			var descriptor = CompileFromPort(sourcePort.firstConnectedPort, visited);
			if (descriptor == null)
			{
				error = "Failed to compile connected node.";
				return null;
			}

			return NodeEncoder.Encode(descriptor);
		}

		static NodeDescriptor CompileFromPort(IPort outputPort,
			Dictionary<FastNoiseEditorNode, NodeDescriptor> visited)
		{
			var ownerNode = GraphToolkitBridge.GetNodeFromPort(outputPort) as FastNoiseEditorNode;
			if (ownerNode == null)
				return null;

			if (visited.TryGetValue(ownerNode, out var existing))
				return existing;

			return CompileEditorNode(ownerNode, visited);
		}

		static NodeDescriptor CompileEditorNode(FastNoiseEditorNode node,
			Dictionary<FastNoiseEditorNode, NodeDescriptor> visited)
		{
#if FN2_USER_SIGNED
			FastNoise.Metadata meta;
			try { meta = FastNoise.GetNodeMetadata(node.nodeTypeName); }
			catch { return null; }

			var variables = new Dictionary<string, int>();
			var nodeLookups = new Dictionary<string, NodeDescriptor>();
			var hybrids = new Dictionary<string, HybridValue>();

			foreach (var kv in meta.members)
			{
				var member = kv.Value;
				switch (member.type)
				{
					case FastNoise.Metadata.Member.Type.Float:
					{
						string optionId = FastNoiseEditorNode.VarPrefix + kv.Key;
						bool hasStored = node.variableValues.TryGetInt(optionId, out int storedRaw);
						var option = node.GetNodeOptionByName(optionId);
						if (option != null && option.TryGetValue<float>(out float fval))
						{
							int bits = BitConverter.SingleToInt32Bits(fval);
							if (hasStored || bits != 0)
								variables[kv.Key] = bits;
						}
						else if (hasStored)
						{
							variables[kv.Key] = storedRaw;
						}
						break;
					}
					case FastNoise.Metadata.Member.Type.Int:
					case FastNoise.Metadata.Member.Type.Enum:
					{
						string optionId = FastNoiseEditorNode.VarPrefix + kv.Key;
						bool hasStored = node.variableValues.TryGetInt(optionId, out int storedRaw);
						var option = node.GetNodeOptionByName(optionId);
						if (option != null && option.TryGetValue<int>(out int ival))
						{
							if (hasStored || ival != 0)
								variables[kv.Key] = ival;
						}
						else if (hasStored)
						{
							variables[kv.Key] = storedRaw;
						}
						break;
					}
					case FastNoise.Metadata.Member.Type.NodeLookup:
					{
						string portId = FastNoiseEditorNode.NodeLookupPrefix + kv.Key;
						var port = node.GetInputPortByName(portId);
						if (port != null && port.isConnected)
						{
							var child = CompileFromPort(port.firstConnectedPort, visited);
							if (child != null)
								nodeLookups[kv.Key] = child;
						}
						break;
					}
					case FastNoise.Metadata.Member.Type.Hybrid:
					{
						string portId = FastNoiseEditorNode.HybridPortPrefix + kv.Key;
						var port = node.GetInputPortByName(portId);
						if (port != null && port.isConnected)
						{
							var child = CompileFromPort(port.firstConnectedPort, visited);
							if (child != null)
								hybrids[kv.Key] = new HybridValue(child);
						}
						else
						{
							string optionId = FastNoiseEditorNode.HybridValuePrefix + kv.Key;
							bool hasStored = node.hybridDefaults.TryGetFloat(optionId, out float storedFloat);
							var option = node.GetNodeOptionByName(optionId);
							if (option != null && option.TryGetValue<float>(out float hval))
							{
								int bits = BitConverter.SingleToInt32Bits(hval);
								if (hasStored || bits != 0)
									hybrids[kv.Key] = new HybridValue(hval);
							}
							else if (hasStored)
							{
								hybrids[kv.Key] = new HybridValue(storedFloat);
							}
						}
						break;
					}
				}
			}

			var descriptor = new NodeDescriptor(node.nodeTypeName, variables, nodeLookups, hybrids);
			visited[node] = descriptor;
			return descriptor;
#else
			return null;
#endif
		}
	}
}
