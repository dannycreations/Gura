using System;
using System.Collections.Generic;
using System.Globalization;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class WeatherInfoShort : MonoBehaviour
{
	public void Refresh(IList<WeatherDesc> weathers, IList<WeatherDesc> weatherMidnight = null)
	{
		if (this.Day1 != null)
		{
			this.WeatherSet(this.Day1, weathers[0]);
			if (this.Day1.GetComponent<WeatherInfoBrief>() != null && weatherMidnight != null)
			{
				this.Day1.GetComponent<WeatherInfoBrief>().weatherDesc = weathers[0];
				this.Day1.GetComponent<WeatherInfoBrief>().weatherDescMidnight = weatherMidnight[0];
				this.Day1.GetComponent<Toggle>().isOn = false;
				this.Day1.GetComponent<Toggle>().isOn = true;
			}
		}
		if (this.Day2 != null)
		{
			this.WeatherSet(this.Day2, weathers[1]);
			if (this.Day2.GetComponent<WeatherInfoBrief>() != null && weatherMidnight != null)
			{
				this.Day2.GetComponent<WeatherInfoBrief>().weatherDesc = weathers[1];
				this.Day2.GetComponent<WeatherInfoBrief>().weatherDescMidnight = weatherMidnight[1];
			}
		}
		if (this.Day3 != null)
		{
			this.WeatherSet(this.Day3, weathers[2]);
			if (this.Day3.GetComponent<WeatherInfoBrief>() != null && weatherMidnight != null)
			{
				this.Day3.GetComponent<WeatherInfoBrief>().weatherDesc = weathers[2];
				this.Day3.GetComponent<WeatherInfoBrief>().weatherDescMidnight = weatherMidnight[2];
			}
		}
		if (this.Day4 != null)
		{
			this.WeatherSet(this.Day4, weathers[3]);
			if (this.Day4.GetComponent<WeatherInfoBrief>() != null && weatherMidnight != null)
			{
				this.Day4.GetComponent<WeatherInfoBrief>().weatherDesc = weathers[3];
				this.Day4.GetComponent<WeatherInfoBrief>().weatherDescMidnight = weatherMidnight[3];
			}
		}
		if (this.Day5 != null)
		{
			this.WeatherSet(this.Day5, weathers[4]);
			if (this.Day5.GetComponent<WeatherInfoBrief>() != null && weatherMidnight != null)
			{
				this.Day5.GetComponent<WeatherInfoBrief>().weatherDesc = weathers[4];
				this.Day5.GetComponent<WeatherInfoBrief>().weatherDescMidnight = weatherMidnight[4];
			}
		}
		if (this.Day6 != null && weathers.Count >= 6)
		{
			this.WeatherSet(this.Day6, weathers[5]);
			if (this.Day6.GetComponent<WeatherInfoBrief>() != null && weatherMidnight != null)
			{
				this.Day6.GetComponent<WeatherInfoBrief>().weatherDesc = weathers[5];
				this.Day6.GetComponent<WeatherInfoBrief>().weatherDescMidnight = weatherMidnight[5];
			}
		}
		if (this.Day7 != null && weathers.Count >= 7)
		{
			this.WeatherSet(this.Day7, weathers[6]);
			if (this.Day7.GetComponent<WeatherInfoBrief>() != null && weatherMidnight != null)
			{
				this.Day7.GetComponent<WeatherInfoBrief>().weatherDesc = weathers[6];
				this.Day7.GetComponent<WeatherInfoBrief>().weatherDescMidnight = weatherMidnight[6];
			}
		}
	}

	private void WeatherSet(GameObject day, WeatherDesc weather)
	{
		Transform transform = day.transform.Find("Temperature");
		if (transform != null)
		{
			transform.GetComponent<Text>().text = ((int)MeasuringSystemManager.Temperature((float)weather.AirTemperature)).ToString(CultureInfo.InvariantCulture) + " " + MeasuringSystemManager.TemperatureSufix();
		}
		Transform transform2 = day.transform.Find("Icon");
		if (transform2 != null)
		{
			string text = string.Empty;
			text = WeatherHelper.GetWeatherIcon(weather.Icon);
			transform2.GetComponent<Text>().text = text;
		}
	}

	public GameObject Day1;

	public GameObject Day2;

	public GameObject Day3;

	public GameObject Day4;

	public GameObject Day5;

	public GameObject Day6;

	public GameObject Day7;
}
