using System;

namespace FastNoise2.Editor.GraphEditor.Nodes
{
	[Serializable] public class Add : FN2EditorNode { public override string NodeTypeName => "Add"; }
	[Serializable] public class Subtract : FN2EditorNode { public override string NodeTypeName => "Subtract"; }
	[Serializable] public class Multiply : FN2EditorNode { public override string NodeTypeName => "Multiply"; }
	[Serializable] public class Divide : FN2EditorNode { public override string NodeTypeName => "Divide"; }
	[Serializable] public class Min : FN2EditorNode { public override string NodeTypeName => "Min"; }
	[Serializable] public class Max : FN2EditorNode { public override string NodeTypeName => "Max"; }
	[Serializable] public class MinSmooth : FN2EditorNode { public override string NodeTypeName => "MinSmooth"; }
	[Serializable] public class MaxSmooth : FN2EditorNode { public override string NodeTypeName => "MaxSmooth"; }
	[Serializable] public class Fade : FN2EditorNode { public override string NodeTypeName => "Fade"; }
	[Serializable] public class PowFloat : FN2EditorNode { public override string NodeTypeName => "PowFloat"; }
	[Serializable] public class Modulus : FN2EditorNode { public override string NodeTypeName => "Modulus"; }
}
