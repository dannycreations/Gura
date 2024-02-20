using System;
using System.Collections.Generic;
using UnityEngine;

public class DayNightSwither : MonoBehaviour
{
	private void Awake()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			child.gameObject.SetActive(this._isNight);
			this._children.Add(child);
		}
	}

	private void Update()
	{
		this._time += Time.deltaTime;
		if (this._time > this._updateDelaySeconds)
		{
			this._time = 0f;
			bool flag = false;
			if (TimeAndWeatherManager.CurrentTime != null)
			{
				DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
				dateTime = dateTime.Add(TimeAndWeatherManager.CurrentTime.Value);
				flag = dateTime.Hour >= this._nightHourStart || dateTime.Hour <= this._nightHourEnd;
			}
			if (this._isNight != flag)
			{
				this._isNight = flag;
				this._children.ForEach(delegate(Transform p)
				{
					p.gameObject.SetActive(this._isNight);
				});
			}
		}
	}

	[SerializeField]
	private int _nightHourStart = 21;

	[SerializeField]
	private int _nightHourEnd = 7;

	[SerializeField]
	private float _updateDelaySeconds = 15f;

	private List<Transform> _children = new List<Transform>();

	private bool _isNight;

	private float _time;
}
