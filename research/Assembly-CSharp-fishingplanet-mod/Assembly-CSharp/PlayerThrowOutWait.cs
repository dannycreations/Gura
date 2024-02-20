using System;

public class PlayerThrowOutWait : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				this.AnimationState = base.Player.PlayAnimation("BaitThrownIdle", 1f, 1f, 0f);
			}
		}
		else
		{
			this.AnimationState = base.Player.PlayAnimation("ThrowWait", 1f, 1f, 0f);
		}
		PhotonConnectionFactory.Instance.SaveTelemetryInfo(1, string.Format("(slot {0}): ", base.Player.Rod.RodAssembly.RodInterface.Slot) + string.Format("Throw: POSITION {0}; ANGLES {1}", GameFactory.Player.CameraController.transform.position.ToString("###.000000"), GameFactory.Player.CameraController.transform.eulerAngles.ToString("###.000000")));
	}

	protected override Type onUpdate()
	{
		if (StaticUserData.RodInHand.IsRodDisassembled || !base.Player.IsReadyForRod)
		{
			return typeof(PlayerDrawOut);
		}
		if (base.Player.Tackle.IsInWater)
		{
			return typeof(PlayerThrowToIdleThrown);
		}
		if (base.Player.Tackle.IsOnTip || base.Player.Tackle.IsHangingAfterThrow)
		{
			return typeof(PlayerThrowToIdleThrown);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.ThrowSounds.Source.Stop();
	}
}
