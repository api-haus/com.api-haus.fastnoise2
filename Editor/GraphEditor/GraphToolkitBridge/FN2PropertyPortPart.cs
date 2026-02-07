using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
using Unity.GraphToolkit.Editor.Implementation;
using UnityEngine;
using UnityEngine.UIElements;


namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Unified member list that replaces both <c>NodeOptionsInspector</c> and
	/// <c>InOutPortContainerPart</c>. Each FN2 member gets a single row containing
	/// its port connector (if applicable) and value editor (if applicable).
	/// For hybrid members, the value editor hides when a wire is connected.
	///
	/// Extends <see cref="GraphViewPart"/> (not <c>BaseModelViewPart</c>) so that
	/// <c>GraphElement.EnableCulling</c> routes through <see cref="SetCullingState"/>
	/// instead of directly ripping the Root from the hierarchy. This lets us cache
	/// port positions before removal, which <c>Port.GetGlobalCenter()</c> needs
	/// to return correct wire endpoints while the node is culled off-screen.
	/// </summary>
	class FN2PropertyPortPart : GraphViewPart
	{
		public static readonly string ussClassName = "fn2-property-port-part";
		public static readonly string rowUssClassName = "fn2-member-row";
		public static readonly string rowConnectedUssClassName = "fn2-member-row--connected";
		public static readonly string portSlotUssClassName = "fn2-member-port-slot";
		public static readonly string labelUssClassName = "fn2-member-label";
		public static readonly string editorSlotUssClassName = "fn2-member-editor-slot";

		// Approximate character width for estimating node min-width
		const float CharWidth = 8.5f;
		const float BaseOverhead = 150f; // port connector + value field + margins

		VisualElement m_Root;
		readonly List<MemberRowState> m_Rows = new();
		Port m_OutputPortView;

		struct MemberRowState
		{
			public VisualElement Row;
			public FN2BridgeMemberInfo MemberInfo;
			public PortModel PortModel;      // null for pure variables
			public Port PortView;            // null for pure variables
			public VisualElement EditorSlot;  // null for pure node lookups
			public BaseModelPropertyField Editor; // null for pure node lookups
		}

		public static FN2PropertyPortPart Create(string name, Model model,
			ChildView ownerElement, string parentClassName)
		{
			return new FN2PropertyPortPart(name, model, ownerElement, parentClassName);
		}

		FN2PropertyPortPart(string name, Model model, ChildView ownerElement,
			string parentClassName)
			: base(name, model, ownerElement, parentClassName) { }

		public override VisualElement Root => m_Root;

		protected override void BuildUI(VisualElement parent)
		{
			m_Root = new VisualElement();
			m_Root.AddToClassList(ussClassName);
			parent.Add(m_Root);

			BuildRows();
		}

		void BuildRows()
		{
			m_Root.Clear();
			m_Rows.Clear();
			m_OutputPortView = null;

			var nodeModel = m_Model as InputOutputPortsNodeModel;
			if (nodeModel == null)
				return;

			var userNodeModel = nodeModel as IUserNodeModelImp;
			if (userNodeModel == null)
				return;

			string nodeTypeName = FN2BridgeCallbacks.GetNodeTypeName?.Invoke(userNodeModel.Node);
			if (string.IsNullOrEmpty(nodeTypeName))
				return;

			var memberInfos = FN2BridgeCallbacks.GetMemberInfos?.Invoke(nodeTypeName);
			if (memberInfos == null)
				return;

			var ownerView = (ModelView)m_OwnerElement;

			// Compute min-width from the longest member name and apply to the node view
			float maxNameLen = 6f; // "Output" baseline
			foreach (var member in memberInfos)
			{
				if (member.Name.Length > maxNameLen)
					maxNameLen = member.Name.Length;
			}
			float estimatedMinWidth = Mathf.Max(200f, maxNameLen * CharWidth + BaseOverhead);
			// Apply on the node view itself so it actually constrains the node width
			((VisualElement)m_OwnerElement).style.minWidth = estimatedMinWidth;

			// Create the output port view but don't add it to any row yet —
			// it will be placed inline on the first NodeLookup row (or first row as fallback).
			VisualElement outputPortSlot = null;
			if (nodeModel.OutputsById.TryGetValue("output", out var outputPortModel))
			{
				var portView = ModelViewFactory.CreateUI<Port>(
					ownerView.RootView, outputPortModel);
				if (portView != null)
				{
					portView.AddToClassList("fn2-output-port");
					m_OutputPortView = portView;
					outputPortSlot = new VisualElement();
					outputPortSlot.AddToClassList(portSlotUssClassName);
					outputPortSlot.Add(portView);
				}
			}

			bool outputPlaced = false;
			VisualElement firstRow = null;

			// Build input member rows in metadata order
			foreach (var member in memberInfos)
			{
				var row = new VisualElement();
				row.AddToClassList(rowUssClassName);

				var state = new MemberRowState
				{
					Row = row,
					MemberInfo = member,
				};

				switch (member.Type)
				{
					case FN2BridgeMemberType.NodeLookup:
					{
						// Port connector only — the Port view has its own label
						CreatePortSlot(member, nodeModel, ownerView, row, ref state);
						break;
					}

					case FN2BridgeMemberType.Hybrid:
					{
						// Port connector (label hidden — the value editor provides it for drag support)
						CreatePortSlot(member, nodeModel, ownerView, row, ref state);
						if (state.PortView != null)
						{
							state.PortView.AddToClassList("fn2-hybrid-port");
							if (state.PortView.Label != null)
								state.PortView.Label.style.display = DisplayStyle.None;
						}

						CreateEditorSlot(member, userNodeModel, ownerView, row, ref state);
						break;
					}

					case FN2BridgeMemberType.Float:
					case FN2BridgeMemberType.Int:
					case FN2BridgeMemberType.Enum:
					{
						CreateEditorSlot(member, userNodeModel, ownerView, row, ref state);
						break;
					}
				}

				m_Root.Add(row);
				m_Rows.Add(state);

				if (firstRow == null)
					firstRow = row;

				// Place output port on the first NodeLookup row
				if (!outputPlaced && outputPortSlot != null &&
					member.Type == FN2BridgeMemberType.NodeLookup)
				{
					var spacer = new VisualElement();
					spacer.AddToClassList("fn2-member-spacer");
					row.Add(spacer);
					row.Add(outputPortSlot);
					outputPlaced = true;
				}
			}

			// Fallback: place output port on the first row if no NodeLookup was found
			if (!outputPlaced && outputPortSlot != null && firstRow != null)
			{
				var spacer = new VisualElement();
				spacer.AddToClassList("fn2-member-spacer");
				firstRow.Add(spacer);
				firstRow.Add(outputPortSlot);
			}
		}

		void CreatePortSlot(FN2BridgeMemberInfo member, InputOutputPortsNodeModel nodeModel,
			ModelView ownerView, VisualElement row, ref MemberRowState state)
		{
			if (member.PortId == null ||
				!nodeModel.InputsById.TryGetValue(member.PortId, out var portModel))
				return;

			state.PortModel = portModel;
			var portView = ModelViewFactory.CreateUI<Port>(ownerView.RootView, portModel);
			state.PortView = portView;
			if (portView != null)
			{
				var portSlot = new VisualElement();
				portSlot.AddToClassList(portSlotUssClassName);
				portSlot.Add(portView);
				row.Add(portSlot);
			}
		}

		void CreateEditorSlot(FN2BridgeMemberInfo member, IUserNodeModelImp userNodeModel,
			ModelView ownerView, VisualElement row, ref MemberRowState state)
		{
			var editorSlot = new VisualElement();
			editorSlot.AddToClassList(editorSlotUssClassName);
			if (member.OptionId != null)
			{
				var option = userNodeModel.GetNodeOptionByName(member.OptionId)
					as NodeOption;
				if (option?.PortModel?.EmbeddedValue != null)
				{
					var constants = new List<Constant> { option.PortModel.EmbeddedValue };
					var ownerModels = new List<GraphElementModel> { option.PortModel };

					BaseModelPropertyField editor;
					if (member.Type == FN2BridgeMemberType.Enum &&
						member.EnumValues is { Length: > 0 })
					{
						editor = new FN2EnumField(ownerView.RootView, constants,
							ownerModels, member.EnumValues, member.Name);
					}
					else
					{
						editor = InlineValueEditor.CreateEditorForConstants(
							ownerView.RootView, ownerModels, constants, member.Name);
					}

					if (editor != null)
					{
						editorSlot.Add(editor);
						editor.UpdateDisplayedValue();
						state.Editor = editor;
					}
				}
			}

			if (!string.IsNullOrEmpty(member.Tooltip))
				editorSlot.tooltip = member.Tooltip;

			state.EditorSlot = editorSlot;
			row.Add(editorSlot);
		}

		public override void UpdateUIFromModel(UpdateFromModelVisitor visitor)
		{
			// Refresh all editor fields from their constant values
			foreach (var state in m_Rows)
			{
				state.Editor?.UpdateDisplayedValue();

				if (state.MemberInfo.Type != FN2BridgeMemberType.Hybrid)
					continue;

				bool connected = state.PortModel != null && state.PortModel.IsConnected();
				state.Row.EnableInClassList(rowConnectedUssClassName, connected);

				// When connected: show Port label (name only), hide value editor
				// When disconnected: hide Port label, show value editor (with draggable label)
				if (state.PortView?.Label != null)
					state.PortView.Label.style.display =
						connected ? DisplayStyle.Flex : DisplayStyle.None;
			}
		}

		/// <summary>
		/// Cache port positions before the base class removes Root from the hierarchy
		/// during culling, and clear the cache when culling is disabled. Without this,
		/// <c>Port.GetGlobalCenter()</c> returns garbage for culled ports because
		/// <c>connector.LocalToWorld()</c> has no valid ancestor transform chain.
		/// </summary>
		public override void SetCullingState(GraphViewCullingState cullingState)
		{
			if (cullingState == GraphViewCullingState.Enabled)
			{
				var cullingRef = (VisualElement)m_OwnerElement;
				foreach (var state in m_Rows)
					state.PortView?.PrepareCulling(cullingRef);
				m_OutputPortView?.PrepareCulling(cullingRef);
			}

			base.SetCullingState(cullingState);

			if (cullingState == GraphViewCullingState.Disabled)
			{
				foreach (var state in m_Rows)
					state.PortView?.ClearCulling();
				m_OutputPortView?.ClearCulling();
			}
		}
	}
}
