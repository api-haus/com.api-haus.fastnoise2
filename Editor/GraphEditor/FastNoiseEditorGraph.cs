using System;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	[Graph("fn2graph")]
	[Serializable]
	public class FastNoiseEditorGraph : Graph
	{
		[SerializeField] float m_PreviewFrequency = 0.02f;
		[SerializeField] float m_PanOffsetX;
		[SerializeField] float m_PanOffsetY;
		[SerializeField] float m_CameraYaw;
		[SerializeField] float m_CameraPitch = 0.7f;
		[SerializeField] int m_PreviewMode;
		[SerializeField] int m_PreviewSize = 128;
		[SerializeField] float m_HeightScale = 0.15f;

		internal void LoadPreviewState()
		{
			FN2BridgeCallbacks.PreviewFrequency = m_PreviewFrequency;
			FN2BridgeCallbacks.PanOffset = new Vector2(m_PanOffsetX, m_PanOffsetY);
			FN2BridgeCallbacks.CameraYaw = m_CameraYaw;
			FN2BridgeCallbacks.CameraPitch = m_CameraPitch;
			FN2BridgeCallbacks.PreviewModeValue = m_PreviewMode;
			FN2BridgeCallbacks.PreviewSizeValue = m_PreviewSize;
			FN2BridgeCallbacks.HeightScale = m_HeightScale;
		}

		internal void SavePreviewState()
		{
			m_PreviewFrequency = FN2BridgeCallbacks.PreviewFrequency;
			m_PanOffsetX = FN2BridgeCallbacks.PanOffset.x;
			m_PanOffsetY = FN2BridgeCallbacks.PanOffset.y;
			m_CameraYaw = FN2BridgeCallbacks.CameraYaw;
			m_CameraPitch = FN2BridgeCallbacks.CameraPitch;
			m_PreviewMode = FN2BridgeCallbacks.PreviewModeValue;
			m_PreviewSize = FN2BridgeCallbacks.PreviewSizeValue;
			m_HeightScale = FN2BridgeCallbacks.HeightScale;
		}

		[MenuItem("Assets/Create/FastNoise2/Noise Graph")]
		static void CreateNoiseGraphAsset()
		{
			GraphDatabase.PromptInProjectBrowserToCreateNewAsset<FastNoiseEditorGraph>("New Noise Graph");
		}

		public override void OnGraphChanged(GraphLogger graphLogger)
		{
			int outputCount = GetNodes().OfType<FastNoiseOutputNode>().Count();
			if (outputCount == 0)
				graphLogger.LogWarning("Graph has no Output node.");
			else if (outputCount > 1)
				graphLogger.LogWarning("Graph has multiple Output nodes. Only one is allowed.");

			foreach (var node in GetNodes())
			{
				if (node is FN2EditorNode editorNode)
					editorNode.ValidateConnections(graphLogger);
			}
		}
	}
}
