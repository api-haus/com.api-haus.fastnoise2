using FastNoise2.Bindings;

namespace FastNoise2.Generators
{
	using System;

	/// <summary>
	/// Deferred noise graph node. Stores a closure that materializes a <see cref="FastNoise"/>
	/// handle when <see cref="Build"/> is called. Fluent methods return new nodes that capture
	/// the parent, forming an immutable computation graph.
	/// </summary>
	public class NoiseNode
	{
		readonly Func<FastNoise> m_BuildFunc;

		internal NoiseNode(Func<FastNoise> buildFunc)
		{
			m_BuildFunc = buildFunc ?? throw new ArgumentNullException(nameof(buildFunc));
		}

		/// <summary>
		/// Materializes the noise graph into a native <see cref="FastNoise"/> handle.
		/// The caller owns the returned handle and must dispose it.
		/// </summary>
		public FastNoise Build() => m_BuildFunc();

		#region Fractal

		public NoiseNode Fbm(Hybrid gain = default, Hybrid weightedStrength = default,
			int octaves = 3, float lacunarity = 2f)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("FractalFBm");
				fn.Set("Source", source.Build());
				gain.Apply(fn, "Gain");
				weightedStrength.Apply(fn, "WeightedStrength");
				fn.Set("Octaves", octaves);
				fn.Set("Lacunarity", lacunarity);
				return fn;
			});
		}

		public NoiseNode Ridged(Hybrid gain = default, Hybrid weightedStrength = default,
			int octaves = 3, float lacunarity = 2f)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("FractalRidged");
				fn.Set("Source", source.Build());
				gain.Apply(fn, "Gain");
				weightedStrength.Apply(fn, "WeightedStrength");
				fn.Set("Octaves", octaves);
				fn.Set("Lacunarity", lacunarity);
				return fn;
			});
		}

		#endregion

		#region Blend

		public NoiseNode Min(Hybrid rhs)
		{
			NoiseNode lhs = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("Min");
				fn.Set("LHS", lhs.Build());
				rhs.Apply(fn, "RHS");
				return fn;
			});
		}

		public NoiseNode Max(Hybrid rhs)
		{
			NoiseNode lhs = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("Max");
				fn.Set("LHS", lhs.Build());
				rhs.Apply(fn, "RHS");
				return fn;
			});
		}

		public NoiseNode MinSmooth(Hybrid rhs, Hybrid smoothness = default)
		{
			NoiseNode lhs = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("MinSmooth");
				fn.Set("LHS", lhs.Build());
				rhs.Apply(fn, "RHS");
				smoothness.Apply(fn, "Smoothness");
				return fn;
			});
		}

		public NoiseNode MaxSmooth(Hybrid rhs, Hybrid smoothness = default)
		{
			NoiseNode lhs = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("MaxSmooth");
				fn.Set("LHS", lhs.Build());
				rhs.Apply(fn, "RHS");
				smoothness.Apply(fn, "Smoothness");
				return fn;
			});
		}

		public NoiseNode Fade(NoiseNode b, Hybrid fade = default,
			Hybrid fadeMin = default, Hybrid fadeMax = default,
			FadeInterpolation interpolation = FadeInterpolation.Linear)
		{
			NoiseNode a = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("Fade");
				fn.Set("A", a.Build());
				fn.Set("B", b.Build());
				fade.Apply(fn, "Fade");
				fadeMin.Apply(fn, "FadeMin");
				fadeMax.Apply(fn, "FadeMax");
				fn.Set("Interpolation", interpolation.ToMetadataString());
				return fn;
			});
		}

		public NoiseNode PowFloat(Hybrid pow)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("PowFloat");
				Hybrid sourceHybrid = source;
				sourceHybrid.Apply(fn, "Value");
				pow.Apply(fn, "Pow");
				return fn;
			});
		}

		public NoiseNode PowInt(int pow = 2)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("PowInt");
				fn.Set("Value", source.Build());
				fn.Set("Pow", pow);
				return fn;
			});
		}

		#endregion

		#region Domain Warp

		public DomainWarpNode DomainWarpGradient(Hybrid warpAmplitude = default,
			float featureScale = 100f)
		{
			NoiseNode source = this;
			return new DomainWarpNode(() =>
			{
				FastNoise fn = new("DomainWarpGradient");
				fn.Set("Source", source.Build());
				warpAmplitude.Apply(fn, "WarpAmplitude");
				fn.Set("FeatureScale", featureScale);
				return fn;
			});
		}

		public DomainWarpNode DomainWarpSimplex(Hybrid warpAmplitude = default,
			float featureScale = 100f,
			VectorizationScheme scheme = VectorizationScheme.OrthogonalGradientMatrix)
		{
			NoiseNode source = this;
			return new DomainWarpNode(() =>
			{
				FastNoise fn = new("DomainWarpSimplex");
				fn.Set("Source", source.Build());
				warpAmplitude.Apply(fn, "WarpAmplitude");
				fn.Set("FeatureScale", featureScale);
				fn.Set("VectorizationScheme", scheme.ToMetadataString());
				return fn;
			});
		}

		public DomainWarpNode DomainWarpSuperSimplex(Hybrid warpAmplitude = default,
			float featureScale = 100f,
			VectorizationScheme scheme = VectorizationScheme.OrthogonalGradientMatrix)
		{
			NoiseNode source = this;
			return new DomainWarpNode(() =>
			{
				FastNoise fn = new("DomainWarpSuperSimplex");
				fn.Set("Source", source.Build());
				warpAmplitude.Apply(fn, "WarpAmplitude");
				fn.Set("FeatureScale", featureScale);
				fn.Set("VectorizationScheme", scheme.ToMetadataString());
				return fn;
			});
		}

		#endregion

		#region Modifiers

		public NoiseNode DomainScale(float scaling = 1f)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("DomainScale");
				fn.Set("Source", source.Build());
				fn.Set("Scaling", scaling);
				return fn;
			});
		}

		public NoiseNode DomainOffset(Hybrid x = default, Hybrid y = default,
			Hybrid z = default, Hybrid w = default)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("DomainOffset");
				fn.Set("Source", source.Build());
				x.Apply(fn, "OffsetX");
				y.Apply(fn, "OffsetY");
				z.Apply(fn, "OffsetZ");
				w.Apply(fn, "OffsetW");
				return fn;
			});
		}

		public NoiseNode DomainRotate(float yaw = 0f, float pitch = 0f, float roll = 0f)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("DomainRotate");
				fn.Set("Source", source.Build());
				fn.Set("Yaw", yaw);
				fn.Set("Pitch", pitch);
				fn.Set("Roll", roll);
				return fn;
			});
		}

		public NoiseNode DomainAxisScale(float x = 1f, float y = 1f,
			float z = 1f, float w = 1f)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("DomainAxisScale");
				fn.Set("Source", source.Build());
				fn.Set("ScalingX", x);
				fn.Set("ScalingY", y);
				fn.Set("ScalingZ", z);
				fn.Set("ScalingW", w);
				return fn;
			});
		}

		public NoiseNode DomainRotatePlane(
			PlaneRotationType type = PlaneRotationType.ImproveXYPlanes)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("DomainRotatePlane");
				fn.Set("Source", source.Build());
				fn.Set("RotationType", type.ToMetadataString());
				return fn;
			});
		}

		public NoiseNode SeedOffset(int offset = 0)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("SeedOffset");
				fn.Set("Source", source.Build());
				fn.Set("SeedOffset", offset);
				return fn;
			});
		}

		public NoiseNode Remap(Hybrid fromMin = default, Hybrid fromMax = default,
			Hybrid toMin = default, Hybrid toMax = default)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("Remap");
				fn.Set("Source", source.Build());
				fromMin.Apply(fn, "FromMin");
				fromMax.Apply(fn, "FromMax");
				toMin.Apply(fn, "ToMin");
				toMax.Apply(fn, "ToMax");
				return fn;
			});
		}

		public NoiseNode ConvertRgba8(float min = -1f, float max = 1f)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("ConvertRGBA8");
				fn.Set("Source", source.Build());
				fn.Set("Min", min);
				fn.Set("Max", max);
				return fn;
			});
		}

		public NoiseNode Terrace(float stepCount = 4f, Hybrid smoothness = default)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("Terrace");
				fn.Set("Source", source.Build());
				fn.Set("StepCount", stepCount);
				smoothness.Apply(fn, "Smoothness");
				return fn;
			});
		}

		public NoiseNode AddDimension(Hybrid newDimensionPosition = default)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("AddDimension");
				fn.Set("Source", source.Build());
				newDimensionPosition.Apply(fn, "NewDimensionPosition");
				return fn;
			});
		}

		public NoiseNode RemoveDimension(Dimension dimension = Dimension.W)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("RemoveDimension");
				fn.Set("Source", source.Build());
				fn.Set("RemoveDimension", dimension.ToMetadataString());
				return fn;
			});
		}

		public NoiseNode Cache()
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("GeneratorCache");
				fn.Set("Source", source.Build());
				return fn;
			});
		}

		public NoiseNode PingPong(Hybrid strength = default)
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("PingPong");
				fn.Set("Source", source.Build());
				strength.Apply(fn, "PingPongStrength");
				return fn;
			});
		}

		public NoiseNode Abs()
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("Abs");
				fn.Set("Source", source.Build());
				return fn;
			});
		}

		public NoiseNode SignedSqrt()
		{
			NoiseNode source = this;
			return new NoiseNode(() =>
			{
				FastNoise fn = new("SignedSquareRoot");
				fn.Set("Source", source.Build());
				return fn;
			});
		}

		#endregion

		#region Operators

		static NoiseNode BinaryOp(string nodeType, NoiseNode lhs, Hybrid rhs)
		{
			return new NoiseNode(() =>
			{
				FastNoise fn = new(nodeType);
				fn.Set("LHS", lhs.Build());
				rhs.Apply(fn, "RHS");
				return fn;
			});
		}

		static NoiseNode BinaryOpGeneratorSources(string nodeType, NoiseNode lhs, NoiseNode rhs)
		{
			return new NoiseNode(() =>
			{
				FastNoise fn = new(nodeType);
				fn.Set("LHS", lhs.Build());
				fn.Set("RHS", rhs.Build());
				return fn;
			});
		}

		// Add: LHS is GeneratorSource, RHS is HybridSource
		public static NoiseNode operator +(NoiseNode lhs, NoiseNode rhs) =>
			BinaryOpGeneratorSources("Add", lhs, rhs);

		public static NoiseNode operator +(NoiseNode lhs, float rhs) =>
			BinaryOp("Add", lhs, rhs);

		public static NoiseNode operator +(float lhs, NoiseNode rhs) =>
			BinaryOp("Add", Noise.Constant(lhs), rhs);

		// Subtract: both HybridSource
		public static NoiseNode operator -(NoiseNode lhs, NoiseNode rhs) =>
			BinaryOp("Subtract", lhs, rhs);

		public static NoiseNode operator -(NoiseNode lhs, float rhs) =>
			BinaryOp("Subtract", lhs, rhs);

		public static NoiseNode operator -(float lhs, NoiseNode rhs) =>
			BinaryOp("Subtract", Noise.Constant(lhs), rhs);

		// Multiply: LHS is GeneratorSource, RHS is HybridSource
		public static NoiseNode operator *(NoiseNode lhs, NoiseNode rhs) =>
			BinaryOpGeneratorSources("Multiply", lhs, rhs);

		public static NoiseNode operator *(NoiseNode lhs, float rhs) =>
			BinaryOp("Multiply", lhs, rhs);

		public static NoiseNode operator *(float lhs, NoiseNode rhs) =>
			BinaryOp("Multiply", Noise.Constant(lhs), rhs);

		// Divide: both HybridSource
		public static NoiseNode operator /(NoiseNode lhs, NoiseNode rhs) =>
			BinaryOp("Divide", lhs, rhs);

		public static NoiseNode operator /(NoiseNode lhs, float rhs) =>
			BinaryOp("Divide", lhs, rhs);

		public static NoiseNode operator /(float lhs, NoiseNode rhs) =>
			BinaryOp("Divide", Noise.Constant(lhs), rhs);

		// Modulus: both HybridSource
		public static NoiseNode operator %(NoiseNode lhs, NoiseNode rhs) =>
			BinaryOp("Modulus", lhs, rhs);

		public static NoiseNode operator %(NoiseNode lhs, float rhs) =>
			BinaryOp("Modulus", lhs, rhs);

		public static NoiseNode operator %(float lhs, NoiseNode rhs) =>
			BinaryOp("Modulus", Noise.Constant(lhs), rhs);

		// Negate: node * -1
		public static NoiseNode operator -(NoiseNode node) => node * -1f;

		#endregion
	}
}
