using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FastNoise2.Editor.Ipc
{
#if FN2_USER_SIGNED
	[InitializeOnLoad]
	internal static class NodeEditorSession
	{
		public static event Action<string> OnGraphChanged;

		const int POLL_BUFFER_SIZE = 65536;

		static IntPtr s_Ctx;
		static bool s_Polling;
		static byte[] s_PollBuffer;
		static string s_NodeEditorPath;

		static NodeEditorSession()
		{
			EditorApplication.quitting += Release;
			AssemblyReloadEvents.beforeAssemblyReload += Release;
		}

		static string ResolveNodeEditorPath()
		{
			if (s_NodeEditorPath != null) return s_NodeEditorPath;

			var pkgInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(
				typeof(NodeEditorSession).Assembly
			);
			if (pkgInfo == null)
			{
				Debug.LogError("[FN2 IPC] Cannot resolve package path for NodeEditorIpc.");
				return null;
			}

			string basePath = pkgInfo.resolvedPath;

#if UNITY_EDITOR_WIN
			string relPath = "Plugins/windows/bin/NodeEditor.exe";
#elif UNITY_EDITOR_OSX
			string relPath = "Plugins/macos/bin/NodeEditor";
#else
			string relPath = "Plugins/linux/bin/NodeEditor";
#endif
			string fullPath = Path.Combine(basePath, relPath);
			if (!File.Exists(fullPath))
			{
				Debug.LogError($"[FN2 IPC] NodeEditor binary not found at: {fullPath}");
				return null;
			}

			s_NodeEditorPath = fullPath;
			return fullPath;
		}

		static void EnsureExecutable(string path)
		{
#if UNITY_EDITOR_LINUX || UNITY_EDITOR_OSX
			var psi = new ProcessStartInfo("chmod", $"+x \"{path}\"")
			{
				UseShellExecute = false,
				CreateNoWindow = true,
			};
			try { Process.Start(psi)?.WaitForExit(2000); }
			catch (Exception e) { Debug.LogWarning($"[FN2 IPC] chmod failed: {e.Message}"); }
#endif
		}

		static bool Setup()
		{
			if (s_Ctx != IntPtr.Zero) return true;

			string editorPath = ResolveNodeEditorPath();
			if (editorPath == null) return false;

			EnsureExecutable(editorPath);

			s_Ctx = NodeEditorIpc.fnEditorIpcSetup(0);
			if (s_Ctx == IntPtr.Zero)
			{
				Debug.LogError("[FN2 IPC] fnEditorIpcSetup returned null context");
				return false;
			}

			s_PollBuffer = new byte[POLL_BUFFER_SIZE];
			return true;
		}

		public static void EditGraph(string encodedGraph)
		{
			if (!IsNodeEditorRunning())
			{
				// Release stale context so shared memory is fresh
				Release();

				if (!Setup()) return;

				StartNodeEditor(encodedGraph);
			}
			else
			{
				if (!Setup()) return;

				if (!string.IsNullOrEmpty(encodedGraph))
					NodeEditorIpc.fnEditorIpcSendImportRequest(s_Ctx, encodedGraph);
			}

			StartPolling();
		}

		static void StartNodeEditor(string encodedGraph)
		{
			string path = ResolveNodeEditorPath();
			if (path == null) return;

			string binDir = Path.GetDirectoryName(path)!;
			string libDir = Path.GetFullPath(Path.Combine(binDir, "..", "lib"));

			var psi = new ProcessStartInfo(path)
			{
				UseShellExecute = false,
				CreateNoWindow = false,
				WorkingDirectory = binDir,
				RedirectStandardError = true,
			};

			psi.ArgumentList.Add("--detached");
			if (!string.IsNullOrEmpty(encodedGraph))
			{
				psi.ArgumentList.Add("--import-ent");
				psi.ArgumentList.Add(encodedGraph);
			}

#if UNITY_EDITOR_LINUX
			string existing = psi.Environment.ContainsKey("LD_LIBRARY_PATH")
				? psi.Environment["LD_LIBRARY_PATH"] : "";
			psi.Environment["LD_LIBRARY_PATH"] = string.IsNullOrEmpty(existing)
				? libDir : $"{libDir}:{existing}";
#elif UNITY_EDITOR_OSX
			string existing = psi.Environment.ContainsKey("DYLD_LIBRARY_PATH")
				? psi.Environment["DYLD_LIBRARY_PATH"] : "";
			psi.Environment["DYLD_LIBRARY_PATH"] = string.IsNullOrEmpty(existing)
				? libDir : $"{libDir}:{existing}";
#endif

			try
			{
				var proc = Process.Start(psi);
				if (proc == null)
				{
					Debug.LogError("[FN2 IPC] Process.Start returned null");
					return;
				}

				Debug.Log($"[FN2 IPC] NodeEditor started (PID {proc.Id})");

				// Watch for early exit — if it dies within 1s, log stderr
				proc.EnableRaisingEvents = true;
				proc.Exited += (_, _) =>
				{
					string stderr = "";
					try { stderr = proc.StandardError.ReadToEnd(); }
					catch { /* stream already closed */ }

					if (!string.IsNullOrEmpty(stderr))
						Debug.LogError($"[FN2 IPC] NodeEditor exited (code {proc.ExitCode}): {stderr}");
					else
						Debug.LogWarning($"[FN2 IPC] NodeEditor exited (code {proc.ExitCode})");

					proc.Dispose();
				};
			}
			catch (Exception e)
			{
				Debug.LogError($"[FN2 IPC] Failed to start NodeEditor: {e.Message}");
			}
		}

		static bool IsNodeEditorRunning()
		{
			try
			{
				var procs = Process.GetProcessesByName("NodeEditor");
				bool alive = procs.Length > 0;
				foreach (var p in procs) p.Dispose();
				return alive;
			}
			catch
			{
				return false;
			}
		}

		static void StartPolling()
		{
			if (s_Polling) return;
			s_Polling = true;
			EditorApplication.update += PollUpdate;
		}

		static void StopPolling()
		{
			if (!s_Polling) return;
			s_Polling = false;
			EditorApplication.update -= PollUpdate;
		}

		static void PollUpdate()
		{
			if (s_Ctx == IntPtr.Zero) return;

			int msgType = NodeEditorIpc.fnEditorIpcPollMessage(
				s_Ctx, s_PollBuffer, s_PollBuffer.Length
			);

			// 0 = no message, negative = error, 1-2 = valid message type
			if (msgType <= 0) return;

			// Find null terminator in buffer
			int len = Array.IndexOf(s_PollBuffer, (byte)0);
			if (len < 0) len = s_PollBuffer.Length;

			if (len > 0)
			{
				string message = Encoding.UTF8.GetString(s_PollBuffer, 0, len);
				OnGraphChanged?.Invoke(message);
			}
		}

		static void Release()
		{
			StopPolling();

			if (s_Ctx == IntPtr.Zero) return;
			IntPtr ctx = s_Ctx;
			s_Ctx = IntPtr.Zero;

			try { NodeEditorIpc.fnEditorIpcRelease(ctx); }
			catch (Exception e) { Debug.LogWarning($"[FN2 IPC] Release failed: {e.Message}"); }
		}
	}
#endif
}
