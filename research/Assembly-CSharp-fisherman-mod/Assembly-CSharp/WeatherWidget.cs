using System;
using System.Globalization;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeatherWidget
{
	public void Init(GameObject weatherControl, GameObject pressureControl, GameObject windDirectionControl, GameObject windPowerControl, GameObject windPowerSuffix, GameObject temperatureControl, GameObject windCompass, GameObject tWaterControl, bool isTmp = false)
	{
		this._temperatureWaterControl = tWaterControl;
		this._weatherControl = weatherControl;
		this._pressureControl = pressureControl;
		this._windDirectionControl = windDirectionControl;
		this._windPowerControl = windPowerControl;
		this._windPowerSuffix = windPowerSuffix;
		this._temperatureControl = temperatureControl;
		this._windCompass = windCompass;
		this._isTmp = isTmp;
	}

	public void VisualizeWeather()
	{
		if (StaticUserData.CurrentPond == null)
		{
			return;
		}
		WeatherDesc currentWeather = TimeAndWeatherManager.CurrentWeather;
		if (currentWeather == null)
		{
			return;
		}
		bool flag = currentWeather.WindSpeed > 0f;
		string text;
		string text2;
		string text3;
		string text4;
		string text5;
		this.GetData(currentWeather, out text, out text2, out text3, out text4, out text5);
		string text6 = null;
		string text7 = null;
		if (flag)
		{
			this.GetWindData(currentWeather, out text6, out text7);
		}
		if (this._isTmp)
		{
			this.UpdateDataTmp(flag, text, text2, text3, text4, text6, text7, text5);
		}
		else
		{
			this.UpdateData(flag, text, text2, text3, text4, text6, text7, text5);
		}
		this.UpdateVisibility(flag);
	}

	private void UpdateVisibility(bool hasWind)
	{
		this._windPowerControl.SetActive(hasWind);
		this._windDirectionControl.SetActive(hasWind);
		this._windPowerSuffix.SetActive(hasWind);
		this._windCompass.SetActive(hasWind);
	}

	private void GetData(WeatherDesc currentWeather, out string iconWather, out string temperature, out string pressure, out string windDirection, out string temperatureWater)
	{
		iconWather = WeatherHelper.GetWeatherIcon(currentWeather.Icon);
		temperature = ((int)MeasuringSystemManager.Temperature((float)currentWeather.AirTemperature)).ToString(CultureInfo.InvariantCulture) + string.Empty + MeasuringSystemManager.TemperatureSufix();
		temperatureWater = ((int)MeasuringSystemManager.Temperature((float)currentWeather.WaterTemperature)).ToString(CultureInfo.InvariantCulture) + string.Empty + MeasuringSystemManager.TemperatureSufix();
		pressure = MeasuringSystemManager.GetPreassureIcon(currentWeather);
		windDirection = ScriptLocalization.Get(currentWeather.WindDirection);
	}

	private void GetWindData(WeatherDesc currentWeather, out string windPower, out string windPowerSuffix)
	{
		windPower = MeasuringSystemManager.WindSpeed(currentWeather.WindSpeed).ToString("F1");
		windPowerSuffix = MeasuringSystemManager.WindSpeedSufix();
	}

	private void UpdateData(bool hasWind, string iconWather, string temperature, string pressure, string windDirection, string windPower, string windPowerSuffix, string temperatureWater)
	{
		this._weatherControl.GetComponent<Text>().text = iconWather;
		this._temperatureControl.GetComponent<Text>().text = temperature;
		this._pressureControl.GetComponent<Text>().text = pressure;
		this._windDirectionControl.GetComponent<Text>().text = windDirection;
		this._temperatureWaterControl.GetComponent<Text>().text = temperatureWater;
		if (hasWind)
		{
			this._windPowerControl.GetComponent<Text>().text = windPower;
			this._windPowerSuffix.GetComponent<Text>().text = windPowerSuffix;
		}
	}

	private void UpdateDataTmp(bool hasWind, string iconWather, string temperature, string pressure, string windDirection, string windPower, string windPowerSuffix, string temperatureWater)
	{
		this._weatherControl.GetComponent<TextMeshProUGUI>().text = iconWather;
		this._temperatureControl.GetComponent<TextMeshProUGUI>().text = temperature;
		this._pressureControl.GetComponent<TextMeshProUGUI>().text = pressure;
		this._windDirectionControl.GetComponent<TextMeshProUGUI>().text = windDirection;
		this._temperatureWaterControl.GetComponent<TextMeshProUGUI>().text = temperatureWater;
		if (hasWind)
		{
			this._windPowerControl.GetComponent<TextMeshProUGUI>().text = windPower;
			this._windPowerSuffix.GetComponent<TextMeshProUGUI>().text = windPowerSuffix;
		}
	}

	private GameObject _weatherControl;

	private GameObject _pressureControl;

	private GameObject _windDirectionControl;

	private GameObject _windPowerControl;

	private GameObject _windPowerSuffix;

	private GameObject _temperatureControl;

	private GameObject _temperatureWaterControl;

	private GameObject _windCompass;

	private bool _isTmp;
}
