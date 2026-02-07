using System;
using UnityEngine;

namespace Unity.GraphToolkit.Editor.Implementation
{
	/// <summary>
	/// Custom UserNodeModelImp subclass that overrides Title to return the
	/// FN2 node type name (e.g. "Perlin") instead of the C# class name ("FN2GenericNode").
	/// </summary>
	[Serializable]
	class FN2UserNodeModelImp : UserNodeModelImp
	{
		public override string Title
		{
			get
			{
				string name = FastNoise2.Editor.GraphEditor.FN2BridgeCallbacks.GetNodeTypeName?.Invoke(Node);
				return !string.IsNullOrEmpty(name) ? name : base.Title;
			}
		}
	}
}
