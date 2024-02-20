using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CutChumComponent : CutControllerBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<float, ChumIngredient> OnAccepted = delegate(float f, ChumIngredient ingredient)
	{
	};

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(true);
		this._normalColor = this._value.textComponent.color;
	}

	protected override void Update()
	{
		if (Math.Abs(this.Value - this.SliderControl.value) > 0.01f)
		{
			this.Value = this.SliderControl.value;
			this._value.text = this.SliderControl.value.ToString(CultureInfo.InvariantCulture);
			this.UpdatePrc();
			this.BaseLength.text = (this._totalWeight - this.Value).ToString(CultureInfo.InvariantCulture);
			this.TargetLength.text = Mathf.Floor(this._chumWeight + this.Value).ToString(CultureInfo.InvariantCulture);
			this.AcceptBtn.interactable = this._weightMax == null || this.SliderControl.value <= this._weightMax.Value;
			this._value.textComponent.color = ((!this.AcceptBtn.interactable) ? Color.red : this._normalColor);
			if (!this.AcceptBtn.interactable)
			{
				UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
			}
		}
		base.Update();
	}

	protected override void AcceptActionCalled()
	{
		this.OnAccepted(this.SliderControl.value, this._ingredient);
	}

	public void Init(ChumIngredient ii, float chumWeight, float weightInChum, float? maxWeight)
	{
		if (maxWeight != null)
		{
			this._weightMax = new float?(Mathf.Floor(MeasuringSystemManager.Kilograms2Grams(maxWeight.Value)));
			this._maxWeight.text = string.Format(ScriptLocalization.Get("UpToCaption"), string.Format("{0}{1}", this._weightMax, MeasuringSystemManager.GramsOzWeightSufix()));
		}
		else
		{
			this._maxWeight.text = string.Empty;
		}
		this._ingredient = ii;
		this._chumWeight = MeasuringSystemManager.Kilograms2Grams(chumWeight - weightInChum);
		this._totalWeight = Mathf.Floor(MeasuringSystemManager.Kilograms2Grams(this._ingredient.Amount));
		Slider sliderControl = this.SliderControl;
		float? weightMax = this._weightMax;
		sliderControl.maxValue = ((weightMax == null) ? this._totalWeight : weightMax.Value);
		this.SliderControl.minValue = 0f;
		this.SliderControl.value = MeasuringSystemManager.Kilograms2Grams(weightInChum);
		this.UpdatePrc();
		this.BaseLength.text = this._totalWeight.ToString(CultureInfo.InvariantCulture);
		this._value.text = this.SliderControl.value.ToString(CultureInfo.InvariantCulture);
		this.BaseImageLdbl.Load(this._ingredient.ThumbnailBID, this.BaseImage, "Textures/Inventory/{0}");
		this.TargetLength.text = this._chumWeight.ToString(CultureInfo.InvariantCulture);
		this._value.onValueChanged.AddListener(new UnityAction<string>(this.value_changed));
		base.Open();
	}

	public void OpenKeyboard()
	{
		base.StartCoroutine(this.StartEdit());
	}

	private void value_changed(string v)
	{
		bool flag = false;
		this._value.onValueChanged.RemoveListener(new UnityAction<string>(this.value_changed));
		flag = true;
		try
		{
			float num = 0f;
			if (!string.IsNullOrEmpty(v))
			{
				num = float.Parse(v);
			}
			else
			{
				this._value.text = num.ToString(CultureInfo.InvariantCulture);
			}
			if (num > this._totalWeight)
			{
				num = this._totalWeight;
				this._value.text = this._totalWeight.ToString(CultureInfo.InvariantCulture);
			}
			if (Math.Abs(num - this.SliderControl.value) > 0.001f)
			{
				this.SliderControl.value = num;
			}
			this.UpdatePrc();
		}
		finally
		{
			if (flag)
			{
				this._value.onValueChanged.AddListener(new UnityAction<string>(this.value_changed));
			}
		}
	}

	private void ActivateInputField()
	{
		UINavigation.SetSelectedGameObject(this._value.gameObject);
	}

	private IEnumerator StartEdit()
	{
		yield return new WaitForEndOfFrame();
		this._value.GetComponent<ScreenKeyboard>().OnInputFieldSelect(string.Empty, false, ScreenKeyboard.VirtualKeyboardScope.Default);
		this.ActivateInputField();
		this._value.Select();
		this._value.ActivateInputField();
		this._value.MoveTextEnd(true);
		yield break;
	}

	private void UpdatePrc()
	{
		float num = this._chumWeight + this.SliderControl.value;
		float num2 = ((num <= 0f) ? 0f : (100f * this.SliderControl.value / num));
		this._valuePrc.text = string.Format("{0}%", num2.ToString("F2").TrimEnd(new char[] { '0' }).TrimEnd(new char[] { '.' })
			.TrimEnd(new char[] { ',' }));
	}

	[SerializeField]
	private Text _maxWeight;

	[SerializeField]
	private InputField _value;

	[SerializeField]
	private TextMeshProUGUI _valuePrc;

	private ChumIngredient _ingredient;

	private float _chumWeight;

	private float _totalWeight;

	private float? _weightMax;

	private Color _normalColor;

	protected float Value;
}
