using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class WeatherInfoBrief : MonoBehaviour
{
	public void SelectedWeather()
	{
		if (!base.GetComponent<Toggle>().isOn)
		{
			return;
		}
		WeatherInfoDetail component = base.transform.parent.GetComponent<WeatherInfoDetail>();
		if (component == null)
		{
			return;
		}
		component.SetDetailWeather(this.weatherDesc, this.weatherDescMidnight);
	}

	[HideInInspector]
	public WeatherDesc weatherDesc;

	public WeatherDesc weatherDescMidnight;
}
