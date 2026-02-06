using System;

namespace FastNoise2.Editor.GraphEditor.Nodes
{
	[Serializable] public class Constant : FN2EditorNode { public override string NodeTypeName => "Constant"; }
	[Serializable] public class White : FN2EditorNode { public override string NodeTypeName => "White"; }
	[Serializable] public class Checkerboard : FN2EditorNode { public override string NodeTypeName => "Checkerboard"; }
	[Serializable] public class SineWave : FN2EditorNode { public override string NodeTypeName => "SineWave"; }
	[Serializable] public class Gradient : FN2EditorNode { public override string NodeTypeName => "Gradient"; }
	[Serializable] public class DistanceToPoint : FN2EditorNode { public override string NodeTypeName => "DistanceToPoint"; }
	[Serializable] public class Simplex : FN2EditorNode { public override string NodeTypeName => "Simplex"; }
	[Serializable] public class SuperSimplex : FN2EditorNode { public override string NodeTypeName => "SuperSimplex"; }
	[Serializable] public class Perlin : FN2EditorNode { public override string NodeTypeName => "Perlin"; }
	[Serializable] public class Value : FN2EditorNode { public override string NodeTypeName => "Value"; }
}
