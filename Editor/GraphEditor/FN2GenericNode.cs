using System;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	[Serializable]
	public class FN2GenericNode : FN2EditorNode
	{
		[SerializeField] string m_NodeTypeName;
		public override string NodeTypeName => m_NodeTypeName;

		public FN2GenericNode() { }

		internal void SetNodeTypeName(string name) => m_NodeTypeName = name;
	}
}
