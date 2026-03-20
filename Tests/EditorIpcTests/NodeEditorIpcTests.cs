using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace FastNoise2.Tests
{
#if FN2_USER_SIGNED
	using Editor.Ipc;

	public class NodeEditorIpcTests : IpcTestBase
	{
		#region IPC Lifecycle

		[Test, Order(0)]
		public void Setup_ReturnsValidContext()
		{
			Assert.That(Ctx, Is.Not.EqualTo(IntPtr.Zero));
		}

		#endregion

		#region NodeEditor Binary

		[Test]
		public void NodeEditorBinary_Exists()
		{
			Assert.That(NodeEditorPath, Is.Not.Null, "NodeEditor binary not found at expected package path");
			Assert.That(File.Exists(NodeEditorPath), Is.True);
		}

		#endregion

		#region Message Passing (no editor running)

		[Test, Order(1)]
		public void PollMessage_OnFreshContext_ReturnsNull()
		{
			// Drain any stale messages left from prior runs
			while (PollMessage() != null) { }

			Assert.That(PollMessage(), Is.Null);
		}

		[Test]
		public void SendImport_WithValidContext_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => NodeEditorIpc.fnEditorIpcSendImportRequest(Ctx, DEFAULT_GRAPH));
		}

		[Test]
		public void SendSelected_WithValidContext_DoesNotThrow()
		{
			Assert.DoesNotThrow(() => NodeEditorIpc.fnEditorIpcSendSelectedNode(Ctx, DEFAULT_GRAPH));
		}

		#endregion

		#region Event Wiring

		[Test]
		public void OnGraphChanged_SubscribeDoesNotThrow()
		{
			bool fired = false;
			void Handler(string s) => fired = true;

			Assert.DoesNotThrow(() => NodeEditorSession.OnGraphChanged += Handler);
			Assert.DoesNotThrow(() => NodeEditorSession.OnGraphChanged -= Handler);
			Assert.That(fired, Is.False);
		}

		#endregion

		#region E2E Process Lifecycle

		[Test]
		public void LaunchNodeEditor_ProcessStarts()
		{
			KillNodeEditors();

			var proc = LaunchNodeEditor(DEFAULT_GRAPH);
			Assert.That(proc, Is.Not.Null, "Process.Start returned null");

			try
			{
				bool started = WaitForNodeEditor(5000);

				if (!started && proc.HasExited)
				{
					string stderr = proc.StandardError.ReadToEnd();
					Assert.Fail($"NodeEditor exited immediately with code {proc.ExitCode}. stderr: {stderr}");
				}

				Assert.That(started, Is.True, "NodeEditor process did not appear within 5s");
			}
			finally
			{
				KillNodeEditors();
			}
		}

		[Test]
		public void LaunchNodeEditor_KillAndRelaunch()
		{
			KillNodeEditors();

			// First launch
			var proc1 = LaunchNodeEditor(DEFAULT_GRAPH);
			Assert.That(proc1, Is.Not.Null);
			Assert.That(WaitForNodeEditor(5000), Is.True, "First launch: process not found");

			// Kill it
			KillNodeEditors();
			System.Threading.Thread.Sleep(500);

			// Verify dead
			var procs = Process.GetProcessesByName("NodeEditor");
			bool stillAlive = procs.Length > 0;
			foreach (var p in procs) p.Dispose();
			Assert.That(stillAlive, Is.False, "NodeEditor should be dead after kill");

			// Re-launch
			var proc2 = LaunchNodeEditor(DEFAULT_GRAPH);
			Assert.That(proc2, Is.Not.Null);
			Assert.That(WaitForNodeEditor(5000), Is.True, "Second launch: process not found after kill+relaunch");

			KillNodeEditors();
		}

		#endregion
	}
#endif
}
