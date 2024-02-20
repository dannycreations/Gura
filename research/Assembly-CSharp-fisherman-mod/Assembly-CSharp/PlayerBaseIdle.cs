using System;
using ObjectModel;

public class PlayerBaseIdle : PlayerStateBase
{
	public override bool CantOpenInventory
	{
		get
		{
			return false;
		}
	}

	protected override void onEnter()
	{
		base.Player.OnEnterIdle();
		base.Player.RodSlot.Tackle.ThrowData.IsOvercasting = false;
		GameFactory.Player.HudFishingHandler.CastTargetHandler.CurrentValue = 0f;
		base.ProcessSavedMenuPage();
		base.Player.ZoomCamera(false);
	}

	protected override Type onUpdate()
	{
		if (!base.Player.IsSailing)
		{
			base.Player.UpdateSelectedBoat();
		}
		base.Player.UpdateRodPods();
		if (base.Player.FastUseRodPod || ControlsController.ControlsActions.EmptyHands.WasReleased || base.Player.IsEmptyHandsMode)
		{
			base.Player.IsEmptyHandsMode = true;
			return typeof(PlayerDrawOut);
		}
		if ((ControlsController.ControlsActions.HandThrow.WasReleased || base.Player.IsHandThrowMode) && base.Player.IsReadyForRod)
		{
			Chum chum = FeederHelper.FindPreparedChumOnDoll();
			Chum chum2 = FeederHelper.FindPreparedChumInHand();
			if (chum != null || chum2 != null)
			{
				if (chum2 == null)
				{
					PhotonConnectionFactory.Instance.MoveItemOrCombine(chum, null, StoragePlaces.Hands, true);
				}
				base.Player.IsHandThrowMode = true;
				return typeof(PlayerDrawOut);
			}
			base.Player.IsHandThrowMode = false;
		}
		bool flag = false;
		bool flag2 = base.Player.RodSlotToTakeFromPod != -1 || base.Player.CanTakeOrReplaceRodOnStand(true);
		if (flag2)
		{
			ShowHudElements.Instance.SetCrossHairState(CrossHair.CrossHairState.TakeRod);
		}
		else if (base.Player.CanPickUpRodPod())
		{
			ShowHudElements.Instance.SetCrossHairState(CrossHair.CrossHairState.TakeStand);
			flag = true;
		}
		if (base.Player.RodSlotToTakeFromPod != -1 || (ControlsController.ControlsActions.RodStandSubmit.WasFastLongPressed && base.Player.InitTakeRodFromPod(true)))
		{
			return typeof(PlayerDrawOut);
		}
		if (!base.Player.IsSailing && (ControlsController.ControlsActions.RodPod.WasReleased || (ControlsController.ControlsActions.RodStandCancel.WasLongPressed && flag) || base.Player.IsDrawRodPodRequest))
		{
			base.Player.IsDrawRodPodRequest = false;
			if (base.Player.InitPickUpRodPod() || base.Player.IsOneMoreRodPodPresent())
			{
				base.Player.IsWithRodPodMode = true;
				return typeof(PlayerDrawOut);
			}
		}
		if (base.Player.IsSailing && !base.Player.IsBoatFishing)
		{
			return typeof(PlayerOnBoat);
		}
		if (base.IsOpenMapRequest)
		{
			base.Player.IsTransitionToMap = true;
			return typeof(PlayerDrawOut);
		}
		if (base.Player.EmptyOnMoveSwitch)
		{
			base.Player.ResetEmptyOnMoveSwitch();
			base.Player.OnDrawOut();
			return typeof(PlayerEmpty);
		}
		base.Player.ResetEmptyOnMoveSwitch();
		if (base.Player.IsRequestedRodDestroying || base.Player.RequestedRod != null || !base.Player.IsReadyForRod || StaticUserData.RodInHand.IsRodDisassembled)
		{
			return typeof(PlayerDrawOut);
		}
		if (base.Player.RequestedFireworkItem != null || (ControlsController.ControlsActions.GetTool.WasPressed && (!flag2 || SettingsManager.InputType != InputModuleManager.InputType.GamePad) && base.Player.RequestFirework()))
		{
			return typeof(PlayerDrawOut);
		}
		if (base.Player.IsCouldBoardBoat())
		{
			return typeof(PlayerDrawOut);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.ShowSleeves = true;
		base.Player.ClearRodPodsHighlighting();
	}
}
