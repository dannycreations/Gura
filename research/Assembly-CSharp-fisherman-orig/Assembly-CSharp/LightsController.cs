using System;
using UnityEngine;

public class LightsController : MonoBehaviour
{
	private void Start()
	{
		for (int i = 0; i < this.oddLights.Length; i++)
		{
			if (this.oddLights[i] != null)
			{
				this.oddLights[i].intensity = 0f;
			}
		}
		for (int i = 0; i < this.evenLights.Length; i++)
		{
			if (this.evenLights[i])
			{
				this.evenLights[i].intensity = 0f;
			}
		}
	}

	private void Update()
	{
		this.frame++;
		if (this.frame < 3)
		{
			return;
		}
		this.frame = 0;
		for (int i = 0; i < this.oddLights.Length; i++)
		{
			if (this.oddLights[i] != null)
			{
				this.oddLights[i].intensity = Mathf.Abs(Mathf.Sin(Time.time * 2f)) * 8f;
			}
		}
		for (int i = 0; i < this.evenLights.Length; i++)
		{
			if (this.evenLights[i] != null)
			{
				this.evenLights[i].intensity = Mathf.Abs(Mathf.Cos(Time.time * 2f)) * 8f;
			}
		}
	}

	public Light[] oddLights;

	public Light[] evenLights;

	private float phase;

	private int frame;
}
