using System;
using UnityEngine;
using UnityEngine.UI;

public class SetRPMToSlider : MonoBehaviour
{
	private void Start()
	{
		this.res = this.controllerGameobject.GetComponent<RealisticEngineSound>();
		this.rpmSlider.maxValue = this.res.maxRPMLimit;
		this.carSimulator = this.gasPedalButton.GetComponent<CarSimulator>();
		if (!this.simulated)
		{
			this._rpm = (int)this.rpmSlider.value;
		}
		else
		{
			this._rpm = (int)this.carSimulator.rpm;
		}
		this._pitch = this.pitchSlider.value;
	}

	private void Update()
	{
		if (!this.simulated)
		{
			if (this._rpm != (int)this.rpmSlider.value)
			{
				this.rpmText.text = "Engine RPM: " + (int)this.rpmSlider.value;
				this.res.engineCurrentRPM = (float)((int)this.rpmSlider.value);
				this._rpm = (int)this.rpmSlider.value;
			}
		}
		else if (this._rpm != (int)this.carSimulator.rpm)
		{
			this.rpmText.text = "Engine RPM: " + (int)this.carSimulator.rpm;
			this.res.engineCurrentRPM = (float)((int)this.carSimulator.rpm);
			this.rpmSlider.value = (float)((int)this.carSimulator.rpm);
			this.res.gasPedalPressing = this.carSimulator.gasPedalPressing;
			this.gasPedalCheckbox.isOn = this.carSimulator.gasPedalPressing;
			this._rpm = (int)this.carSimulator.rpm;
		}
		if (this._pitch != this.pitchSlider.value)
		{
			this.res.pitchMultiplier = this.pitchSlider.value;
			this.pitchText.text = string.Empty + this.pitchSlider.value;
			this._pitch = this.pitchSlider.value;
		}
	}

	public void GasPedalCheckbox()
	{
		if (this.res != null)
		{
			if (this.res.gasPedalPressing)
			{
				this.res.gasPedalPressing = false;
			}
			else
			{
				this.res.gasPedalPressing = true;
			}
		}
	}

	public void ReverseGearCheckbox()
	{
		if (this.res != null)
		{
			if (this.res.enableReverseGear)
			{
				this.res.enableReverseGear = false;
				this.ReversingCheckbox.gameObject.SetActive(false);
				this.ReversingCheckbox.isOn = false;
			}
			else
			{
				this.res.enableReverseGear = true;
				this.ReversingCheckbox.gameObject.SetActive(true);
			}
		}
	}

	public void Reversing()
	{
		if (this.res != null)
		{
			if (this.res.isReversing)
			{
				this.res.isReversing = false;
			}
			else
			{
				this.res.isReversing = true;
			}
		}
	}

	public void RPMLimit()
	{
		if (this.res != null)
		{
			if (this.res.useRPMLimit)
			{
				this.res.useRPMLimit = false;
			}
			else
			{
				this.res.useRPMLimit = true;
			}
		}
	}

	private RealisticEngineSound res;

	public GameObject controllerGameobject;

	public Slider rpmSlider;

	public Slider pitchSlider;

	public Text pitchText;

	public Text rpmText;

	public Toggle ReversingCheckbox;

	public Toggle gasPedalCheckbox;

	public bool simulated = true;

	public GameObject gasPedalButton;

	private CarSimulator carSimulator;

	private int _rpm;

	private float _pitch;
}
