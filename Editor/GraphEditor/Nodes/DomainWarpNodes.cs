using System;

namespace FastNoise2.Editor.GraphEditor.Nodes
{
	[Serializable] public class DomainWarpSimplex : FN2EditorNode { public override string NodeTypeName => "DomainWarpSimplex"; }
	[Serializable] public class DomainWarpSuperSimplex : FN2EditorNode { public override string NodeTypeName => "DomainWarpSuperSimplex"; }
	[Serializable] public class DomainWarpGradient : FN2EditorNode { public override string NodeTypeName => "DomainWarpGradient"; }
	[Serializable] public class DomainWarpFractalProgressive : FN2EditorNode { public override string NodeTypeName => "DomainWarpFractalProgressive"; }
	[Serializable] public class DomainWarpFractalIndependent : FN2EditorNode { public override string NodeTypeName => "DomainWarpFractalIndependent"; }
}
