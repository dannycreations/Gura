using System;
using System.Collections.Generic;

public class SkyList
{
	private SkyList()
	{
	}

	public static List<SkyInfo> Instance()
	{
		List<SkyInfo> list;
		if ((list = SkyList._instanceSkyTypes) == null)
		{
			list = (SkyList._instanceSkyTypes = SkyList.GetSkyInfosInstance());
		}
		return list;
	}

	private static List<SkyInfo> GetSkyInfosInstance()
	{
		SkyList skyList = new SkyList();
		List<SkyInfo> list = new List<SkyInfo>();
		list.AddRange(skyList.GetInstanceClearSky());
		list.AddRange(skyList.GetInstanceCloudySky());
		list.AddRange(skyList.GetInstancePartlyCloudySky());
		list.AddRange(skyList.GetInstanceRainySky());
		list.AddRange(skyList.GetInstanceNightSky());
		return list;
	}

	private IEnumerable<SkyInfo> GetInstanceClearSky()
	{
		return new List<SkyInfo>
		{
			new SkyInfo
			{
				TypeName = "pSky1_4",
				WeatherType = SkyWeather.Clear,
				StartTime = 4,
				EndTime = 5,
				AssetBundleName = "psky1_4",
				LightPrefabName = "pLight1_4",
				SkyPrefabName = "pSky1_4"
			},
			new SkyInfo
			{
				TypeName = "pSky1_8",
				WeatherType = SkyWeather.Clear,
				StartTime = 6,
				EndTime = 8,
				AssetBundleName = "psky1_8",
				LightPrefabName = "pLight1_8",
				SkyPrefabName = "pSky1_8"
			},
			new SkyInfo
			{
				TypeName = "pSky1_12",
				WeatherType = SkyWeather.Clear,
				StartTime = 9,
				EndTime = 10,
				AssetBundleName = "psky1_12",
				LightPrefabName = "pLight1_12",
				SkyPrefabName = "pSky1_12"
			},
			new SkyInfo
			{
				TypeName = "pSky1_16",
				WeatherType = SkyWeather.Clear,
				StartTime = 11,
				EndTime = 12,
				AssetBundleName = "psky1_16",
				LightPrefabName = "pLight1_16",
				SkyPrefabName = "pSky1_16"
			},
			new SkyInfo
			{
				TypeName = "pSky1_20",
				WeatherType = SkyWeather.Clear,
				StartTime = 13,
				EndTime = 14,
				AssetBundleName = "psky1_20",
				LightPrefabName = "pLight1_20",
				SkyPrefabName = "pSky1_20"
			},
			new SkyInfo
			{
				TypeName = "pSky1_24",
				WeatherType = SkyWeather.Clear,
				StartTime = 15,
				EndTime = 16,
				AssetBundleName = "psky1_24",
				LightPrefabName = "pLight1_24",
				SkyPrefabName = "pSky1_24"
			},
			new SkyInfo
			{
				TypeName = "pSky1_28",
				WeatherType = SkyWeather.Clear,
				StartTime = 17,
				EndTime = 18,
				AssetBundleName = "psky1_28",
				LightPrefabName = "pLight1_28",
				SkyPrefabName = "pSky1_28"
			},
			new SkyInfo
			{
				TypeName = "pSky1_32",
				WeatherType = SkyWeather.Clear,
				StartTime = 19,
				EndTime = 20,
				AssetBundleName = "psky1_32",
				LightPrefabName = "pLight1_32",
				SkyPrefabName = "pSky1_32"
			}
		};
	}

	private IEnumerable<SkyInfo> GetInstanceCloudySky()
	{
		return new List<SkyInfo>
		{
			new SkyInfo
			{
				TypeName = "pSky3_4",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 4,
				EndTime = 5,
				AssetBundleName = "psky3_4",
				LightPrefabName = "pLight3_4",
				SkyPrefabName = "pSky3_4"
			},
			new SkyInfo
			{
				TypeName = "pSky3_8",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 6,
				EndTime = 8,
				AssetBundleName = "psky3_8",
				LightPrefabName = "pLight3_8",
				SkyPrefabName = "pSky3_8"
			},
			new SkyInfo
			{
				TypeName = "pSky3_12",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 9,
				EndTime = 10,
				AssetBundleName = "psky3_12",
				LightPrefabName = "pLight3_12",
				SkyPrefabName = "pSky3_12"
			},
			new SkyInfo
			{
				TypeName = "pSky3_16",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 11,
				EndTime = 12,
				AssetBundleName = "psky3_16",
				LightPrefabName = "pLight3_16",
				SkyPrefabName = "pSky3_16"
			},
			new SkyInfo
			{
				TypeName = "pSky3_20",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 13,
				EndTime = 14,
				AssetBundleName = "psky3_20",
				LightPrefabName = "pLight3_20",
				SkyPrefabName = "pSky3_20"
			},
			new SkyInfo
			{
				TypeName = "pSky3_24",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 15,
				EndTime = 16,
				AssetBundleName = "psky3_24",
				LightPrefabName = "pLight3_24",
				SkyPrefabName = "pSky3_24"
			},
			new SkyInfo
			{
				TypeName = "pSky3_28",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 17,
				EndTime = 18,
				AssetBundleName = "psky3_28",
				LightPrefabName = "pLight3_28",
				SkyPrefabName = "pSky3_28"
			},
			new SkyInfo
			{
				TypeName = "pSky3_32",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 19,
				EndTime = 20,
				AssetBundleName = "psky3_32",
				LightPrefabName = "pLight3_32",
				SkyPrefabName = "pSky3_32"
			}
		};
	}

