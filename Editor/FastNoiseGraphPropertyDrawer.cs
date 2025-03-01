using System;
using FastNoise2.Authoring.NoiseGraph;
using UnityEditor;
using UnityEngine;

namespace FastNoise2.Editor
{
	[CustomPropertyDrawer(typeof(FastNoiseGraph))]
	public class FastNoiseGraphPropertyDrawer : PropertyDrawer
	{
		const int EDIT_BUTTON_WIDTH = 60;
		const int PADDING = 5;
		const string ENCODED_GRAPH_PROPERTY_PATH = "encodedGraph";

		static Action<FastNoiseGraphPropertyDrawer, bool> s_editorWasActivatedAction;

		bool m_IsEditing;
		SerializedProperty m_Property;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			m_Property = property;

			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Draw label
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Don't make child fields be indented
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			Rect encodedValueRect = new Rect(position.x, position.y, position.width - EDIT_BUTTON_WIDTH - PADDING, position.height);
			Rect buttonRect = new Rect(position.x + (position.width - EDIT_BUTTON_WIDTH), position.y, EDIT_BUTTON_WIDTH,
				position.height);

			// Draw fields - pass GUIContent.none to each so they are drawn without labels
			EditorGUI.PropertyField(encodedValueRect, property.FindPropertyRelative(ENCODED_GRAPH_PROPERTY_PATH),
				GUIContent.none);
			bool isButtonClicked = EditorGUI.LinkButton(buttonRect, "Edit Noise");

			if (isButtonClicked)
			{
				OnEditButtonClicked();
			}

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			s_editorWasActivatedAction -= OnEditorWasActivated;
			s_editorWasActivatedAction += OnEditorWasActivated;

			EditorGUI.EndProperty();
		}

		void OnEditorWasActivated(FastNoiseGraphPropertyDrawer editor, bool wasActivated)
		{
			if (wasActivated && editor != this)
				OnDeactivate();
		}

		void OnEditButtonClicked()
		{
			if (m_IsEditing)
				OnDeactivate();
			else
				OnActivate();
		}

		void OnActivate()
		{
			if (m_IsEditing) return;
			m_IsEditing = true;

			s_editorWasActivatedAction?.Invoke(this, true);

			System.Diagnostics.Process myProcess = NoiseToolProxy.NoiseToolProxy.LaunchNoiseTool();
			myProcess.Exited += (_, _) =>
			{
				OnDeactivate(true);
			};

			NoiseToolProxy.NoiseToolProxy.CopiedNodeSettings += OnCopiedNodeSettings;
		}

		void OnCopiedNodeSettings(string encodedNode)
		{
			m_Property.FindPropertyRelative(ENCODED_GRAPH_PROPERTY_PATH).stringValue = encodedNode;
			m_Property.serializedObject.ApplyModifiedProperties();
		}

		void OnDeactivate(bool force = false)
		{
			NoiseToolProxy.NoiseToolProxy.CopiedNodeSettings -= OnCopiedNodeSettings;

			if (!m_IsEditing && !force) return;
			m_IsEditing = false;

			s_editorWasActivatedAction?.Invoke(this, false);
		}
	}
}
