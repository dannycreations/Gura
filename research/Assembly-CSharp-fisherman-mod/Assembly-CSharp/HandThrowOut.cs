using System;

public class HandThrowOut : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.ASimpleThrow, false);
		base.Player.FinishThrowPowerGainProcess();
		base.Player.HudFishingHandler.State = HudState.HandIdle;
		base.Player.HudFishingHandler.CastSimpleHandler.CurrentValue = 0f;
		this.AnimationState = base.Player.PlayAnimation("LureThrowOut", 1f, 1f, 0f);
	}

	protected override Type onUpdate()
	{
		if (base.IsAnimationFinished)
		{
			return typeof(HandTrowToLoading);
		}
		return null;
	}
}