	private IEnumerable<SkyInfo> GetInstancePartlyCloudySky()
	{
		return new List<SkyInfo>
		{
			new SkyInfo
			{
				TypeName = "pSky2_4",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 4,
				EndTime = 5,
				AssetBundleName = "psky2_4",
				LightPrefabName = "pLight2_4",
				SkyPrefabName = "pSky2_4"
			},
			new SkyInfo
			{
				TypeName = "pSky2_8",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 6,
				EndTime = 8,
				AssetBundleName = "psky2_8",
				LightPrefabName = "pLight2_8",
				SkyPrefabName = "pSky2_8"
			},
			new SkyInfo
			{
				TypeName = "pSky2_12",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 9,
				EndTime = 10,
				AssetBundleName = "psky2_12",
				LightPrefabName = "pLight2_12",
				SkyPrefabName = "pSky2_12"
			},
			new SkyInfo
			{
				TypeName = "pSky2_16",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 11,
				EndTime = 12,
				AssetBundleName = "psky2_16",
				LightPrefabName = "pLight2_16",
				SkyPrefabName = "pSky2_16"
			},
			new SkyInfo
			{
				TypeName = "pSky2_20",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 13,
				EndTime = 14,
				AssetBundleName = "psky2_20",
				LightPrefabName = "pLight2_20",
				SkyPrefabName = "pSky2_20"
			},
			new SkyInfo
			{
				TypeName = "pSky2_24",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 15,
				EndTime = 16,
				AssetBundleName = "psky2_24",
				LightPrefabName = "pLight2_24",
				SkyPrefabName = "pSky2_24"
			},
			new SkyInfo
			{
				TypeName = "pSky2_28",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 17,
				EndTime = 18,
				AssetBundleName = "psky2_28",
				LightPrefabName = "pLight2_28",
				SkyPrefabName = "pSky2_28"
			},
			new SkyInfo
			{
				TypeName = "pSky2_32",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 19,
				EndTime = 20,
				AssetBundleName = "psky2_32",
				LightPrefabName = "pLight2_32",
				SkyPrefabName = "pSky2_32"
			}
		};
	}

	private IEnumerable<SkyInfo> GetInstanceRainySky()
	{
		return new List<SkyInfo>
		{
			new SkyInfo
			{
				TypeName = "pSky4_4",
				WeatherType = SkyWeather.Rainy,
				StartTime = 4,
				EndTime = 5,
				AssetBundleName = "psky4_4",
				LightPrefabName = "pLight4_4",
				SkyPrefabName = "pSky4_4"
			},
			new SkyInfo
			{
				TypeName = "pSky4_8",
				WeatherType = SkyWeather.Rainy,
				StartTime = 6,
				EndTime = 8,
				AssetBundleName = "psky4_8",
				LightPrefabName = "pLight4_8",
				SkyPrefabName = "pSky4_8"
			},
			new SkyInfo
			{
				TypeName = "pSky4_12",
				WeatherType = SkyWeather.Rainy,
				StartTime = 9,
				EndTime = 10,
				AssetBundleName = "psky4_12",
				LightPrefabName = "pLight4_12",
				SkyPrefabName = "pSky4_12"
			},
			new SkyInfo
			{
				TypeName = "pSky4_16",
				WeatherType = SkyWeather.Rainy,
				StartTime = 11,
				EndTime = 12,
				AssetBundleName = "psky4_16",
				LightPrefabName = "pLight4_16",
				SkyPrefabName = "pSky4_16"
			},
			new SkyInfo
			{
				TypeName = "pSky4_20",
				WeatherType = SkyWeather.Rainy,
				StartTime = 13,
				EndTime = 14,
				AssetBundleName = "psky4_20",
				LightPrefabName = "pLight4_20",
				SkyPrefabName = "pSky4_20"
			},
			new SkyInfo
			{
				TypeName = "pSky4_24",
				WeatherType = SkyWeather.Rainy,
				StartTime = 15,
				EndTime = 16,
				AssetBundleName = "psky4_24",
				LightPrefabName = "pLight4_24",
				SkyPrefabName = "pSky4_24"
			},
			new SkyInfo
			{
				TypeName = "pSky4_28",
				WeatherType = SkyWeather.Rainy,
				StartTime = 17,
				EndTime = 18,
				AssetBundleName = "psky4_28",
				LightPrefabName = "pLight4_28",
				SkyPrefabName = "pSky4_28"
			},
			new SkyInfo
			{
				TypeName = "pSky4_32",
				WeatherType = SkyWeather.Rainy,
				StartTime = 19,
				EndTime = 20,
				AssetBundleName = "psky4_32",
				LightPrefabName = "pLight4_32",
				SkyPrefabName = "pSky4_32"
			}
		};
	}

