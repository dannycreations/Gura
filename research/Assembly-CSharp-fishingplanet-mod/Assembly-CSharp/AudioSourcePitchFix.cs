using System;
using UnityEngine;

public class AudioSourcePitchFix : MonoBehaviour
{
	private void Start()
	{
		if (base.gameObject != null)
		{
			AudioSource component = base.gameObject.GetComponent<AudioSource>();
			if (component != null)
			{
				this.LinkedVolume = component.volume;
			}
		}
		if (Time.timeScale != 0f && base.GetComponent<AudioSource>() != null)
		{
			base.GetComponent<AudioSource>().pitch = Time.timeScale;
		}
	}

	private void Update()
	{
	}

	public float LinkedVolume = 1f;

	public bool isAmbient = true;
}
