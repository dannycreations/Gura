using System;
using UnityEngine;
using UnityEngine.UI;

public class TwoButtonToggle : ToggleGroup
{
	private void Awake()
	{
		this._on.group = this;
		this._off.group = this;
	}

	private void Update()
	{
		if (base.allowSwitchOff)
		{
			base.allowSwitchOff = false;
		}
	}

	public bool isOn
	{
		get
		{
			return this._on.isOn;
		}
		set
		{
			if (value)
			{
				this._off.isOn = false;
				this._on.isOn = true;
			}
			else
			{
				this._on.isOn = false;
				this._off.isOn = true;
			}
		}
	}

	[SerializeField]
	private Toggle _on;

	[SerializeField]
	private Toggle _off;

	public Toggle.ToggleEvent onValueChanged = new Toggle.ToggleEvent();
}
