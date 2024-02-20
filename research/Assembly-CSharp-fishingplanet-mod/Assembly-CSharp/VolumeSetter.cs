using System;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSetter : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Slider>().value = SettingsManager.SoundVolume;
	}
}