	private IEnumerable<SkyInfo> GetInstanceNightSky()
	{
		return new List<SkyInfo>
		{
			new SkyInfo
			{
				TypeName = "pMoon_Full",
				WeatherType = SkyWeather.Clear,
				StartTime = 21,
				EndTime = 24,
				AssetBundleName = "pmoon_full",
				LightPrefabName = "pLight_Moon_Full",
				SkyPrefabName = "pMoon_Full"
			},
			new SkyInfo
			{
				TypeName = "pMoon_Full",
				WeatherType = SkyWeather.Clear,
				StartTime = 0,
				EndTime = 4,
				AssetBundleName = "pmoon_full",
				LightPrefabName = "pLight_Moon_Full",
				SkyPrefabName = "pMoon_Full"
			},
			new SkyInfo
			{
				TypeName = "pMoon_Half",
				WeatherType = SkyWeather.ClearHalfMoon,
				StartTime = 21,
				EndTime = 24,
				AssetBundleName = "pmoon_small",
				LightPrefabName = "pLight_Moon_Small",
				SkyPrefabName = "pMoon_Small"
			},
			new SkyInfo
			{
				TypeName = "pMoon_Half",
				WeatherType = SkyWeather.ClearHalfMoon,
				StartTime = 0,
				EndTime = 4,
				AssetBundleName = "pmoon_small",
				LightPrefabName = "pLight_Moon_Small",
				SkyPrefabName = "pMoon_Small"
			},
			new SkyInfo
			{
				TypeName = "pMoon_Small_Cloudy",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 21,
				EndTime = 24,
				AssetBundleName = "pmoon_small",
				LightPrefabName = "pLight_Moon_Small",
				SkyPrefabName = "pMoon_Small"
			},
			new SkyInfo
			{
				TypeName = "pMoon_Small_Cloudy",
				WeatherType = SkyWeather.Cloudy,
				StartTime = 0,
				EndTime = 4,
				AssetBundleName = "pmoon_small",
				LightPrefabName = "pLight_Moon_Small",
				SkyPrefabName = "pMoon_Small"
			},
			new SkyInfo
			{
				TypeName = "pMoon_Small_Cloudy",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 21,
				EndTime = 24,
				AssetBundleName = "pno_moon",
				LightPrefabName = "pLight_No_Moon",
				SkyPrefabName = "pNo_Moon"
			},
			new SkyInfo
			{
				TypeName = "pMoon_Small_Cloudy",
				WeatherType = SkyWeather.PartlyCloudy,
				StartTime = 0,
				EndTime = 4,
				AssetBundleName = "pno_moon",
				LightPrefabName = "pLight_No_Moon",
				SkyPrefabName = "pNo_Moon"
			},
			new SkyInfo
			{
				TypeName = "pNo_Moon",
				WeatherType = SkyWeather.Rainy,
				StartTime = 21,
				EndTime = 24,
				AssetBundleName = "pno_moon",
				LightPrefabName = "pLight_No_Moon",
				SkyPrefabName = "pNo_Moon"
			},
			new SkyInfo
			{
				TypeName = "pNo_Moon",
				WeatherType = SkyWeather.Rainy,
				StartTime = 0,
				EndTime = 4,
				AssetBundleName = "pno_moon",
				LightPrefabName = "pLight_No_Moon",
				SkyPrefabName = "pNo_Moon"
			},
			new SkyInfo
			{
				TypeName = "pHalloween",
				WeatherType = SkyWeather.Event,
				StartTime = 21,
				EndTime = 24,
				AssetBundleName = "phaloween_moon",
				LightPrefabName = "pLight_Haloween_Moon",
				SkyPrefabName = "phaloween_moon"
			},
			new SkyInfo
			{
				TypeName = "pHalloween",
				WeatherType = SkyWeather.Event,
				StartTime = 0,
				EndTime = 4,
				AssetBundleName = "phaloween_moon",
				LightPrefabName = "pLight_Haloween_Moon",
				SkyPrefabName = "phaloween_moon"
			}
		};
	}

	private static List<SkyInfo> _instanceSkyTypes;
}
