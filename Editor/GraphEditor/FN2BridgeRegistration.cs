using FastNoise2.Generators;
using UnityEditor;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Registers cross-assembly bridge callbacks at editor startup so the
	/// GraphToolkitBridge code (compiled into Unity.GraphToolkit.Editor) can
	/// access FN2 types without direct references.
	/// </summary>
	[InitializeOnLoad]
	static class FN2BridgeRegistration
	{
		static FN2BridgeRegistration()
		{
			FN2BridgeCallbacks.IsFN2Node = node => node is FN2EditorNode;

			FN2BridgeCallbacks.GetNodeTypeName = node =>
				node is FN2EditorNode fn2Node ? fn2Node.NodeTypeName : null;

			FN2BridgeCallbacks.GetMemberInfos = GetMemberInfos;
			FN2BridgeCallbacks.GetNodeDescription = FN2Descriptions.GetNodeDescription;

			FN2BridgeCallbacks.RenderNodePreview = RenderNodePreview;

			FN2BridgeCallbacks.CompileNodeSubtree = node =>
				node is FN2EditorNode fn2Node ? FastNoiseSubtreeCompiler.CompileSubtree(fn2Node) : null;

			FN2BridgeCallbacks.GenerateHeightmapWithFrequency = FastNoiseTerrainPreview.GenerateHeightmap;

			FN2BridgeCallbacks.IsFN2Graph = graph => graph is FastNoiseEditorGraph;

			FN2BridgeCallbacks.CompileFullGraph = graph =>
				FastNoiseGraphCompiler.Compile(graph as FastNoiseEditorGraph);

			FN2BridgeCallbacks.RenderPreviewWithFrequency = FastNoisePreview.RenderPreview;

			FN2BridgeCallbacks.GenerateTerrainHeightmap = FastNoiseTerrainPreview.GenerateHeightmap;
			FN2BridgeCallbacks.BlitTerrain = FastNoiseTerrainPreview.BlitTerrain;

			FN2BridgeCallbacks.LoadPreviewState = graph =>
			{
				if (graph is FastNoiseEditorGraph fn2Graph)
					fn2Graph.LoadPreviewState();
			};
			FN2BridgeCallbacks.SavePreviewState = graph =>
			{
				if (graph is FastNoiseEditorGraph fn2Graph)
					fn2Graph.SavePreviewState();
			};

			FN2BridgeCallbacks.WindowIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
				"Packages/FastNoise2Unity/Editor/Resources/Icons/NoiseGraph_Grey.png");

			FN2BridgeCallbacks.GetAllNodeNames = () => FN2NodeRegistry.AllNodeNames;

			FN2BridgeCallbacks.CreateFN2NodeInstance = nodeTypeName =>
			{
				var node = new FN2GenericNode();
				node.SetNodeTypeName(nodeTypeName);
				return node;
			};

			FN2BridgeCallbacks.GetFN2EditorNodeType = () => typeof(FN2EditorNode);

			FN2BridgeCallbacks.GetNodeCategoryPath = nodeTypeName =>
			{
				try
				{
					var def = FN2NodeRegistry.GetNodeDef(nodeTypeName);
					return def.Groups.Length > 0 ? def.Groups[0] : null;
				}
				catch { return null; }
			};

			FN2BridgeCallbacks.GetNodeCategoryColor = nodeTypeName =>
			{
				try
				{
					var def = FN2NodeRegistry.GetNodeDef(nodeTypeName);
					if (def.Groups.Length > 0)
					{
						string group = def.Groups[0];
						return FN2NodeCategory.GetGroupColor(group);
					}
				}
				catch { }
				return default;
			};

			EditorApplication.update += GraphToolkitBridge.ApplyWindowIcons;
			EditorApplication.update += GraphToolkitBridge.ApplyWindowCustomizations;
		}

		static FN2BridgeMemberInfo[] GetMemberInfos(string nodeTypeName)
		{
			FN2NodeDef def;
			try { def = FN2NodeRegistry.GetNodeDef(nodeTypeName); }
			catch { return null; }

			var infos = new FN2BridgeMemberInfo[def.Members.Length];
			for (int i = 0; i < def.Members.Length; i++)
			{
				var m = def.Members[i];
				var info = new FN2BridgeMemberInfo
				{
					Name = m.Name,
					LookupKey = m.LookupKey,
					Type = (FN2BridgeMemberType)(int)m.Type,
					EnumValues = m.EnumValues,
					Tooltip = FN2Descriptions.GetMemberDescription(nodeTypeName, m.LookupKey),
				};

				// Map member type to port/option IDs used by FN2EditorNode
				switch (m.Type)
				{
					case FN2MemberType.Float:
					case FN2MemberType.Int:
					case FN2MemberType.Enum:
						info.OptionId = FN2EditorNode.VarPrefix + m.LookupKey;
						break;

					case FN2MemberType.NodeLookup:
						info.PortId = FN2EditorNode.NodeLookupPrefix + m.LookupKey;
						break;

					case FN2MemberType.Hybrid:
						info.PortId = FN2EditorNode.HybridPortPrefix + m.LookupKey;
						info.OptionId = FN2EditorNode.HybridValuePrefix + m.LookupKey;
						break;
				}

				infos[i] = info;
			}

			return infos;
		}

		static Texture2D RenderNodePreview(Unity.GraphToolkit.Editor.Node node,
			int width, int height)
		{
			if (node is not FN2EditorNode fn2Node)
				return null;

			string encoded = FastNoiseSubtreeCompiler.CompileSubtree(fn2Node);
			if (string.IsNullOrEmpty(encoded))
				return null;

			return FastNoisePreview.RenderPreview(encoded, width, height, FN2BridgeCallbacks.PreviewFrequency);
		}
	}
}
