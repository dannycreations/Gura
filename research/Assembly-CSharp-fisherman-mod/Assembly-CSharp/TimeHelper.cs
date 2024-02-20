using System;

public class TimeHelper
{
	public static DateTime UtcTime()
	{
		return PhotonConnectionFactory.Instance.ServerUtcNow;
	}

	public static DateTime LocalUtcTime()
	{
		return DateTime.UtcNow;
	}
}
