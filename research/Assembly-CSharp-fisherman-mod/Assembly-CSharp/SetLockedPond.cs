using System;
using UnityEngine;
using UnityEngine.UI;

public class SetLockedPond : MonoBehaviour
{
	public void SetLocked()
	{
		base.GetComponent<ChangeColor>().UseColor[1] = this.LockedColor;
		this.ImageToSet.color = this.LockedColor;
		this.LockedIcon.SetActive(true);
		this.WeatherIcon.SetActive(false);
	}

	public void SetUnLocked()
	{
		this.ImageToSet.color = base.GetComponent<ChangeColor>().UseColor[2];
		this.LockedIcon.SetActive(false);
		this.WeatherIcon.SetActive(true);
	}

	public Image ImageToSet;

	public Color LockedColor;

	public GameObject LockedIcon;

	public GameObject WeatherIcon;
}
