using System.Collections.Generic;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Tooltip descriptions for FN2 graph editor nodes and members,
	/// sourced from the FastNoise2 C++ documentation.
	/// </summary>
	static class FN2Descriptions
	{
		static readonly Dictionary<string, string> Nodes = new()
		{
			// Basic generators
			{ "Constant", "Outputs a constant float value" },
			{ "White", "Uncorrelated random values per coordinate" },
			{ "Checkerboard", "Alternating checkerboard pattern, each cell sized by feature scale" },
			{ "SineWave", "Sine wave along a configurable axis" },
			{ "Gradient", "Linear gradient: (input + offset) \u00d7 multiplier, summed across dimensions" },
			{ "DistanceToPoint", "Distance from input position to a fixed target point" },

			// Coherent noise
			{ "Simplex", "OpenSimplex2S noise with minimal directional artifacts" },
			{ "SuperSimplex", "Smoother variant of OpenSimplex" },
			{ "Perlin", "Classic Perlin gradient noise on an N-dimensional grid" },
			{ "Value", "Interpolated random values on a grid" },

			// Cellular
			{ "CellularValue", "Returns the value of the Nth closest cell point" },
			{ "CellularDistance", "Returns distance to cell points, combining Index0 and Index1 by return type" },
			{ "CellularLookup", "Evaluates a lookup source at the nearest cell center" },

			// Blends
			{ "Add", "Arithmetic addition of two sources" },
			{ "Subtract", "Arithmetic subtraction of two sources" },
			{ "Multiply", "Arithmetic multiplication of two sources" },
			{ "Divide", "Arithmetic division of two sources" },
			{ "Modulus", "Arithmetic modulus of two sources" },
			{ "Min", "Select minimum of two inputs" },
			{ "Max", "Select maximum of two inputs" },
			{ "MinSmooth", "Quadratic smooth minimum (see iquilezles.org/articles/smin)" },
			{ "MaxSmooth", "Quadratic smooth maximum (see iquilezles.org/articles/smin)" },
			{ "Fade", "Blends between A and B; Fade Min = 100% A, Fade Max = 100% B" },
			{ "PowFloat", "std::powf(value, pow)" },
			{ "PowInt", "Integer power (faster than PowFloat)" },

			// Fractals
			{ "FractalFBm", "Fractional Brownian Motion \u2014 layered octaves for terrain/clouds" },
			{ "FractalRidged", "Sharp ridges and valleys \u2014 inverted octaves for mountain ranges" },
			{ "PingPong", "Bounces values between extremes, producing contour-like flow patterns" },

			// Domain warp
			{ "DomainWarpGradient", "Warps position using uniform grid gradient, similar to Perlin gradients" },
			{ "DomainWarpSimplex", "Higher quality domain warp using simplex grid" },
			{ "DomainWarpSuperSimplex", "Smoothest domain warp, highest quality at cost of performance" },
			{ "DomainWarpFractalProgressive", "Each octave's warped output feeds the next octave" },
			{ "DomainWarpFractalIndependent", "All octaves receive original position, warp offsets accumulate" },

			// Modifiers
			{ "Abs", "Absolute value of source output" },
			{ "SignedSquareRoot", "Square root preserving original sign" },
			{ "SeedOffset", "Offsets seed before passing to source" },
			{ "Remap", "Remaps output from one range to another, optionally clamped" },
			{ "ConvertRGBA8", "Converts float to greyscale RGBA8" },
			{ "Terrace", "Cuts values into steps for terraced terrain" },
			{ "GeneratorCache", "Caches output for identical coordinates and seeds" },

			// Domain transforms
			{ "DomainScale", "Uniform scale of input coordinates" },
			{ "DomainOffset", "Adds offset to input coordinates" },
			{ "DomainRotate", "Rotates input coordinates around origin" },
			{ "DomainAxisScale", "Scales each axis independently" },
			{ "DomainRotatePlane", "Preset rotation to reduce axis-aligned artifacts in 3D" },

			// Dimension
			{ "AddDimension", "Adds a dimension (always the last)" },
			{ "RemoveDimension", "Removes specified dimension from input coordinates" },
		};

		/// <summary>
		/// Member descriptions keyed by "NodeType.lookupkey".
		/// Falls back to <see cref="SharedMembers"/> when no node-specific entry exists.
		/// </summary>
		static readonly Dictionary<string, string> Members = new()
		{
			// Cellular - Distance Function
			{ "CellularDistance.distancefunction", "How distance between cell points is measured" },
			{ "CellularValue.distancefunction", "How distance between cell points is measured" },
			{ "CellularLookup.distancefunction", "How distance between cell points is measured" },
			{ "DistanceToPoint.distancefunction", "How distance to the target point is measured" },

			// Cellular - Return Type
			{ "CellularDistance.returntype", "How Index0 and Index1 distances are combined" },
			{ "CellularDistance.distanceindex0", "Rank of the first distance value (0 = closest)" },
			{ "CellularDistance.distanceindex1", "Rank of the second distance value (1 = second closest)" },
			{ "CellularValue.valueindex", "Rank of the cell point whose value is returned (0 = closest)" },

			// Fade
			{ "Fade.interpolation", "Smoothing curve applied to the fade factor" },
			{ "Fade.fade", "Blend factor between A and B" },
			{ "Fade.fademin", "Fade value that maps to 100% A" },
			{ "Fade.fademax", "Fade value that maps to 100% B" },

			// Remap
			{ "Remap.frommin", "Lower bound of the input range" },
			{ "Remap.frommax", "Upper bound of the input range" },
			{ "Remap.tomin", "Lower bound of the output range" },
			{ "Remap.tomax", "Upper bound of the output range" },

			// ConvertRGBA8
			{ "ConvertRGBA8.min", "Input value that maps to black (0)" },
			{ "ConvertRGBA8.max", "Input value that maps to white (255)" },

			// Terrace
			{ "Terrace.stepcount", "Number of discrete terrace levels" },
			{ "Terrace.smoothness", "How rounded the terrace edges are (0 = sharp steps)" },

			// DomainRotate
			{ "DomainRotate.yaw", "Rotation around the vertical axis (degrees)" },
			{ "DomainRotate.pitch", "Rotation around the lateral axis (degrees)" },
			{ "DomainRotate.roll", "Rotation around the forward axis (degrees)" },

			// DomainRotatePlane
			{ "DomainRotatePlane.rotationtype", "Which plane pair to rotate for artifact reduction" },

			// RemoveDimension
			{ "RemoveDimension.removedimension", "Which axis to remove from the input" },

			// AddDimension
			{ "AddDimension.newdimensionposition", "Coordinate value for the new dimension" },

			// Domain Warp
			{ "DomainWarpSimplex.vectorizationscheme", "Method used to compute warp vectors" },
			{ "DomainWarpSuperSimplex.vectorizationscheme", "Method used to compute warp vectors" },

			// PowInt
			{ "PowInt.pow", "Integer exponent (faster than float power)" },

			// PingPong
			{ "PingPong.pingpongstrength", "Amplitude of the ping-pong bounce effect" },
		};

		/// <summary>
		/// Shared member descriptions keyed by lookupkey alone.
		/// Used as fallback when no node-specific entry exists in <see cref="Members"/>.
		/// </summary>
		static readonly Dictionary<string, string> SharedMembers = new()
		{
			{ "featurescale", "Size of one noise period in world units" },
			{ "seedoffset", "Offsets the random seed for variation" },
			{ "octaves", "Number of noise layers to combine" },
			{ "lacunarity", "Frequency multiplier between successive octaves" },
			{ "gain", "Amplitude multiplier between successive octaves" },
			{ "weightedstrength", "How much each octave's amplitude depends on the previous octave's output" },
			{ "warpamplitude", "Strength of coordinate distortion" },
			{ "scaling", "Uniform scale factor applied to all axes" },
			{ "gridjitter", "How far cell points wander from grid centers (0\u20131)" },
			{ "sizejitter", "Random variation in cell size (0\u20131)" },
			{ "minkowskip", "Minkowski distance exponent (1 = Manhattan, 2 = Euclidean)" },
			{ "smoothness", "Blending radius for smooth min/max" },
		};

		public static string GetNodeDescription(string nodeTypeName)
		{
			return Nodes.TryGetValue(nodeTypeName, out var desc) ? desc : null;
		}

		public static string GetMemberDescription(string nodeTypeName, string lookupKey)
		{
			if (Members.TryGetValue(nodeTypeName + "." + lookupKey, out var desc))
				return desc;
			if (SharedMembers.TryGetValue(lookupKey, out desc))
				return desc;
			return null;
		}
	}
}
