using System;

public class PlayerDrawOut : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.onEnter();
		base.PlaySound(PlayerStateBase.Sounds.RodDrawOut);
		base.Player.OnDrawOut();
		if (base.Player.IsPitching)
		{
			ReelTypes reelType = base.Player.ReelType;
			if (reelType != ReelTypes.Spinning)
			{
				if (reelType == ReelTypes.Baitcasting)
				{
					this.AnimationState = base.Player.PlayReverseAnimation("BaitDrawToIdlePitch", 1.5f * base.AnimationSpeedK);
				}
			}
			else
			{
				this.AnimationState = base.Player.PlayReverseAnimation("DrawToIdlePitch", 1.5f * base.AnimationSpeedK);
			}
		}
		else
		{
			ReelTypes reelType2 = base.Player.ReelType;
			if (reelType2 != ReelTypes.Spinning)
			{
				if (reelType2 == ReelTypes.Baitcasting)
				{
					this.AnimationState = base.Player.PlayReverseAnimation("BaitDraw", 1.5f * base.AnimationSpeedK);
				}
			}
			else
			{
				this.AnimationState = base.Player.PlayReverseAnimation("OpenDraw", 1.5f * base.AnimationSpeedK);
			}
		}
	}

	protected override Type onUpdate()
	{
		if (base.IsAnimationFinished)
		{
			base.Player.DestroyRod(false);
			return typeof(PlayerEmpty);
		}
		return null;
	}
}
