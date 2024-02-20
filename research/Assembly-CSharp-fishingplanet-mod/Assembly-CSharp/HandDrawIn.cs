using System;

public class HandDrawIn : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 6);
		GameFactory.Player.HudFishingHandler.State = HudState.HandIdle;
		base.Player.RequestedRod = null;
		this.AnimationState = base.Player.PlayAnimation("TakeLure", 1f, 1f, 0f);
		base.Player.CreateChumBall();
	}

	protected override Type onUpdate()
	{
		if (base.IsAnimationFinished)
		{
			return typeof(HandIdle);
		}
		return null;
	}
}
