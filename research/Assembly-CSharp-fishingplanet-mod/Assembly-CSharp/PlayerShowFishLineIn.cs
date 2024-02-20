using System;
using UnityEngine;

public class PlayerShowFishLineIn : PlayerStateBase
{
	protected override void onEnter()
	{
		base.Player.OnFishCaugh();
		base.Player.ZoomCamera(false);
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, 0f);
		base.Player.SaveCatchedFishPos();
		base.Player.IsNewFish = true;
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.CatchedFishType, 1);
		base.Player.ShowSleeves = false;
		float num = Random.Range(0.65f, 0.85f);
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				this.showFishState = base.Player.PlayAnimation("BaitFishLineIn", num, 1f, 0f);
			}
		}
		else
		{
			this.showFishState = base.Player.PlayAnimation("FishLineIn", num, 1f, 0f);
		}
	}

	protected override Type onUpdate()
	{
		if (!base.Player.Tackle.IsShowing)
		{
			return typeof(PlayerShowFishLineOut);
		}
		if (this.showFishState == null || this.showFishState.time >= this.showFishState.length)
		{
			return typeof(PlayerShowFishLineIdle);
		}
		return null;
	}

	private AnimationState showFishState;
}
