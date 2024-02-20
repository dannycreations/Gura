using System;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;

public class TimeAndWeatherManager : MonoBehaviour
{
	public static TimeSpan? CurrentTime
	{
		get
		{
			if (!TimeAndWeatherManager._timeInited)
			{
				return null;
			}
			return new TimeSpan?(TimeAndWeatherManager._currentTime);
		}
	}

	public static WeatherDesc CurrentWeather
	{
		get
		{
			return TimeAndWeatherManager._currentWeather;
		}
	}

	public static WeatherDesc NextWeather
	{
		get
		{
			if (TimeAndWeatherManager.CurrentWeathers == null)
			{
				return null;
			}
			WeatherDesc weatherDesc = TimeAndWeatherManager.CurrentWeathers.FirstOrDefault((WeatherDesc x) => x.TimeOfDay == TimeAndWeatherManager.GetNextNextTimeOfDay(TimeAndWeatherManager._currentTime.ToTimeOfDay()).ToString());
			if (weatherDesc == null)
			{
				TimeOfDay timeOfDay = TimeAndWeatherManager._currentTime.ToTimeOfDay();
				string text = "___kocha TimeAndWeatherManager - NextWeather is null _currentTime:{0} GetNextNextTimeOfDay:{1} CurrentWeathers:{2}";
				object[] array = new object[3];
				array[0] = timeOfDay;
				array[1] = TimeAndWeatherManager.GetNextNextTimeOfDay(timeOfDay).ToString();
				array[2] = string.Join(",", TimeAndWeatherManager.CurrentWeathers.Select((WeatherDesc p) => p.TimeOfDay).ToArray<string>());
				LogHelper.Error(text, array);
			}
			return weatherDesc;
		}
	}

	private static TimeOfDay GetNextNextTimeOfDay(TimeOfDay timeOfDay)
	{
		switch (timeOfDay)
		{
		case TimeOfDay.EarlyMorning:
			return TimeOfDay.Morning;
		case TimeOfDay.Morning:
			return TimeOfDay.Midday;
		case TimeOfDay.Midday:
			return TimeOfDay.Evening;
		case TimeOfDay.Evening:
			return TimeOfDay.LateEvening;
		case TimeOfDay.LateEvening:
			return TimeOfDay.EarlyNight;
		case TimeOfDay.EarlyNight:
			return TimeOfDay.MidNight;
		case TimeOfDay.MidNight:
			return TimeOfDay.LateNight;
		case TimeOfDay.LateNight:
			return TimeOfDay.EarlyMorning;
		default:
			return TimeOfDay.EarlyMorning;
		}
	}

	public static string GetTimeOfDayCaption(TimeOfDay time)
	{
		switch (time)
		{
		case TimeOfDay.EarlyMorning:
			return ScriptLocalization.Get("TimeOfDayEarlyMorning");
		case TimeOfDay.Morning:
			return ScriptLocalization.Get("TimeOfDayMorning");
		case TimeOfDay.Midday:
			return ScriptLocalization.Get("TimeOfDayMidday");
		case TimeOfDay.Evening:
			return ScriptLocalization.Get("TimeOfDayEvening");
		case TimeOfDay.LateEvening:
			return ScriptLocalization.Get("TimeOfDayLateEvening");
		case TimeOfDay.EarlyNight:
			return ScriptLocalization.Get("TimeOfDayEarlyNight");
		case TimeOfDay.MidNight:
			return ScriptLocalization.Get("TimeOfDayNight");
		case TimeOfDay.LateNight:
			return ScriptLocalization.Get("TimeOfDayLateNight");
		default:
			return string.Empty;
		}
	}

	public static void SetNightMode(float lightIntensity)
	{
		if (lightIntensity <= 0.31f && !Shader.IsKeywordEnabled("NIGHT_MODE_ON"))
		{
			Shader.EnableKeyword("NIGHT_MODE_ON");
			Shader.DisableKeyword("NIGHT_MODE_OFF");
		}
		if (lightIntensity > 0.31f && !Shader.IsKeywordEnabled("NIGHT_MODE_OFF"))
		{
			Shader.EnableKeyword("NIGHT_MODE_OFF");
			Shader.DisableKeyword("NIGHT_MODE_ON");
		}
	}

	internal void Start()
	{
		PhotonConnectionFactory.Instance.OnGotTime += TimeAndWeatherManager.OnGotTime;
		PhotonConnectionFactory.Instance.OnGotPondWeather += this.OnGotPondWeather;
		Shader.DisableKeyword("NIGHT_MODE_ON");
		Shader.DisableKeyword("NIGHT_MODE_OFF");
		TimeAndWeatherManager._timer = 15f;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotTime -= TimeAndWeatherManager.OnGotTime;
		PhotonConnectionFactory.Instance.OnGotPondWeather -= this.OnGotPondWeather;
	}

	internal void Update()
	{
		if (PhotonConnectionFactory.Instance.Peer == null)
		{
			return;
		}
		if (!PhotonConnectionFactory.Instance.IsAuthenticated || StaticUserData.CurrentPond == null)
		{
			return;
		}
		if (!CacheLibrary.AllChachesInited)
		{
			return;
		}
		TimeAndWeatherManager._timer += Time.deltaTime;
		if (TimeAndWeatherManager._timer >= 15f)
		{
			TimeAndWeatherManager._timer = 0f;
			PhotonConnectionFactory.Instance.GetTime();
			if (TimeAndWeatherManager._isForce || (TimeAndWeatherManager._currentTime.Hours == 5 && TimeAndWeatherManager._currentWeatherTime.Hours != 5))
			{
				this._isForceSky = TimeAndWeatherManager._isForce;
				TimeAndWeatherManager._isForce = false;
				PhotonConnectionFactory.Instance.GetPondWeather(StaticUserData.CurrentPond.PondId, 1);
				TimeAndWeatherManager._currentWeatherTime = TimeAndWeatherManager._currentTime;
			}
		}
	}

