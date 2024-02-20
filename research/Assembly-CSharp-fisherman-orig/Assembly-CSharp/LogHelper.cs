using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class LogHelper
{
	public static string Vector3ToString(string label, Vector3 v)
	{
		return string.Format("{0} = [{1:f3}, {2:f3}, {3:f3}]", new object[] { label, v.x, v.y, v.z });
	}

	[Conditional("LOG_HELPER")]
	public static void LogTimed(float delay, string label, string textPattern, params object[] args)
	{
		if (label != null && (!LogHelper._timers.ContainsKey(label) || LogHelper._timers[label] < Time.time))
		{
			LogHelper._timers[label] = Time.time + delay;
		}
	}

	[Conditional("LOG_HELPER")]
	public static void LogTimeouted(float timeout, string label, string textPattern, params object[] args)
	{
		if (label != null)
		{
			if (LogHelper._timeouts.ContainsKey(label))
			{
				if (DateTime.UtcNow > LogHelper._timeouts[label])
				{
					LogHelper._timeouts[label] = DateTime.UtcNow.AddSeconds((double)timeout);
				}
			}
			else
			{
				LogHelper._timeouts[label] = DateTime.UtcNow.AddSeconds((double)timeout);
			}
		}
	}

	public static void Log(string text)
	{
		global::UnityEngine.Debug.Log(string.Format("UserLog   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), text));
	}

	public static void Log(string textPattern, params object[] args)
	{
		global::UnityEngine.Debug.Log(string.Format("UserLog   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), string.Format(textPattern, args)));
	}

	public static bool Check(bool expresion, string message = "")
	{
		if (!expresion)
		{
			LogHelper.Log(string.Format("CHECK FAILED:   {0}", message));
		}
		return expresion;
	}

	[Conditional("LOG_HELPER")]
	public static void Debug(string text)
	{
		global::UnityEngine.Debug.Log(string.Format("UserLog   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), text));
	}

	[Conditional("LOG_HELPER")]
	public static void Debug(string textPattern, params object[] args)
	{
		global::UnityEngine.Debug.Log(string.Format("UserLog   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), string.Format(textPattern, args)));
	}

	public static void Error(string textPattern, params object[] args)
	{
		global::UnityEngine.Debug.LogError(string.Format("UserError   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), string.Format(textPattern, args)));
	}

	[Conditional("LOG_HELPER")]
	public static void DebugError(string textPattern, params object[] args)
	{
		global::UnityEngine.Debug.LogError(string.Format("UserError   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), string.Format(textPattern, args)));
	}

	[Conditional("LOG_HELPER")]
	public static void DebugWarning(string textPattern, params object[] args)
	{
		global::UnityEngine.Debug.LogWarning(string.Format("UserWarning   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), string.Format(textPattern, args)));
	}

	public static void Warning(string textPattern, params object[] args)
	{
		global::UnityEngine.Debug.LogWarning(string.Format("UserWarning   {0}       {1}", DateTime.Now.ToString("hh:mm:ss.ff"), string.Format(textPattern, args)));
	}

	[Conditional("UNITY_PS4")]
	public static void LogPS4(string textPattern, params object[] args)
	{
	}

	private static Dictionary<string, float> _timers = new Dictionary<string, float>();

	private static Dictionary<string, DateTime> _timeouts = new Dictionary<string, DateTime>();
}
