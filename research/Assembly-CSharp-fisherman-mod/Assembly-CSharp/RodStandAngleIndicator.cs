using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RodStandAngleIndicator : MonoBehaviour
{
	private void Awake()
	{
		this.SetActive(this._isActive);
	}

	public bool IsActive()
	{
		return this._isActive;
	}

	private void RefreshHelpTexts()
	{
		bool flag;
		this.helpUp.text = HotkeyIcons.GetIcoByActionName("RodStandAddAngle", out flag);
		this.helpDown.text = HotkeyIcons.GetIcoByActionName("RodStandDecAngle", out flag);
	}

	public void SetActive(bool active)
	{
		if (active)
		{
			this.RefreshHelpTexts();
		}
		this._isActive = active;
		DOTween.Kill(this.canvasGroup, false);
		ShortcutExtensions.DOFade(this.canvasGroup, (!active) ? 0f : 1f, 0.4f);
	}

	public void SetNormalizedAngle(float angle, bool immediate = false)
	{
		this._currentAngle = angle;
		if (immediate)
		{
			this.scrollRect.verticalNormalizedPosition = this._currentAngle;
		}
	}

	private void Update()
	{
		if (this._isActive)
		{
			this.scrollRect.verticalNormalizedPosition = Mathf.Lerp(this.scrollRect.verticalNormalizedPosition, this._currentAngle, 0.2f);
		}
		else if (this.scrollRect.verticalNormalizedPosition != this._currentAngle)
		{
			this.scrollRect.verticalNormalizedPosition = this._currentAngle;
		}
	}

	[SerializeField]
	private TextMeshProUGUI helpUp;

	[SerializeField]
	private TextMeshProUGUI helpDown;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private float _currentAngle = 0.5f;

	private bool _isActive;
}
