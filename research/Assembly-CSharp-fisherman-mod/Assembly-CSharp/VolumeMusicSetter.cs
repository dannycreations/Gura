using System;
using UnityEngine;
using UnityEngine.UI;

public class VolumeMusicSetter : MonoBehaviour
{
	private void Start()
	{
		base.GetComponent<Slider>().value = SettingsManager.MusicVolume;
	}
}
