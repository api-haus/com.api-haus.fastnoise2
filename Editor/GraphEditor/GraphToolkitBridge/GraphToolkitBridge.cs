using UnityEngine;
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
	}
}
