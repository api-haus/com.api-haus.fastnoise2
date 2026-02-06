using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FastNoise2.Editor.Signing
{
	public class FastNoise2SigningWindow : EditorWindow
	{
		[MenuItem("Window/FastNoise2")]
		public static void ShowWindow()
		{
			var window = GetWindow<FastNoise2SigningWindow>();
			window.titleContent = new GUIContent("FastNoise2");
			window.minSize = new Vector2(400, 260);
		}

		[InitializeOnLoadMethod]
		static void AutoPopup()
		{
#if !FN2_USER_SIGNED
			EditorApplication.delayCall += () => ShowWindow();
#endif
		}

		public void CreateGUI()
		{
			var root = rootVisualElement;
			root.style.paddingTop = 12;
			root.style.paddingBottom = 12;
			root.style.paddingLeft = 12;
			root.style.paddingRight = 12;

			var header = new Label("[FastNoise2]");
			header.style.fontSize = 24;
			header.style.color = new Color(0.3f, 0.5f, 1f);
			header.style.unityFontStyleAndWeight = FontStyle.Bold;
			header.style.marginBottom = 12;
			root.Add(header);

			var disclaimerBox = new VisualElement();
			disclaimerBox.style.backgroundColor = new Color(0f, 0f, 0f, 0.15f);
			disclaimerBox.style.borderTopWidth = 1;
			disclaimerBox.style.borderBottomWidth = 1;
			disclaimerBox.style.borderLeftWidth = 1;
			disclaimerBox.style.borderRightWidth = 1;
			disclaimerBox.style.borderTopColor = new Color(1f, 1f, 1f, 0.1f);
			disclaimerBox.style.borderBottomColor = new Color(1f, 1f, 1f, 0.1f);
			disclaimerBox.style.borderLeftColor = new Color(1f, 1f, 1f, 0.1f);
			disclaimerBox.style.borderRightColor = new Color(1f, 1f, 1f, 0.1f);
			disclaimerBox.style.borderTopLeftRadius = 4;
			disclaimerBox.style.borderTopRightRadius = 4;
			disclaimerBox.style.borderBottomLeftRadius = 4;
			disclaimerBox.style.borderBottomRightRadius = 4;
			disclaimerBox.style.paddingTop = 8;
			disclaimerBox.style.paddingBottom = 8;
			disclaimerBox.style.paddingLeft = 10;
			disclaimerBox.style.paddingRight = 10;
			disclaimerBox.style.marginBottom = 12;

			var disclaimer = new Label(
				"FastNoise2 ships pre-built native libraries that are unsigned. " +
				"On macOS, unsigned libraries trigger OS-level security dialogs that prevent Unity from loading them. " +
				"On Linux, libraries need execute permissions.\n\n" +
				"Clicking 'Sign Libraries' will run platform-specific commands to clear quarantine " +
				"attributes (macOS) or set execute permissions (Linux), then enable a scripting define " +
				"(FN2_USER_SIGNED) so that native interop code compiles.");
			disclaimer.style.whiteSpace = WhiteSpace.Normal;
			disclaimerBox.Add(disclaimer);
			root.Add(disclaimerBox);

			var platformInfo = new Label(GetPlatformInfoText());
			platformInfo.style.whiteSpace = WhiteSpace.Normal;
			platformInfo.style.marginBottom = 12;
			platformInfo.style.unityFontStyleAndWeight = FontStyle.Italic;
			root.Add(platformInfo);

#if FN2_USER_SIGNED
			var status = new Label("Libraries are signed. Native code is enabled.");
			status.style.color = new Color(0.3f, 0.8f, 0.3f);
			status.style.unityFontStyleAndWeight = FontStyle.Bold;
			status.style.marginBottom = 8;
			root.Add(status);

			var removeButton = new Button(() =>
			{
				LibrarySigner.RemoveSigningDefine();
			})
			{
				text = "Remove Signing Define"
			};
			root.Add(removeButton);
#else
			var buttonRow = new VisualElement();
			buttonRow.style.flexDirection = FlexDirection.Row;

			var signButton = new Button(() =>
			{
				LibrarySigner.SignLibraries();
				LibrarySigner.AddSigningDefine();
			})
			{
				text = "Sign Libraries"
			};
			signButton.style.flexGrow = 1;
			buttonRow.Add(signButton);

			var dismissButton = new Button(() => Close())
			{
				text = "Dismiss"
			};
			dismissButton.style.width = 80;
			buttonRow.Add(dismissButton);

			root.Add(buttonRow);
#endif
		}

		static string GetPlatformInfoText()
		{
			var paths = LibrarySigner.GetLibraryPaths();
			if (paths.Count == 0)
				return "Platform: " + Application.platform + " (no signing commands needed)";

			string cmds = Application.platform switch
			{
				RuntimePlatform.OSXEditor => "chmod +x, xattr -dr com.apple.quarantine",
				RuntimePlatform.LinuxEditor => "chmod +x",
				_ => "none",
			};

			return $"Platform: {Application.platform}\nCommands: {cmds}\nPaths: {string.Join(", ", paths)}";
		}
	}
}
