using System;

public class HandTrowToLoading : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		base.Player.ThrowChumByHand();
		this.AnimationState = base.Player.PlayAnimation("LureThrowToIdle", 1f, 1f, 0f);
	}

	protected override Type onUpdate()
	{
		if (!base.IsAnimationFinished)
		{
			return null;
		}
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.SubState, 0);
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 0);
		if (base.IsHandChumLoadingRequired)
		{
			return typeof(HandLoadingIdle);
		}
		base.Player.IsHandThrowMode = false;
		base.Player.OnHandMode(false);
		return typeof(PlayerEmpty);
	}
}
