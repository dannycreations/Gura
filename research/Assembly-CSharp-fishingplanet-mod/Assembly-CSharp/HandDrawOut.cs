using System;
using ObjectModel;

public class HandDrawOut : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		if (base.Player.IsHandThrowMode)
		{
			Chum chum = FeederHelper.FindPreparedChumOnDoll();
			Chum chum2 = FeederHelper.FindPreparedChumInHand();
			if ((chum != null || chum2 != null) && chum2 != null)
			{
				PhotonConnectionFactory.Instance.MoveItemOrCombine(chum2, null, StoragePlaces.Doll, true);
			}
		}
		if (!base.Player.IsTransitionToMap)
		{
			base.Player.IsHandThrowMode = false;
		}
		this.AnimationState = base.Player.PlayReverseAnimation("TakeLure", 1.3f);
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 0);
	}

	protected override Type onUpdate()
	{
		if (base.IsAnimationFinished)
		{
			return typeof(PlayerEmpty);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.OnHandMode(false);
		base.Player.DestroyChumBall();
	}
}
