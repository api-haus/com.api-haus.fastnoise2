using System;
using System.Collections.Generic;

namespace FastNoise2.Generators
{
	/// <summary>
	/// Pure C# metadata registry for all FastNoise2 node types.
	/// Mirrors the native <c>FastNoise.Metadata</c> but requires no native binary.
	/// Node IDs and member indices match the C++ registration order in FastSIMD_Build.inl.
	/// </summary>
	static class FN2NodeRegistry
	{
		static readonly FN2NodeDef[] s_NodeDefs;
		static readonly Dictionary<string, FN2NodeDef> s_ByName;
		static readonly string[] s_AllNodeNames;

		static readonly string[] DistanceFunctionEnum =
			{ "Euclidean", "EuclideanSquared", "Manhattan", "Hybrid", "MaxAxis", "Minkowski" };

		static readonly string[] CellularReturnTypeEnum =
			{ "Index0", "Index0Add1", "Index0Sub1", "Index0Mul1", "Index0Div1" };

		static readonly string[] FadeInterpolationEnum =
			{ "Linear", "Hermite", "Quintic" };

		static readonly string[] VectorizationSchemeEnum =
			{ "OrthogonalGradientMatrix", "GradientOuterProduct" };

		static readonly string[] PlaneRotationEnum =
			{ "ImproveXYPlanes", "ImproveXZPlanes" };

		static readonly string[] DimensionEnum =
			{ "X", "Y", "Z", "W" };

		static readonly string[] BooleanEnum =
			{ "False", "True" };

