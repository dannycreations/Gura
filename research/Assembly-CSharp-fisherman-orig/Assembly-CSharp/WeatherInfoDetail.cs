using System;
using System.Globalization;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class WeatherInfoDetail : MonoBehaviour
{
	public void SetDetailWeather(WeatherDesc weather, WeatherDesc weatherMidnight)
	{
		this.TemperatureOfDay.GetComponent<Text>().text = string.Format("{2}{1} - {0}{1}", ((int)MeasuringSystemManager.Temperature((float)weather.AirTemperature)).ToString(CultureInfo.InvariantCulture), MeasuringSystemManager.TemperatureSufix(), ((int)MeasuringSystemManager.Temperature((float)weatherMidnight.AirTemperature)).ToString(CultureInfo.InvariantCulture));
		this.WindDirection.GetComponent<Text>().text = ScriptLocalization.Get(weather.WindDirection);
		this.WindPower.GetComponent<Text>().text = MeasuringSystemManager.WindSpeed(weather.WindSpeed).ToString("F1");
		this.WindSufix.GetComponent<Text>().text = MeasuringSystemManager.WindSpeedSufix();
		this.Pressure.GetComponent<Text>().text = MeasuringSystemManager.GetPreassureIcon(weather);
		if (this.FishForecastDiagramm != null)
		{
			this.FishForecastDiagrammLoadable.Image = this.FishForecastDiagramm;
			this.FishForecastDiagrammLoadable.Load(string.Format("Textures/Inventory/{0}", weather.FishingDiagramImageId));
		}
		this.Tips.text = weather.FishingDiagramDesc;
	}

	public Text TemperatureOfDay;

	public Text WindPower;

	public Text WindSufix;

	public Text WindDirection;

	public Text Pressure;

	public Image FishForecastDiagramm;

	private ResourcesHelpers.AsyncLoadableImage FishForecastDiagrammLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public Text Tips;
}
