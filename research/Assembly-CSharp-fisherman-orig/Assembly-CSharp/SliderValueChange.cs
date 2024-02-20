using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderValueChange : MonoBehaviour
{
	private Slider _slider
	{
		get
		{
			if (this._cachedSlider == null)
			{
				this._cachedSlider = base.GetComponent<Slider>();
			}
			return this._cachedSlider;
		}
	}

	public float value
	{
		get
		{
			return this._slider.value;
		}
		set
		{
			this._slider.value = value;
			this._percentageLabel.text = string.Format("{0}%", (int)(this._slider.value * 100f));
		}
	}

	public Slider.SliderEvent onValueChanged
	{
		get
		{
			return this._slider.onValueChanged;
		}
		set
		{
			this._slider.onValueChanged = value;
		}
	}

	private void OnEnable()
	{
		base.StartCoroutine(this.UpdateCoroutine());
	}

	private void OnDisable()
	{
		base.StopAllCoroutines();
	}

	public void Inc()
	{
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			return;
		}
		this.StopInc();
		this.StopDec();
		this._isIncrementing = true;
		base.StartCoroutine(this.UpdateCoroutine());
	}

	public void IncOnce()
	{
		this.StopInc();
		this.StopDec();
		this.ChangeValue(0.01f);
	}

	public void DecOnce()
	{
		this.StopInc();
		this.StopDec();
		this.ChangeValue(-0.01f);
	}

	public void UpdateLabel()
	{
		this._percentageLabel.text = string.Format("{0}%", (int)(this._slider.value * 100f));
	}

	public void StopInc()
	{
		this._isIncrementing = false;
		base.StopAllCoroutines();
	}

	public void Dec()
	{
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			return;
		}
		this.StopInc();
		this.StopDec();
		this._isDecrementing = true;
		base.StartCoroutine(this.UpdateCoroutine());
	}

	public void StopDec()
	{
		this._isDecrementing = false;
		base.StopAllCoroutines();
	}

	private void ChangeValue(float _addValue)
	{
		this._slider.value += _addValue;
		this._percentageLabel.text = string.Format("{0}%", (int)(this._slider.value * 100f));
	}

	private IEnumerator UpdateCoroutine()
	{
		for (;;)
		{
			if (this._isDecrementing)
			{
				this.ChangeValue(-0.01f);
			}
			if (this._isIncrementing)
			{
				this.ChangeValue(0.01f);
			}
			yield return new WaitForSeconds(0.2f);
		}
		yield break;
	}

	[SerializeField]
	private Text _percentageLabel;

	private Slider _cachedSlider;

	private const float _step = 0.01f;

	private const float _updateTime = 0.2f;

	private bool _isIncrementing;

	private bool _isDecrementing;
}
