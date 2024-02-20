using System;
using UnityEngine;

namespace DeltaDNA
{
	public static class Logger
	{
		public static void SetLogLevel(Logger.Level logLevel)
		{
			Logger.sLogLevel = logLevel;
		}

		internal static void LogDebug(string msg)
		{
			if (Logger.sLogLevel <= Logger.Level.DEBUG)
			{
				Logger.Log(msg, Logger.Level.DEBUG);
			}
		}

		internal static void LogInfo(string msg)
		{
			if (Logger.sLogLevel <= Logger.Level.INFO)
			{
				Logger.Log(msg, Logger.Level.INFO);
			}
		}

		internal static void LogWarning(string msg)
		{
			if (Logger.sLogLevel <= Logger.Level.WARNING)
			{
				Logger.Log(msg, Logger.Level.WARNING);
			}
		}

		internal static void LogError(string msg)
		{
			if (Logger.sLogLevel <= Logger.Level.ERROR)
			{
				Logger.Log(msg, Logger.Level.ERROR);
			}
		}

		private static void Log(string msg, Logger.Level level)
		{
			string text = "[DDSDK] ";
			if (level != Logger.Level.ERROR)
			{
				if (level != Logger.Level.WARNING)
				{
					Debug.Log(text + msg);
				}
				else
				{
					Debug.LogWarning(text + msg);
				}
			}
			else
			{
				Debug.LogError(text + msg);
			}
		}

		private static Logger.Level sLogLevel = Logger.Level.WARNING;

		public enum Level
		{
			DEBUG,
			INFO,
			WARNING,
			ERROR
		}
	}
}
