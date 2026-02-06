using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine.UIElements;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Factory extension that creates <see cref="FN2NodeView"/> for FN2 editor nodes
	/// and <see cref="Wire"/> views with connectivity callbacks for auto-refresh.
	/// Targets <see cref="UserNodeModelImp"/> / <see cref="WireModel"/> at toolDefaultPriority.
	/// </summary>
	[GraphElementsExtensionMethodsCache(typeof(GraphView),
		GraphElementsExtensionMethodsCacheAttribute.toolDefaultPriority)]
	static class FN2ViewFactory
	{
		public static ModelView CreateNode(this ElementBuilder elementBuilder,
			UserNodeModelImp model)
		{
			ModelView ui;

			if (FN2BridgeCallbacks.IsFN2Node != null && FN2BridgeCallbacks.IsFN2Node(model.Node))
			{
				ui = new FN2NodeView();
			}
			else
			{
				ui = new CollapsibleInOutNodeView();
			}

			ui.SetupBuildAndUpdate(model, elementBuilder.View, elementBuilder.Context);
			return ui;
		}

		/// <summary>
		/// Factory for wire views. When a wire connects to an FN2 node, registers
		/// panel attach/detach callbacks that schedule deferred node view updates
		/// so hybrid editors and texture previews refresh immediately.
		/// </summary>
		public static ModelView CreateWire(this ElementBuilder elementBuilder, WireModel model)
		{
			var ui = new Wire();
			ui.SetupBuildAndUpdate(model, elementBuilder.View, elementBuilder.Context);

			if (!HasFN2Endpoint(model))
				return ui;

			var rootView = elementBuilder.View;

			ui.RegisterCallback<AttachToPanelEvent>(_ =>
			{
				ScheduleConnectedNodeUpdates(rootView, model);
			});

			ui.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				ScheduleConnectedNodeUpdates(rootView, model);
			});

			return ui;
		}

		static bool HasFN2Endpoint(WireModel wireModel)
		{
			if (FN2BridgeCallbacks.IsFN2Node == null)
				return false;

			var toNode = wireModel.ToPort?.NodeModel as IUserNodeModelImp;
			var fromNode = wireModel.FromPort?.NodeModel as IUserNodeModelImp;

			return (toNode != null && FN2BridgeCallbacks.IsFN2Node(toNode.Node))
				|| (fromNode != null && FN2BridgeCallbacks.IsFN2Node(fromNode.Node));
		}

		static void ScheduleConnectedNodeUpdates(RootView rootView, WireModel wireModel)
		{
			// Defer to avoid interfering with the ongoing PartialUpdate cycle
			rootView.schedule.Execute(() =>
			{
				var toNodeModel = wireModel.ToPort?.NodeModel;
				if (toNodeModel == null)
					return;

				var graphModel = toNodeModel.GraphModel;
				var visited = new HashSet<AbstractNodeModel>();

				UpdateDownstreamFN2Nodes(rootView, toNodeModel, graphModel, visited);
			}).ExecuteLater(0);
		}

		static void UpdateDownstreamFN2Nodes(RootView rootView,
			AbstractNodeModel startNode, GraphModel graphModel,
			HashSet<AbstractNodeModel> visited)
		{
			var queue = new Queue<AbstractNodeModel>();
			queue.Enqueue(startNode);
			visited.Add(startNode);

			while (queue.Count > 0)
			{
				var node = queue.Dequeue();

				// Update this node's view if it's an FN2 node
				if (node is IUserNodeModelImp userNode
					&& FN2BridgeCallbacks.IsFN2Node != null
					&& FN2BridgeCallbacks.IsFN2Node(userNode.Node))
				{
					var nodeView = node.GetView<ModelView>(rootView);
					nodeView?.UpdateView(UpdateFromModelVisitor.genericUpdateFromModelVisitor);
				}

				// Enqueue downstream nodes via output ports
				if (node is not InputOutputPortsNodeModel ioNode)
					continue;

				foreach (var outputPort in ioNode.OutputPorts)
				{
					foreach (var wire in outputPort.GetConnectedWires())
					{
						var downstream = wire.ToPort?.NodeModel;
						if (downstream != null && visited.Add(downstream))
							queue.Enqueue(downstream);
					}
				}
			}
		}
	}
}
