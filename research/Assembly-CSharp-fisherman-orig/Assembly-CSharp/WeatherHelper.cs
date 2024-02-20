using System;
using System.Globalization;
using System.Linq;
using ObjectModel;

public class WeatherHelper
{
	public static string GetTemperature(int t)
	{
		return ((int)MeasuringSystemManager.Temperature((float)t)).ToString(CultureInfo.InvariantCulture);
	}

	public static string GetWeatherIcon(int iconId)
	{
		string text = string.Empty;
		switch (iconId)
		{
		case 1:
			text = "\ue617";
			break;
		case 2:
			text = "\ue618";
			break;
		case 3:
			text = "\ue619";
			break;
		case 4:
			text = "\ue61a";
			break;
		case 5:
			text = "\ue61b";
			break;
		case 6:
			text = "\ue61c";
			break;
		case 7:
			text = "\ue61d";
			break;
		case 8:
			text = "\ue727";
			break;
		case 9:
			text = "\ue728";
			break;
		case 10:
			text = "\ue729";
			break;
		case 11:
			text = "\ue730";
			break;
		case 12:
			text = "\ue731";
			break;
		case 13:
			text = "\ue732";
			break;
		}
		return text;
	}

	public static string GetPondWeatherIcon(Pond p)
	{
		if (p.Weather != null && p.Weather.Length > 0)
		{
			WeatherDesc weatherDesc = p.Weather.First((WeatherDesc x) => x.TimeOfDay == TimeOfDay.Midday.ToString());
			if (weatherDesc != null)
			{
				return WeatherHelper.GetWeatherIcon(weatherDesc.Icon);
			}
		}
		return string.Empty;
	}
}
