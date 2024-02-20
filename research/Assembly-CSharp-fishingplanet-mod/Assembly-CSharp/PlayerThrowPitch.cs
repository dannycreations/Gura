using System;
using Assets.Scripts.Phy.Simulations;

public class PlayerThrowPitch : PlayerStateBase
{
	protected override void onEnter()
	{
		base.Player.ShowSleeves = false;
		base.PlayOpenReelSound();
		base.Player.StartThrowState();
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.ASimpleThrow, true);
		base.onEnter();
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				this.AnimationState = base.Player.PlayAnimation("BaitThrowPitch", 1f, 1f, 0f);
			}
		}
		else
		{
			this.AnimationState = base.Player.PlayAnimation("ThrowPitch", 1f, 1f, 0f);
		}
		base.Player.BeginThrowing();
		base.Player.RodSlot.Sim.TackleMoveCompensationMode = TackleMoveCompensationMode.None;
		base.Player.Invoke("ThrowPitchTackle", 0.95f);
	}

	protected override Type onUpdate()
	{
		return (!base.IsAnimationFinished) ? null : typeof(PlayerPitchToIdleThrown);
	}
}
