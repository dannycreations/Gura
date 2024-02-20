using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderLineController : MonoBehaviour
{
	public void Initialize(float length, float maxLength = 2f)
	{
		float num = length / maxLength;
		this.SolidLineImg.fillAmount = num;
		float num2 = this.SliderZone.rect.height * num - this.HookBg.rect.height;
		this.HookBg.anchoredPosition = -Vector2.up * num2;
		this.LineLength.text = string.Format("{0}\n{1}", MeasuringSystemManager.ToStringCentimeters(MeasuringSystemManager.LineLeashLength(length)), MeasuringSystemManager.LineLeashLengthSufix());
	}

	[SerializeField]
	private RectTransform HookBg;

	[SerializeField]
	private RectTransform SliderZone;

	[SerializeField]
	private Image SolidLineImg;

	[SerializeField]
	private TextMeshProUGUI LineLength;
}
