using System;
using ObjectModel;

public class PlayerIdlePitch : PlayerBaseIdle
{
	protected override void onEnter()
	{
		base.onEnter();
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				base.Player.PlayAnimation("BaitIdlePitch", 1f, 1f, 0f);
			}
		}
		else
		{
			base.Player.PlayAnimation("IdlePitch", 1f, 1f, 0f);
		}
		base.Player.RodSlot.Sim.TurnLimitsOn(false);
		base.Player.IsPitching = true;
	}

	protected override Type onUpdate()
	{
		Type type = base.onUpdate();
		if (type != null)
		{
			return type;
		}
		bool flag = SettingsManager.InputType != InputModuleManager.InputType.GamePad && base.Player.IsRodPodVisible();
		if (ControlsController.ControlsActions.Fire1.WasPressed && base.Player.RodSlot.Tackle != null && base.Player.RodSlot.Tackle.IsOnTip && base.Player.RodSlot.Tackle.IsOnTipComplete && !ControlsController.ControlsActions.IsBlockedAxis && !flag)
		{
			base.Player.CastType = CastTypes.Simple;
			return typeof(PlayerThrowPitch);
		}
		if (ControlsController.ControlsActions.PitchMode.WasPressed && !ControlsController.ControlsActions.IsBlockedAxis)
		{
			base.Player.IsPitching = false;
			if (!StaticUserData.IS_IN_TUTORIAL)
			{
				PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.Pitching, base.Player.IsPitching, null);
			}
			return typeof(PlayerIdlePitchToIdle);
		}
		return null;
	}
}
