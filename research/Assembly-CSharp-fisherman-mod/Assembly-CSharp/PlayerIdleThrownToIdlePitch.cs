using System;
using UnityEngine;

public class PlayerIdleThrownToIdlePitch : PlayerStateBase
{
	protected override void onEnter()
	{
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 0);
		this.waitState = base.Player.PlayAnimation("IdleThrownToIdlePitch", 1f, 1f, 0f);
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
