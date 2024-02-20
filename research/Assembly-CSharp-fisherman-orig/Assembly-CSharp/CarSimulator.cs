using System;
using UnityEngine;
using UnityEngine.UI;

public class CarSimulator : MonoBehaviour
{
	private void Start()
	{
		this.rpm = this.idle;
	}

	private void Update()
	{
		if (this.gasPedalPressing)
		{
			if (this.rpm <= this.maxRPM)
			{
				this.rpm = Mathf.Lerp(this.rpm, this.rpm + this.accelerationSpeed * this.accelSlider.value, Time.deltaTime);
			}
		}
		else if (this.rpm > this.idle)
		{
			this.rpm = Mathf.Lerp(this.rpm, this.rpm - this.decelerationSpeed * this.accelSlider.value, Time.deltaTime);
		}
	}

	public void onPointerDownRaceButton()
	{
		this.gasPedalPressing = true;
	}

	public void onPointerUpRaceButton()
	{
		this.gasPedalPressing = false;
	}

	public bool gasPedalPressing;

	public float maxRPM = 7000f;

	public float idle = 900f;

	public float rpm;

	public float accelerationSpeed = 1000f;

	public float decelerationSpeed = 1200f;

	public Slider accelSlider;
}
