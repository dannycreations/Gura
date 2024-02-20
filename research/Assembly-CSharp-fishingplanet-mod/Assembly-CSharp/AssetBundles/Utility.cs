using System;
using UnityEngine;

namespace AssetBundles
{
	public class Utility
	{
		public static string GetPlatformName()
		{
			return Utility.GetPlatformForAssetBundles(Application.platform);
		}

		private static string GetPlatformForAssetBundles(RuntimePlatform platform)
		{
			if (platform == 1)
			{
				return "OSX";
			}
			if (platform == 2)
			{
				return "x32";
			}
			switch (platform)
			{
			case 25:
				return "Ps4";
			default:
				if (platform != 13)
				{
					return null;
				}
				return "Linux";
			case 27:
				return "XboxOne";
			}
		}

		public const string AssetBundlesOutputPath = "AssetBundles";
	}
}
