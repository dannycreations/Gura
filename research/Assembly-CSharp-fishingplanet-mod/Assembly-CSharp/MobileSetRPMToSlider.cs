using System;
using UnityEngine;
using UnityEngine.UI;

public class MobileSetRPMToSlider : MonoBehaviour
{
	private void Start()
	{
		this.res_mob = this.controllerGameobject.GetComponent<RealisticEngineSound_mobile>();
		this.rpmSlider.maxValue = this.res_mob.maxRPMLimit;
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

	public void SetRPM()
	{
		if (this.res_mob != null)
		{
			this.res_mob.engineCurrentRPM = this.rpmSlider.value;
		}
	}

	private void Update()
	{
		if (!this.simulated)
		{
			if (this._rpm != (int)this.rpmSlider.value)
			{
				this.rpmText.text = "Engine RPM: " + (int)this.rpmSlider.value;
				this.res_mob.engineCurrentRPM = (float)((int)this.rpmSlider.value);
				this._rpm = (int)this.rpmSlider.value;
			}
		}
		else if (this._rpm != (int)this.carSimulator.rpm)
		{
			this.rpmText.text = "Engine RPM: " + (int)this.carSimulator.rpm;
			this.res_mob.engineCurrentRPM = (float)((int)this.carSimulator.rpm);
			this.rpmSlider.value = (float)((int)this.carSimulator.rpm);
			this._rpm = (int)this.carSimulator.rpm;
		}
		if (this._pitch != this.pitchSlider.value)
		{
			this.res_mob.pitchMultiplier = this.pitchSlider.value;
			this.pitchText.text = string.Empty + this.pitchSlider.value;
			this._pitch = this.pitchSlider.value;
		}
	}

	public void ReverseGearCheckbox()
	{
		if (this.res_mob != null)
		{
			if (this.res_mob.enableReverseGear)
			{
				this.res_mob.enableReverseGear = false;
				this.ReversingCheckbox.gameObject.SetActive(false);
				this.ReversingCheckbox.isOn = false;
			}
			else
			{
				this.res_mob.enableReverseGear = true;
				this.ReversingCheckbox.gameObject.SetActive(true);
			}
		}
	}

	public void Reversing()
	{
		if (this.res_mob != null)
		{
			if (this.res_mob.isReversing)
			{
				this.res_mob.isReversing = false;
			}
			else
			{
				this.res_mob.isReversing = true;
			}
		}
	}

	public void RPMLimit()
	{
		if (this.res_mob != null)
		{
			if (this.res_mob.useRPMLimit)
			{
				this.res_mob.useRPMLimit = false;
			}
			else
			{
				this.res_mob.useRPMLimit = true;
			}
		}
	}

	private RealisticEngineSound_mobile res_mob;

	public GameObject controllerGameobject;

	public Slider rpmSlider;

	public Slider pitchSlider;

	public Text pitchText;

	public Text rpmText;

	public Toggle ReversingCheckbox;

	public bool simulated = true;

	public GameObject gasPedalButton;

	private CarSimulator carSimulator;

	private int _rpm;

	private float _pitch;
}
