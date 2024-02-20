using System;
using UnityEngine;

public class HintFightBandOne : HintFightBand
{
	protected override Transform[] GetTransform4FillData()
	{
		FishingHandler fishingHndl = ShowHudElements.Instance.FishingHndl;
		return new Transform[]
		{
			fishingHndl.FightOneBandHandler.ReelDamageGlow.transform,
			fishingHndl.FightOneBandHandler.RodDamageGlow.transform,
			fishingHndl.FightOneBandHandler.LineDamageGlow.transform
		};
	}

	protected override void UpdateVisual(bool isDestroy = false)
	{
		base.UpdateVisual(isDestroy);
		FishingHandler fishingHndl = ShowHudElements.Instance.FishingHndl;
		fishingHndl.FightOneBandHandler.DamageHandler.IsHintActive = this.shouldShow && !isDestroy;
	}
}
