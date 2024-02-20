using System;
using UnityEngine;

public class PlayerIdleToIdlePitch : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		this.waitState = base.Player.PlayAnimation("IdleToPitchIdle", 0.5f, 1f, 0f);
	}

	protected override Type onUpdate()
	{
		if (this.waitState == null || this.waitState.time >= this.waitState.length)
		{
			return typeof(PlayerIdlePitch);
		}
		return null;
	}

	private AnimationState waitState;
}
