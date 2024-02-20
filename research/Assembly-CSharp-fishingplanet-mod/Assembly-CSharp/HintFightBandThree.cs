using System;
using UnityEngine;

public class HintFightBandThree : HintFightBand
{
	protected override Transform[] GetTransform4FillData()
	{
		FishingHandler fishingHndl = ShowHudElements.Instance.FishingHndl;
		return new Transform[]
		{
			fishingHndl.FightThreeBandHandler.ReelDamageGlow.transform,
			fishingHndl.FightThreeBandHandler.RodDamageGlow.transform,
			fishingHndl.FightThreeBandHandler.LineDamageGlow.transform
		};
	}

	protected override void UpdateVisual(bool isDestroy = false)
	{
		base.UpdateVisual(isDestroy);
		FishingHandler fishingHndl = ShowHudElements.Instance.FishingHndl;
		fishingHndl.FightThreeBandHandler.DamageHandler.IsHintActive = this.shouldShow && !isDestroy;
	}
}
