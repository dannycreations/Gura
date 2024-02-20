using System;
using UnityEngine;

public class UiEffectsUtil
{
	public static void TextPulsation(GameObject enableWarnOn, Color color, float pulseTime = 1f, float minAlpha = 0f, bool enable = true)
	{
		TextColorPulsation textColorPulsation = enableWarnOn.GetComponent<TextColorPulsation>() ?? enableWarnOn.AddComponent<TextColorPulsation>();
		textColorPulsation.enabled = enable;
		textColorPulsation.StartColor = color;
		textColorPulsation.MinAlpha = minAlpha;
		textColorPulsation.PulseTime = pulseTime;
	}

	public static void ImagePulsation(GameObject enableWarnOn, Color color, float pulseTime = 1f, float minAlpha = 0f, bool enable = true)
	{
		ColorPulsation colorPulsation = enableWarnOn.GetComponent<ColorPulsation>() ?? enableWarnOn.AddComponent<ColorPulsation>();
		colorPulsation.enabled = enable;
		colorPulsation.StartColor = color;
		colorPulsation.MinAlpha = minAlpha;
		colorPulsation.PulseTime = pulseTime;
	}
}
