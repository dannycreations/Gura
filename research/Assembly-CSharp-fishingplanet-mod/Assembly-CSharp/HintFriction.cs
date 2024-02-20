using System;
using DG.Tweening;
using UnityEngine;

public class HintFriction : HintFrictionSpeed
{
	protected override void DoFade(CanvasGroup[] imgs)
	{
		FrictionAndSpeedHandler frictionHandler = ShowHudElements.Instance.FishingHndl.FrictionHandler;
		frictionHandler.DoFadeSectors((this.State != HintFrictionSpeed.States.Hiding) ? 0f : 1f, this.AnimTime);
		for (int i = 0; i < this.Value; i++)
		{
			ShortcutExtensions.DOFade(imgs[i], (this.State != HintFrictionSpeed.States.Hiding) ? 1f : 0f, this.AnimTime);
		}
	}

	protected override void Dispose()
	{
		ShowHudElements.Instance.FishingHndl.FrictionHandler.DoFadeSectors(1f, 0f);
	}

	protected override string GetTextUpActionName()
	{
		return "IncFriction";
	}

	protected override string GetTextDownActionName()
	{
		return "DecFriction";
	}

	protected override bool FrictionValueChanged()
	{
		if (GameFactory.Player.Reel != null && this.FrictionValue != GameFactory.Player.Reel.CurrentFrictionSection)
		{
			this.FrictionValue = GameFactory.Player.Reel.CurrentFrictionSection;
			return true;
		}
		return false;
	}
}
