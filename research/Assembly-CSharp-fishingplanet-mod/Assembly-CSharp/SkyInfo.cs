using System;

public class SkyInfo
{
	public string TypeName { get; set; }

	public SkyWeather WeatherType { get; set; }

	public int StartTime { get; set; }

	public int EndTime { get; set; }

	public string AssetBundleName { get; set; }

	public string SkyPrefabName { get; set; }

	public string LightPrefabName { get; set; }
}
