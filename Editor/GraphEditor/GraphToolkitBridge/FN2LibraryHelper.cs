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

			foreach (var nodeType in ((GraphModelImp)GraphModel).SupportedNodes)
			{
				bool isContextNode = typeof(ContextNode).IsAssignableFrom(nodeType);

				string categoryPath;
				if (isContextNode)
					categoryPath = "Contexts";
				else if (FN2NodeCategory.TryGetCategoryPath(nodeType.Name, out var path))
					categoryPath = path;
				else
					categoryPath = "Nodes";

				var nodeDef = new GraphNodeModelLibraryItem(
					nodeType.Name,
					new NodeItemLibraryData(nodeType),
					d => isContextNode
						? GraphModelImp.CreateContextNodeFromData(d, nodeType)
						: GraphModelImp.CreateNodeFromData(d, nodeType))
				{
					CategoryPath = categoryPath,
					Synonyms = FN2NodeCategory.GetSynonyms(nodeType.Name)
				};

				db.Items.Add(nodeDef);
			}

			return db;
		}
	}
}
