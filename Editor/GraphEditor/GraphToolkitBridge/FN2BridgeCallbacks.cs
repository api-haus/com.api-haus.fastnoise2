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
	}

	/// <summary>
	/// Static delegate callbacks for cross-assembly communication between the
	/// GraphToolkitBridge (compiled into Unity.GraphToolkit.Editor) and the
	/// FN2 editor assembly. Registered at editor startup via [InitializeOnLoad].
	/// </summary>
	public static class FN2BridgeCallbacks
	{
		/// <summary>
		/// Returns true if the given Node is an FN2 editor node.
		/// </summary>
		public static Func<Unity.GraphToolkit.Editor.Node, bool> IsFN2Node;

		/// <summary>
		/// Returns the FN2 node type name (e.g. "Simplex") for a given node.
		/// </summary>
		public static Func<Unity.GraphToolkit.Editor.Node, string> GetNodeTypeName;

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
		/// Optional icon to display in the graph editor window title bar.
		/// Set by the FN2 editor assembly at startup.
		/// </summary>
		public static Texture2D WindowIcon;
	}
}
