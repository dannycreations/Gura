using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;

public static class ExtensionsTools
{
	public static string[] SubArray(this string[] data, int index, int length)
	{
		string[] array = new string[length];
		Array.Copy(data, index, array, 0, length);
		return array;
	}

	public static IEnumerable<int> GetDayMaxTemperatures(this WeatherDesc[] data)
	{
		return ExtensionsTools.GetDayTemperatures(data, new Func<int, int, int>(Math.Max), false);
	}

	public static IEnumerable<int> GetDayMinTemperatures(this WeatherDesc[] data)
	{
		return ExtensionsTools.GetDayTemperatures(data, new Func<int, int, int>(Math.Min), false);
	}

	public static IEnumerable<int> GetNightMaxTemperatures(this WeatherDesc[] data)
	{
		return ExtensionsTools.GetNightTemperatures(data, new Func<int, int, int>(Math.Max), false);
	}

	public static IEnumerable<int> GetNightMinTemperatures(this WeatherDesc[] data)
	{
		return ExtensionsTools.GetNightTemperatures(data, new Func<int, int, int>(Math.Min), false);
	}

	public static IEnumerable<int> GetDayMaxTemperaturesWater(this WeatherDesc[] data)
	{
		return ExtensionsTools.GetDayTemperatures(data, new Func<int, int, int>(Math.Max), true);
	}

	public static IEnumerable<int> GetDayMinTemperaturesWater(this WeatherDesc[] data)
	{
		return ExtensionsTools.GetDayTemperatures(data, new Func<int, int, int>(Math.Min), true);
	}

	public static IEnumerable<int> GetNightMaxTemperaturesWater(this WeatherDesc[] data)
	{
		return ExtensionsTools.GetNightTemperatures(data, new Func<int, int, int>(Math.Max), true);
	}

	public static IEnumerable<int> GetNightMinTemperaturesWater(this WeatherDesc[] data)
	{
		return ExtensionsTools.GetNightTemperatures(data, new Func<int, int, int>(Math.Min), true);
	}

	private static IEnumerable<int> GetDayTemperatures(WeatherDesc[] data, Func<int, int, int> func, bool isWater)
	{
		List<WeatherDesc> list = new List<WeatherDesc>();
		List<WeatherDesc> earlyMorningWeathers = new List<WeatherDesc>();
		List<WeatherDesc> eveningWeathers = new List<WeatherDesc>();
		List<WeatherDesc> lateEveningWeathers = new List<WeatherDesc>();
		List<WeatherDesc> morningWeathers = new List<WeatherDesc>();
		foreach (WeatherDesc weatherDesc in data)
		{
			if (weatherDesc.TimeOfDay == TimeOfDay.Midday.ToString())
			{
				list.Add(weatherDesc);
			}
			else if (weatherDesc.TimeOfDay == TimeOfDay.EarlyMorning.ToString())
			{
				earlyMorningWeathers.Add(weatherDesc);
			}
			else if (weatherDesc.TimeOfDay == TimeOfDay.Evening.ToString())
			{
				eveningWeathers.Add(weatherDesc);
			}
			else if (weatherDesc.TimeOfDay == TimeOfDay.LateEvening.ToString())
			{
				lateEveningWeathers.Add(weatherDesc);
			}
			else if (weatherDesc.TimeOfDay == TimeOfDay.Morning.ToString())
			{
				morningWeathers.Add(weatherDesc);
			}
		}
		return list.Select((WeatherDesc s, int i) => func((!isWater) ? s.AirTemperature : s.WaterTemperature, func((!isWater) ? earlyMorningWeathers[i].AirTemperature : earlyMorningWeathers[i].WaterTemperature, func((!isWater) ? eveningWeathers[i].AirTemperature : eveningWeathers[i].WaterTemperature, func((!isWater) ? lateEveningWeathers[i].AirTemperature : lateEveningWeathers[i].WaterTemperature, (!isWater) ? morningWeathers[i].AirTemperature : morningWeathers[i].WaterTemperature)))));
	}

	private static IEnumerable<int> GetNightTemperatures(WeatherDesc[] data, Func<int, int, int> func, bool isWater)
	{
		List<WeatherDesc> list = new List<WeatherDesc>();
		List<WeatherDesc> earlyNightWeathers = new List<WeatherDesc>();
		List<WeatherDesc> lateNightWeathers = new List<WeatherDesc>();
		foreach (WeatherDesc weatherDesc in data)
		{
			if (weatherDesc.TimeOfDay == TimeOfDay.MidNight.ToString())
			{
				list.Add(weatherDesc);
			}
			else if (weatherDesc.TimeOfDay == TimeOfDay.EarlyNight.ToString())
			{
				earlyNightWeathers.Add(weatherDesc);
			}
			else if (weatherDesc.TimeOfDay == TimeOfDay.LateNight.ToString())
			{
				lateNightWeathers.Add(weatherDesc);
			}
		}
		return list.Select((WeatherDesc s, int i) => func((!isWater) ? s.AirTemperature : s.WaterTemperature, func((!isWater) ? earlyNightWeathers[i].AirTemperature : earlyNightWeathers[i].WaterTemperature, (!isWater) ? lateNightWeathers[i].AirTemperature : lateNightWeathers[i].WaterTemperature)));
	}
}
