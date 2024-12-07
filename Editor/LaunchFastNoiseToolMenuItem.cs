using UnityEditor;

namespace FastNoise2.Editor
{
	public static class LaunchFastNoiseToolMenuItem
	{
		[MenuItem("Tools/Launch FastNoise Graph Editor")]
		static void DoLaunch()
		{
			NoiseToolProxy.NoiseToolProxy.LaunchNoiseTool();
		}
	}
}
