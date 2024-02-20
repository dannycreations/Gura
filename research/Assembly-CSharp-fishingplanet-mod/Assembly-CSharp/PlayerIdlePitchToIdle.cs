using System;
using UnityEngine;

public class PlayerIdlePitchToIdle : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		this.waitState = base.Player.PlayAnimation("PitchIdleToIdle", 1f, 1f, 0f);
	}

	protected override Type onUpdate()
	{
		if (this.waitState == null || this.waitState.time >= this.waitState.length)
		{
			return typeof(PlayerIdle);
		}
		return null;
	}

	protected override void onExit()
	{
	}

	private AnimationState waitState;
}
