using System.IO;
using FastNoise2.Bindings;
using FastNoise2.Editor.GraphEditor;
using FastNoise2.Generators;
using NUnit.Framework;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace FastNoise2.Tests
{
	/// <summary>
	/// EditMode tests for the FastNoise2 GraphToolkit graph editor.
	/// Creates temporary .fn2graph assets, manipulates them programmatically,
	/// compiles to encoded noise, and verifies the output is usable by the native library.
	/// </summary>
	public class GraphEditorTests
	{
		const string TestAssetDir = "Assets/FastNoise2TestTemp";
		string m_TestGraphPath;

		[SetUp]
		public void SetUp()
		{
			if (!AssetDatabase.IsValidFolder(TestAssetDir))
				AssetDatabase.CreateFolder("Assets", "FastNoise2TestTemp");

			m_TestGraphPath = $"{TestAssetDir}/Test_{GUID.Generate()}.fn2graph";
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(m_TestGraphPath))
				AssetDatabase.DeleteAsset(m_TestGraphPath);

			if (AssetDatabase.IsValidFolder(TestAssetDir))
			{
				// Only delete if empty
				string[] remaining = AssetDatabase.FindAssets("", new[] { TestAssetDir });
				if (remaining.Length == 0)
					AssetDatabase.DeleteAsset(TestAssetDir);
			}
		}

		// ───────────── Graph creation ─────────────

		[Test]
		public void CreateGraph_ProducesValidAsset()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);
			Assert.That(graph, Is.Not.Null, "CreateGraph returned null");
			Assert.That(File.Exists(m_TestGraphPath), Is.True, "Asset file not on disk");
		}

		[Test]
		public void CreateGraph_CanReload()
		{
			GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);
			var loaded = GraphDatabase.LoadGraph<FastNoiseEditorGraph>(m_TestGraphPath);
			Assert.That(loaded, Is.Not.Null, "LoadGraph returned null");
		}

		// ───────────── Node creation via bridge ─────────────

		[Test]
		public void Bridge_AddNodesIncreasesNodeCount()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);
			int initialCount = graph.nodeCount;

			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			GraphToolkitBridge.CreateNode(graph, simplexNode, Vector2.zero);

			Assert.That(graph.nodeCount, Is.EqualTo(initialCount + 1));
		}

		[Test]
		public void Bridge_AddMultipleNodesAndOutput()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);

			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			GraphToolkitBridge.CreateNode(graph, simplexNode, new Vector2(0, 0));

			var outputNode = new FastNoiseOutputNode();
			GraphToolkitBridge.CreateNode(graph, outputNode, new Vector2(300, 0));

			Assert.That(graph.nodeCount, Is.EqualTo(2));
		}

		// ───────────── Wiring ─────────────

		[Test]
		public void Bridge_WireConnectsPorts()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);

			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			GraphToolkitBridge.CreateNode(graph, simplexNode, new Vector2(0, 0));

			var outputNode = new FastNoiseOutputNode();
			GraphToolkitBridge.CreateNode(graph, outputNode, new Vector2(300, 0));

			var fromPort = simplexNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var toPort = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);

			Assert.That(fromPort, Is.Not.Null, "Output port not found on Simplex node");
			Assert.That(toPort, Is.Not.Null, "Input port not found on Output node");

			GraphToolkitBridge.CreateWire(graph, toPort, fromPort);

			Assert.That(toPort.isConnected, Is.True, "Input port not connected after wiring");
		}

		// ───────────── Compiler: Simplex → Output ─────────────

