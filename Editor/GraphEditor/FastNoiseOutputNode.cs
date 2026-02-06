using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	[Serializable]
	public class FastNoiseOutputNode : Node
	{
		internal const string InputPortName = "nl_source";
		internal const string InputDisplayName = "Source";

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			context.AddInputPort<NoiseSignal>(InputPortName)
				.WithDisplayName(InputDisplayName)
				.Build();
		}
	}
}
