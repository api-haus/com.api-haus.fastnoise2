using System;
using UnityEditor;
using UnityEngine;
using FastNoise2.Authoring.NoiseGraph;

namespace FastNoise2.Editor
{
	[CustomPropertyDrawer(typeof(FastNoiseGraph))]
	public class FastNoiseGraphPropertyDrawer : PropertyDrawer
	{
		const int BUTTON_WIDTH = 40;
		const int EDIT_BUTTON_WIDTH = 40;
		const int PADDING = 4;
		const string ENCODED_GRAPH_PROPERTY_PATH = "encodedGraph";

		// Track which property path is actively being edited via IPC
		static string s_ActivePropertyPath;
		static SerializedObject s_ActiveSerializedObject;
		static bool s_Subscribed;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var encodedProp = property.FindPropertyRelative(ENCODED_GRAPH_PROPERTY_PATH);
			string propertyPath = property.propertyPath;
			bool isActive = s_ActivePropertyPath == propertyPath
				&& s_ActiveSerializedObject == property.serializedObject;

#if FN2_USER_SIGNED
			// Layout: [encoded field] [Edit] [Copy] [Paste]
			float buttonsWidth = EDIT_BUTTON_WIDTH + BUTTON_WIDTH * 2 + PADDING * 3;
			Rect fieldRect = new(position.x, position.y, position.width - buttonsWidth, position.height);
			Rect editRect = new(fieldRect.xMax + PADDING, position.y, EDIT_BUTTON_WIDTH, position.height);
			Rect copyRect = new(editRect.xMax + PADDING, position.y, BUTTON_WIDTH, position.height);
			Rect pasteRect = new(copyRect.xMax + PADDING, position.y, BUTTON_WIDTH, position.height);

			EditorGUI.PropertyField(fieldRect, encodedProp, GUIContent.none);

			// Tint the Edit button when this property is the active IPC target
			var prevBg = GUI.backgroundColor;
			if (isActive)
				GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);

			if (GUI.Button(editRect, "Edit"))
			{
				s_ActivePropertyPath = propertyPath;
				s_ActiveSerializedObject = property.serializedObject;
				Subscribe();
				Ipc.NodeEditorSession.EditGraph(encodedProp.stringValue);
			}

			GUI.backgroundColor = prevBg;
#else
			// No IPC without signing — fallback layout without Edit button
			float buttonsWidth = BUTTON_WIDTH * 2 + PADDING * 2;
			Rect fieldRect = new(position.x, position.y, position.width - buttonsWidth, position.height);
			Rect copyRect = new(fieldRect.xMax + PADDING, position.y, BUTTON_WIDTH, position.height);
			Rect pasteRect = new(copyRect.xMax + PADDING, position.y, BUTTON_WIDTH, position.height);

			EditorGUI.PropertyField(fieldRect, encodedProp, GUIContent.none);
#endif

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

#if FN2_USER_SIGNED
		static void Subscribe()
		{
			if (s_Subscribed) return;
			s_Subscribed = true;
			Ipc.NodeEditorSession.OnGraphChanged += OnGraphChanged;
		}

		static void OnGraphChanged(string encodedGraph)
		{
			if (s_ActiveSerializedObject == null || string.IsNullOrEmpty(s_ActivePropertyPath))
				return;

			// targetObject may have been destroyed (deselected, deleted, domain reload)
			if (s_ActiveSerializedObject.targetObject == null)
			{
				s_ActiveSerializedObject = null;
				s_ActivePropertyPath = null;
				return;
			}

			s_ActiveSerializedObject.Update();

			var prop = s_ActiveSerializedObject.FindProperty(s_ActivePropertyPath);
			if (prop == null) return;

			var encodedProp = prop.FindPropertyRelative(ENCODED_GRAPH_PROPERTY_PATH);
			if (encodedProp == null) return;

			encodedProp.stringValue = encodedGraph;
			s_ActiveSerializedObject.ApplyModifiedProperties();

			EditorUtility.SetDirty(s_ActiveSerializedObject.targetObject);
			UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
		}
#endif
	}
}
