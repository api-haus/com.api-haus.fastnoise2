using System;
using UnityEditor;
using UnityEngine;
using FastNoise2.Authoring.NoiseGraph;

namespace FastNoise2.Editor
{
	[CustomPropertyDrawer(typeof(FastNoiseGraph))]
	public class FastNoiseGraphPropertyDrawer : PropertyDrawer
	{
		const int BUTTON_WIDTH = 50;
		const int PADDING = 4;
		const string ENCODED_GRAPH_PROPERTY_PATH = "encodedGraph";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Layout: [encoded field] [Copy] [Paste]
			float buttonsWidth = BUTTON_WIDTH * 2 + PADDING * 2;
			Rect fieldRect = new(position.x, position.y, position.width - buttonsWidth, position.height);
			Rect copyRect = new(fieldRect.xMax + PADDING, position.y, BUTTON_WIDTH, position.height);
			Rect pasteRect = new(copyRect.xMax + PADDING, position.y, BUTTON_WIDTH, position.height);

			var encodedProp = property.FindPropertyRelative(ENCODED_GRAPH_PROPERTY_PATH);
			EditorGUI.PropertyField(fieldRect, encodedProp, GUIContent.none);

			if (GUI.Button(copyRect, "Copy"))
				EditorGUIUtility.systemCopyBuffer = encodedProp.stringValue;

			if (GUI.Button(pasteRect, "Paste"))
			{
				string clipboard = EditorGUIUtility.systemCopyBuffer;
				if (!string.IsNullOrEmpty(clipboard))
				{
					encodedProp.stringValue = clipboard;
					property.serializedObject.ApplyModifiedProperties();
				}
			}

			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}
	}
}
