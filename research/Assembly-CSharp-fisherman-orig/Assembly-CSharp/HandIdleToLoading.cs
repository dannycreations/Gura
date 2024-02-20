using System;

public class HandIdleToLoading : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		this.AnimationState = base.Player.PlayReverseAnimation("TakeLure", 1.3f);
	}

	protected override Type onUpdate()
	{
		if (!base.IsAnimationFinished)
		{
			return null;
		}
		base.Player.DestroyChumBall();
		if (base.IsHandChumLoadingRequired)
		{
			return typeof(HandLoadingIdle);
		}
		return typeof(HandDrawIn);
	}
}
