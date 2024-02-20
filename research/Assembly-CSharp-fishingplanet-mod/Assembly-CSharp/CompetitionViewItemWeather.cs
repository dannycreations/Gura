using System;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;

public class CompetitionViewItemWeather : CompetitionViewItem
{
	public void UpdateData(WeatherDesc dayWeather, int inGameStartHour)
	{
		TournamentHelper.TournamentWeatherDesc tournamentWeather = TournamentHelper.GetTournamentWeather(dayWeather);
		this._tempIco.text = tournamentWeather.WeatherIcon;
		this._tempIco2.text = tournamentWeather.PressureIcon;
		this._temp.text = tournamentWeather.WeatherTemperature;
		this._tempWater.text = tournamentWeather.WaterTemperature;
		this._wind.text = string.Format("{0} {1} {2}", tournamentWeather.WeatherWindDirection, tournamentWeather.WeatherWindPower, tournamentWeather.WeatherWindSuffix);
		this._startTime.text = string.Format("{0} {1}", MeasuringSystemManager.GetHourString(new DateTime(2000, 1, 1, inGameStartHour, 0, 0)), UgcConsts.GetGrey(string.Format(" ({0})", ScriptLocalization.Get("UGC_InGameCaption"))));
		this._weather.SetActive(true);
		this.Hint.gameObject.SetActive(false);
	}

	public override void UpdateData(string value, string hint = null)
	{
		base.UpdateData(value, hint);
		if (string.IsNullOrEmpty(value))
		{
			this._weather.SetActive(false);
		}
	}

	[SerializeField]
	private GameObject _weather;

	[SerializeField]
	private TextMeshProUGUI _startTime;

	[SerializeField]
	private TextMeshProUGUI _temp;

	[SerializeField]
	private TextMeshProUGUI _tempIco;

	[SerializeField]
	private TextMeshProUGUI _tempIco2;

	[SerializeField]
	private TextMeshProUGUI _tempWater;

	[SerializeField]
	private TextMeshProUGUI _tempWaterIco;

	[SerializeField]
	private TextMeshProUGUI _wind;
}
