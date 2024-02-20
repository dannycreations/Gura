using System;
using UnityEngine;

public class PlayerShowFishIn : PlayerStateBase
{
	protected override void onEnter()
	{
		base.Player.OnFishCaugh();
		base.Player.ZoomCamera(false);
		base.Player.SaveCatchedFishPos();
		base.Player.IsNewFish = true;
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.CatchedFishType, 2);
		base.Player.ShowSleeves = false;
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				this.showFishState = base.Player.PlayAnimationInSeconds("BaitFishGripIn", Mathf.Max(TackleBehaviour.ShowGripInAnimDuration(base.Player.Tackle.Fish) * 2f, 2.5f), 1f);
			}
		}
		else
		{
			this.showFishState = base.Player.PlayAnimationInSeconds("FishGripIn", Mathf.Max(TackleBehaviour.ShowGripInAnimDuration(base.Player.Tackle.Fish) * 2f, 2.5f), 1f);
		}
	}

	protected override Type onUpdate()
	{
		if (this.showFishState == null || this.showFishState.time >= this.showFishState.length)
		{
			return typeof(PlayerShowFishIdle);
		}
		return null;
	}

	private const float MinAnimationTime = 2.5f;

	private AnimationState showFishState;
}
