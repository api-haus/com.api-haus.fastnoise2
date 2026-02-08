using System;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Member type for the bridge (mirrors FN2MemberType but decoupled from Runtime assembly).
	/// Int values must match <c>FN2MemberType</c>.
	/// </summary>
	public enum FN2BridgeMemberType
	{
		Float = 0,
		Int = 1,
		Enum = 2,
		NodeLookup = 3,
		Hybrid = 4,
	}

	/// <summary>
	/// Metadata for a single node member, passed across the assembly boundary.
	/// </summary>
	public struct FN2BridgeMemberInfo
	{
		public string Name;
		public string LookupKey;
		public FN2BridgeMemberType Type;
		public string PortId;
		public string OptionId;
		public string[] EnumValues;
		public string Tooltip;
	}

	/// <summary>
	/// Static delegate callbacks for cross-assembly communication between the
	/// GraphToolkitBridge (compiled into Unity.GraphToolkit.Editor) and the
	/// FN2 editor assembly. Registered at editor startup via [InitializeOnLoad].
	/// </summary>
	public static class FN2BridgeCallbacks
	{
		#region Preview State

		/// <summary>
		/// Currently previewed node (null = use output node).
		/// Set by the preview button or P hotkey on a node view.
		/// </summary>
		public static Unity.GraphToolkit.Editor.Node PreviewOverrideNode;

		/// <summary>
		/// Shared preview frequency/scale used by both the main preview and node previews.
		/// Adjusted by scroll-wheel on the main preview element.
		/// </summary>
		public static float PreviewFrequency = 0.02f;

		/// <summary>
		/// Height scale for the 3D heightfield preview shader.
		/// Adjusted by the vertical slider in the main preview.
		/// </summary>
		public static float HeightScale = 0.15f;

		/// <summary>
		/// Pan offset in noise space, applied as xOffset/yOffset when generating noise.
		/// Adjusted by left-mouse-button drag on the preview.
		/// </summary>
		public static Vector2 PanOffset = Vector2.zero;

		/// <summary>
		/// Camera yaw angle (radians) for 3D heightfield orbit.
		/// Controlled by horizontal hover position on the preview.
		/// </summary>
		public static float CameraYaw = 0f;

		/// <summary>
		/// Camera pitch angle (radians) for 3D heightfield orbit.
		/// Controlled by vertical hover position on the preview.
		/// </summary>
		public static float CameraPitch = 0.7f;

		/// <summary>
		/// Preview mode as int for cross-assembly access (0 = Texture, 1 = Heightfield).
		/// Bridge code syncs this with FN2PreviewWidget.Mode.
		/// </summary>
		public static int PreviewModeValue;

		/// <summary>
		/// Preview size in pixels for cross-assembly access.
		/// Synced by FN2MainPreview on resize and graph load/save.
		/// </summary>
		public static int PreviewSizeValue = 128;

		#endregion

		/// <summary>
		/// Compiles the subtree rooted at the given node to an encoded string.
		/// Returns null on failure.
		/// </summary>
		public static Func<Unity.GraphToolkit.Editor.Node, string> CompileNodeSubtree;

		/// <summary>
		/// Generates an RFloat heightmap at the specified size and frequency from an encoded tree.
		/// Parameters: encoded, width, height, frequency. Returns null on failure.
		/// </summary>
		public static Func<string, int, int, float, Texture2D> GenerateHeightmapWithFrequency;

		/// <summary>
		/// Returns true if the given Node is an FN2 editor node.
		/// </summary>
		public static Func<Unity.GraphToolkit.Editor.Node, bool> IsFN2Node;

		/// <summary>
		/// Returns the FN2 node type name (e.g. "Simplex") for a given node.
		/// </summary>
		public static Func<Unity.GraphToolkit.Editor.Node, string> GetNodeTypeName;

		/// <summary>
		/// Returns a tooltip description for the given node type name, or null if none.
		/// </summary>
		public static Func<string, string> GetNodeDescription;

		/// <summary>
		/// Returns ordered member metadata for a given node type name.
		/// </summary>
		public static Func<string, FN2BridgeMemberInfo[]> GetMemberInfos;

		/// <summary>
		/// Compiles the subtree rooted at the given node and renders a noise preview.
		/// Parameters: node, width, height. Returns null on failure.
		/// </summary>
		public static Func<Unity.GraphToolkit.Editor.Node, int, int, Texture2D> RenderNodePreview;

		/// <summary>
		/// Returns true if the given Graph is an FN2 editor graph.
		/// </summary>
		public static Func<Unity.GraphToolkit.Editor.Graph, bool> IsFN2Graph;

		/// <summary>
		/// Compiles the entire graph from its output node to an encoded string.
		/// Parameter: graph. Returns null on failure.
		/// </summary>
		public static Func<Unity.GraphToolkit.Editor.Graph, string> CompileFullGraph;

		/// <summary>
		/// Renders a noise preview from an encoded string with custom frequency.
		/// Parameters: encoded, width, height, frequency. Returns null on failure.
		/// </summary>
		public static Func<string, int, int, float, Texture2D> RenderPreviewWithFrequency;

		/// <summary>
		/// Generates a 512x512 RFloat heightmap from an encoded node tree for terrain preview.
		/// Parameter: encoded string. Returns null on failure.
		/// </summary>
		public static Func<string, Texture2D> GenerateTerrainHeightmap;

		/// <summary>
		/// Blits a heightmap through the terrain raymarching shader into the target RenderTexture.
		/// Parameters: heightmap, target RenderTexture.
		/// </summary>
		public static Action<Texture2D, RenderTexture> BlitTerrain;

		/// <summary>
		/// Loads persisted preview state from the given graph into the static fields.
		/// Called when a graph is first opened in the preview.
		/// </summary>
		public static Action<Unity.GraphToolkit.Editor.Graph> LoadPreviewState;

		/// <summary>
		/// Saves current preview state from the static fields into the given graph.
		/// Called when preview settings change or the editor closes.
		/// </summary>
		public static Action<Unity.GraphToolkit.Editor.Graph> SavePreviewState;

		/// <summary>
		/// Optional icon to display in the graph editor window title bar.
		/// Set by the FN2 editor assembly at startup.
		/// </summary>
		public static Texture2D WindowIcon;

		/// <summary>
		/// Returns all registered FN2 node type names from the native metadata registry.
		/// </summary>
		public static Func<string[]> GetAllNodeNames;

		/// <summary>
		/// Creates an FN2 Node instance for the given node type name.
		/// The returned Node will be passed to UserNodeModelImp.InitCustomNode.
		/// </summary>
		public static Func<string, Unity.GraphToolkit.Editor.Node> CreateFN2NodeInstance;

		/// <summary>
		/// Returns the Type object for FN2EditorNode (or its subclass used for generic nodes).
		/// Used by the library helper to check SupportedNodes filtering.
		/// </summary>
		public static Func<Type> GetFN2EditorNodeType;

		/// <summary>
		/// Tries to get the category path (group name) for a node type.
		/// Parameters: nodeTypeName. Returns category path or null.
		/// </summary>
		public static Func<string, string> GetNodeCategoryPath;

		/// <summary>
		/// Tries to get the category color for a node type.
		/// Parameters: nodeTypeName. Returns Color with alpha > 0 on success, default on failure.
		/// </summary>
		public static Func<string, Color> GetNodeCategoryColor;
	}
}