	public static void ResetTimeAndWeather()
	{
		TimeAndWeatherManager._timeInited = false;
		TimeAndWeatherManager.CurrentWeathers = null;
		PhotonConnectionFactory.Instance.GetTime();
		PhotonConnectionFactory.Instance.GetPondWeather(StaticUserData.CurrentPond.PondId, 1);
		PhotonConnectionFactory.Instance.RequestFishCage();
	}

	private static void OnGotTime(TimeSpan time)
	{
		if (TimeAndWeatherManager._isClientWeather)
		{
			return;
		}
		TimeAndWeatherManager._dayWasChanged = TimeAndWeatherManager._timeInited && PondHelper.DayByTime(new TimeSpan?(time)) != PondHelper.DayByTime(new TimeSpan?(TimeAndWeatherManager._currentTime));
		TimeAndWeatherManager._timeInited = true;
		if (TimeAndWeatherManager._dayWasChanged && StaticUserData.CurrentPond != null)
		{
			TimeAndWeatherManager._currentTime = time;
			PhotonConnectionFactory.Instance.GetPondWeather(StaticUserData.CurrentPond.PondId, 1);
			return;
		}
		if (time.Hours != TimeAndWeatherManager._currentTime.Hours)
		{
			TimeAndWeatherManager.RefreshWeather(time);
		}
		TimeAndWeatherManager._currentTime = time;
	}

	public static void SetClientWeather(int hours, string precipitation, string sky)
	{
		if (!TimeAndWeatherManager._isClientWeather)
		{
			TimeAndWeatherManager._savedPrecepitation = TimeAndWeatherManager.CurrentWeather.Precipitation;
			TimeAndWeatherManager._savedSky = TimeAndWeatherManager.CurrentWeather.Sky;
			TimeAndWeatherManager._isClientWeather = true;
			TimeAndWeatherManager._currentTime = new TimeSpan(hours, 0, 0);
			TimeAndWeatherManager.CurrentWeather.Precipitation = precipitation;
			TimeAndWeatherManager.CurrentWeather.Sky = sky;
			GameFactory.SkyControllerInstance.ForceChangeSky();
		}
	}

	public static void RestoreServerWeather()
	{
		if (TimeAndWeatherManager._isClientWeather)
		{
			TimeAndWeatherManager.CurrentWeather.Precipitation = TimeAndWeatherManager._savedPrecepitation;
			TimeAndWeatherManager.CurrentWeather.Sky = TimeAndWeatherManager._savedSky;
			TimeAndWeatherManager._isClientWeather = false;
			TimeAndWeatherManager.ForceUpdateTime();
		}
	}

	private static void RefreshWeather(TimeSpan time)
	{
		string st = time.ToTimeOfDay().ToString();
		TimeAndWeatherManager._currentWeather = ((TimeAndWeatherManager.CurrentWeathers != null) ? TimeAndWeatherManager.CurrentWeathers.FirstOrDefault((WeatherDesc x) => x.TimeOfDay == st) : null);
	}

	private void OnGotPondWeather(WeatherDesc[] weather)
	{
		TimeAndWeatherManager._currentWeatherTime = TimeAndWeatherManager._currentTime;
		int num = ((TimeAndWeatherManager.CurrentWeathers != null) ? TimeAndWeatherManager.CurrentWeathers.Length : 0);
		int num2 = ((weather != null) ? weather.Length : 0);
		TimeAndWeatherManager.CurrentWeathers = weather;
		TimeAndWeatherManager._currentPondWeather = StaticUserData.CurrentPond;
		if (num != num2 || TimeAndWeatherManager._dayWasChanged)
		{
			TimeAndWeatherManager._dayWasChanged = false;
			TimeAndWeatherManager.RefreshWeather(TimeAndWeatherManager._currentTime);
		}
		if (GameFactory.SkyControllerInstance != null && this._isForceSky)
		{
			this._isForceSky = false;
			GameFactory.SkyControllerInstance.ForceChangeSky();
		}
	}

	public static void ForceUpdateTime()
	{
		TimeAndWeatherManager._timer = 15f;
		TimeAndWeatherManager._isForce = true;
	}

	public static DateTime CurrentInGameTime()
	{
		if (TimeAndWeatherManager.CurrentTime == null)
		{
			return default(DateTime);
		}
		TimeSpan value = TimeAndWeatherManager.CurrentTime.Value;
		DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
		dateTime = dateTime.Add(value);
		return dateTime;
	}

	private static float _timer;

	private const float TimerMax = 15f;

	private static bool _isForce;

	private bool _isForceSky;

	private static Pond _currentPondWeather;

	private static TimeSpan _currentWeatherTime;

	public static WeatherDesc[] CurrentWeathers;

	public static string DevSky = string.Empty;

	private static bool _isClientWeather;

	[HideInInspector]
	private static TimeSpan _currentTime = new TimeSpan(0, 0, 0, 0);

	private static bool _timeInited;

	private static WeatherDesc _currentWeather;

	private static bool _dayWasChanged;

	private static string _savedPrecepitation;

	private static string _savedSky;
}
