using System;
using Cayman;
using UnityEngine;

public class PlayerThrowToIdleThrown : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		if (CaymanGenerator.Instance != null)
		{
			CaymanGenerator.Instance.OnTackleTakeWater(base.Player.Tackle.transform.position);
		}
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.IsThrowFinished, true);
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				this.AnimationState = base.Player.PlayAnimation("BaitThrow2Idle", 1f, 1f, 0f);
				base.Player.Invoke("SetRodRootLeft", 0.7f);
			}
		}
		else
		{
			base.Player.ShowSleeves = false;
			this.AnimationState = base.Player.PlayAnimation("CloseRollStart", 1f, 1f, 0f);
			base.PlaySound(PlayerStateBase.Sounds.CloseReel);
		}
	}

	protected override Type onUpdate()
	{
		if (base.IsAnimationFinished)
		{
			base.Player.throwFinishTime = Time.time;
			base.Player.InitThrownPos();
			return typeof(PlayerIdleThrown);
		}
		return null;
	}
}
