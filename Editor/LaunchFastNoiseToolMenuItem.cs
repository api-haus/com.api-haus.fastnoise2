using UnityEditor;

namespace FastNoise2.Editor
{
	public static class LaunchFastNoiseToolMenuItem
	{
		[MenuItem("Tools/Launch FastNoise Graph Editor")]
		private static void DoLaunch() => NoiseToolProxy.NoiseToolProxy.LaunchNoiseTool();
	}
}
