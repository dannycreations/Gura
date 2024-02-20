using System;
using UnityEngine;

public class PlayerIdleThrownToIdle : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 0);
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				if (!base.Player.IsPitching)
				{
					this.waitState = base.Player.PlayAnimation("BaitRoll2Idle", 1f * base.AnimationSpeedK, 1f, 0f);
				}
				base.Player.Invoke("SetRodRootRight", 0.5f / base.AnimationSpeedK);
			}
		}
		else if (!base.Player.IsPitching)
		{
			this.waitState = base.Player.PlayAnimation("Idle2Roll1", 1f * base.AnimationSpeedK, 1f, 0f);
		}
	}

	protected override Type onUpdate()
	{
		if (!(this.waitState == null) && this.waitState.time < this.waitState.length)
		{
			return null;
		}
		if (base.Player.IsPitching)
		{
			return typeof(PlayerIdleThrownToIdlePitch);
		}
		return typeof(PlayerIdle);
	}

	protected override void onExit()
	{
	}

	private AnimationState waitState;
}
