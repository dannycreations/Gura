using System;
using UnityEngine;
using UnityEngine.UI;

public class SetBobberSize : ToggleGroup
{
	private void Awake()
	{
		this._small.group = this;
		this._medium.group = this;
		this._big.group = this;
	}

	private void Update()
	{
		if (base.allowSwitchOff)
		{
			base.allowSwitchOff = false;
		}
	}

	public void SetValue()
	{
		if (this._small.isOn)
		{
			this._slider.value = 0.5f;
		}
		if (this._medium.isOn)
		{
			this._slider.value = 0.75f;
		}
		if (this._big.isOn)
		{
			this._slider.value = 1f;
		}
	}

	public void ValueChanged()
	{
		if (this._slider.value < 0.6f)
		{
			this._medium.isOn = false;
			this._big.isOn = false;
			this._small.isOn = true;
		}
		else if (this._slider.value < 0.8f)
		{
			this._small.isOn = false;
			this._big.isOn = false;
			this._medium.isOn = true;
		}
		else
		{
			this._small.isOn = false;
			this._medium.isOn = false;
			this._big.isOn = true;
		}
	}

	[SerializeField]
	private Toggle _small;

	[SerializeField]
	private Toggle _medium;

	[SerializeField]
	private Toggle _big;

	[SerializeField]
	private Slider _slider;
}
