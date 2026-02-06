using FastNoise2.Authoring.NoiseGraph;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	[ScriptedImporter(2, "fn2graph")]
	public class FastNoiseGraphImporter : ScriptedImporter
	{
		public override void OnImportAsset(AssetImportContext ctx)
		{
			var graph = GraphDatabase.LoadGraphForImporter<FastNoiseEditorGraph>(ctx.assetPath);
			if (graph == null)
			{
				ctx.LogImportError($"Failed to load noise graph: {ctx.assetPath}");
				return;
			}

			string encoded = FastNoiseGraphCompiler.Compile(graph, out string error);

			var asset = ScriptableObject.CreateInstance<NoiseGraphAsset>();
			asset.name = "NoiseGraph";

			if (!string.IsNullOrEmpty(encoded))
			{
				asset.SetEncodedGraph(encoded);

				var preview = FastNoisePreview.RenderPreview(encoded, 128, 128);
				if (preview != null)
				{
					preview.name = "NoisePreview";
					ctx.AddObjectToAsset("preview", preview);
				}
			}
			else if (error != null)
			{
				ctx.LogImportWarning(error);
			}

			ctx.AddObjectToAsset("main", asset);
			ctx.SetMainObject(asset);
		}
	}
}
