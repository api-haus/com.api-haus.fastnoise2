using System;
using System.Collections.Generic;
using FastNoise2.Generators;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace FastNoise2.Editor.GraphEditor
{
	/// <summary>
	/// Abstract base for all FastNoise2 editor nodes. Concrete subclasses declare
	/// their node type name via <see cref="NodeTypeName"/>; ports and options are
	/// driven automatically from <see cref="FN2NodeRegistry"/> metadata.
	/// </summary>
	[Serializable]
	public abstract class FN2EditorNode : Node
	{
		// -- Port / option ID prefixes to avoid collisions --
		internal const string VarPrefix = "var_";
		internal const string HybridValuePrefix = "hyb_val_";
		internal const string NodeLookupPrefix = "nl_";
		internal const string HybridPortPrefix = "hyb_";
		internal const string OutputPortName = "output";

		/// <summary>
		/// The FN2 node type name this editor node represents.
		/// Must match a name in <see cref="FN2NodeRegistry"/>.
		/// </summary>
		public abstract string NodeTypeName { get; }

		[SerializeField] internal SerializableDictionary variableValues = new();
		[SerializeField] internal SerializableDictionary hybridDefaults = new();

		// ───────────────────── Options ─────────────────────

		protected override void OnDefineOptions(IOptionDefinitionContext context)
		{
			FN2NodeDef def;
			try { def = FN2NodeRegistry.GetNodeDef(NodeTypeName); }
			catch { return; }

			foreach (var member in def.Members)
			{
				switch (member.Type)
				{
					case FN2MemberType.Float:
					{
						string id = VarPrefix + member.LookupKey;
						float defaultVal = variableValues.TryGetFloat(id, out float v) ? v : 0f;
						context.AddOption<float>(id)
							.WithDisplayName(member.Name)
							.WithDefaultValue(defaultVal)
							.Build();
						break;
					}
					case FN2MemberType.Int:
					{
						string id = VarPrefix + member.LookupKey;
						int defaultVal = variableValues.TryGetInt(id, out int iv) ? iv : 0;
						context.AddOption<int>(id)
							.WithDisplayName(member.Name)
							.WithDefaultValue(defaultVal)
							.Build();
						break;
					}
					case FN2MemberType.Enum:
					{
						string id = VarPrefix + member.LookupKey;
						int defaultVal = variableValues.TryGetInt(id, out int ev) ? ev : 0;
						context.AddOption<int>(id)
							.WithDisplayName(member.Name)
							.WithDefaultValue(defaultVal)
							.Build();
						break;
					}
					case FN2MemberType.Hybrid:
					{
						string id = HybridValuePrefix + member.LookupKey;
						float defaultVal = hybridDefaults.TryGetFloat(id, out float hv) ? hv : 0f;
						context.AddOption<float>(id)
							.WithDisplayName(member.Name + "*")
							.WithTooltip("Default value used when no node is connected")
							.WithDefaultValue(defaultVal)
							.Build();
						break;
					}
				}
			}
		}

		// ───────────────────── Ports ─────────────────────

		protected override void OnDefinePorts(IPortDefinitionContext context)
		{
			// Output port — every noise node has one
			context.AddOutputPort<NoiseSignal>(OutputPortName)
				.WithDisplayName("Output")
				.Build();

			FN2NodeDef def;
			try { def = FN2NodeRegistry.GetNodeDef(NodeTypeName); }
			catch { return; }

			foreach (var member in def.Members)
			{
				switch (member.Type)
				{
					case FN2MemberType.NodeLookup:
					{
						string id = NodeLookupPrefix + member.LookupKey;
						context.AddInputPort<NoiseSignal>(id)
							.WithDisplayName(member.Name)
							.Build();
						break;
					}
					case FN2MemberType.Hybrid:
					{
						string id = HybridPortPrefix + member.LookupKey;
						context.AddInputPort<NoiseSignal>(id)
							.WithDisplayName(member.Name)
							.Build();
						break;
					}
				}
			}
		}

		// ───────────────────── Validation ─────────────────────

		internal void ValidateConnections(GraphLogger logger)
		{
			FN2NodeDef def;
			try { def = FN2NodeRegistry.GetNodeDef(NodeTypeName); }
			catch { return; }

			foreach (var member in def.Members)
			{
				if (member.Type != FN2MemberType.NodeLookup)
					continue;

				string portId = NodeLookupPrefix + member.LookupKey;
				var port = GetInputPortByName(portId);
				if (port != null && !port.isConnected)
				{
					logger.LogWarning(
						$"{NodeTypeName}: Required input '{member.Name}' is not connected.");
				}
			}
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
