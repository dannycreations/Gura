using System;
using ObjectModel;

public class PlayerIdleThrown : PlayerStateBase
{
	protected override void onEnter()
	{
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 10);
		ControlsController.ControlsActions.SetFishingMappings();
		base.Player.ShowSleeves = false;
		float num = base.Player.RodForce.magnitude * 0.1f;
		float rotationAngle = base.Player.GetRotationAngle();
		float num2 = 20f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = base.calcAnimationWeight(rotationAngle, num, num2);
		if (rotationAngle < 0f)
		{
			num3 = num5;
		}
		if (rotationAngle >= 0f)
		{
			num4 = num5;
		}
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, Math.Max((10f - num5 * 3.5f) / 10f, 0f));
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.LeftWeight, Math.Min(num3 * 3.5f / 10f, 1f));
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.RightWeight, Math.Min(num4 * 3.5f / 10f, 1f));
		base.Player.SetAnimationWeight("RollLeft4Blend", num3);
		base.Player.SetAnimationWeight("RollRight4Blend", num4);
		base.Player.SetAnimationWeight("BaitRollLeft4Blend", num3);
		base.Player.SetAnimationWeight("BaitRollRight4Blend", num4);
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				base.Player.PlayAnimationCheckPlayed("BaitIdleThrown", 1f, 1f, 0.5f);
				base.Player.PlayAnimationBlended("BaitRollLeft4Blend", 1f, num3, 0.5f);
				base.Player.PlayAnimationBlended("BaitRollRight4Blend", 1f, num4, 0.5f);
				base.Player.EUpdateTargetState(true);
			}
		}
		else
		{
			base.Player.PlayAnimationCheckPlayed("IdleThrown", 1f, 1f, 0.5f);
			base.Player.PlayAnimationBlended("RollLeft4Blend", 1f, num3, 0.5f);
			base.Player.PlayAnimationBlended("RollRight4Blend", 1f, num4, 0.5f);
			base.Player.EUpdateTargetState(true);
		}
	}

	protected override Type onUpdate()
	{
		base.Player.UpdateRodPods();
		if (ControlsController.ControlsActions.CameraZoom.WasPressed && !ControlsController.ControlsActions.AddReelClipGamePadPart1.IsPressed)
		{
			base.Player.ZoomCamera(!base.Player.IsCameraZoomed);
		}
		float num = base.Player.RodForce.magnitude * 0.1f;
		float rotationAngle = base.Player.GetRotationAngle();
		float num2 = 20f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = base.calcAnimationWeight(rotationAngle, num, num2);
		base.Player.SetPullAngel(rotationAngle, num5);
		if (rotationAngle < 0f)
		{
			num3 = num5;
		}
		if (rotationAngle >= 0f)
		{
			num4 = num5;
		}
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, Math.Max((10f - num5 * 3.5f) / 10f, 0f));
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.LeftWeight, Math.Min(num3 * 3.5f / 10f, 1f));
		base.Player.Update3dCharMecanimParameter(TPMMecanimFParameter.RightWeight, Math.Min(num4 * 3.5f / 10f, 1f));
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				base.Player.SetAnimationWeight("BaitRollLeft4Blend", num3);
				base.Player.SetAnimationWeight("BaitRollRight4Blend", num4);
			}
		}
		else
		{
			base.Player.SetAnimationWeight("RollLeft4Blend", num3);
			base.Player.SetAnimationWeight("RollRight4Blend", num4);
		}
		if (StaticUserData.RodInHand.RodTemplate.IsChumFishingTemplate() && (ControlsController.ControlsActions.AddReelClip.WasClicked || ControlsController.ControlsActions.AddReelClipGamePadPart1.IsPressedSimultaneous(ControlsController.ControlsActions.AddReelClipGamePadPart2)))
		{
			if (base.Player.RodSlot.LineClips.Count == 0)
			{
				base.Player.RodSlot.AddReelClip();
			}
			else
			{
				base.Player.RodSlot.ClearLastReelClip();
			}
		}
		if (base.Player.Tackle != null && base.Player.Tackle.HasHitTheGround)
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowTackleHitTheGround();
			}
			base.Player.Rod.ReinitializeSimulation(null);
			if (base.Player.IsPitching)
			{
				return typeof(PlayerIdleThrownToIdlePitch);
			}
			return typeof(PlayerIdleThrownToIdle);
		}
		else if (base.Player.Tackle != null && base.Player.Tackle.IsPitchTooShort)
		{
			if (GameFactory.Message != null)
			{
				GameFactory.Message.ShowPitchIsTooShort();
			}
			if (base.Player.Tackle.UnderwaterItem != null)
			{
				return typeof(PlayerShowFishLineIn);
			}
			if (base.Player.IsPitching)
			{
				return typeof(PlayerIdleThrownToIdlePitch);
			}
			return typeof(PlayerIdleThrownToIdle);
		}
		else if (base.Player.Tackle != null && base.Player.Tackle.IsIdle)
		{
			if (base.Player.Tackle.UnderwaterItem != null)
			{
				return typeof(PlayerShowFishLineIn);
			}
			if (base.Player.IsPitching)
			{
				return typeof(PlayerIdleThrownToIdlePitch);
			}
			return typeof(PlayerIdleThrownToIdle);
		}
		else
		{
			if (base.Player.CanPutRodOnPod())
			{
				ShowHudElements.Instance.SetCrossHairState(CrossHair.CrossHairState.PutRod);
			}
			else if (base.Player.CanTakeOrReplaceRodOnStand(false))
			{
				ShowHudElements.Instance.SetCrossHairState(CrossHair.CrossHairState.ChangeRodOnStand);
			}
			if (StaticUserData.RodInHand.IsRodDisassembled)
			{
				return typeof(PlayerDrawOut);
			}
			int requestedSlotId = base.Player.GetRequestedSlotId();
			if ((requestedSlotId != -1 && (ControlsController.ControlsActions.RodStandStandaloneHotkeyModifier.IsPressed || ControlsController.ControlsActions.RodPanel.IsPressed) && base.Player.TryToPutRodOnPod(requestedSlotId)) || (ControlsController.ControlsActions.RodStandSubmit.WasPressed && base.Player.InitPutRodOnPod()))
			{
				return typeof(PutRodOnPodIn);
			}
			if ((requestedSlotId != -1 && (ControlsController.ControlsActions.RodStandStandaloneHotkeyModifier.IsPressed || ControlsController.ControlsActions.RodPanel.IsPressed) && base.Player.TryToReplaceRod(requestedSlotId)) || (ControlsController.ControlsActions.RodStandSubmit.WasFastLongPressed && base.Player.InitTakeRodFromPod(false)))
			{
				return typeof(ReplaceRodOnPodLean);
			}
			bool flag = SettingsManager.InputType != InputModuleManager.InputType.GamePad && base.Player.IsRodPodVisible();
			if (ControlsController.ControlsActions.Fire1.IsPressed && !flag)
			{
				return typeof(PlayerRoll);
			}
			return null;
		}
	}

	protected override void onExit()
	{
		base.Player.ClearRodPodsHighlighting();
		base.Player.Tackle.IsPitchTooShort = false;
		base.Player.EUpdateTargetState(false);
	}

	private const float SIDE_WEIGHT_K = 3.5f;

	private const float MAX_WEIGHT = 10f;

	private const float BLEND_TIME = 0.5f;
}
