using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Unity.GraphToolkit.Editor
{
	/// <summary>
	/// Dropdown field for FN2 enum members. Replaces the default integer editor
	/// with a named dropdown populated from C++ enum metadata.
	/// </summary>
	class FN2EnumField : BaseModelPropertyField
	{
		readonly IReadOnlyList<Constant> m_Constants;
		readonly IReadOnlyList<GraphElementModel> m_OwnerModels;
		readonly string[] m_EnumNames;
		readonly DropdownField m_Dropdown;

		public FN2EnumField(
			RootView rootView,
			IReadOnlyList<Constant> constants,
			IReadOnlyList<GraphElementModel> ownerModels,
			string[] enumNames,
			string label)
			: base(rootView)
		{
			m_Constants = constants;
			m_OwnerModels = ownerModels;
			m_EnumNames = enumNames;

			m_Dropdown = new DropdownField(label, enumNames.ToList(), 0);
			m_Dropdown.RegisterValueChangedCallback(OnDropdownChanged);

			Field = m_Dropdown;
			hierarchy.Add(m_Dropdown);
		}

		void OnDropdownChanged(ChangeEvent<string> evt)
		{
			int newIndex = System.Array.IndexOf(m_EnumNames, evt.newValue);
			if (newIndex < 0)
				return;

			var command = new UpdateConstantsValueCommand(m_Constants, newIndex);
			CommandTarget.Dispatch(command);
		}

		public override void UpdateDisplayedValue()
		{
			if (m_Constants == null || m_Constants.Count == 0)
				return;

			if (m_Constants[0].TryGetValue<int>(out int index) &&
				index >= 0 && index < m_EnumNames.Length)
			{
				m_Dropdown.SetValueWithoutNotify(m_EnumNames[index]);
			}
		}
	}
}
