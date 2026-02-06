using System;
using FastNoise2.Bindings;

namespace FastNoise2.Generators
{
	/// <summary>
	/// Builds a native <see cref="FastNoise"/> handle tree from a <see cref="NodeDescriptor"/> tree.
	/// </summary>
	static class NodeBuilder
	{
		public static FastNoise Build(NodeDescriptor descriptor)
		{
#if FN2_USER_SIGNED
			FastNoise fn = new(descriptor.NodeName);
			FastNoise.Metadata meta = FastNoise.GetNodeMetadata(descriptor.NodeName);

			foreach (var kv in descriptor.Variables)
			{
				if (!meta.members.TryGetValue(FormatLookup(kv.Key), out var member))
					throw new ArgumentException(
						$"Unknown member '{kv.Key}' on node '{descriptor.NodeName}'");

				switch (member.type)
				{
					case FastNoise.Metadata.Member.Type.Float:
						FastNoise.fnSetVariableFloat(fn.mNodeHandle, member.index,
							BitConverter.Int32BitsToSingle(kv.Value));
						break;

					case FastNoise.Metadata.Member.Type.Int:
					case FastNoise.Metadata.Member.Type.Enum:
						FastNoise.fnSetVariableIntEnum(fn.mNodeHandle, member.index, kv.Value);
						break;

					default:
						throw new ArgumentException(
							$"Member '{kv.Key}' on '{descriptor.NodeName}' is not a variable type");
				}
			}

			foreach (var kv in descriptor.NodeLookups)
			{
				if (!meta.members.TryGetValue(FormatLookup(kv.Key), out var member))
					throw new ArgumentException(
						$"Unknown member '{kv.Key}' on node '{descriptor.NodeName}'");

				FastNoise child = Build(kv.Value);
				IntPtr childHandle = child.mNodeHandle;
				FastNoise.fnSetNodeLookup(fn.mNodeHandle, member.index, ref childHandle);
			}

			foreach (var kv in descriptor.Hybrids)
			{
				if (!meta.members.TryGetValue(FormatLookup(kv.Key), out var member))
					throw new ArgumentException(
						$"Unknown member '{kv.Key}' on node '{descriptor.NodeName}'");

				if (kv.Value.IsNode)
				{
					FastNoise child = Build(kv.Value.NodeValue);
					IntPtr childHandle = child.mNodeHandle;
					FastNoise.fnSetHybridNodeLookup(fn.mNodeHandle, member.index, ref childHandle);
				}
				else
				{
					FastNoise.fnSetHybridFloat(fn.mNodeHandle, member.index, kv.Value.FloatValue);
				}
			}

			return fn;
#else
			throw new InvalidOperationException("FastNoise2 native libraries are not signed. Use Window > FastNoise2 to sign them.");
#endif
		}

		static string FormatLookup(string s) => s.Replace(" ", "").ToLower();
	}
}
