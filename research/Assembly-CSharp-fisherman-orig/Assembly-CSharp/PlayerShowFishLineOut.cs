using System;
using UnityEngine;

public class PlayerShowFishLineOut : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.CatchedFishType, 0);
		this.priorTime = -1f;
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				this.showFishState = base.Player.PlayAnimation("BaitFishLineOut", 1f, 1f, 0f);
			}
		}
		else
		{
			this.showFishState = base.Player.PlayAnimation("FishLineOut", 1f, 1f, 0f);
		}
	}

	protected override Type onUpdate()
	{
		if (base.Player.FastUseRodPod || this.showFishState == null || this.showFishState.time >= this.showFishState.length || this.priorTime == this.showFishState.time)
		{
			base.Player.ShowSleeves = false;
			return typeof(PlayerIdleThrownToIdle);
		}
		this.priorTime = this.showFishState.time;
		return null;
	}

	protected override void onExit()
	{
		base.Player.RodSlot.RestoreReelClip();
		base.Player.Rod.ReinitializeSimulation(null);
	}

	private AnimationState showFishState;

	private float priorTime;
}
