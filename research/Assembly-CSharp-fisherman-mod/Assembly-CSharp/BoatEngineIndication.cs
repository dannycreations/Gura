using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BoatEngineIndication : MonoBehaviour
{
	private void Awake()
	{
		this._startPos = (base.transform as RectTransform).anchoredPosition;
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	public void Shake(bool success)
	{
		RectTransform rectTransform = base.transform as RectTransform;
		ShortcutExtensions.DOKill(rectTransform, false);
		rectTransform.anchoredPosition = this._startPos;
		ShortcutExtensions.DOShakePosition(rectTransform, (!success) ? this.failShakeDuration : this.successShakeDuration, (!success) ? this.failShakeForce : this.successShakeForce, (!success) ? this.failShakeFrequency : this.successShakeFrequency, 90f, false, true);
		if (success)
		{
			this.IndicateSuccess();
		}
	}

	public void IndicateFail()
	{
		ShortcutExtensions.DOKill(this.Sides, false);
		TweenSettingsExtensions.SetLoops<Tweener>(TweenSettingsExtensions.OnStepComplete<Tweener>(ShortcutExtensions.DOColor(this.Sides, Color.red, 0.2f), delegate
		{
			ShortcutExtensions.DOColor(this.Sides, Color.clear, 0.1f);
		}), 3, 2);
	}

	public void IndicateSuccess()
	{
		ShortcutExtensions.DOKill(this.Sides, false);
		TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetLoops<Tweener>(TweenSettingsExtensions.OnStepComplete<Tweener>(ShortcutExtensions.DOColor(this.Sides, this.IndicatorOnColor, 0.4f), delegate
		{
			ShortcutExtensions.DOColor(this.Sides, Color.clear, 0.1f);
		}), 2, 0), delegate
		{
			ShortcutExtensions.DOColor(this.Sides, this.SidesOnColor, 0.2f);
		});
	}

	public void SetTurnedOn(bool value, float time = 0.2f)
	{
		Color color = ((!value) ? this.SidesOffColor : this.SidesOnColor);
		Color color2 = ((!value) ? this.IndicatorOffColor : this.IndicatorOnColor);
		Vector2 vector = ((!value) ? this.IndicatorOff.anchoredPosition : this.IndicatorOn.anchoredPosition);
		ShortcutExtensions.DOKill(this.Sides, false);
		this.Sides.color = color;
		ShortcutExtensions.DOKill(this.Indicator, false);
		ShortcutExtensions.DOKill(this.IndicatorFiller, false);
		ShortcutExtensions.DOColor(this.IndicatorFiller, color2, time);
		ShortcutExtensions.DOAnchorPos(this.Indicator, vector, time, false);
		if (value && time > 0f)
		{
			this.Shake(value);
		}
	}

	[SerializeField]
	private Color SidesOnColor;

	[SerializeField]
	private Color SidesOffColor;

	[Space(10f)]
	[SerializeField]
	private Color IndicatorOnColor;

	[SerializeField]
	private Color IndicatorOffColor;

	[Space(10f)]
	[SerializeField]
	private Image Sides;

	[Space(10f)]
	[SerializeField]
	private Image IndicatorFiller;

	[SerializeField]
	private RectTransform Indicator;

	[SerializeField]
	private RectTransform IndicatorOff;

	[SerializeField]
	private RectTransform IndicatorOn;

	[Space(10f)]
	[SerializeField]
	private float successShakeDuration = 1f;

	[Space(10f)]
	[SerializeField]
	private float failShakeDuration = 0.5f;

	[Space(10f)]
	[SerializeField]
	private float successShakeForce = 12f;

	[Space(10f)]
	[SerializeField]
	private float failShakeForce = 6f;

	[Space(10f)]
	[SerializeField]
	private int successShakeFrequency = 30;

	[Space(10f)]
	[SerializeField]
	private int failShakeFrequency = 16;

	private Vector2 _startPos;
}
