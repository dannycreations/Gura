using System;
using ObjectModel;

public class PlayerIdle : PlayerBaseIdle
{
	protected override void onEnter()
	{
		base.onEnter();
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				base.Player.PlayAnimation("BaitIdle", 1f, 1f, 0f);
			}
		}
		else
		{
			base.Player.PlayAnimation("Idle", 1f, 1f, 0f);
		}
		base.Player.IsPitching = false;
	}

	protected override Type onUpdate()
	{
		Type type = base.onUpdate();
		if (type != null)
		{
			return type;
		}
		if (base.Player.IsRodActive && (base.IsChumLoadingRequired || base.ChumClearingRequired))
		{
			return typeof(PlayerDrawOut);
		}
		if (StaticUserData.RodInHand.RodTemplate.IsChumFishingTemplate() && (ControlsController.ControlsActions.AddReelClip.WasClicked || ControlsController.ControlsActions.AddReelClipGamePadPart1.IsPressedSimultaneous(ControlsController.ControlsActions.AddReelClipGamePadPart2)))
		{
			base.Player.RodSlot.ClearLastReelClip();
		}
		bool flag = SettingsManager.InputType != InputModuleManager.InputType.GamePad && base.Player.IsRodPodVisible();
		if (ControlsController.ControlsActions.Fire1.WasPressed && !ControlsController.ControlsActions.IsBlockedAxis && !flag)
		{
			base.Player.CastType = CastTypes.Simple;
			return typeof(PlayerSimpleThrow);
		}
		if (ControlsController.ControlsActions.Fire2.WasPressed && !ControlsController.ControlsActions.IsBlockedAxis && !flag)
		{
			return typeof(PlayerIdleTarget);
		}
		if (ControlsController.ControlsActions.PitchMode.WasPressed && !ControlsController.ControlsActions.IsBlockedAxis && StaticUserData.RodInHand.RodTemplate == RodTemplate.Float)
		{
			base.Player.IsPitching = true;
			if (!StaticUserData.IS_IN_TUTORIAL)
			{
				PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.Pitching, base.Player.IsPitching, null);
			}
			return typeof(PlayerIdleToIdlePitch);
		}
		if (ControlsController.ControlsActions.PitchMode.WasPressed && !ControlsController.ControlsActions.IsBlockedAxis && StaticUserData.RodInHand.RodTemplate == RodTemplate.Float)
		{
			base.Player.IsPitching = true;
			PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.Pitching, base.Player.IsPitching, null);
			return typeof(PlayerIdleToIdlePitch);
		}
		return null;
	}
}
