using System;
using UnityEngine;

public class ToolSetup : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.SubState, 1);
		base.Player.DisableMovement();
		this.timeSpent = 0f;
		this.setup = false;
		this.AnimationState = base.Player.PlayAnimation("putBox", 1f, 1f, 0f);
		base.Player.InitTransitionToZeroXRotation();
	}

	protected override Type onUpdate()
	{
		base.Player.TransitToZeroXRotation();
		this.timeSpent += Time.deltaTime;
		if (this.timeSpent < this.AnimationState.length * 0.7f)
		{
			float num = Mathf.InverseLerp(0f, this.AnimationState.length * 0.7f, this.timeSpent);
			GameFactory.Player.BowDown = Mathf.Lerp(0f, 1f, num);
		}
		else
		{
			if (!this.setup)
			{
				this.setup = true;
				base.Player.PutFirework();
			}
			float num2 = Mathf.InverseLerp(this.AnimationState.length * 0.7f, this.AnimationState.length, this.timeSpent);
			GameFactory.Player.BowDown = Mathf.Lerp(1f, 0f, num2);
		}
		if (base.IsAnimationFinished)
		{
			return typeof(PlayerEmpty);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.FinalizeTransitionToZeroXRotation();
		base.Player.EnableMovement();
	}

	private float timeSpent;

	private bool setup;
}
