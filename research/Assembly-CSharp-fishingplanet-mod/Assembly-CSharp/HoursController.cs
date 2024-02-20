using System;
using System.Collections.Generic;
using UnityEngine;

public class HoursController : MonoBehaviour
{
	private void Awake()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			HoursChild component = base.transform.GetChild(i).GetComponent<HoursChild>();
			if (component != null)
			{
				this._childs.Add(component);
			}
		}
		if (this._childs.Count > 0)
		{
			PhotonConnectionFactory.Instance.OnGotTime += this.OnUpdateTime;
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	private void OnUpdateTime(TimeSpan time)
	{
		DateTime dateTime = TimeAndWeatherManager.CurrentInGameTime();
		int dayHour = this.GetDayHour(dateTime.Day, dateTime.Hour);
		if (this._hours.Contains(dateTime.Hour) && !this._processedHours.Contains(dayHour))
		{
			this._processedHours.Add(dayHour);
			for (int i = 0; i < this._childs.Count; i++)
			{
				this._childs[i].StartEvent();
			}
		}
	}

	private int GetDayHour(int day, int hour)
	{
		return day * 100 + hour;
	}

	private void OnDestroy()
	{
		if (this._childs.Count > 0)
		{
			this._childs.Clear();
			PhotonConnectionFactory.Instance.OnGotTime -= this.OnUpdateTime;
		}
	}

	[SerializeField]
	private List<int> _hours;

	private List<HoursChild> _childs = new List<HoursChild>();

	private HashSet<int> _processedHours = new HashSet<int>();
}
