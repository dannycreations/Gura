using System;

public class PlayerThrowInOneHandLeft : PlayerStateBase
{
	protected override void onEnter()
	{
		base.PlayOpenReelSound();
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 3);
				this.AnimationState = base.Player.PlayAnimation("BaitThrowLeftIn", 1f, 1f, 0f);
			}
		}
		else
		{
			base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 3);
			this.AnimationState = base.Player.PlayAnimation("ThrowOneHandLeftIn", 1f, 1f, 0f);
		}
		base.Player.BeginThrowing();
		base.Player.Invoke("RaiseCanThrow", this.AnimationState.length);
	}

	protected override Type onUpdate()
	{
		if (this.canThrow != base.Player.CanThrow && base.Player.CanThrow)
		{
			this.canThrow = true;
		}
		if (this.canThrow)
		{
			return typeof(PlayerThrowOutOneHandLeft);
		}
		return null;
	}

	private bool canThrow;
}
