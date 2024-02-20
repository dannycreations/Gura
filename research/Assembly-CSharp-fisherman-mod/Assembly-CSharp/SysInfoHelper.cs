using System;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;

public class SysInfoHelper
{
	public static void Init()
	{
		SysInfoHelper.GetSysInfoCached();
	}

	public static void SendSystemInfo()
	{
		if (!PlayerPrefs.HasKey("SentSystemInfo2"))
		{
			SysInfo sysInfoCached = SysInfoHelper.GetSysInfoCached();
			PhotonConnectionFactory.Instance.PinSysInfo(sysInfoCached);
			PlayerPrefs.SetString("SentSystemInfo2", "sent");
		}
	}

	public static SysInfo GetSysInfoCached()
	{
		if (SysInfoHelper.SysInfoCached == null)
		{
			SysInfoHelper.SysInfoCached = SysInfoHelper.GetSysInfo();
		}
		return SysInfoHelper.SysInfoCached;
	}

	internal static SysInfo GetSysInfo()
	{
		SysInfo sysInfo = new SysInfo();
		SysInfoHelper.LoadIpMac(sysInfo);
		SysInfoHelper.LoadSystemInfo(sysInfo);
		return sysInfo;
	}

	private static void LoadSystemInfo(SysInfo si)
	{
		si.Os = SystemInfo.operatingSystem;
		si.PcName = SystemInfo.deviceName;
		si.Locale = CultureInfo.CurrentCulture.Name;
		si.Cpu = SystemInfo.processorType;
		si.Ram = SystemInfo.systemMemorySize.ToString();
		si.DirectX = SystemInfo.graphicsDeviceVersion;
		si.VideoAdapter = string.Format("Vendor :{0}; Device: {1}; Video Memory: {2};", SystemInfo.graphicsDeviceVendor, SystemInfo.graphicsDeviceName, SystemInfo.graphicsMemorySize);
		si.Monitor = string.Format("Resolution: {0}x{1}", Screen.currentResolution.width, Screen.currentResolution.height);
		si.AdditionalInfo = string.Format("Quality: {0}; SSAO: {1}; Dynamic water: {2}; Antialiasing: {3}; Full screen: {4}; VSync: {5}", new object[]
		{
			Enum.GetName(typeof(RenderQualities), SettingsManager.RenderQuality),
			SettingsManager.SSAO,
			Enum.GetName(typeof(DynWaterValue), SettingsManager.DynWater),
			Enum.GetName(typeof(AntialiasingValue), SettingsManager.Antialiasing),
			SettingsManager.IsFullScreen,
			SettingsManager.VSync
		});
	}

	private static void LoadIpMac(SysInfo si)
	{
		try
		{
			string hostName = Dns.GetHostName();
			if (hostName != null)
			{
				IPAddress[] hostAddresses = Dns.GetHostAddresses(hostName);
				if (hostAddresses.Length > 0)
				{
					si.Ip = hostAddresses[0].ToString();
					si.Mac = SysInfoHelper.GetMacAddress(hostAddresses[0].GetAddressBytes());
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private static string GetMacAddress(byte[] ip)
	{
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		string text = string.Empty;
		foreach (NetworkInterface networkInterface in allNetworkInterfaces)
		{
			text = networkInterface.GetPhysicalAddress().ToString();
			if (text != string.Empty)
			{
				return text;
			}
		}
		return string.Empty;
	}

	private static SysInfo SysInfoCached;
}
