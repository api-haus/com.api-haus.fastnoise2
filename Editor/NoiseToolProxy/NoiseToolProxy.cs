using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;

namespace FastNoise2.Editor.NoiseToolProxy
{
	/// <summary>
	/// Since we rely heavily on NoiseTools internal save-state, this is a singleton.
	/// It has only one NoiseTool instance present.
	/// </summary>
	///
	///
	public static class NoiseToolProxy
	{
		static async Task ReadProcessNotifyNode()
		{
			while (NoiseToolProcess is { HasExited: false })
			{
				try
				{
					string line = await NoiseToolProcess.StandardOutput.ReadLineAsync();

					if (!string.IsNullOrEmpty(line) && EncodedNoiseRegex.IsMatch(line))
						CopiedNodeSettings?.Invoke(line);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		public static Process NoiseToolProcess;
		static EditorCoroutine LogsCoroutine;

		static readonly Regex EncodedNoiseRegex = new(@"^[-A-Za-z0-9+/]*={0,3}$");

		public static Process LaunchNoiseTool(string profile = "")
		{
			Kill();
			if (!Directory.Exists(GetWorkingDir()))
				Directory.CreateDirectory(GetWorkingDir());

			EditorGUIUtility.systemCopyBuffer = profile;

			NoiseToolProcess ??= Process.Start(
				new ProcessStartInfo
				{
					Arguments = null,
					CreateNoWindow = false,
					Domain = null,
					ErrorDialog = false,
					ErrorDialogParentHandle = default,
					FileName = GetAbsoluteToolPath(),
					LoadUserProfile = true,
					RedirectStandardError = false,
					RedirectStandardInput = false,
					RedirectStandardOutput = true,
					StandardErrorEncoding = null,
					StandardOutputEncoding = new UTF8Encoding(),
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Normal,
					WorkingDirectory = GetWorkingDir(),
					StandardInputEncoding = null,
				}
			);

			ReadProcessNotifyNode();

			return NoiseToolProcess;
		}

		public static void Kill()
		{
			if (NoiseToolProcess is { HasExited: false })
				NoiseToolProcess?.Kill();
			NoiseToolProcess = null;
		}

		public static string GetPackageRoot()
		{
			return Path.Join(Application.dataPath, "../Packages/com.auburn.fastnoise2/");
		}

		/// <summary>
		/// NoiseTool writes a file named NoiseTool.ini here.
		/// It contains saved node graph.
		/// </summary>
		public static string GetWorkingDir()
		{
			return Path.Join(Application.dataPath, "../Library/com.auburn.fastnoise2/");
		}

		public static string GetAbsoluteToolPath()
		{
			return Path.Join(GetPackageRoot(), GetPlatformToolPath());
		}

		public static string GetPlatformToolPath()
		{
			switch (Application.platform)
			{
				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.OSXServer:
					return "Plugins/macos/bin/NoiseTool";
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsServer:
					return "Plugins/windows/bin/NoiseTool.exe";
				case RuntimePlatform.Android:
				case RuntimePlatform.LinuxPlayer:
				case RuntimePlatform.LinuxEditor:
				case RuntimePlatform.LinuxServer:
					return "Plugins/linux/bin/NoiseTool";
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal delegate void CopiedNodeSettingsDelegate(string nodeSettings);

		internal static CopiedNodeSettingsDelegate CopiedNodeSettings;
	}
}
