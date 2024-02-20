using System;
using UnityEngine;

public class FogDisabler : MonoBehaviour
{
	public void OnPreRender()
	{
		this.previousFogDensity = RenderSettings.fogDensity;
		RenderSettings.fogDensity = this.desiredDensity;
	}

	public void OnPostRender()
	{
		RenderSettings.fogDensity = this.previousFogDensity;
	}

	private float desiredDensity;

	private float previousFogDensity;
}
