using System;
using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;

namespace FastNoise2.Editor.GraphEditor
{
	class FN2LibraryHelper : PublicLibraryHelper
	{
		public FN2LibraryHelper(GraphModel graphModel) : base(graphModel) { }

		public override IItemDatabaseProvider GetItemDatabaseProvider()
		{
			return m_DatabaseProvider ??= new FN2DatabaseProvider(GraphModel);
		}
	}

	class FN2DatabaseProvider : PublicDatabaseProviderImp
	{
		public FN2DatabaseProvider(GraphModel graphModel) : base(graphModel) { }

		protected override GraphElementItemDatabase InitialGraphElementDatabase()
		{
			var db = new PublicGraphElementItemDatabase(GraphModel);

			// Context nodes from SupportedNodes (e.g. FastNoiseOutputNode)
			foreach (var nodeType in ((GraphModelImp)GraphModel).SupportedNodes)
			{
				if (!typeof(ContextNode).IsAssignableFrom(nodeType))
					continue;

				var nodeDef = new GraphNodeModelLibraryItem(
					nodeType.Name,
					new NodeItemLibraryData(nodeType),
					d => GraphModelImp.CreateContextNodeFromData(d, nodeType))
				{
					CategoryPath = "Contexts"
				};

				db.Items.Add(nodeDef);
			}

			// FN2 noise nodes from native metadata registry
			var allNodeNames = FN2BridgeCallbacks.GetAllNodeNames?.Invoke();
			if (allNodeNames == null)
				return db;

			var fn2NodeType = FN2BridgeCallbacks.GetFN2EditorNodeType?.Invoke();

			foreach (string nodeName in allNodeNames)
			{
				string captured = nodeName;

				string categoryPath;
				if (FN2NodeCategory.TryGetCategoryPath(captured, out var path))
					categoryPath = path;
				else
					categoryPath = "Nodes";

				var nodeDef = new GraphNodeModelLibraryItem(
					captured,
					new NodeItemLibraryData(fn2NodeType ?? typeof(Node)),
					d =>
					{
						var customNode = FN2BridgeCallbacks.CreateFN2NodeInstance(captured);
						return d.CreateNode(typeof(FN2UserNodeModelImp), string.Empty,
							n => ((FN2UserNodeModelImp)n).InitCustomNode(customNode));
					})
				{
					CategoryPath = categoryPath,
					Synonyms = FN2NodeCategory.GetSynonyms(captured)
				};

				db.Items.Add(nodeDef);
			}

			// Non-context, non-FN2 nodes from SupportedNodes (if any)
			foreach (var nodeType in ((GraphModelImp)GraphModel).SupportedNodes)
			{
				if (typeof(ContextNode).IsAssignableFrom(nodeType))
					continue;
				if (fn2NodeType != null && fn2NodeType.IsAssignableFrom(nodeType))
					continue;

				var nodeDef = new GraphNodeModelLibraryItem(
					nodeType.Name,
					new NodeItemLibraryData(nodeType),
					d => GraphModelImp.CreateNodeFromData(d, nodeType))
				{
					CategoryPath = "Nodes"
				};

				db.Items.Add(nodeDef);
			}

			return db;
		}
	}
}
