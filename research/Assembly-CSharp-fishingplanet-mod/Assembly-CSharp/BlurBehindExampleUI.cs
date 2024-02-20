using System;
using UnityEngine;
using UnityEngine.UI;

public class BlurBehindExampleUI : MonoBehaviour
{
	public void OnValueChanged()
	{
		int num = Mathf.RoundToInt(this.layersSlider.value);
		this.effect0.enabled = num > 0;
		this.effect1.enabled = num > 1;
		this.effect0.radius = Mathf.Pow(this.radiusSlider.value, 2f);
		this.effect1.radius = this.effect0.radius;
		if (this.downsampleSlider.value > 12f)
		{
			this.effect0.downsample = float.PositiveInfinity;
		}
		else
		{
			this.effect0.downsample = Mathf.Pow(2f, this.downsampleSlider.value);
		}
		this.effect1.downsample = this.effect0.downsample;
		this.effect0.iterations = Mathf.RoundToInt(this.iterationsSlider.value);
		this.effect1.iterations = this.effect0.iterations;
	}

	private void Redraw()
	{
		if (this.effect1.enabled)
		{
			this.layersSlider.value = 2f;
			this.layersValue.text = "2";
		}
		else if (this.effect0.enabled)
		{
			this.layersSlider.value = 1f;
			this.layersValue.text = "1";
		}
		else
		{
			this.layersSlider.value = 0f;
			this.layersValue.text = "0";
		}
		this.radiusSlider.value = Mathf.Sqrt(this.effect0.radius);
		this.radiusValue.text = Mathf.RoundToInt(this.effect0.radius).ToString();
		this.standardButton.interactable = this.effect0.settings != BlurBehind.Settings.Standard;
		this.smoothButton.interactable = this.effect0.settings != BlurBehind.Settings.Smooth;
		this.manualButton.interactable = this.effect0.settings != BlurBehind.Settings.Manual;
		if (this.effect0.settings == BlurBehind.Settings.Manual)
		{
			this.group.interactable = true;
			this.group.alpha = 1f;
		}
		else
		{
			this.group.interactable = false;
			this.group.alpha = 0.35f;
		}
		if (this.effect0.downsample > 4096f)
		{
			this.downsampleSlider.value = 13f;
			this.downsampleValue.text = "Inf";
		}
		else
		{
			this.downsampleSlider.value = Mathf.Log(this.effect0.downsample, 2f);
			this.downsampleValue.text = Mathf.RoundToInt(this.effect0.downsample).ToString();
		}
		this.iterationsSlider.value = (float)this.effect0.iterations;
		this.iterationsValue.text = this.effect0.iterations.ToString();
	}

	public void SetSettings(int state)
	{
		this.effect0.settings = (BlurBehind.Settings)state;
		this.effect1.settings = this.effect0.settings;
	}

	private void Start()
	{
		this.Redraw();
	}

	private void Update()
	{
		this.Redraw();
	}

	public BlurBehind effect0;

	public BlurBehind effect1;

	public Slider layersSlider;

	public Text layersValue;

	public Slider radiusSlider;

	public Text radiusValue;

	public Button standardButton;

	public Button smoothButton;

	public Button manualButton;

	public CanvasGroup group;

	public Slider downsampleSlider;

	public Text downsampleValue;

	public Slider iterationsSlider;

	public Text iterationsValue;
}
