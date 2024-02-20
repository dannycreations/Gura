using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HintFrictionSpeed : HintColorBase
{
	protected override void Init()
	{
		base.Init();
		this.TextUpPosY = this._textUp.GetComponent<RectTransform>().anchoredPosition.y;
		this.TextDownPosY = this._textDown.GetComponent<RectTransform>().anchoredPosition.y;
		this.UpdateFrictionVisibility();
		base.CloneFontMaterial(this._textUp);
		base.CloneFontMaterial(this._textDown);
		this.UpdateHelpTexts();
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	public override void SetObserver(ManagedHint observer, int id)
	{
		base.SetObserver(observer, id);
		if (observer != null && observer.Message != null)
		{
			this.Value = observer.Message.Value;
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateAnims();
	}

	protected virtual void UpdateAnims()
	{
		if (this.Value >= 0)
		{
			if (!this.shouldShow)
			{
				if (this.State != HintFrictionSpeed.States.None)
				{
					this.Dispose();
				}
				return;
			}
			if (this.FrictionValueChanged())
			{
				this.UpdateHelpTexts();
				this.UpdateAnimTexts(true);
				this.UpdateAnimTexts(false);
			}
			if (this.State == HintFrictionSpeed.States.None)
			{
				this.State = HintFrictionSpeed.States.Hide;
			}
			CanvasGroup[] currentSpeedImages = this.GetCurrentSpeedImages();
			if (this.Value < currentSpeedImages.Length)
			{
				if (this.State == HintFrictionSpeed.States.Hide)
				{
					this.State = HintFrictionSpeed.States.Showing;
					this.DoFade(currentSpeedImages);
					base.StartCoroutine(this.AnimFinished(HintFrictionSpeed.States.Show));
				}
				else if (this.State == HintFrictionSpeed.States.Show)
				{
					this.State = HintFrictionSpeed.States.Hiding;
					this.DoFade(currentSpeedImages);
					base.StartCoroutine(this.AnimFinished(HintFrictionSpeed.States.Hide));
				}
			}
		}
	}

	protected virtual bool FrictionValueChanged()
	{
		if (this.FrictionValue != GameFactory.Player.Reel.CurrentReelSpeedSection)
		{
			this.FrictionValue = GameFactory.Player.Reel.CurrentReelSpeedSection;
			return true;
		}
		return false;
	}

	protected virtual void DoFade(CanvasGroup[] imgs)
	{
		FrictionAndSpeedHandler frictionHandler = ShowHudElements.Instance.FishingHndl.FrictionHandler;
		frictionHandler.DoFade((this.State != HintFrictionSpeed.States.Hiding) ? this.AnimAlphaFriction : 1f, this.AnimTimeFriction);
		ShortcutExtensions.DOFade(imgs[this.Value], (this.State != HintFrictionSpeed.States.Hiding) ? this.AnimAlpha : 0f, this.AnimTime);
	}

	protected virtual IEnumerator AnimFinished(HintFrictionSpeed.States newState)
	{
		yield return new WaitForSeconds(Mathf.Max(this.AnimTime, this.AnimTimeFriction) + this.AnimDelayTime);
		this.State = newState;
		yield break;
	}

	protected override void OnDestroy()
	{
		this.Dispose();
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		base.StopAllCoroutines();
		base.OnDestroy();
	}

	protected virtual void Dispose()
	{
		ShowHudElements.Instance.FishingHndl.FrictionHandler.DoFade(1f, 0f);
	}

	protected virtual CanvasGroup[] GetCurrentSpeedImages()
	{
		FrictionAndSpeedHandler frictionHandler = ShowHudElements.Instance.FishingHndl.FrictionHandler;
		if (frictionHandler.FrictionCount == 6)
		{
			return this.Speed6;
		}
		if (frictionHandler.FrictionCount == 8)
		{
			return this.Speed8;
		}
		return this.Speed12;
	}

	protected virtual void UpdateFrictionVisibility()
	{
		FrictionAndSpeedHandler frictionHandler = ShowHudElements.Instance.FishingHndl.FrictionHandler;
		if (this.FrictionCount == frictionHandler.FrictionCount)
		{
			return;
		}
		this.FrictionCount = frictionHandler.FrictionCount;
		this.Speed6Go.SetActive(this.FrictionCount == 6);
		this.Speed8Go.SetActive(this.FrictionCount == 8);
		this.Speed12Go.SetActive(this.FrictionCount == 12);
	}

	protected virtual void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		this.UpdateHelpTexts();
	}

	protected virtual void UpdateHelpTexts()
	{
		bool flag;
		this._textUp.text = HotkeyIcons.GetIcoByActionName(this.GetTextUpActionName(), out flag);
		this._textDown.text = HotkeyIcons.GetIcoByActionName(this.GetTextDownActionName(), out flag);
		if (this.FrictionValue == -1)
		{
			this.FrictionValueChanged();
		}
		this._textUpGlow.SetActive(flag && this.Value > this.FrictionValue);
		this._textDownGlow.SetActive(flag && this.Value < this.FrictionValue);
	}

	protected virtual string GetTextUpActionName()
	{
		return "IncSpeed";
	}

	protected virtual string GetTextDownActionName()
	{
		return "DecSpeed";
	}

	protected override void UpdateVisual(bool isDestroy = false)
	{
		base.UpdateVisual(isDestroy);
		this.UpdateAnimTexts(isDestroy);
	}

	protected void AddAnim(Sequence s, TextMeshProUGUI t)
	{
		RectTransform component = t.GetComponent<RectTransform>();
		float y = component.anchoredPosition.y;
		TweenSettingsExtensions.Append(this.MySequence, ShortcutExtensions.DOAnchorPos(component, new Vector2(component.anchoredPosition.x, y - 5f), 0.5f, false));
		TweenSettingsExtensions.Append(this.MySequence, ShortcutExtensions.DOAnchorPos(component, new Vector2(component.anchoredPosition.x, y), 0.5f, false));
	}

	protected virtual void UpdateAnimTexts(bool isDestroy)
	{
		if (this.shouldShow && !isDestroy)
		{
			if (this.FrictionValue == -1)
			{
				this.FrictionValueChanged();
			}
			if (this.MySequence == null)
			{
				this.MySequence = DOTween.Sequence();
				this._textDown.gameObject.SetActive(this.Value < this.FrictionValue);
				this._textUp.gameObject.SetActive(this.Value > this.FrictionValue);
				this.InitAnim(this._textDown);
				this.InitAnim(this._textUp);
				TweenSettingsExtensions.SetLoops<Sequence>(this.MySequence, 1000, 0);
			}
		}
		else
		{
			if (this.MySequence != null)
			{
				TweenExtensions.Kill(this.MySequence, false);
				this.MySequence = null;
			}
			this.ClearAnim(this._textUp, this.TextUpPosY);
			this.ClearAnim(this._textDown, this.TextDownPosY);
		}
	}

	protected void ClearAnim(TextMeshProUGUI t, float startPos)
	{
		if (t.gameObject.activeSelf)
		{
			t.GetComponent<RectTransform>().anchoredPosition = new Vector2(t.GetComponent<RectTransform>().anchoredPosition.x, startPos);
			t.fontSharedMaterial.DisableKeyword(ShaderUtilities.Keyword_Glow);
			t.UpdateMeshPadding();
			t.gameObject.SetActive(false);
		}
	}

	protected void InitAnim(TextMeshProUGUI t)
	{
		if (t.gameObject.activeSelf)
		{
			t.fontSharedMaterial.EnableKeyword(ShaderUtilities.Keyword_Glow);
			t.UpdateMeshPadding();
			this.AddAnim(this.MySequence, t);
		}
	}

	[SerializeField]
	protected float AnimAlphaFriction = 0.6f;

	[SerializeField]
	protected float AnimAlpha = 0.8f;

	[SerializeField]
	protected float AnimTime = 0.6f;

	[SerializeField]
	protected float AnimTimeFriction = 0.6f;

	[SerializeField]
	protected float AnimDelayTime = 0.2f;

	[SerializeField]
	protected GameObject _textUpGlow;

	[SerializeField]
	protected GameObject _textDownGlow;

	[SerializeField]
	protected TextMeshProUGUI _textUp;

	[SerializeField]
	protected TextMeshProUGUI _textDown;

	[SerializeField]
	protected GameObject Speed6Go;

	[SerializeField]
	protected GameObject Speed8Go;

	[SerializeField]
	protected GameObject Speed12Go;

	[SerializeField]
	protected CanvasGroup[] Speed6;

	[SerializeField]
	protected CanvasGroup[] Speed8;

	[SerializeField]
	protected CanvasGroup[] Speed12;

	protected int Value = -1;

	protected int FrictionValue = -1;

	protected HintFrictionSpeed.States State;

	protected int FrictionCount = -1;

	protected Sequence MySequence;

	protected float TextUpPosY;

	protected float TextDownPosY;

	protected enum States : byte
	{
		None,
		Show,
		Showing,
		Hide,
		Hiding
	}
}
