using System;
using UnityEngine;

public class PlayerPitchToIdleThrown : PlayerStateBase
{
	protected override void onEnter()
	{
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, 0f);
		base.Player.Update3dCharMecanimParameter(TPMMecanimBParameter.ASimpleThrow, false);
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 5);
		this.waitState = base.Player.PlayAnimation("PitchToIdleThrown", 1f, 1f, 0f);
		PhotonConnectionFactory.Instance.SaveTelemetryInfo(1, string.Format("(slot {0}): ", base.Player.Rod.RodAssembly.RodInterface.Slot) + string.Format("Throw: POSITION {0}; ANGLES {1}", GameFactory.Player.CameraController.transform.position.ToString("###.000000"), GameFactory.Player.CameraController.transform.eulerAngles.ToString("###.000000")));
	}

	protected override Type onUpdate()
	{
		if (this.waitState == null || this.waitState.time >= this.waitState.length)
		{
			base.Player.InitThrownPos();
			base.Player.throwFinishTime = Time.time;
			base.Player.FinishThrowing();
			return typeof(PlayerIdleThrown);
		}
		return null;
	}

	protected override void onExit()
	{
		base.PlaySound(PlayerStateBase.Sounds.CloseReel);
	}

	private AnimationState waitState;
}
