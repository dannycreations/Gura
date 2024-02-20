using System;
using ObjectModel;

public static class ClientDebugHelper
{
	public static void Error(ProfileFlag flag, string message)
	{
		message = "ERROR: " + message;
		LogHelper.Error(message, new object[0]);
		if (ProfileFlags.HasFlag(flag))
		{
			ClientDebugHelper.Send(flag, message);
		}
	}

	public static void Log(ProfileFlag flag, string message)
	{
		LogHelper.Log(message);
		if (ProfileFlags.HasFlag(flag))
		{
			ClientDebugHelper.Send(flag, message);
		}
	}

	public static void ErrorIfEnabled(ProfileFlag flag, string message)
	{
		if (ProfileFlags.HasFlag(flag))
		{
			message = "ERROR: " + message;
			LogHelper.Error(message, new object[0]);
			ClientDebugHelper.Send(flag, message);
		}
	}

	public static void LogIfEnabled(ProfileFlag flag, string message)
	{
		if (ProfileFlags.HasFlag(flag))
		{
			LogHelper.Log(message);
			ClientDebugHelper.Send(flag, message);
		}
	}

	private static void Send(ProfileFlag flag, string message)
	{
		PhotonConnectionFactory.Instance.DebugClient((byte)flag + 1, message);
	}
}