		static FN2NodeRegistry()
		{
			var defs = new List<FN2NodeDef>(47);

			// Helper aliases
			const FN2MemberType F = FN2MemberType.Float;
			const FN2MemberType I = FN2MemberType.Int;
			const FN2MemberType E = FN2MemberType.Enum;
			const FN2MemberType NL = FN2MemberType.NodeLookup;
			const FN2MemberType H = FN2MemberType.Hybrid;

			// 0: Constant
			defs.Add(Def(0, "Constant",
				new FN2MemberDef("Value", F, 0)
			));

			// 1: White — inherits VariableRange<Seeded<Generator>>
			// Seeded: SeedOffset(I,0) → VariableRange: OutputMin(F,1), OutputMax(F,2)
			defs.Add(Def(1, "White",
				new FN2MemberDef("SeedOffset", I, 0),
				new FN2MemberDef("OutputMin", F, 1),
				new FN2MemberDef("OutputMax", F, 2)
			));

			// 2: Checkerboard — inherits VariableRange<ScalableGenerator>
			// ScalableGenerator: FeatureScale(F,0) → VariableRange: OutputMin(F,1), OutputMax(F,2)
			defs.Add(Def(2, "Checkerboard",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("OutputMin", F, 1),
				new FN2MemberDef("OutputMax", F, 2)
			));

			// 3: SineWave — inherits VariableRange<ScalableGenerator>
			defs.Add(Def(3, "SineWave",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("OutputMin", F, 1),
				new FN2MemberDef("OutputMax", F, 2)
			));

			// 4: Gradient
			// PerDimensionVariable "Multiplier" → multiplierx(F,0)..w(F,3)
			// PerDimensionHybrid "Offset" → offsetx(H,0)..w(H,3)
			defs.Add(Def(4, "Gradient",
				new FN2MemberDef("MultiplierX", F, 0),
				new FN2MemberDef("MultiplierY", F, 1),
				new FN2MemberDef("MultiplierZ", F, 2),
				new FN2MemberDef("MultiplierW", F, 3),
				new FN2MemberDef("OffsetX", H, 0),
				new FN2MemberDef("OffsetY", H, 1),
				new FN2MemberDef("OffsetZ", H, 2),
				new FN2MemberDef("OffsetW", H, 3)
			));

			// 5: DistanceToPoint
			// DistanceFunction(E,0), PerDimensionHybrid "Point" x..w(H,0..3), MinkowskiP(H,4)
			defs.Add(Def(5, "DistanceToPoint",
				new FN2MemberDef("DistanceFunction", E, 0, DistanceFunctionEnum),
				new FN2MemberDef("PointX", H, 0),
				new FN2MemberDef("PointY", H, 1),
				new FN2MemberDef("PointZ", H, 2),
				new FN2MemberDef("PointW", H, 3),
				new FN2MemberDef("MinkowskiP", H, 4)
			));

			// 6: Simplex — VariableRange<Seeded<ScalableGenerator>>
			// FeatureScale(F,0), SeedOffset(I,1), OutputMin(F,2), OutputMax(F,3)
			defs.Add(Def(6, "Simplex",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("OutputMin", F, 2),
				new FN2MemberDef("OutputMax", F, 3)
			));

			// 7: SuperSimplex — same as Simplex
			defs.Add(Def(7, "SuperSimplex",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("OutputMin", F, 2),
				new FN2MemberDef("OutputMax", F, 3)
			));

			// 8: Perlin — same as Simplex
			defs.Add(Def(8, "Perlin",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("OutputMin", F, 2),
				new FN2MemberDef("OutputMax", F, 3)
			));

			// 9: Value — same as Simplex
			defs.Add(Def(9, "Value",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("OutputMin", F, 2),
				new FN2MemberDef("OutputMax", F, 3)
			));

			// 10: CellularValue — Cellular<> = Cellular<VariableRange<Seeded<ScalableGenerator>>>
			// FeatureScale(F,0), SeedOffset(I,1), OutputMin(F,2), OutputMax(F,3),
			// DistanceFunction(E,4), ValueIndex(I,5)
			// Hybrids: MinkowskiP(H,0), GridJitter(H,1), SizeJitter(H,2)
			defs.Add(Def(10, "CellularValue",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("OutputMin", F, 2),
				new FN2MemberDef("OutputMax", F, 3),
				new FN2MemberDef("DistanceFunction", E, 4, DistanceFunctionEnum),
				new FN2MemberDef("ValueIndex", I, 5),
				new FN2MemberDef("MinkowskiP", H, 0),
				new FN2MemberDef("GridJitter", H, 1),
				new FN2MemberDef("SizeJitter", H, 2)
			));

			// 11: CellularDistance — Cellular<>
			// FeatureScale(F,0), SeedOffset(I,1), OutputMin(F,2), OutputMax(F,3),
			// DistanceFunction(E,4), DistanceIndex0(I,5), DistanceIndex1(I,6), ReturnType(E,7)
			// Hybrids: MinkowskiP(H,0), GridJitter(H,1), SizeJitter(H,2)
			defs.Add(Def(11, "CellularDistance",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("OutputMin", F, 2),
				new FN2MemberDef("OutputMax", F, 3),
				new FN2MemberDef("DistanceFunction", E, 4, DistanceFunctionEnum),
				new FN2MemberDef("DistanceIndex0", I, 5),
				new FN2MemberDef("DistanceIndex1", I, 6),
				new FN2MemberDef("ReturnType", E, 7, CellularReturnTypeEnum),
				new FN2MemberDef("MinkowskiP", H, 0),
				new FN2MemberDef("GridJitter", H, 1),
				new FN2MemberDef("SizeJitter", H, 2)
			));

			// 12: CellularLookup — Cellular<Seeded<ScalableGenerator>>
			// FeatureScale(F,0), SeedOffset(I,1), DistanceFunction(E,2)
			// NodeLookup: Lookup(NL,0)
			// Hybrids: MinkowskiP(H,0), GridJitter(H,1), SizeJitter(H,2)
			defs.Add(Def(12, "CellularLookup",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("DistanceFunction", E, 2, DistanceFunctionEnum),
				new FN2MemberDef("MinkowskiP", H, 0),
				new FN2MemberDef("GridJitter", H, 1),
				new FN2MemberDef("SizeJitter", H, 2),
				new FN2MemberDef("Lookup", NL, 0)
			));

			// 13: FractalFBm — Fractal<>
			// Source(NL,0), Gain(H,0), WeightedStrength(H,1), Octaves(I,0), Lacunarity(F,1)
			defs.Add(Def(13, "FractalFBm",
				new FN2MemberDef("Octaves", I, 0),
				new FN2MemberDef("Lacunarity", F, 1),
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("Gain", H, 0),
				new FN2MemberDef("WeightedStrength", H, 1)
			));

			// 14: PingPong — Modifier (NOT Fractal)
			// Source(NL,0), PingPongStrength(H,0)
			defs.Add(Def(14, "PingPong",
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("PingPongStrength", H, 0)
			));

			// 15: FractalRidged — Fractal<>
			defs.Add(Def(15, "FractalRidged",
				new FN2MemberDef("Octaves", I, 0),
				new FN2MemberDef("Lacunarity", F, 1),
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("Gain", H, 0),
				new FN2MemberDef("WeightedStrength", H, 1)
			));

			// 16: DomainWarpSimplex — DomainWarp + VectorizationScheme
			// DomainWarp inherits Seeded<ScalableGenerator>:
			//   FeatureScale(F,0), SeedOffset(I,1)
			// DomainWarp adds: Source(NL,0), WarpAmplitude(H,0),
			//   AmplitudeScalingX(F,2)..W(F,5)
			// DomainWarpSimplex adds: VectorizationScheme(E,6)
			defs.Add(Def(16, "DomainWarpSimplex",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("AmplitudeScalingX", F, 2),
				new FN2MemberDef("AmplitudeScalingY", F, 3),
				new FN2MemberDef("AmplitudeScalingZ", F, 4),
				new FN2MemberDef("AmplitudeScalingW", F, 5),
				new FN2MemberDef("VectorizationScheme", E, 6, VectorizationSchemeEnum),
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("WarpAmplitude", H, 0)
			));

			// 17: DomainWarpSuperSimplex — inherits DomainWarpSimplex (same members)
			defs.Add(Def(17, "DomainWarpSuperSimplex",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("AmplitudeScalingX", F, 2),
				new FN2MemberDef("AmplitudeScalingY", F, 3),
				new FN2MemberDef("AmplitudeScalingZ", F, 4),
				new FN2MemberDef("AmplitudeScalingW", F, 5),
				new FN2MemberDef("VectorizationScheme", E, 6, VectorizationSchemeEnum),
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("WarpAmplitude", H, 0)
			));

			// 18: DomainWarpGradient — DomainWarp (no VectorizationScheme)
			defs.Add(Def(18, "DomainWarpGradient",
				new FN2MemberDef("FeatureScale", F, 0),
				new FN2MemberDef("SeedOffset", I, 1),
				new FN2MemberDef("AmplitudeScalingX", F, 2),
				new FN2MemberDef("AmplitudeScalingY", F, 3),
				new FN2MemberDef("AmplitudeScalingZ", F, 4),
				new FN2MemberDef("AmplitudeScalingW", F, 5),
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("WarpAmplitude", H, 0)
			));

			// 19: DomainWarpFractalProgressive — Fractal<DomainWarp>
			// DomainWarpSource(NL,0), Gain(H,0), WeightedStrength(H,1),
			// Octaves(I,0), Lacunarity(F,1)
			defs.Add(Def(19, "DomainWarpFractalProgressive",
				new FN2MemberDef("Octaves", I, 0),
				new FN2MemberDef("Lacunarity", F, 1),
				new FN2MemberDef("DomainWarpSource", NL, 0),
				new FN2MemberDef("Gain", H, 0),
				new FN2MemberDef("WeightedStrength", H, 1)
			));

			// 20: DomainWarpFractalIndependent — same as Progressive
			defs.Add(Def(20, "DomainWarpFractalIndependent",
				new FN2MemberDef("Octaves", I, 0),
				new FN2MemberDef("Lacunarity", F, 1),
				new FN2MemberDef("DomainWarpSource", NL, 0),
				new FN2MemberDef("Gain", H, 0),
				new FN2MemberDef("WeightedStrength", H, 1)
			));

			// 21: Add — OperatorSourceLHS
			// LHS(NL,0), RHS(H,0)
			defs.Add(Def(21, "Add",
				new FN2MemberDef("LHS", NL, 0),
				new FN2MemberDef("RHS", H, 0)
			));

			// 22: Subtract — OperatorHybridLHS
			// LHS(H,0), RHS(H,1)
			defs.Add(Def(22, "Subtract",
				new FN2MemberDef("LHS", H, 0),
				new FN2MemberDef("RHS", H, 1)
			));

			// 23: Multiply — OperatorSourceLHS
			defs.Add(Def(23, "Multiply",
				new FN2MemberDef("LHS", NL, 0),
				new FN2MemberDef("RHS", H, 0)
			));

			// 24: Divide — OperatorHybridLHS
			defs.Add(Def(24, "Divide",
				new FN2MemberDef("LHS", H, 0),
				new FN2MemberDef("RHS", H, 1)
			));

			// 25: Abs — Modifier
			defs.Add(Def(25, "Abs",
				new FN2MemberDef("Source", NL, 0)
			));

			// 26: Min — OperatorSourceLHS
			defs.Add(Def(26, "Min",
				new FN2MemberDef("LHS", NL, 0),
				new FN2MemberDef("RHS", H, 0)
			));

			// 27: Max — OperatorSourceLHS
			defs.Add(Def(27, "Max",
				new FN2MemberDef("LHS", NL, 0),
				new FN2MemberDef("RHS", H, 0)
			));

			// 28: MinSmooth — OperatorSourceLHS + Smoothness
			defs.Add(Def(28, "MinSmooth",
				new FN2MemberDef("LHS", NL, 0),
				new FN2MemberDef("RHS", H, 0),
				new FN2MemberDef("Smoothness", H, 1)
			));

			// 29: MaxSmooth — OperatorSourceLHS + Smoothness
			defs.Add(Def(29, "MaxSmooth",
				new FN2MemberDef("LHS", NL, 0),
				new FN2MemberDef("RHS", H, 0),
				new FN2MemberDef("Smoothness", H, 1)
			));

			// 30: SignedSquareRoot — Modifier
			defs.Add(Def(30, "SignedSquareRoot",
				new FN2MemberDef("Source", NL, 0)
			));

			// 31: PowFloat
			// Value(H,0), Pow(H,1)
			defs.Add(Def(31, "PowFloat",
				new FN2MemberDef("Value", H, 0),
				new FN2MemberDef("Pow", H, 1)
			));

			// 32: PowInt
			// Value(NL,0), Pow(I,0)
			defs.Add(Def(32, "PowInt",
				new FN2MemberDef("Pow", I, 0),
				new FN2MemberDef("Value", NL, 0)
			));

			// 33: DomainScale
			// Source(NL,0), Scaling(F,0)
			defs.Add(Def(33, "DomainScale",
				new FN2MemberDef("Scaling", F, 0),
				new FN2MemberDef("Source", NL, 0)
			));

			// 34: DomainOffset
			// Source(NL,0), PerDimensionHybrid OffsetX..W(H,0..3)
			defs.Add(Def(34, "DomainOffset",
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("OffsetX", H, 0),
				new FN2MemberDef("OffsetY", H, 1),
				new FN2MemberDef("OffsetZ", H, 2),
				new FN2MemberDef("OffsetW", H, 3)
			));

			// 35: DomainRotate
			// Source(NL,0), Yaw(F,0), Pitch(F,1), Roll(F,2)
			defs.Add(Def(35, "DomainRotate",
				new FN2MemberDef("Yaw", F, 0),
				new FN2MemberDef("Pitch", F, 1),
				new FN2MemberDef("Roll", F, 2),
				new FN2MemberDef("Source", NL, 0)
			));

			// 36: DomainAxisScale
			// Source(NL,0), PerDimensionVariable ScalingX..W(F,0..3)
			defs.Add(Def(36, "DomainAxisScale",
				new FN2MemberDef("ScalingX", F, 0),
				new FN2MemberDef("ScalingY", F, 1),
				new FN2MemberDef("ScalingZ", F, 2),
				new FN2MemberDef("ScalingW", F, 3),
				new FN2MemberDef("Source", NL, 0)
			));

			// 37: SeedOffset (Modifier)
			// Source(NL,0), SeedOffset(I,0)
			defs.Add(Def(37, "SeedOffset",
				new FN2MemberDef("SeedOffset", I, 0),
				new FN2MemberDef("Source", NL, 0)
			));

			// 38: ConvertRGBA8
			// Source(NL,0), Min(F,0), Max(F,1)
			defs.Add(Def(38, "ConvertRGBA8",
				new FN2MemberDef("Min", F, 0),
				new FN2MemberDef("Max", F, 1),
				new FN2MemberDef("Source", NL, 0)
			));

			// 39: GeneratorCache
			// Source(NL,0)
			defs.Add(Def(39, "GeneratorCache",
				new FN2MemberDef("Source", NL, 0)
			));

			// 40: Fade
			// A(NL,0), B(NL,1), Fade(H,0), FadeMin(H,1), FadeMax(H,2),
			// Interpolation(E,0)
			defs.Add(Def(40, "Fade",
				new FN2MemberDef("Interpolation", E, 0, FadeInterpolationEnum),
				new FN2MemberDef("A", NL, 0),
				new FN2MemberDef("B", NL, 1),
				new FN2MemberDef("Fade", H, 0),
				new FN2MemberDef("FadeMin", H, 1),
				new FN2MemberDef("FadeMax", H, 2)
			));

			// 41: Remap
			// Source(NL,0), FromMin(H,0), FromMax(H,1), ToMin(H,2), ToMax(H,3),
			// ClampOutput(E,0)
			defs.Add(Def(41, "Remap",
				new FN2MemberDef("ClampOutput", E, 0, BooleanEnum),
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("FromMin", H, 0),
				new FN2MemberDef("FromMax", H, 1),
				new FN2MemberDef("ToMin", H, 2),
				new FN2MemberDef("ToMax", H, 3)
			));

			// 42: Terrace
			// Source(NL,0), StepCount(F,0), Smoothness(H,0)
			defs.Add(Def(42, "Terrace",
				new FN2MemberDef("StepCount", F, 0),
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("Smoothness", H, 0)
			));

			// 43: AddDimension
			// Source(NL,0), NewDimensionPosition(H,0)
			defs.Add(Def(43, "AddDimension",
				new FN2MemberDef("Source", NL, 0),
				new FN2MemberDef("NewDimensionPosition", H, 0)
			));

			// 44: RemoveDimension
			// Source(NL,0), RemoveDimension(E,0)
			defs.Add(Def(44, "RemoveDimension",
				new FN2MemberDef("RemoveDimension", E, 0, DimensionEnum),
				new FN2MemberDef("Source", NL, 0)
			));

			// 45: Modulus — OperatorHybridLHS
			defs.Add(Def(45, "Modulus",
				new FN2MemberDef("LHS", H, 0),
				new FN2MemberDef("RHS", H, 1)
			));

			// 46: DomainRotatePlane
			// Source(NL,0), RotationType(E,0)
			defs.Add(Def(46, "DomainRotatePlane",
				new FN2MemberDef("RotationType", E, 0, PlaneRotationEnum),
				new FN2MemberDef("Source", NL, 0)
			));

			s_NodeDefs = defs.ToArray();
			s_ByName = new Dictionary<string, FN2NodeDef>(
				s_NodeDefs.Length, StringComparer.OrdinalIgnoreCase);
			s_AllNodeNames = new string[s_NodeDefs.Length];

			for (int i = 0; i < s_NodeDefs.Length; i++)
			{
				s_ByName[s_NodeDefs[i].NodeName] = s_NodeDefs[i];
				s_AllNodeNames[i] = s_NodeDefs[i].NodeName;
			}
		}

		static FN2NodeDef Def(int id, string name, params FN2MemberDef[] members) =>
			new(id, name, members);

		public static FN2NodeDef GetNodeDef(string nodeTypeName)
		{
			if (s_ByName.TryGetValue(nodeTypeName, out var def))
				return def;
			throw new ArgumentException($"Unknown FN2 node type: {nodeTypeName}");
		}

		public static FN2NodeDef GetNodeDefById(int id)
		{
			if (id < 0 || id >= s_NodeDefs.Length)
				return null;
			return s_NodeDefs[id];
		}

		public static string[] AllNodeNames => s_AllNodeNames;

		public static int NodeCount => s_NodeDefs.Length;
	}
}
