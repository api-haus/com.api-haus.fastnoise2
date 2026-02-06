using System.Collections.Generic;
using System.Runtime.InteropServices;
using FastNoise2.Bindings;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Caches proper-cased node type and member names from the native FN2 metadata.
	/// All lookups are lowercased internally; this cache recovers original casing
	/// for display in the graph editor UI.
	/// </summary>
	static class FN2MetadataCache
	{
		static string[] s_NodeTypeNames;
		static Dictionary<int, string> s_ProperNodeNames;
		static Dictionary<int, Dictionary<string, string>> s_ProperMemberNames;

		static void EnsureInitialized()
		{
			if (s_NodeTypeNames != null)
				return;

#if FN2_USER_SIGNED
			int count = FastNoise.MetadataCount;
			s_NodeTypeNames = new string[count];
			s_ProperNodeNames = new Dictionary<int, string>(count);
			s_ProperMemberNames = new Dictionary<int, Dictionary<string, string>>(count);

			for (int id = 0; id < count; id++)
			{
				string properName = Marshal.PtrToStringAnsi(FastNoise.fnGetMetadataName(id));
				s_NodeTypeNames[id] = properName;
				s_ProperNodeNames[id] = properName;

				var memberNames = new Dictionary<string, string>();

				int varCount = FastNoise.fnGetMetadataVariableCount(id);
				for (int vi = 0; vi < varCount; vi++)
				{
					string raw = Marshal.PtrToStringAnsi(FastNoise.fnGetMetadataVariableName(id, vi));
					memberNames[FormatLookup(raw)] = raw;
				}

				int nlCount = FastNoise.fnGetMetadataNodeLookupCount(id);
				for (int ni = 0; ni < nlCount; ni++)
				{
					string raw = Marshal.PtrToStringAnsi(FastNoise.fnGetMetadataNodeLookupName(id, ni));
					memberNames[FormatLookup(raw)] = raw;
				}

				int hybCount = FastNoise.fnGetMetadataHybridCount(id);
				for (int hi = 0; hi < hybCount; hi++)
				{
					string raw = Marshal.PtrToStringAnsi(FastNoise.fnGetMetadataHybridName(id, hi));
					memberNames[FormatLookup(raw)] = raw;
				}

				s_ProperMemberNames[id] = memberNames;
			}
#else
			s_NodeTypeNames = System.Array.Empty<string>();
			s_ProperNodeNames = new Dictionary<int, string>(0);
			s_ProperMemberNames = new Dictionary<int, Dictionary<string, string>>(0);
#endif
		}

		public static string[] GetAllNodeTypeNames()
		{
			EnsureInitialized();
			return s_NodeTypeNames;
		}

		public static string GetProperNodeName(int metadataId)
		{
			EnsureInitialized();
			return s_ProperNodeNames.TryGetValue(metadataId, out var name) ? name : null;
		}

		public static string GetProperMemberName(int metadataId, string lowercasedKey)
		{
			EnsureInitialized();
			if (s_ProperMemberNames.TryGetValue(metadataId, out var members))
				if (members.TryGetValue(lowercasedKey, out var properName))
					return properName;
			return lowercasedKey;
		}

		static string FormatLookup(string s) => s.Replace(" ", "").ToLower();
	}
}
