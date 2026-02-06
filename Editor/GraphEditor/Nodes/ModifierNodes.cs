using System;

namespace FastNoise2.Editor.GraphEditor.Nodes
{
	[Serializable] public class PingPong : FN2EditorNode { public override string NodeTypeName => "PingPong"; }
	[Serializable] public class Abs : FN2EditorNode { public override string NodeTypeName => "Abs"; }
	[Serializable] public class SignedSquareRoot : FN2EditorNode { public override string NodeTypeName => "SignedSquareRoot"; }
	[Serializable] public class PowInt : FN2EditorNode { public override string NodeTypeName => "PowInt"; }
	[Serializable] public class DomainScale : FN2EditorNode { public override string NodeTypeName => "DomainScale"; }
	[Serializable] public class DomainOffset : FN2EditorNode { public override string NodeTypeName => "DomainOffset"; }
	[Serializable] public class DomainRotate : FN2EditorNode { public override string NodeTypeName => "DomainRotate"; }
	[Serializable] public class DomainAxisScale : FN2EditorNode { public override string NodeTypeName => "DomainAxisScale"; }
	[Serializable] public class SeedOffset : FN2EditorNode { public override string NodeTypeName => "SeedOffset"; }
	[Serializable] public class ConvertRGBA8 : FN2EditorNode { public override string NodeTypeName => "ConvertRGBA8"; }
	[Serializable] public class GeneratorCache : FN2EditorNode { public override string NodeTypeName => "GeneratorCache"; }
	[Serializable] public class Remap : FN2EditorNode { public override string NodeTypeName => "Remap"; }
	[Serializable] public class Terrace : FN2EditorNode { public override string NodeTypeName => "Terrace"; }
	[Serializable] public class AddDimension : FN2EditorNode { public override string NodeTypeName => "AddDimension"; }
	[Serializable] public class RemoveDimension : FN2EditorNode { public override string NodeTypeName => "RemoveDimension"; }
	[Serializable] public class DomainRotatePlane : FN2EditorNode { public override string NodeTypeName => "DomainRotatePlane"; }
}
