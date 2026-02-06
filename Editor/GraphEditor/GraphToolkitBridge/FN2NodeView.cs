using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
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

			// Move output port into preview container for overlay positioning
			var outputRow = this.Q(className: "fn2-member-row--output");
			var preview = this.Q(className: "fn2-texture-preview-part");
			if (outputRow != null && preview != null)
			{
				outputRow.RemoveFromHierarchy();
				preview.Add(outputRow);
			}
		}
	}
}
