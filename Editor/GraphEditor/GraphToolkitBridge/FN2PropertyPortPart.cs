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
	/// </summary>
	class FN2PropertyPortPart : BaseModelViewPart
	{
		public static readonly string ussClassName = "fn2-property-port-part";
		public static readonly string rowUssClassName = "fn2-member-row";
		public static readonly string rowConnectedUssClassName = "fn2-member-row--connected";
		public static readonly string rowOutputUssClassName = "fn2-member-row--output";
		public static readonly string portSlotUssClassName = "fn2-member-port-slot";
		public static readonly string labelUssClassName = "fn2-member-label";
		public static readonly string editorSlotUssClassName = "fn2-member-editor-slot";

		// Approximate character width for estimating node min-width
		const float CharWidth = 8.5f;
		const float BaseOverhead = 150f; // port connector + value field + margins

		VisualElement m_Root;
		readonly List<MemberRowState> m_Rows = new();

		struct MemberRowState
		{
			public VisualElement Row;
			public FN2BridgeMemberInfo MemberInfo;
			public PortModel PortModel;      // null for pure variables
			public Port PortView;            // null for pure variables
			public VisualElement EditorSlot;  // null for pure node lookups
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

			// Build the output port row first
			if (nodeModel.OutputsById.TryGetValue("output", out var outputPortModel))
			{
				var row = new VisualElement();
				row.AddToClassList(rowUssClassName);
				row.AddToClassList(rowOutputUssClassName);

				// Spacer pushes port to the right
				var spacer = new VisualElement();
				spacer.AddToClassList("fn2-member-spacer");
				row.Add(spacer);

				var portView = ModelViewFactory.CreateUI<Port>(
					ownerView.RootView, outputPortModel);
				if (portView != null)
				{
					var portSlot = new VisualElement();
					portSlot.AddToClassList(portSlotUssClassName);
					portSlot.Add(portView);
					row.Add(portSlot);
				}

				m_Root.Add(row);
				m_Rows.Add(new MemberRowState
				{
					Row = row,
					PortModel = outputPortModel,
					PortView = portView,
				});
			}

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
						if (member.PortId != null &&
							nodeModel.InputsById.TryGetValue(member.PortId, out var portModel))
						{
							state.PortModel = portModel;
							var portView = ModelViewFactory.CreateUI<Port>(
								ownerView.RootView, portModel);
							state.PortView = portView;
							if (portView != null)
							{
								var portSlot = new VisualElement();
								portSlot.AddToClassList(portSlotUssClassName);
								portSlot.Add(portView);
								row.Add(portSlot);
							}
						}
						break;
					}

					case FN2BridgeMemberType.Hybrid:
					{
						// Port connector (label hidden — the value editor provides it for drag support)
						if (member.PortId != null &&
							nodeModel.InputsById.TryGetValue(member.PortId, out var portModel))
						{
							state.PortModel = portModel;
							var portView = ModelViewFactory.CreateUI<Port>(
								ownerView.RootView, portModel);
							state.PortView = portView;
							if (portView != null)
							{
								portView.AddToClassList("fn2-hybrid-port");
								// Hide the Port's own label — the value editor label
								// provides drag-to-adjust instead
								if (portView.Label != null)
									portView.Label.style.display = DisplayStyle.None;
								var portSlot = new VisualElement();
								portSlot.AddToClassList(portSlotUssClassName);
								portSlot.Add(portView);
								row.Add(portSlot);
							}
						}

						// Value editor with label (supports drag-to-adjust on the label)
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
								var editor = InlineValueEditor.CreateEditorForConstants(
									ownerView.RootView, ownerModels, constants, member.Name);
								if (editor != null)
									editorSlot.Add(editor);
							}
						}

						state.EditorSlot = editorSlot;
						row.Add(editorSlot);
						break;
					}

					case FN2BridgeMemberType.Float:
					case FN2BridgeMemberType.Int:
					case FN2BridgeMemberType.Enum:
					{
						// Value editor only (label embedded in the editor)
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
								var editor = InlineValueEditor.CreateEditorForConstants(
									ownerView.RootView, ownerModels, constants, member.Name);
								if (editor != null)
									editorSlot.Add(editor);
							}
						}

						state.EditorSlot = editorSlot;
						row.Add(editorSlot);
						break;
					}
				}

				m_Root.Add(row);
				m_Rows.Add(state);
			}
		}

		public override void UpdateUIFromModel(UpdateFromModelVisitor visitor)
		{
			// Toggle connected class on hybrid rows and swap label visibility
			foreach (var state in m_Rows)
			{
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
	}
}
