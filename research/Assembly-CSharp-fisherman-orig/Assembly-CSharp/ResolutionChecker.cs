using System;
using UnityEngine;

public class ResolutionChecker : MonoBehaviour
{
	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		if (Application.isEditor)
		{
			this.ForceFullscreen = false;
		}
	}

	private void Update()
	{
		if (this.ForceFullscreen && (Screen.width != Screen.currentResolution.width || !Screen.fullScreen))
		{
			Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
		}
	}

	public bool ForceFullscreen = true;
}