#if FN2_USER_SIGNED
		[Test]
		public void Compile_Diagnostics_GraphNodesAccessible()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);

			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			var simplexINode = GraphToolkitBridge.CreateNode(graph, simplexNode, new Vector2(0, 0));
			Assert.That(simplexINode, Is.Not.Null, "Bridge returned null INode for Simplex");

			var outputNode = new FastNoiseOutputNode();
			var outputINode = GraphToolkitBridge.CreateNode(graph, outputNode, new Vector2(300, 0));
			Assert.That(outputINode, Is.Not.Null, "Bridge returned null INode for Output");

			Assert.That(graph.nodeCount, Is.GreaterThanOrEqualTo(2),
				$"Expected at least 2 nodes, got {graph.nodeCount}");

			// Check GetNodes() returns our nodes directly as INode
			int userNodeCount = 0;
			FastNoiseOutputNode foundOutput = null;
			foreach (var inode in graph.GetNodes())
			{
				if (inode is FastNoiseEditorNode) userNodeCount++;
				if (inode is FastNoiseOutputNode outN) { foundOutput = outN; userNodeCount++; }
			}

			Assert.That(userNodeCount, Is.GreaterThanOrEqualTo(2),
				$"GetNodes() found only {userNodeCount} user nodes (OfType check)");
			Assert.That(foundOutput, Is.Not.Null,
				"GetNodes() did not find a FastNoiseOutputNode");

			// Check port wiring
			var fromPort = simplexNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var toPort = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);
			Assert.That(fromPort, Is.Not.Null, "Simplex output port is null");
			Assert.That(toPort, Is.Not.Null, "Output input port is null");

			GraphToolkitBridge.CreateWire(graph, toPort, fromPort);
			Assert.That(toPort.isConnected, Is.True, "Input port not connected");
			Assert.That(toPort.firstConnectedPort, Is.Not.Null, "firstConnectedPort is null");

			// Check that GetNodeFromPort works
			var connectedNode = GraphToolkitBridge.GetNodeFromPort(toPort.firstConnectedPort);
			Assert.That(connectedNode, Is.Not.Null, "GetNodeFromPort returned null");
			Assert.That(connectedNode, Is.InstanceOf<FastNoiseEditorNode>(),
				$"GetNodeFromPort returned {connectedNode.GetType().Name}, expected FastNoiseEditorNode");

			// Compile
			string encoded = FastNoiseGraphCompiler.Compile(graph, out string error);
			Assert.That(encoded, Is.Not.Null.And.Not.Empty,
				$"Compile failed: {error ?? "null error"}. NodeCount={graph.nodeCount}");
		}

		[Test]
		public void Compile_NodeBuilder_SimplexProducesValidNoise()
		{
			// Bypass the encoder entirely — build from NodeDescriptor via P/Invoke
			var descriptor = new NodeDescriptor("Simplex");
			using FastNoise fn = NodeBuilder.Build(descriptor);
			Assert.That(fn.IsCreated, Is.True, "NodeBuilder.Build failed for Simplex");

			float val = fn.GenSingle2D(1f, 1f, 1337);
			Assert.That(val, Is.Not.NaN, "NodeBuilder Simplex produced NaN");
			Assert.That(val, Is.InRange(-1f, 1f));
		}

		[Test]
		public void Compile_SimplexToOutput_ProducesValidEncodedString()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);

			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			GraphToolkitBridge.CreateNode(graph, simplexNode, new Vector2(0, 0));

			var outputNode = new FastNoiseOutputNode();
			GraphToolkitBridge.CreateNode(graph, outputNode, new Vector2(300, 0));

			var fromPort = simplexNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var toPort = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);
			GraphToolkitBridge.CreateWire(graph, toPort, fromPort);

			string encoded = FastNoiseGraphCompiler.Compile(graph, out string error);
			Assert.That(encoded, Is.Not.Null.And.Not.Empty, $"Compile failed: {error}");

			// Also verify via NodeBuilder (bypasses encoding)
			var descriptor = new NodeDescriptor("Simplex");
			using FastNoise fnDirect = NodeBuilder.Build(descriptor);
			float directVal = fnDirect.GenSingle2D(1f, 1f, 1337);
			Assert.That(directVal, Is.Not.NaN, "Direct NodeBuilder Simplex is NaN");

			// Verify the encoder output
			using FastNoise fn = FastNoise.FromEncodedNodeTree(encoded);
			Assert.That(fn != FastNoise.Invalid, Is.True,
				$"C++ decode failed for encoded='{encoded}'");
			Assert.That(fn.IsCreated, Is.True);

			float val = fn.GenSingle2D(1f, 1f, 1337);
			Assert.That(val, Is.Not.NaN,
				$"Encoded Simplex GenSingle2D=NaN. encoded='{encoded}', direct={directVal}");
		}

		[Test]
		public void Compile_SimplexToOutput_ProducesCoherentNoise()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);

			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			GraphToolkitBridge.CreateNode(graph, simplexNode, new Vector2(0, 0));

			var outputNode = new FastNoiseOutputNode();
			GraphToolkitBridge.CreateNode(graph, outputNode, new Vector2(300, 0));

			var fromPort = simplexNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var toPort = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);
			GraphToolkitBridge.CreateWire(graph, toPort, fromPort);

			string encoded = FastNoiseGraphCompiler.Compile(graph, out string error);
			Assert.That(encoded, Is.Not.Null.And.Not.Empty, $"Compile failed: {error}");

			using FastNoise fn = FastNoise.FromEncodedNodeTree(encoded);
			Assert.That(fn != FastNoise.Invalid, Is.True, $"C++ decode failed for: {encoded}");

			float[] data = new float[64 * 64];
			FastNoise.OutputMinMax minMax = fn.GenUniformGrid2D(
				data, 0f, 0f, 64, 64, 0.02f, 0.02f, 1337);

			// Simplex noise should have varying values
			Assert.That(minMax.max, Is.GreaterThan(minMax.min),
				"Noise output is flat — not coherent");
			Assert.That(minMax.min, Is.GreaterThanOrEqualTo(-1f));
			Assert.That(minMax.max, Is.LessThanOrEqualTo(1f));
		}

		// ───────────── Compiler: FractalFBm(Simplex) → Output ─────────────

		[Test]
		public void Compile_FractalFbmSimplex_ProducesValidNoise()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);

			// Simplex source node
			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			GraphToolkitBridge.CreateNode(graph, simplexNode, new Vector2(0, 0));

			// FractalFBm node — has a NodeLookup "source" input
			var fbmNode = new FastNoiseEditorNode { nodeTypeName = "FractalFBm" };
			GraphToolkitBridge.CreateNode(graph, fbmNode, new Vector2(300, 0));

			// Output node
			var outputNode = new FastNoiseOutputNode();
			GraphToolkitBridge.CreateNode(graph, outputNode, new Vector2(600, 0));

			// Wire Simplex → FBm source
			var simplexOut = simplexNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var fbmSourcePort = fbmNode.GetInputPortByName(FastNoiseEditorNode.NodeLookupPrefix + "source");
			Assert.That(fbmSourcePort, Is.Not.Null, "FBm 'source' input port not found");
			GraphToolkitBridge.CreateWire(graph, fbmSourcePort, simplexOut);

			// Wire FBm → Output
			var fbmOut = fbmNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var outputIn = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);
			GraphToolkitBridge.CreateWire(graph, outputIn, fbmOut);

			string encoded = FastNoiseGraphCompiler.Compile(graph, out string error);
			Assert.That(encoded, Is.Not.Null.And.Not.Empty, $"Compile failed: {error}");

			// Verify round-trip through NodeDescriptor
			NodeDescriptor descriptor = NodeDecoder.Decode(encoded);
			Assert.That(descriptor, Is.Not.Null);
			Assert.That(descriptor.NodeName.ToLower(), Does.Contain("fractalfbm").IgnoreCase);

			// Verify native library can use it
			using FastNoise fn = FastNoise.FromEncodedNodeTree(encoded);
			Assert.That(fn.IsCreated, Is.True);

			float[] data = new float[64 * 64];
			FastNoise.OutputMinMax minMax = fn.GenUniformGrid2D(
				data, 0f, 0f, 64, 64, 0.02f, 0.02f, 1337);
			Assert.That(minMax.max, Is.GreaterThan(minMax.min));
		}

		// ───────────── Compiler: Add(Simplex, Perlin) → Output ─────────────

		[Test]
		public void Compile_AddTwoSources_ProducesValidNoise()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);

			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			GraphToolkitBridge.CreateNode(graph, simplexNode, new Vector2(0, 0));

			var perlinNode = new FastNoiseEditorNode { nodeTypeName = "Perlin" };
			GraphToolkitBridge.CreateNode(graph, perlinNode, new Vector2(0, 200));

			// Add node has NodeLookup "lhs" and Hybrid "rhs"
			var addNode = new FastNoiseEditorNode { nodeTypeName = "Add" };
			GraphToolkitBridge.CreateNode(graph, addNode, new Vector2(300, 100));

			var outputNode = new FastNoiseOutputNode();
			GraphToolkitBridge.CreateNode(graph, outputNode, new Vector2(600, 100));

			// Wire Simplex → Add LHS
			var simplexOut = simplexNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var addLhs = addNode.GetInputPortByName(FastNoiseEditorNode.NodeLookupPrefix + "lhs");
			Assert.That(addLhs, Is.Not.Null, "Add 'lhs' port not found");
			GraphToolkitBridge.CreateWire(graph, addLhs, simplexOut);

			// Wire Perlin → Add RHS (hybrid port)
			var perlinOut = perlinNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var addRhs = addNode.GetInputPortByName(FastNoiseEditorNode.HybridPortPrefix + "rhs");
			Assert.That(addRhs, Is.Not.Null, "Add 'rhs' hybrid port not found");
			GraphToolkitBridge.CreateWire(graph, addRhs, perlinOut);

			// Wire Add → Output
			var addOut = addNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var outputIn = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);
			GraphToolkitBridge.CreateWire(graph, outputIn, addOut);

			string encoded = FastNoiseGraphCompiler.Compile(graph, out string error);
			Assert.That(encoded, Is.Not.Null.And.Not.Empty, $"Compile failed: {error}");

			using FastNoise fn = FastNoise.FromEncodedNodeTree(encoded);
			Assert.That(fn.IsCreated, Is.True);

			float[] data = new float[32 * 32];
			FastNoise.OutputMinMax minMax = fn.GenUniformGrid2D(
				data, 0f, 0f, 32, 32, 0.02f, 0.02f, 1337);
			Assert.That(minMax.max, Is.GreaterThan(minMax.min));
		}

		// ───────────── Preview ─────────────

		[Test]
		public void Preview_FromCompiledGraph_ProducesTexture()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);

			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			GraphToolkitBridge.CreateNode(graph, simplexNode, new Vector2(0, 0));

			var outputNode = new FastNoiseOutputNode();
			GraphToolkitBridge.CreateNode(graph, outputNode, new Vector2(300, 0));

			var fromPort = simplexNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var toPort = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);
			GraphToolkitBridge.CreateWire(graph, toPort, fromPort);

			string encoded = FastNoiseGraphCompiler.Compile(graph);
			Assert.That(encoded, Is.Not.Null.And.Not.Empty);

			Texture2D preview = FastNoisePreview.RenderPreview(encoded, 64, 64);
			Assert.That(preview, Is.Not.Null, "Preview returned null");
			Assert.That(preview.width, Is.EqualTo(64));
			Assert.That(preview.height, Is.EqualTo(64));
			Object.DestroyImmediate(preview);
		}

		// ───────────── Metadata cache ─────────────

		[Test]
		public void MetadataCache_ReturnsNodeTypes()
		{
			string[] types = FN2MetadataCache.GetAllNodeTypeNames();
			Assert.That(types, Is.Not.Null);
			Assert.That(types.Length, Is.GreaterThan(0), "No node types returned");
		}

		[Test]
		public void MetadataCache_ContainsKnownTypes()
		{
			string[] types = FN2MetadataCache.GetAllNodeTypeNames();
			Assert.That(types, Does.Contain("Simplex"));
			Assert.That(types, Does.Contain("Perlin"));
			Assert.That(types, Does.Contain("FractalFBm"));
		}

		// ───────────── Decompiler round-trip ─────────────

		[Test]
		public void Decompile_KnownEncoded_CreatesNodes()
		{
			var graph = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);
			int initialCount = graph.nodeCount;

			// Known Simplex encoded string
			string simplexEncoded = "Bg@AMhCBA==";
			bool result = FastNoiseGraphDecompiler.Decompile(simplexEncoded, graph);
			Assert.That(result, Is.True, "Decompile returned false");
			Assert.That(graph.nodeCount, Is.GreaterThan(initialCount),
				"Decompile did not add any nodes");
		}

		[Test]
		public void CompileDecompileRoundTrip_ProducesSameNoise()
		{
			// Step 1: Build a graph manually
			var graph1 = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(m_TestGraphPath);

			var simplexNode = new FastNoiseEditorNode { nodeTypeName = "Simplex" };
			GraphToolkitBridge.CreateNode(graph1, simplexNode, new Vector2(0, 0));

			var outputNode = new FastNoiseOutputNode();
			GraphToolkitBridge.CreateNode(graph1, outputNode, new Vector2(300, 0));

			var fromPort = simplexNode.GetOutputPortByName(FastNoiseEditorNode.OutputPortName);
			var toPort = outputNode.GetInputPortByName(FastNoiseOutputNode.InputPortName);
			GraphToolkitBridge.CreateWire(graph1, toPort, fromPort);

			// Step 2: Compile
			string encoded1 = FastNoiseGraphCompiler.Compile(graph1);
			Assert.That(encoded1, Is.Not.Null.And.Not.Empty);

			// Step 3: Decompile into a new graph
			string graph2Path = $"{TestAssetDir}/Test2_{GUID.Generate()}.fn2graph";
			try
			{
				var graph2 = GraphDatabase.CreateGraph<FastNoiseEditorGraph>(graph2Path);
				bool ok = FastNoiseGraphDecompiler.Decompile(encoded1, graph2);
				Assert.That(ok, Is.True);

				// Step 4: Recompile
				string encoded2 = FastNoiseGraphCompiler.Compile(graph2);
				Assert.That(encoded2, Is.Not.Null.And.Not.Empty);

				// Step 5: Both encoded strings should produce the same noise
				using FastNoise fn1 = FastNoise.FromEncodedNodeTree(encoded1);
				using FastNoise fn2 = FastNoise.FromEncodedNodeTree(encoded2);
				Assert.That(fn1.IsCreated, Is.True);
				Assert.That(fn2.IsCreated, Is.True);

				float[] data1 = new float[32 * 32];
				float[] data2 = new float[32 * 32];
				fn1.GenUniformGrid2D(data1, 0f, 0f, 32, 32, 0.02f, 0.02f, 1337);
				fn2.GenUniformGrid2D(data2, 0f, 0f, 32, 32, 0.02f, 0.02f, 1337);

				for (int i = 0; i < data1.Length; i++)
				{
					Assert.That(data2[i], Is.EqualTo(data1[i]).Within(0.001f),
						$"Noise mismatch at pixel {i}");
				}
			}
			finally
			{
				if (File.Exists(graph2Path))
					AssetDatabase.DeleteAsset(graph2Path);
			}
		}
#endif // FN2_USER_SIGNED
	}
}
