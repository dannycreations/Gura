using System;

public class PlayerThrowOutOneHandRight : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		base.PlaySound(PlayerStateBase.Sounds.RodCast);
		base.Player.StartThrowState();
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				this.AnimationState = base.Player.PlayAnimation("BaitThrowRightOut", 1f, 1f, 0f);
			}
		}
		else
		{
			this.AnimationState = base.Player.PlayAnimation("ThrowOneHandRightOut", 1f, 1f, 0f);
		}
		base.Player.ThrowSounds.playRandomSound(0.05f);
		base.Player.FinishThrowPowerGainProcess();
		base.Player.Invoke("ThrowTackle", 0.2f);
		base.Player.Invoke("FinishThrowPowerGainProcessSwing", 0.2f);
	}

	protected override Type onUpdate()
	{
		if (StaticUserData.RodInHand.IsRodDisassembled)
		{
			return typeof(PlayerDrawOut);
		}
		if (base.IsAnimationFinished)
		{
			return typeof(PlayerThrowOutWait);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.FinishThrowing();
		GameFactory.Player.HudFishingHandler.State = HudState.Fight;
		GameFactory.Player.HudFishingHandler.CastSimpleHandler.CurrentValue = 0f;
	}
}
