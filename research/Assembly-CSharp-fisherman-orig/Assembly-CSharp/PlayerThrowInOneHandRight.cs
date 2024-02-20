using System;

public class PlayerThrowInOneHandRight : PlayerStateBase
{
	protected override void onEnter()
	{
		base.PlayOpenReelSound();
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 4);
				this.AnimationState = base.Player.PlayAnimation("BaitThrowRightIn", 1f, 1f, 0f);
			}
		}
		else
		{
			base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 4);
			this.AnimationState = base.Player.PlayAnimation("ThrowOneHandRightIn", 1f, 1f, 0f);
		}
		base.Player.BeginThrowing();
		base.Player.Invoke("RaiseCanThrow", this.AnimationState.length);
	}

	protected override Type onUpdate()
	{
		if (this.canThrow != base.Player.CanThrow && base.Player.CanThrow)
		{
			this.canThrow = true;
			base.Player.BeginThrowPowerGainProcess();
		}
		if (this.canThrow)
		{
			return typeof(PlayerThrowOutOneHandRight);
		}
		return null;
	}

	private bool canThrow;
}
