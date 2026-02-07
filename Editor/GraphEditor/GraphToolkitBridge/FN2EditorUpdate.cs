using System;
using System.Collections.Generic;
using UnityEditor;

namespace FastNoise2.Editor.GraphEditor
{
	public class Throttle
	{
		readonly Action m_Action;
		readonly double m_Interval;
		double m_NextTime;

		public Throttle(Action action, double intervalSeconds)
		{
			m_Action = action;
			m_Interval = intervalSeconds;
		}

		internal void Update(double now)
		{
			if (now < m_NextTime)
				return;

			m_NextTime = now + m_Interval;
			m_Action();
		}
	}

	public class Debounce
	{
		readonly Action m_Action;
		readonly double m_Delay;
		double m_FireTime;
		bool m_Pending;

		public Debounce(Action action, double delaySeconds)
		{
			m_Action = action;
			m_Delay = delaySeconds;
		}

		public void Signal()
		{
			m_Pending = true;
			m_FireTime = EditorApplication.timeSinceStartup + m_Delay;
		}

		internal void Update(double now)
		{
			if (!m_Pending || now < m_FireTime)
				return;

			m_Pending = false;
			m_Action();
		}
	}

	/// <summary>
	/// Central editor update loop that drives all registered Throttle and Debounce instances.
	/// Self-registers via [InitializeOnLoad] since it lives in the GraphToolkitBridge assembly.
	/// </summary>
	[InitializeOnLoad]
	static class FN2EditorUpdate
	{
		public static event Action GraphChanged;
		public static void NotifyGraphChanged() => GraphChanged?.Invoke();

		static readonly List<Throttle> s_Throttles = new();
		static readonly List<Debounce> s_Debounces = new();

		static FN2EditorUpdate()
		{
			EditorApplication.update += Tick;
		}

		public static void Register(Throttle t) { if (!s_Throttles.Contains(t)) s_Throttles.Add(t); }
		public static void Unregister(Throttle t) { s_Throttles.Remove(t); }
		public static void Register(Debounce d) { if (!s_Debounces.Contains(d)) s_Debounces.Add(d); }
		public static void Unregister(Debounce d) { s_Debounces.Remove(d); }

		static void Tick()
		{
			double now = EditorApplication.timeSinceStartup;

			for (int i = s_Throttles.Count - 1; i >= 0; i--)
				s_Throttles[i].Update(now);

			for (int i = s_Debounces.Count - 1; i >= 0; i--)
				s_Debounces[i].Update(now);
		}
	}
}
