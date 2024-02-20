using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class WeathersCache : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<WeatherDesc[]> OnPondWeather;

	private void Awake()
	{
		PhotonConnectionFactory.Instance.OnGotPondWeather += this.OnGotPondWeather;
		PhotonConnectionFactory.Instance.OnGotPondWeatherForecast += this.OnGotPondWeather;
		this._lastActive = DateTime.UtcNow;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnGotPondWeather -= this.OnGotPondWeather;
		PhotonConnectionFactory.Instance.OnGotPondWeatherForecast -= this.OnGotPondWeather;
	}

	private void OnGotPondWeather(WeatherDesc[] weathers)
	{
		Vector4 vector = this._requestQueue.Dequeue();
		int num = (int)vector.x;
		int num2 = (int)vector.y;
		int num3 = (int)vector.z;
		bool flag = (int)vector.w == 1;
		WeatherDesc[] array;
		if (flag)
		{
			List<WeatherDesc> list = null;
			array = new WeatherDesc[num3 * 8];
			if (!this._weathers.TryGetValue(num, out list))
			{
				list = new List<WeatherDesc>();
				this._weathers.Add(num, weathers.ToList<WeatherDesc>());
			}
			int count = list.Count;
			for (int i = count; i < (num2 - 1) * 8 + num3 * 8; i++)
			{
				list.Add(null);
			}
			for (int j = 0; j < num3 * 8; j++)
			{
				list[j + (num2 - 1) * 8] = weathers[j];
				array[j] = weathers[j];
			}
		}
		else
		{
			array = weathers;
		}
		if (this.OnPondWeather != null)
		{
			this.OnPondWeather(array);
		}
	}

	private bool TryGetWeather(out WeatherDesc[] outWeathers, int pondId, int days = 1)
	{
		outWeathers = new WeatherDesc[days * 8];
		List<WeatherDesc> list = null;
		if (!this._weathers.TryGetValue(pondId, out list) || list.Count < days * 8 + (this.GetCurrentDay() - 1) * 8)
		{
			return false;
		}
		int num = 1;
		if (StaticUserData.CurrentPond != null && TimeAndWeatherManager.CurrentTime != null)
		{
			num = this.GetCurrentDay();
		}
		for (int i = 0; i < days * 8; i++)
		{
			if (list[(num - 1) * 8 + i] == null)
			{
				return false;
			}
			outWeathers[i] = list[(num - 1) * 8 + i];
		}
		return true;
	}

	private void ValidateCache()
	{
		if (StaticUserData.CurrentPond == null && this._lastActive.Date.Day != DateTime.UtcNow.Date.Day)
		{
			this._lastActive = DateTime.UtcNow;
			foreach (KeyValuePair<int, List<WeatherDesc>> keyValuePair in this._weathers)
			{
				if (keyValuePair.Value != null && keyValuePair.Value.Count > 0)
				{
					keyValuePair.Value.RemoveAt(0);
				}
			}
		}
	}

	private int GetCurrentDay()
	{
		if (TimeAndWeatherManager.CurrentTime == null || StaticUserData.CurrentPond == null)
		{
			return 1;
		}
		TimeSpan value = TimeAndWeatherManager.CurrentTime.Value;
		return value.Days - ((value.Hours >= 5) ? 0 : 1) + 1;
	}

	public void GetPondWeather(int pondId, int days = 1)
	{
		this.ValidateCache();
		WeatherDesc[] array;
		if (PhotonConnectionFactory.Instance.CurrentTournamentId == null && !this._resetChache && this.TryGetWeather(out array, pondId, days))
		{
			if (this.OnPondWeather != null)
			{
				this.OnPondWeather(array);
			}
		}
		else
		{
			if (this._resetChache)
			{
				this._resetChache = false;
			}
			this._requestQueue.Enqueue(new Vector4((float)pondId, (float)((StaticUserData.CurrentPond == null) ? 1 : this.GetCurrentDay()), (float)days, (float)((PhotonConnectionFactory.Instance.CurrentTournamentId != null) ? 0 : 1)));
			PhotonConnectionFactory.Instance.GetPondWeather(pondId, days);
		}
	}

	public void ResetCache()
	{
		this._resetChache = true;
	}

	private Dictionary<int, List<WeatherDesc>> _weathers = new Dictionary<int, List<WeatherDesc>>();

	private Queue<Vector4> _requestQueue = new Queue<Vector4>();

	private DateTime _lastActive;

	private bool _resetChache;
}
