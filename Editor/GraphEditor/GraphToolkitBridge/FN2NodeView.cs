using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine;
using UnityEngine.UIElements;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Custom node view for FN2 noise nodes. Replaces the default
	/// <see cref="CollapsibleInOutNodeView"/> layout with a unified member list
	/// where port connectors sit inline with value editors.
	/// </summary>
	class FN2NodeView : CollapsibleInOutNodeView
	{
		public static readonly string fn2UssClassName = "fn2-node";
		public static readonly string propertyPortPartName = "fn2-property-port";
		public static readonly string texturePreviewPartName = "fn2-texture-preview";

		protected override void BuildPartList()
		{
			// Title
			PartList.AppendPart(NodeTitlePart.Create(titleIconContainerPartName,
				NodeModel, this, ussClassName));

			// Unified property+port list (replaces NodeOptionsInspector + InOutPortContainerPart)
			PartList.AppendPart(FN2PropertyPortPart.Create(propertyPortPartName,
				Model, this, ussClassName));

			// Texture preview at the bottom
			PartList.AppendPart(FN2TexturePreviewPart.Create(texturePreviewPartName,
				Model, this, ussClassName));
		}

		protected override void PostBuildUI()
		{
			base.PostBuildUI();
			AddToClassList(fn2UssClassName);
			GraphElementHelper.AddStylesheet(this, "FN2NodeView.uss",
				"Packages/com.auburn.fastnoise2/Editor/GraphEditor/GraphToolkitBridge/StyleSheets/");

			ApplyCategoryColoring();
		}

		void ApplyCategoryColoring()
		{
			var userNodeModel = Model as IUserNodeModelImp;
			if (userNodeModel == null)
				return;

			string nodeTypeName = FN2BridgeCallbacks.GetNodeTypeName?.Invoke(userNodeModel.Node);
			if (!FN2NodeCategory.TryGetCategoryColor(nodeTypeName, out var color))
				return;

			var titleContainer = this.Q(className: "ge-node__title-icon-container");
			if (titleContainer == null)
				return;

			titleContainer.style.backgroundColor = color;

			var sheen = new VisualElement();
			sheen.AddToClassList("fn2-node-sheen");
			sheen.style.backgroundImage = FN2NodeCategory.SheenTexture;
			titleContainer.Insert(0, sheen);
		}
	}
}
