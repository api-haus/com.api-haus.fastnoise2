using UnityEngine;

namespace FastNoise2.Authoring
{
	[CreateAssetMenu(fileName = "FastNoise Graph", menuName = "FastNoise2/Graph Asset")]
	public class FastNoiseGraphAsset : ScriptableObject
	{
		[SerializeField] FastNoiseGraph savedGraph;
	}
}
