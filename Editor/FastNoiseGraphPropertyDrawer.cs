using System;
using FastNoise2.Authoring;
using UnityEditor;
using UnityEngine;

namespace FastNoise2.Editor
{
	[CustomPropertyDrawer(typeof(FastNoiseGraph))]
	public class FastNoiseGraphPropertyDrawer : PropertyDrawer
	{
		const int EditButtonWidth = 60;
		const int Padding = 5;
		const string EncodedGraphPropertyPath = "encodedGraph";

		static Action<FastNoiseGraphPropertyDrawer, bool> EditorWasActivatedAction;

		bool IsEditing;
		SerializedProperty Property;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Property = property;

			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Draw label
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Don't make child fields be indented
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Calculate rects
			Rect encodedValueRect = new Rect(position.x, position.y, position.width - EditButtonWidth - Padding, position.height);
			Rect buttonRect = new Rect(position.x + (position.width - EditButtonWidth), position.y, EditButtonWidth,
				position.height);

			// Draw fields - pass GUIContent.none to each so they are drawn without labels
			EditorGUI.PropertyField(encodedValueRect, property.FindPropertyRelative(EncodedGraphPropertyPath),
				GUIContent.none);
			bool isButtonClicked = EditorGUI.LinkButton(buttonRect, "Edit Noise");

			if (isButtonClicked)
			{
				OnEditButtonClicked();
			}

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorWasActivatedAction -= OnEditorWasActivated;
			EditorWasActivatedAction += OnEditorWasActivated;

			EditorGUI.EndProperty();
		}

		void OnEditorWasActivated(FastNoiseGraphPropertyDrawer editor, bool wasActivated)
		{
			if (wasActivated && editor != this)
				OnDeactivate();
		}

		void OnEditButtonClicked()
		{
			if (IsEditing)
				OnDeactivate();
			else
				OnActivate();
		}

		void OnActivate()
		{
			if (IsEditing) return;
			IsEditing = true;

			EditorWasActivatedAction?.Invoke(this, true);

			System.Diagnostics.Process myProcess = NoiseToolProxy.NoiseToolProxy.LaunchNoiseTool();
			myProcess.Exited += (_, _) =>
			{
				OnDeactivate(true);
			};

			NoiseToolProxy.NoiseToolProxy.CopiedNodeSettings += OnCopiedNodeSettings;
		}

		void OnCopiedNodeSettings(string encodedNode)
		{
			Property.FindPropertyRelative(EncodedGraphPropertyPath).stringValue = encodedNode;
			Property.serializedObject.ApplyModifiedProperties();
		}

		void OnDeactivate(bool force = false)
		{
			NoiseToolProxy.NoiseToolProxy.CopiedNodeSettings -= OnCopiedNodeSettings;

			if (!IsEditing && !force) return;
			IsEditing = false;

			EditorWasActivatedAction?.Invoke(this, false);
		}
	}
}
