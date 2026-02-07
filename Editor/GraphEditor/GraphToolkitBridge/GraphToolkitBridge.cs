using System.Linq;
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
		static StyleSheet s_PreviewWidgetStyleSheet;

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

				// Always hide unwanted overlays and toolbar toggles (cheap,
				// idempotent — runs outside the sentinel guard to survive
				// overlay-state restoration after domain reloads)
				foreach (string overlayId in k_OverlaysToHide)
					HideOverlay(window, overlayId);

				var bbToggle = window.rootVisualElement.Q(name: "Blackboard");
				if (bbToggle != null)
					bbToggle.style.display = DisplayStyle.None;

				var graphView = window.rootVisualElement.Q<GraphView>();
				if (graphView == null)
					continue;

				// Guard: skip if already customized
				if (graphView.Q(SentinelName) != null)
					continue;

				// Replace the item library helper so the add-node menu uses FN2 categories
				s_ItemLibraryHelperField?.SetValue(graphView, new FN2LibraryHelper(graphModel));

				// Inject window-level USS
				LoadAndApplyStyleSheet(ref s_WindowStyleSheet,
					"Packages/com.auburn.fastnoise2/Editor/GraphEditor/GraphToolkitBridge/StyleSheets/FN2Window.uss",
					window.rootVisualElement);

				// Inject Terrain Background USS
				LoadAndApplyStyleSheet(ref s_TerrainBackgroundStyleSheet,
					"Packages/com.auburn.fastnoise2/Editor/GraphEditor/GraphToolkitBridge/StyleSheets/FN2TerrainBackground.uss",
					window.rootVisualElement);

				// Add Terrain Background on the visual hierarchy (not contentContainer)
				// so it renders behind m_GraphViewContainer (GridBackground + nodes).
				var terrainBackground = new FN2TerrainBackground();
				graphView.hierarchy.Insert(0, terrainBackground);

				// Inject Main Preview USS
				LoadAndApplyStyleSheet(ref s_MainPreviewStyleSheet,
					"Packages/com.auburn.fastnoise2/Editor/GraphEditor/GraphToolkitBridge/StyleSheets/FN2MainPreview.uss",
					window.rootVisualElement);

				// Inject Preview Widget USS
				LoadAndApplyStyleSheet(ref s_PreviewWidgetStyleSheet,
					"Packages/com.auburn.fastnoise2/Editor/GraphEditor/GraphToolkitBridge/StyleSheets/FN2PreviewWidget.uss",
					window.rootVisualElement);

				// Add Main Preview
				var mainPreview = new FN2MainPreview();
				graphView.Add(mainPreview);

				// Register P hotkey for preview override
				graphView.RegisterCallback<KeyDownEvent>(OnPreviewKeyDown, TrickleDown.TrickleDown);

				// Add invisible sentinel
				var sentinel = new VisualElement { name = SentinelName };
				sentinel.style.display = DisplayStyle.None;
				graphView.Add(sentinel);
			}
		}

		static void OnPreviewKeyDown(KeyDownEvent evt)
		{
			if (evt.keyCode != KeyCode.P)
				return;

			var graphView = evt.currentTarget as GraphView;
			if (graphView == null)
				return;

			// Find first selected FN2 node
			var selected = graphView.GetSelection()
				.OfType<ModelView>()
				.Select(v => (v as NodeView)?.NodeModel)
				.OfType<IUserNodeModelImp>()
				.Where(n => FN2BridgeCallbacks.IsFN2Node?.Invoke(n.Node) == true)
				.FirstOrDefault();

			if (selected == null)
				return;

			// Toggle preview
			if (FN2BridgeCallbacks.PreviewOverrideNode == selected.Node)
				FN2BridgeCallbacks.PreviewOverrideNode = null;
			else
				FN2BridgeCallbacks.PreviewOverrideNode = selected.Node;

			FN2NodeView.UpdatePreviewHighlights(graphView);
			FN2EditorUpdate.NotifyGraphChanged();

			evt.StopPropagation();
		}

		static void LoadAndApplyStyleSheet(ref StyleSheet cached, string path, VisualElement target)
		{
			if (cached == null)
				cached = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
			if (cached != null)
				target.styleSheets.Add(cached);
		}

		static void HideOverlay(EditorWindow window, string overlayId)
		{
			if (window.TryGetOverlay(overlayId, out Overlay overlay))
				overlay.displayed = false;
		}
	}
}
