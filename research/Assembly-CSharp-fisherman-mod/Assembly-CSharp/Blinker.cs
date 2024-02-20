using System;
using UnityEngine;

public class Blinker : MonoBehaviour
{
	private void Start()
	{
		this._nextSwitchAt = Time.time + this._delay;
	}

	private void Update()
	{
		if (this._nextSwitchAt < Time.time)
		{
			this._nextSwitchAt = Time.time + this._delay;
			this._obj.SetActive(!this._obj.activeSelf);
		}
	}

	[SerializeField]
	private GameObject _obj;

	[SerializeField]
	private float _delay = 1f;

	private float _nextSwitchAt = -1f;
}
