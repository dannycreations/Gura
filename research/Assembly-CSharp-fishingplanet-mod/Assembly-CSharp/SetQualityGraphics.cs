using System;
using UnityEngine;
using UnityEngine.UI;

public class SetQualityGraphics : MonoBehaviour
{
	public void OnChanged(float value)
	{
	}

	public Text QualityLabel;

	private enum GraphicsQuality : byte
	{
		Fastest,
		Fast,
		Simple,
		Good,
		Beautiful,
		Fantastic
	}
}
