using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
	public void Activate()
	{
		base.gameObject.SetActive(true);
		base.GetComponent<CanvasGroup>().alpha = 1f;
		this.Reset();
	}

	public void Deactivate()
	{
		base.GetComponent<CanvasGroup>().alpha = 0f;
		base.gameObject.SetActive(false);
	}

	internal void Reset()
	{
		this.Slider.minValue = 0f;
		this.Slider.value = this.Slider.minValue;
	}

	internal void UpdateLoading(int maxValue, int currentValue)
	{
		this.Slider.maxValue = (float)maxValue;
		this.Slider.value = (float)currentValue;
	}

	public Slider Slider;

	public Text Text;
}
