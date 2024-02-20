using System;
using UnityEngine;
using UnityEngine.UI;

public class HintCastSimple : HintColorBase
{
	protected override void UpdateVisual(bool isDestroy = false)
	{
		if (this.shouldShow && !isDestroy && this.observer != null && this.observer.Message != null)
		{
			this.Value = this.observer.Message.Length / 1f;
			this._image.fillAmount = this.Value;
		}
	}

	protected override void Update()
	{
		base.Update();
		this.UpdateArrow();
	}

	protected virtual void UpdateArrow()
	{
		if (this.shouldShow && GameFactory.Player != null && GameFactory.Player.HudFishingHandler != null && GameFactory.Player.HudFishingHandler.State != HudState.EngineIgnition)
		{
			this._arrowRect.anchoredPosition = new Vector2(this._arrowRect.localPosition.x, 370f * this.CalcArrowTarget() - 185f);
		}
	}

	protected virtual float CalcArrowTarget()
	{
		return this.Value;
	}

	[SerializeField]
	protected Image _image;

	[SerializeField]
	protected RectTransform _arrowRect;

	protected float Value;
}
