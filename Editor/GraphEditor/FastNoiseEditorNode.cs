using System;
using System.Collections.Generic;
using FastNoise2.Bindings;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Metadata-driven noise node. A single <see cref="nodeTypeName"/> field
	/// drives port/option topology via native FN2 metadata.
	/// </summary>
	[Serializable]
	public class FastNoiseEditorNode : Node
	{
		// -- Port / option ID prefixes to avoid collisions --
		internal const string VarPrefix = "var_";
		internal const string HybridValuePrefix = "hyb_val_";
		internal const string NodeLookupPrefix = "nl_";
		internal const string HybridPortPrefix = "hyb_";
		internal const string OutputPortName = "output";
		internal const string NodeTypeOptionName = "nodeType";

		[SerializeField] internal string nodeTypeName = "Simplex";
		[SerializeField] internal SerializableDictionary variableValues = new();
		[SerializeField] internal SerializableDictionary hybridDefaults = new();

		// ───────────────────── Options ─────────────────────

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			// Node type selector
			context.AddOption<string>(NodeTypeOptionName)
				.WithDisplayName("Node Type")
				.WithDefaultValue(nodeTypeName)
				.Delayed()
				.Build();

#if FN2_USER_SIGNED
			FastNoise.Metadata meta;
			try { meta = FastNoise.GetNodeMetadata(nodeTypeName); }
			catch { return; }

			foreach (var kv in meta.members)
			{
				var member = kv.Value;
				switch (member.type)
				{
					case FastNoise.Metadata.Member.Type.Float:
					{
						string id = VarPrefix + kv.Key;
						float defaultVal = variableValues.TryGetFloat(id, out float v) ? v : 0f;
						context.AddOption<float>(id)
							.WithDisplayName(FN2MetadataCache.GetProperMemberName(meta.id, kv.Key))
							.WithDefaultValue(defaultVal)
							.Build();
						break;
					}
					case FastNoise.Metadata.Member.Type.Int:
					{
						string id = VarPrefix + kv.Key;
						int defaultVal = variableValues.TryGetInt(id, out int iv) ? iv : 0;
						context.AddOption<int>(id)
							.WithDisplayName(FN2MetadataCache.GetProperMemberName(meta.id, kv.Key))
							.WithDefaultValue(defaultVal)
							.Build();
						break;
					}
					case FastNoise.Metadata.Member.Type.Enum:
					{
						string id = VarPrefix + kv.Key;
						int defaultVal = variableValues.TryGetInt(id, out int ev) ? ev : 0;
						context.AddOption<int>(id)
							.WithDisplayName(FN2MetadataCache.GetProperMemberName(meta.id, kv.Key))
							.WithDefaultValue(defaultVal)
							.Build();
						break;
					}
					case FastNoise.Metadata.Member.Type.Hybrid:
					{
						string id = HybridValuePrefix + kv.Key;
						float defaultVal = hybridDefaults.TryGetFloat(id, out float hv) ? hv : 0f;
						context.AddOption<float>(id)
							.WithDisplayName(FN2MetadataCache.GetProperMemberName(meta.id, kv.Key) + " (Default)")
							.WithDefaultValue(defaultVal)
							.Build();
						break;
					}
				}
			}
#endif
		}

		// ───────────────────── Ports ─────────────────────

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			// Output port — every noise node has one
			context.AddOutputPort<NoiseSignal>(OutputPortName)
				.WithDisplayName("Output")
				.Build();

#if FN2_USER_SIGNED
			FastNoise.Metadata meta;
			try { meta = FastNoise.GetNodeMetadata(nodeTypeName); }
			catch { return; }

			foreach (var kv in meta.members)
			{
				var member = kv.Value;
				switch (member.type)
				{
					case FastNoise.Metadata.Member.Type.NodeLookup:
					{
						string id = NodeLookupPrefix + kv.Key;
						context.AddInputPort<NoiseSignal>(id)
							.WithDisplayName(FN2MetadataCache.GetProperMemberName(meta.id, kv.Key))
							.Build();
						break;
					}
					case FastNoise.Metadata.Member.Type.Hybrid:
					{
						string id = HybridPortPrefix + kv.Key;
						context.AddInputPort<NoiseSignal>(id)
							.WithDisplayName(FN2MetadataCache.GetProperMemberName(meta.id, kv.Key))
							.Build();
						break;
					}
				}
			}
#endif
		}

		// ───────────────────── Validation ─────────────────────

		internal void ValidateConnections(GraphLogger logger)
		{
#if FN2_USER_SIGNED
			FastNoise.Metadata meta;
			try { meta = FastNoise.GetNodeMetadata(nodeTypeName); }
			catch { return; }

			foreach (var kv in meta.members)
			{
				if (kv.Value.type != FastNoise.Metadata.Member.Type.NodeLookup)
					continue;

				string portId = NodeLookupPrefix + kv.Key;
				var port = GetInputPortByName(portId);
				if (port != null && !port.isConnected)
				{
					logger.LogWarning(
						$"{nodeTypeName}: Required input '{FN2MetadataCache.GetProperMemberName(meta.id, kv.Key)}' is not connected.");
				}
			}
#endif
		}

		/// <summary>
		/// Serializable string→raw-bytes dictionary for storing variable values.
		/// Int values stored as-is; float values stored via BitConverter.
		/// </summary>
		[Serializable]
		internal class SerializableDictionary : ISerializationCallbackReceiver
		{
			[SerializeField] List<string> keys = new();
			[SerializeField] List<int> values = new();

			Dictionary<string, int> m_Dict = new();

			public void SetInt(string key, int value) => m_Dict[key] = value;
			public void SetFloat(string key, float value) => m_Dict[key] = BitConverter.SingleToInt32Bits(value);

			public bool TryGetInt(string key, out int value) => m_Dict.TryGetValue(key, out value);

			public bool TryGetFloat(string key, out float value)
			{
				if (m_Dict.TryGetValue(key, out int raw))
				{
					value = BitConverter.Int32BitsToSingle(raw);
					return true;
				}
				value = 0f;
				return false;
			}

			public int GetIntOrDefault(string key, int fallback = 0) =>
				m_Dict.TryGetValue(key, out int v) ? v : fallback;

			public float GetFloatOrDefault(string key, float fallback = 0f) =>
				m_Dict.TryGetValue(key, out int raw) ? BitConverter.Int32BitsToSingle(raw) : fallback;

			public IEnumerable<KeyValuePair<string, int>> Entries => m_Dict;

			public void OnBeforeSerialize()
			{
				keys.Clear();
				values.Clear();
				foreach (var kv in m_Dict)
				{
					keys.Add(kv.Key);
					values.Add(kv.Value);
				}
			}

			public void OnAfterDeserialize()
			{
				m_Dict = new Dictionary<string, int>();
				int count = Math.Min(keys.Count, values.Count);
				for (int i = 0; i < count; i++)
					m_Dict[keys[i]] = values[i];
			}
		}
	}
}
