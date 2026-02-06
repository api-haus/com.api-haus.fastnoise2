using System;

namespace FastNoise2.Editor.GraphEditor.Nodes
{
	[Serializable] public class CellularValue : FN2EditorNode { public override string NodeTypeName => "CellularValue"; }
	[Serializable] public class CellularDistance : FN2EditorNode { public override string NodeTypeName => "CellularDistance"; }
	[Serializable] public class CellularLookup : FN2EditorNode { public override string NodeTypeName => "CellularLookup"; }
}
