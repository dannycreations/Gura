using System;
using System.Globalization;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeatherPreviewPanel : MonoBehaviour
{
	public void Init(WeatherPreviewPanel.WeatherData wd, int dayNumber, bool previewOnly = false, bool day = false)
	{
		this._dayImageLdbl.Image = this._dayDiagramm;
		this._nightImageLdbl.Image = this._nightDiagramm;
		this.Init(wd.DayWeather, wd.NightWeather, wd.Day.Early, wd.Day.Late, wd.Night.Early, wd.Night.Late, wd.DayWater.Early, wd.DayWater.Late, wd.NightWater.Early, wd.NightWater.Late, dayNumber, previewOnly, day);
	}

	public void Init(WeatherDesc dayWeather, WeatherDesc nightWeather, int earlyMorning, int lateEvening, int earlyNight, int lateNight, int earlyMorningWater, int lateEveningWater, int earlyNightWater, int lateNightWater, int dayNumber, bool previewOnly = false, bool day = false)
	{
		this._dayImageLdbl.Image = this._dayDiagramm;
		this._nightImageLdbl.Image = this._nightDiagramm;
		if (previewOnly)
		{
			if (day)
			{
				if (this._nightWeather != null)
				{
					this._nightWeather.SetActive(false);
				}
				if (this._dayWeather != null)
				{
					this._dayWeather.SetActive(true);
				}
				this.InitWeather(dayWeather, nightWeather, earlyMorning, lateEvening, dayNumber, this._dayWeatherIcon, this._dayDescription, this._dayPressureIcon, this._dayImageLdbl, earlyMorningWater, lateEveningWater, this._waterDayWeather);
			}
			else
			{
				if (this._nightWeather != null)
				{
					this._nightWeather.SetActive(true);
				}
				if (this._dayWeather != null)
				{
					this._dayWeather.SetActive(false);
				}
				this.InitWeather(nightWeather, dayWeather, earlyNight, lateNight, dayNumber, this._nightWeatherIcon, this._nightDescription, this._nightPressureIcon, this._nightImageLdbl, earlyNightWater, lateNightWater, this._waterNightWeather);
			}
		}
		else
		{
			this.InitWeather(dayWeather, nightWeather, earlyMorning, lateEvening, dayNumber, this._dayWeatherIcon, this._dayDescription, this._dayPressureIcon, this._dayImageLdbl, earlyMorningWater, lateEveningWater, this._waterDayWeather);
			this.InitWeather(nightWeather, dayWeather, earlyNight, lateNight, dayNumber, this._nightWeatherIcon, this._nightDescription, this._nightPressureIcon, this._nightImageLdbl, earlyNightWater, lateNightWater, this._waterNightWeather);
			if (this._description != null)
			{
				this._description.text = dayWeather.FishingDiagramDesc;
			}
			if (this._dayNumber != null)
			{
				this._dayNumber.text = string.Format("{0} {1}", ScriptLocalization.Get("DayCaption"), dayNumber).ToUpper();
			}
			if (this._nightNumber != null)
			{
				this._nightNumber.text = string.Format("{0} {1}", ScriptLocalization.Get("NightCaption").ToUpper(), dayNumber).ToUpper();
			}
		}
	}

	private void InitWeather(WeatherDesc weather, WeatherDesc weatherTo, int tempFrom, int tempTo, int dayNumber, TextMeshProUGUI weatherIcon, TextMeshProUGUI weatherDesc, TextMeshProUGUI weatherPressure, ResourcesHelpers.AsyncLoadableImage weatherDiagramm, int tempFromWater, int tempToWater, TextMeshProUGUI weatherDescWater)
	{
		string text = MeasuringSystemManager.TemperatureSufix();
		string text2 = string.Format("{0}{1} - {2}{1}", WeatherHelper.GetTemperature(tempFromWater), text, WeatherHelper.GetTemperature(tempToWater));
		string text3 = string.Format("{0}{1} - {2}{1}", WeatherHelper.GetTemperature(tempFrom), text, WeatherHelper.GetTemperature(tempTo));
		if (weatherDescWater != null)
		{
			weatherDescWater.text = text2;
		}
		else
		{
			text3 = string.Format("{0} {1}", WeatherHelper.GetTemperature(weather.WaterTemperature), text);
		}
		string text4 = ScriptLocalization.Get(weather.WindDirection);
		string text5 = MeasuringSystemManager.WindSpeed(weather.WindSpeed).ToString("F1");
		string text6 = MeasuringSystemManager.WindSpeedSufix();
		if (weatherIcon != null)
		{
			weatherIcon.text = WeatherHelper.GetWeatherIcon(weather.Icon);
		}
		if (weatherDesc != null)
		{
			weatherDesc.text = string.Format("{1}\n{2} {3} {4}", new object[] { dayNumber, text3, text4, text5, text6 });
		}
		if (weatherPressure != null)
		{
			weatherPressure.text = MeasuringSystemManager.GetPreassureIcon(weather);
		}
		if (weatherDiagramm != null)
		{
			weatherDiagramm.Load(string.Format("Textures/Inventory/{0}", weather.FishingDiagramImageId.Value.ToString(CultureInfo.InvariantCulture)));
		}
	}

	[SerializeField]
	private TextMeshProUGUI _waterDayWeather;

	[SerializeField]
	private TextMeshProUGUI _waterNightWeather;

	[SerializeField]
	private TextMeshProUGUI _dayDescription;

	[SerializeField]
	private TextMeshProUGUI _dayWeatherIcon;

	[SerializeField]
	private TextMeshProUGUI _dayPressureIcon;

	[SerializeField]
	private TextMeshProUGUI _dayNumber;

	[SerializeField]
	private TextMeshProUGUI _nightDescription;

	[SerializeField]
	private TextMeshProUGUI _nightWeatherIcon;

	[SerializeField]
	private TextMeshProUGUI _nightPressureIcon;

	[SerializeField]
	private TextMeshProUGUI _nightNumber;

	[SerializeField]
	private Image _dayDiagramm;

	[SerializeField]
	private Image _nightDiagramm;

	[SerializeField]
	private TextMeshProUGUI _description;

	[SerializeField]
	private GameObject _nightWeather;

	[SerializeField]
	private GameObject _dayWeather;

	private ResourcesHelpers.AsyncLoadableImage _dayImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	private ResourcesHelpers.AsyncLoadableImage _nightImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	public class MinMaxRange
	{
		public MinMaxRange(int early, int late)
		{
			this.Early = early;
			this.Late = late;
		}

		public int Early { get; private set; }

		public int Late { get; private set; }
	}

	public class WeatherData
	{
		public WeatherData(WeatherDesc dayWeather, WeatherDesc nightWeather, WeatherPreviewPanel.MinMaxRange day, WeatherPreviewPanel.MinMaxRange night, WeatherPreviewPanel.MinMaxRange dayWater, WeatherPreviewPanel.MinMaxRange nightWater)
		{
			this.DayWater = dayWater;
			this.NightWater = nightWater;
			this.Day = day;
			this.Night = night;
			this.DayWeather = dayWeather;
			this.NightWeather = nightWeather;
		}

		public WeatherDesc DayWeather { get; private set; }

		public WeatherDesc NightWeather { get; private set; }

		public WeatherPreviewPanel.MinMaxRange Day { get; private set; }

		public WeatherPreviewPanel.MinMaxRange Night { get; private set; }

		public WeatherPreviewPanel.MinMaxRange DayWater { get; private set; }

		public WeatherPreviewPanel.MinMaxRange NightWater { get; private set; }
	}
}
