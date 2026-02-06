using System.Reflection;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Bridge class compiled INTO Unity.GraphToolkit.Editor via .asmref.
	/// Provides access to internal GraphToolkit APIs for programmatic graph mutation.
	/// </summary>
	public static class GraphToolkitBridge
	{
		const string SentinelName = "fn2-graph-customized";

		static readonly string[] k_OverlaysToHide =
		{
			"gtf-blackboard",
			"gtf-options",
			"gtf-error-notifications",
			"gtf-breadcrumbs",
		};

		static readonly FieldInfo s_ItemLibraryHelperField = typeof(GraphView)
			.GetField("m_ItemLibraryHelper", BindingFlags.NonPublic | BindingFlags.Instance);

		static StyleSheet s_WindowStyleSheet;
		static StyleSheet s_MainPreviewStyleSheet;
		static StyleSheet s_TerrainBackgroundStyleSheet;

		public static INode CreateNode(Graph graph, Node node, Vector2 position)
		{
			GraphModelImp impl = graph.m_Implementation;
			var nodeModel = impl.CreateNodeModel(node, position);
			return (INode)nodeModel;
		}

		public static void CreateWire(Graph graph, IPort toPort, IPort fromPort)
		{
			GraphModelImp impl = graph.m_Implementation;
			var toPortModel = (PortModel)toPort;
			var fromPortModel = (PortModel)fromPort;
			impl.CreateWire(toPortModel, fromPortModel);
		}

		/// <summary>
		/// Retrieves the user's <see cref="Node"/> subclass from an <see cref="INode"/>
		/// returned by <see cref="CreateNode"/>. Returns null if the INode is not a user node.
		/// </summary>
		public static Node GetUserNode(INode inode)
		{
			if (inode is IUserNodeModelImp userModel)
				return userModel.Node;
			return null;
		}

		/// <summary>
		/// Retrieves the user's <see cref="Node"/> that owns the given port.
		/// </summary>
		public static Node GetNodeFromPort(IPort port)
		{
			if (port is PortModel portModel && portModel.NodeModel is IUserNodeModelImp userModel)
				return userModel.Node;
			return null;
		}

		/// <summary>
		/// Applies <see cref="FN2BridgeCallbacks.WindowIcon"/> to any open
		/// <see cref="GraphViewEditorWindowImp"/> whose loaded graph is an FN2 graph.
		/// Called from EditorApplication.update in the FN2 editor assembly.
		/// </summary>
		public static void ApplyWindowIcons()
		{
			if (FN2BridgeCallbacks.WindowIcon == null || FN2BridgeCallbacks.IsFN2Graph == null)
				return;

			foreach (var window in Resources.FindObjectsOfTypeAll<GraphViewEditorWindowImp>())
			{
				var tool = window.GraphTool;
				if (tool == null || tool.Icon == FN2BridgeCallbacks.WindowIcon)
					continue;

				if (tool.ToolState?.GraphModel is GraphModelImp graphModel
					&& graphModel.Graph != null
					&& FN2BridgeCallbacks.IsFN2Graph(graphModel.Graph))
				{
					tool.Icon = FN2BridgeCallbacks.WindowIcon;
				}
			}
		}

		/// <summary>
		/// Hides unused overlays, injects window-level USS, and adds a Main Preview
		/// element to any open FN2 graph editor windows. Guarded by a sentinel element
		/// so customizations are applied only once per window.
		/// Called from EditorApplication.update in the FN2 editor assembly.
		/// </summary>
		public static void ApplyWindowCustomizations()
		{
			if (FN2BridgeCallbacks.IsFN2Graph == null)
				return;

			foreach (var window in Resources.FindObjectsOfTypeAll<GraphViewEditorWindowImp>())
			{
				var tool = window.GraphTool;
				if (tool?.ToolState?.GraphModel is not GraphModelImp graphModel
					|| graphModel.Graph == null
					|| !FN2BridgeCallbacks.IsFN2Graph(graphModel.Graph))
					continue;

				var graphView = window.rootVisualElement.Q<GraphView>();
				if (graphView == null)
					continue;

				// Guard: skip if already customized
				if (graphView.Q(SentinelName) != null)
					continue;

				// Replace the item library helper so the add-node menu uses FN2 categories
				s_ItemLibraryHelperField?.SetValue(graphView, new FN2LibraryHelper(graphModel));

				// Hide overlays
				foreach (string overlayId in k_OverlaysToHide)
					HideOverlay(window, overlayId);

				// Inject window-level USS
				if (s_WindowStyleSheet == null)
					s_WindowStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
						"Packages/com.auburn.fastnoise2/Editor/GraphEditor/GraphToolkitBridge/StyleSheets/FN2Window.uss");
				if (s_WindowStyleSheet != null)
					window.rootVisualElement.styleSheets.Add(s_WindowStyleSheet);

				// Inject Terrain Background USS
				if (s_TerrainBackgroundStyleSheet == null)
					s_TerrainBackgroundStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
						"Packages/com.auburn.fastnoise2/Editor/GraphEditor/GraphToolkitBridge/StyleSheets/FN2TerrainBackground.uss");
				if (s_TerrainBackgroundStyleSheet != null)
					window.rootVisualElement.styleSheets.Add(s_TerrainBackgroundStyleSheet);

				// Add Terrain Background on the visual hierarchy (not contentContainer)
				// so it renders behind m_GraphViewContainer (GridBackground + nodes).
				var terrainBackground = new FN2TerrainBackground();
				graphView.hierarchy.Insert(0, terrainBackground);

				// Inject Main Preview USS
				if (s_MainPreviewStyleSheet == null)
					s_MainPreviewStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
						"Packages/com.auburn.fastnoise2/Editor/GraphEditor/GraphToolkitBridge/StyleSheets/FN2MainPreview.uss");
				if (s_MainPreviewStyleSheet != null)
					window.rootVisualElement.styleSheets.Add(s_MainPreviewStyleSheet);

				// Add Main Preview
				var mainPreview = new FN2MainPreview();
				graphView.Add(mainPreview);

				// Add invisible sentinel
				var sentinel = new VisualElement { name = SentinelName };
				sentinel.style.display = DisplayStyle.None;
				graphView.Add(sentinel);
			}
		}

		static void HideOverlay(EditorWindow window, string overlayId)
		{
			if (window.TryGetOverlay(overlayId, out Overlay overlay))
				overlay.displayed = false;
		}
	}
}
