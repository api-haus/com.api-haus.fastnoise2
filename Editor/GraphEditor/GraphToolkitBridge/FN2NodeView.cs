using System.Linq;
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
				"Packages/FastNoise2Unity/Editor/GraphEditor/GraphToolkitBridge/StyleSheets/");

			ApplyCategoryColoring();
			ApplyNodeTooltip();
			AddPreviewButton();
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

		void ApplyNodeTooltip()
		{
			if (FN2BridgeCallbacks.GetNodeDescription == null)
				return;

			var userNodeModel = Model as IUserNodeModelImp;
			if (userNodeModel == null)
				return;

			string nodeTypeName = FN2BridgeCallbacks.GetNodeTypeName?.Invoke(userNodeModel.Node);
			if (string.IsNullOrEmpty(nodeTypeName))
				return;

			string desc = FN2BridgeCallbacks.GetNodeDescription(nodeTypeName);
			if (string.IsNullOrEmpty(desc))
				return;

			var titleContainer = this.Q(className: "ge-node__title-icon-container");
			if (titleContainer != null)
				titleContainer.tooltip = desc;
		}

		void AddPreviewButton()
		{
			var titleContainer = this.Q(className: "ge-node__title-icon-container");
			if (titleContainer == null)
				return;

			var previewBtn = new Button(OnPreviewClicked);
			previewBtn.AddToClassList("fn2-preview-button");
			previewBtn.text = "P";
			previewBtn.tooltip = "Preview this node (P)";
			titleContainer.Add(previewBtn);
		}

		void OnPreviewClicked()
		{
			var userNodeModel = Model as IUserNodeModelImp;
			if (userNodeModel == null)
				return;

			var node = userNodeModel.Node;
			if (FN2BridgeCallbacks.PreviewOverrideNode == node)
				FN2BridgeCallbacks.PreviewOverrideNode = null;
			else
				FN2BridgeCallbacks.PreviewOverrideNode = node;

			FN2EditorUpdate.NotifyGraphChanged();

			var graphView = GetFirstAncestorOfType<GraphView>();
			if (graphView != null)
				UpdatePreviewHighlights(graphView);
		}

		/// <summary>
		/// Updates the <c>fn2-node--previewing</c> class on all FN2 node views
		/// in the given graph view to reflect the current preview override.
		/// </summary>
		public static void UpdatePreviewHighlights(GraphView graphView)
		{
			foreach (var nodeView in graphView.Query<FN2NodeView>().ToList())
			{
				var userModel = nodeView.Model as IUserNodeModelImp;
				if (userModel != null && userModel.Node == FN2BridgeCallbacks.PreviewOverrideNode)
					nodeView.AddToClassList("fn2-node--previewing");
				else
					nodeView.RemoveFromClassList("fn2-node--previewing");
			}
		}
	}
}
