using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DeltaDNA
{
	internal static class ClientInfo
	{
		public static string Platform
		{
			get
			{
				string text;
				if ((text = ClientInfo.platform) == null)
				{
					text = (ClientInfo.platform = ClientInfo.GetPlatform());
				}
				return text;
			}
		}

		public static string DeviceName
		{
			get
			{
				string text;
				if ((text = ClientInfo.deviceName) == null)
				{
					text = (ClientInfo.deviceName = ClientInfo.GetDeviceName());
				}
				return text;
			}
		}

		public static string DeviceModel
		{
			get
			{
				string text;
				if ((text = ClientInfo.deviceModel) == null)
				{
					text = (ClientInfo.deviceModel = ClientInfo.GetDeviceModel());
				}
				return text;
			}
		}

		public static string DeviceType
		{
			get
			{
				string text;
				if ((text = ClientInfo.deviceType) == null)
				{
					text = (ClientInfo.deviceType = ClientInfo.GetDeviceType());
				}
				return text;
			}
		}

		public static string OperatingSystem
		{
			get
			{
				string text;
				if ((text = ClientInfo.operatingSystem) == null)
				{
					text = (ClientInfo.operatingSystem = ClientInfo.GetOperatingSystem());
				}
				return text;
			}
		}

		public static string OperatingSystemVersion
		{
			get
			{
				string text;
				if ((text = ClientInfo.operatingSystemVersion) == null)
				{
					text = (ClientInfo.operatingSystemVersion = ClientInfo.GetOperatingSystemVersion());
				}
				return text;
			}
		}

		public static string Manufacturer
		{
			get
			{
				string text;
				if ((text = ClientInfo.manufacturer) == null)
				{
					text = (ClientInfo.manufacturer = ClientInfo.GetManufacturer());
				}
				return text;
			}
		}

		public static string TimezoneOffset
		{
			get
			{
				string text;
				if ((text = ClientInfo.timezoneOffset) == null)
				{
					text = (ClientInfo.timezoneOffset = ClientInfo.GetCurrentTimezoneOffset());
				}
				return text;
			}
		}

		public static string CountryCode
		{
			get
			{
				string text;
				if ((text = ClientInfo.countryCode) == null)
				{
					text = (ClientInfo.countryCode = ClientInfo.GetCountryCode());
				}
				return text;
			}
		}

		public static string LanguageCode
		{
			get
			{
				string text;
				if ((text = ClientInfo.languageCode) == null)
				{
					text = (ClientInfo.languageCode = ClientInfo.GetLanguageCode());
				}
				return text;
			}
		}

		public static string Locale
		{
			get
			{
				string text;
				if ((text = ClientInfo.locale) == null)
				{
					text = (ClientInfo.locale = ClientInfo.GetLocale());
				}
				return text;
			}
		}

		private static bool RuntimePlatformIs(string platformName)
		{
			return Enum.IsDefined(typeof(RuntimePlatform), platformName) && Application.platform.ToString() == platformName;
		}

		private static float ScreenSizeInches()
		{
			float num = (float)Screen.width / Screen.dpi;
			float num2 = (float)Screen.height / Screen.dpi;
			return (float)Math.Sqrt((double)(num * num + num2 * num2));
		}

		private static bool IsTablet()
		{
			return ClientInfo.ScreenSizeInches() > 6f;
		}

		private static string GetPlatform()
		{
			if (ClientInfo.RuntimePlatformIs("OSXEditor"))
			{
				return "MAC_CLIENT";
			}
			if (ClientInfo.RuntimePlatformIs("OSXPlayer"))
			{
				return "MAC_CLIENT";
			}
			if (ClientInfo.RuntimePlatformIs("WindowsPlayer"))
			{
				return "PC_CLIENT";
			}
			if (ClientInfo.RuntimePlatformIs("OSXWebPlayer"))
			{
				return "WEB";
			}
			if (ClientInfo.RuntimePlatformIs("OSXDashboardPlayer"))
			{
				return "MAC_CLIENT";
			}
			if (ClientInfo.RuntimePlatformIs("WindowsWebPlayer"))
			{
				return "WEB";
			}
			if (ClientInfo.RuntimePlatformIs("WindowsEditor"))
			{
				return "PC_CLIENT";
			}
			if (ClientInfo.RuntimePlatformIs("IPhonePlayer"))
			{
				string text = SystemInfo.deviceModel;
				if (text.Contains("iPad"))
				{
					return "IOS_TABLET";
				}
				return "IOS_MOBILE";
			}
			else
			{
				if (ClientInfo.RuntimePlatformIs("PS3"))
				{
					return "PS3";
				}
				if (ClientInfo.RuntimePlatformIs("XBOX360"))
				{
					return "XBOX360";
				}
				if (ClientInfo.RuntimePlatformIs("Android"))
				{
					return (!ClientInfo.IsTablet()) ? "ANDROID_MOBILE" : "ANDROID_TABLET";
				}
				if (ClientInfo.RuntimePlatformIs("NaCL"))
				{
					return "WEB";
				}
				if (ClientInfo.RuntimePlatformIs("LinuxPlayer"))
				{
					return "LINUX_CLIENT";
				}
				if (ClientInfo.RuntimePlatformIs("WebGLPlayer"))
				{
					return "WEB";
				}
				if (ClientInfo.RuntimePlatformIs("FlashPlayer"))
				{
					return "WEB";
				}
				if (ClientInfo.RuntimePlatformIs("MetroPlayerX86") || ClientInfo.RuntimePlatformIs("MetroPlayerX64") || ClientInfo.RuntimePlatformIs("MetroPlayerARM") || ClientInfo.RuntimePlatformIs("WSAPlayerX86") || ClientInfo.RuntimePlatformIs("WSAPlayerX64") || ClientInfo.RuntimePlatformIs("WSAPlayerARM"))
				{
					if (SystemInfo.deviceType == 1)
					{
						return (!ClientInfo.IsTablet()) ? "WINDOWS_MOBILE" : "WINDOWS_TABLET";
					}
					return "PC_CLIENT";
				}
				else
				{
					if (ClientInfo.RuntimePlatformIs("WP8Player"))
					{
						return (!ClientInfo.IsTablet()) ? "WINDOWS_MOBILE" : "WINDOWS_TABLET";
					}
					if (ClientInfo.RuntimePlatformIs("BB10Player") || ClientInfo.RuntimePlatformIs("BlackBerryPlayer"))
					{
						return (!ClientInfo.IsTablet()) ? "BLACKBERRY_MOBILE" : "BLACKBERRY_TABLET";
					}
					if (ClientInfo.RuntimePlatformIs("TizenPlayer"))
					{
						return (!ClientInfo.IsTablet()) ? "ANDROID_MOBILE" : "ANDROID_TABLET";
					}
					if (ClientInfo.RuntimePlatformIs("PSP2"))
					{
						return "PSVITA";
					}
					if (ClientInfo.RuntimePlatformIs("PS4"))
					{
						return "PS4";
					}
					if (ClientInfo.RuntimePlatformIs("PSMPlayer"))
					{
						return "WEB";
					}
					if (ClientInfo.RuntimePlatformIs("XboxOne"))
					{
						return "XBOXONE";
					}
					if (ClientInfo.RuntimePlatformIs("SamsungTVPlayer"))
					{
						return "ANDROID_CONSOLE";
					}
					if (ClientInfo.RuntimePlatformIs("tvOS"))
					{
						return "IOS_TV";
					}
					return "UNKNOWN";
				}
			}
		}

		private static string GetDeviceName()
		{
			string text = SystemInfo.deviceModel;
			switch (text)
			{
			case "iPhone1,1":
				return "iPhone 1G";
			case "iPhone1,2":
				return "iPhone 3G";
			case "iPhone2,1":
				return "iPhone 3GS";
			case "iPhone3,1":
				return "iPhone 4";
			case "iPhone3,2":
				return "iPhone 4";
			case "iPhone3,3":
				return "iPhone 4";
			case "iPhone4,1":
				return "iPhone 4S";
			case "iPhone5,1":
				return "iPhone 5";
			case "iPhone5,2":
				return "iPhone 5";
			case "iPhone5,3":
				return "iPhone 5C";
			case "iPhone5,4":
				return "iPhone 5C";
			case "iPhone6,1":
				return "iPhone 5S";
			case "iPhone6,2":
				return "iPhone 5S";
			case "iPhone7,2":
				return "iPhone 6";
			case "iPhone7,1":
				return "iPhone 6 Plus";
			case "iPod1,1":
				return "iPod Touch 1G";
			case "iPod2,1":
				return "iPod Touch 2G";
			case "iPod3,1":
				return "iPod Touch 3G";
			case "iPod4,1":
				return "iPod Touch 4G";
			case "iPod5,1":
				return "iPod Touch 5G";
			case "iPad1,1":
				return "iPad 1G";
			case "iPad2,1":
				return "iPad 2";
			case "iPad2,2":
				return "iPad 2";
			case "iPad2,3":
				return "iPad 2";
			case "iPad2,4":
				return "iPad 2";
			case "iPad3,1":
				return "iPad 3G";
			case "iPad3,2":
				return "iPad 3G";
			case "iPad3,3":
				return "iPad 3G";
			case "iPad3,4":
				return "iPad 4G";
			case "iPad3,5":
				return "iPad 4G";
			case "iPad3,6":
				return "iPad 4G";
			case "iPad4,1":
				return "iPad Air";
			case "iPad4,2":
				return "iPad Air";
			case "iPad4,3":
				return "iPad Air";
			case "iPad5,3":
				return "iPad Air 2";
			case "iPad5,4":
				return "iPad Air 2";
			case "iPad2,5":
				return "iPad Mini 1G";
			case "iPad2,6":
				return "iPad Mini 1G";
			case "iPad2,7":
				return "iPad Mini 1G";
			case "iPad4,4":
				return "iPad Mini 2G";
			case "iPad4,5":
				return "iPad Mini 2G";
			case "iPad4,6":
				return "iPad Mini 2G";
			case "iPad4,7":
				return "iPad Mini 3";
			case "iPad4,8":
				return "iPad Mini 3";
			case "iPad4,9":
				return "iPad Mini 3";
			case "Amazon KFSAWA":
				return "Fire HDX 8.9 (4th Gen)";
			case "Amazon KFASWI":
				return "Fire HD 7 (4th Gen)";
			case "Amazon KFARWI":
				return "Fire HD 6 (4th Gen)";
			case "Amazon KFAPWA":
			case "Amazon KFAPWI":
				return "Kindle Fire HDX 8.9 (3rd Gen)";
			case "Amazon KFTHWA":
			case "Amazon KFTHWI":
				return "Kindle Fire HDX 7 (3rd Gen)";
			case "Amazon KFSOWI":
				return "Kindle Fire HD 7 (3rd Gen)";
			case "Amazon KFJWA":
			case "Amazon KFJWI":
				return "Kindle Fire HD 8.9 (2nd Gen)";
			case "Amazon KFTT":
				return "Kindle Fire HD 7 (2nd Gen)";
			case "Amazon KFOT":
				return "Kindle Fire (2nd Gen)";
			case "Amazon Kindle Fire":
				return "Kindle Fire (1st Gen)";
			}
			return text;
		}

		private static string GetDeviceModel()
		{
			return SystemInfo.deviceModel;
		}

		private static string GetDeviceType()
		{
			if (ClientInfo.RuntimePlatformIs("SamsungTVPlayer"))
			{
				return "TV";
			}
			if (ClientInfo.RuntimePlatformIs("tvOS"))
			{
				return "TV";
			}
			switch (SystemInfo.deviceType)
			{
			case 1:
			{
				string text = SystemInfo.deviceModel;
				if (text.StartsWith("iPhone"))
				{
					return "MOBILE_PHONE";
				}
				if (text.StartsWith("iPad"))
				{
					return "TABLET";
				}
				return (!ClientInfo.IsTablet()) ? "MOBILE_PHONE" : "TABLET";
			}
			case 2:
				return "CONSOLE";
			case 3:
				return "PC";
			default:
				return "UNKNOWN";
			}
		}

		private static string GetOperatingSystem()
		{
			if (ClientInfo.RuntimePlatformIs("tvOS"))
			{
				return "TVOS";
			}
			string text = SystemInfo.operatingSystem.ToUpper();
			if (text.Contains("WINDOWS"))
			{
				return "WINDOWS";
			}
			if (text.Contains("OSX"))
			{
				return "OSX";
			}
			if (text.Contains("MAC"))
			{
				return "OSX";
			}
			if (text.Contains("IOS") || text.Contains("IPHONE") || text.Contains("IPAD"))
			{
				return "IOS";
			}
			if (text.Contains("LINUX"))
			{
				return "LINUX";
			}
			if (text.Contains("ANDROID"))
			{
				if (SystemInfo.deviceModel.ToUpper().Contains("AMAZON"))
				{
					return "FIREOS";
				}
				return "ANDROID";
			}
			else
			{
				if (text.Contains("BLACKBERRY"))
				{
					return "BLACKBERRY";
				}
				return "UNKNOWN";
			}
		}

		private static string GetOperatingSystemVersion()
		{
			string text2;
			try
			{
				Regex regex = new Regex("[\\d|\\.]+");
				string text = SystemInfo.operatingSystem;
				Match match = regex.Match(text);
				if (match.Success)
				{
					text2 = match.Groups[0].ToString();
				}
				else
				{
					text2 = string.Empty;
				}
			}
			catch (Exception)
			{
				text2 = null;
			}
			return text2;
		}

		private static string GetManufacturer()
		{
			return null;
		}

		private static string GetCurrentTimezoneOffset()
		{
			string text;
			try
			{
				TimeZone currentTimeZone = TimeZone.CurrentTimeZone;
				DateTime now = DateTime.Now;
				TimeSpan utcOffset = currentTimeZone.GetUtcOffset(now);
				text = string.Format("{0}{1:D2}", (utcOffset.Hours < 0) ? string.Empty : "+", utcOffset.Hours);
			}
			catch (Exception)
			{
				text = null;
			}
			return text;
		}

		private static string GetCountryCode()
		{
			return null;
		}

		private static string GetLanguageCode()
		{
			switch (Application.systemLanguage)
			{
			case 0:
				return "af";
			case 1:
				return "ar";
			case 2:
				return "eu";
			case 3:
				return "be";
			case 4:
				return "bg";
			case 5:
				return "ca";
			case 6:
				return "zh";
			case 7:
				return "cs";
			case 8:
				return "da";
			case 9:
				return "nl";
			case 10:
				return "en";
			case 11:
				return "et";
			case 12:
				return "fo";
			case 13:
				return "fi";
			case 14:
				return "fr";
			case 15:
				return "de";
			case 16:
				return "el";
			case 17:
				return "he";
			case 18:
				return "hu";
			case 19:
				return "is";
			case 20:
				return "id";
			case 21:
				return "it";
			case 22:
				return "ja";
			case 23:
				return "ko";
			case 24:
				return "lv";
			case 25:
				return "lt";
			case 26:
				return "nn";
			case 27:
				return "pl";
			case 28:
				return "pt";
			case 29:
				return "ro";
			case 30:
				return "ru";
			case 31:
				return "sr";
			case 32:
				return "sk";
			case 33:
				return "sl";
			case 34:
				return "es";
			case 35:
				return "sv";
			case 36:
				return "th";
			case 37:
				return "tr";
			case 38:
				return "uk";
			case 39:
				return "vi";
			default:
				return "en";
			}
		}

		private static string GetLocale()
		{
			if (ClientInfo.CountryCode != null)
			{
				return string.Format("{0}_{1}", ClientInfo.LanguageCode, ClientInfo.CountryCode);
			}
			return string.Format("{0}_ZZ", ClientInfo.LanguageCode);
		}

		private static string platform;

		private static string deviceName;

		private static string deviceModel;

		private static string deviceType;

		private static string operatingSystem;

		private static string operatingSystemVersion;

		private static string manufacturer;

		private static string timezoneOffset;

		private static string countryCode;

		private static string languageCode;

		private static string locale;
	}
}
