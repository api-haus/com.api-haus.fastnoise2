using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace FastNoise2.Tests
{
	using Bindings;
	using Generators;

	public class NodeRegistryTests
	{
		[Test]
		public void Registry_HasExpectedNodeCount()
		{
			Assert.That(FN2NodeRegistry.NodeCount, Is.EqualTo(47),
				"Expected 47 node types from FastNoise2 v0.10+");
		}

		[Test]
		public void Registry_AllNodeNamesAreUnique()
		{
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (string name in FN2NodeRegistry.AllNodeNames)
			{
				Assert.That(seen.Add(name), Is.True,
					$"Duplicate node name: {name}");
			}
		}

		[Test]
		public void Registry_GetNodeDefByName_KnownNodes()
		{
			string[] knownNodes =
			{
				"Constant", "White", "Checkerboard", "SineWave", "Gradient",
				"DistanceToPoint", "Simplex", "SuperSimplex", "Perlin", "Value",
				"CellularValue", "CellularDistance", "CellularLookup",
				"FractalFBm", "FractalRidged", "PingPong",
				"DomainWarpSimplex", "DomainWarpSuperSimplex", "DomainWarpGradient",
				"DomainWarpFractalProgressive", "DomainWarpFractalIndependent",
				"Add", "Subtract", "Multiply", "Divide", "Modulus",
				"Abs", "Min", "Max", "MinSmooth", "MaxSmooth",
				"SignedSquareRoot", "PowFloat", "PowInt",
				"DomainScale", "DomainOffset", "DomainRotate", "DomainAxisScale",
				"SeedOffset", "ConvertRGBA8", "GeneratorCache",
				"Fade", "Remap", "Terrace", "AddDimension", "RemoveDimension",
				"DomainRotatePlane"
			};

			foreach (string name in knownNodes)
			{
				FN2NodeDef def = FN2NodeRegistry.GetNodeDef(name);
				Assert.That(def, Is.Not.Null, $"GetNodeDef failed for '{name}'");
				Assert.That(def.NodeName, Is.EqualTo(name));
			}
		}

		[Test]
		public void Registry_GetNodeDefById_RoundTrip()
		{
			for (int id = 0; id < FN2NodeRegistry.NodeCount; id++)
			{
				FN2NodeDef byId = FN2NodeRegistry.GetNodeDefById(id);
				Assert.That(byId, Is.Not.Null, $"GetNodeDefById({id}) returned null");
				Assert.That(byId.Id, Is.EqualTo(id));

				FN2NodeDef byName = FN2NodeRegistry.GetNodeDef(byId.NodeName);
				Assert.That(byName.Id, Is.EqualTo(id),
					$"ID mismatch for '{byId.NodeName}': byId={id} byName={byName.Id}");
			}
		}

		[Test]
		public void Registry_Simplex_HasExpectedMembers()
		{
			FN2NodeDef def = FN2NodeRegistry.GetNodeDef("Simplex");
			Assert.That(def.Id, Is.EqualTo(6));

			Assert.That(def.TryGetMember("featurescale", out var fs), Is.True);
			Assert.That(fs.Type, Is.EqualTo(FN2MemberType.Float));
			Assert.That(fs.Index, Is.EqualTo(0));

			Assert.That(def.TryGetMember("seedoffset", out var so), Is.True);
			Assert.That(so.Type, Is.EqualTo(FN2MemberType.Int));
			Assert.That(so.Index, Is.EqualTo(1));
		}

		[Test]
		public void Registry_FractalFBm_HasExpectedMembers()
		{
			FN2NodeDef def = FN2NodeRegistry.GetNodeDef("FractalFBm");
			Assert.That(def.Id, Is.EqualTo(13));

			// Variables
			Assert.That(def.TryGetMember("octaves", out var oct), Is.True);
			Assert.That(oct.Type, Is.EqualTo(FN2MemberType.Int));
			Assert.That(oct.Index, Is.EqualTo(0));

			Assert.That(def.TryGetMember("lacunarity", out var lac), Is.True);
			Assert.That(lac.Type, Is.EqualTo(FN2MemberType.Float));
			Assert.That(lac.Index, Is.EqualTo(1));

			// NodeLookup
			Assert.That(def.TryGetMember("source", out var src), Is.True);
			Assert.That(src.Type, Is.EqualTo(FN2MemberType.NodeLookup));
			Assert.That(src.Index, Is.EqualTo(0));

			// Hybrids
			Assert.That(def.TryGetMember("gain", out var gain), Is.True);
			Assert.That(gain.Type, Is.EqualTo(FN2MemberType.Hybrid));
			Assert.That(gain.Index, Is.EqualTo(0));

			Assert.That(def.TryGetMember("weightedstrength", out var ws), Is.True);
			Assert.That(ws.Type, Is.EqualTo(FN2MemberType.Hybrid));
			Assert.That(ws.Index, Is.EqualTo(1));
		}

		[Test]
		public void Registry_CellularDistance_HasEnumValues()
		{
			FN2NodeDef def = FN2NodeRegistry.GetNodeDef("CellularDistance");

			Assert.That(def.TryGetMember("distancefunction", out var df), Is.True);
			Assert.That(df.Type, Is.EqualTo(FN2MemberType.Enum));
			Assert.That(df.EnumValues, Is.Not.Null);
			Assert.That(df.EnumValues.Length, Is.GreaterThan(0));

			Assert.That(def.TryGetMember("returntype", out var rt), Is.True);
			Assert.That(rt.Type, Is.EqualTo(FN2MemberType.Enum));
			Assert.That(rt.EnumValues, Is.Not.Null);
			Assert.That(rt.EnumValues, Does.Contain("Index0"));
		}

		[Test]
		public void Registry_EnumIndex_MatchesFluentApi()
		{
			// DistanceFunction.Euclidean should be index 0
			int idx = NoiseNode.EnumIndex("CellularDistance", "DistanceFunction", "Euclidean");
			Assert.That(idx, Is.EqualTo(0));

			// DistanceFunction.Manhattan should be index 2
			idx = NoiseNode.EnumIndex("CellularDistance", "DistanceFunction", "Manhattan");
			Assert.That(idx, Is.EqualTo(2));
		}

		[Test]
		public void Registry_FindVariableMemberByIndex()
		{
			FN2NodeDef def = FN2NodeRegistry.GetNodeDef("Simplex");

			var m0 = def.FindVariableMemberByIndex(0);
			Assert.That(m0.HasValue, Is.True);
			Assert.That(m0.Value.LookupKey, Is.EqualTo("featurescale"));

			var m1 = def.FindVariableMemberByIndex(1);
			Assert.That(m1.HasValue, Is.True);
			Assert.That(m1.Value.LookupKey, Is.EqualTo("seedoffset"));
		}

		[Test]
		public void Registry_FindHybridMemberByIndex()
		{
			FN2NodeDef def = FN2NodeRegistry.GetNodeDef("FractalFBm");

			var h0 = def.FindHybridMemberByIndex(0);
			Assert.That(h0.HasValue, Is.True);
			Assert.That(h0.Value.LookupKey, Is.EqualTo("gain"));

			var h1 = def.FindHybridMemberByIndex(1);
			Assert.That(h1.HasValue, Is.True);
			Assert.That(h1.Value.LookupKey, Is.EqualTo("weightedstrength"));
		}

#if FN2_USER_SIGNED
		[Test]
		public void Validate_RegistryIdsMatchNative()
		{
			int nativeCount = FastNoise.MetadataCount;
			Assert.That(FN2NodeRegistry.NodeCount, Is.EqualTo(nativeCount),
				$"Registry has {FN2NodeRegistry.NodeCount} nodes, native has {nativeCount}");

			for (int id = 0; id < nativeCount; id++)
			{
				FastNoise.Metadata nativeMeta = FastNoise.GetMetadataById(id);
				FN2NodeDef registryDef = FN2NodeRegistry.GetNodeDefById(id);

				Assert.That(registryDef, Is.Not.Null,
					$"Registry missing node at ID {id} (native: '{nativeMeta.name}')");

				Assert.That(registryDef.NodeName.Replace(" ", "").ToLower(),
					Is.EqualTo(nativeMeta.name),
					$"Name mismatch at ID {id}: registry='{registryDef.NodeName}' native='{nativeMeta.name}'");

				// Validate member count
				Assert.That(registryDef.Members.Length, Is.EqualTo(nativeMeta.members.Count),
					$"Member count mismatch for '{registryDef.NodeName}': " +
					$"registry={registryDef.Members.Length} native={nativeMeta.members.Count}");

				// Validate each member
				foreach (var nativeKv in nativeMeta.members)
				{
					Assert.That(registryDef.TryGetMember(nativeKv.Key, out var regMember), Is.True,
						$"Registry missing member '{nativeKv.Key}' on '{registryDef.NodeName}'");

					var nativeMember = nativeKv.Value;

					// Type match
					FN2MemberType expectedType = nativeMember.type switch
					{
						FastNoise.Metadata.Member.Type.Float => FN2MemberType.Float,
						FastNoise.Metadata.Member.Type.Int => FN2MemberType.Int,
						FastNoise.Metadata.Member.Type.Enum => FN2MemberType.Enum,
						FastNoise.Metadata.Member.Type.NodeLookup => FN2MemberType.NodeLookup,
						FastNoise.Metadata.Member.Type.Hybrid => FN2MemberType.Hybrid,
						_ => throw new Exception($"Unknown native type: {nativeMember.type}")
					};
					Assert.That(regMember.Type, Is.EqualTo(expectedType),
						$"Type mismatch for '{nativeKv.Key}' on '{registryDef.NodeName}': " +
						$"registry={regMember.Type} native={nativeMember.type}");

					// Index match
					Assert.That(regMember.Index, Is.EqualTo(nativeMember.index),
						$"Index mismatch for '{nativeKv.Key}' on '{registryDef.NodeName}': " +
						$"registry={regMember.Index} native={nativeMember.index}");

					// Enum values match
					if (nativeMember.type == FastNoise.Metadata.Member.Type.Enum)
					{
						Assert.That(regMember.EnumValues, Is.Not.Null,
							$"Registry enum values null for '{nativeKv.Key}' on '{registryDef.NodeName}'");

						foreach (var enumKv in nativeMember.enumNames)
						{
							bool found = false;
							for (int i = 0; i < regMember.EnumValues.Length; i++)
							{
								if (regMember.EnumValues[i].Replace(" ", "").ToLower() == enumKv.Key)
								{
									Assert.That(i, Is.EqualTo(enumKv.Value),
										$"Enum index mismatch for '{enumKv.Key}' in '{nativeKv.Key}' " +
										$"on '{registryDef.NodeName}': registry={i} native={enumKv.Value}");
									found = true;
									break;
								}
							}
							Assert.That(found, Is.True,
								$"Registry missing enum value '{enumKv.Key}' for '{nativeKv.Key}' " +
								$"on '{registryDef.NodeName}'");
						}
					}
				}
			}
		}

		[Test]
		public void Validate_RegistryEncodesIdenticallyToNative()
		{
			// Encode with registry, decode with native — check round-trip
			NoiseNode original = Noise.Simplex().Fbm(0.5f, 0f, 3, 2f);
			string encoded = original.Encode();

			using FastNoise fn = FastNoise.FromEncodedNodeTree(encoded);
			Assert.That(fn != FastNoise.Invalid, Is.True,
				$"Native failed to decode registry-encoded string: {encoded}");
		}
#endif
	}
}
