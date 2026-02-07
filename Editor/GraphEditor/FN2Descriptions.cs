using FastNoise2.Generators;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Tooltip descriptions for FN2 graph editor nodes and members,
	/// sourced from the native FastNoise2 C++ metadata at runtime.
	/// </summary>
	static class FN2Descriptions
	{
		public static string GetNodeDescription(string nodeTypeName)
		{
			try
			{
				var desc = FN2NodeRegistry.GetNodeDef(nodeTypeName).Description;
				return string.IsNullOrEmpty(desc) ? null : desc;
			}
			catch { return null; }
		}

		public static string GetMemberDescription(string nodeTypeName, string lookupKey)
		{
			try
			{
				var def = FN2NodeRegistry.GetNodeDef(nodeTypeName);
				return def.TryGetMember(lookupKey, out var member)
					&& !string.IsNullOrEmpty(member.Description)
						? member.Description
						: null;
			}
			catch { return null; }
		}
	}
}
