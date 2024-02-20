using System;
using ObjectModel;

public class ToolIdle : PlayerStateBase
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
		base.onEnter();
		this.AnimationState = base.Player.PlayAnimation("idleBox", 1f, 1f, 0f);
		base.ProcessSavedMenuPage();
		base.Player.CurFirework.IsDestroyingRequested = true;
	}

	protected override Type onUpdate()
	{
		if (ControlsController.ControlsActions.Fire1.WasPressed && base.Player.CanPutFirework())
		{
			if (PhotonConnectionFactory.Instance.CurrentTournamentId != null)
			{
				GameFactory.Message.ShowCantSetupFireworkInTournament();
				return null;
			}
			if (EventsController.CurrentEvent == null || EventsController.CurrentEvent.Config.IsFireworkLaunchEnabled == null || !EventsController.CurrentEvent.Config.IsFireworkLaunchEnabled.Value)
			{
				GameFactory.Message.ShowCantSetupFireworkOutEvent();
				return null;
			}
			if (!EventsController.CurrentEvent.Config.FireworkLaunchPonds.Contains(PhotonConnectionFactory.Instance.Profile.PondId.Value))
			{
				GameFactory.Message.ShowCantSetupFirework();
				return null;
			}
			base.Player.CurFirework.IsDestroyingRequested = false;
			return typeof(ToolSetup);
		}
		else
		{
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
					return typeof(ToolDrawOut);
				}
				base.Player.IsHandThrowMode = false;
			}
			if (ControlsController.ControlsActions.EmptyHands.WasReleased || base.Player.IsEmptyHandsMode)
			{
				base.Player.IsEmptyHandsMode = true;
				return typeof(ToolDrawOut);
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
				return typeof(ToolDrawOut);
			}
			if (ControlsController.ControlsActions.RodPod.WasReleased || (ControlsController.ControlsActions.RodStandCancel.WasLongPressed && flag) || base.Player.IsDrawRodPodRequest)
			{
				base.Player.IsDrawRodPodRequest = false;
				if (base.Player.InitPickUpRodPod() || base.Player.IsOneMoreRodPodPresent())
				{
					base.Player.IsWithRodPodMode = true;
					return typeof(ToolDrawOut);
				}
			}
			if ((ControlsController.ControlsActions.GetTool.WasPressed && (!flag2 || SettingsManager.InputType != InputModuleManager.InputType.GamePad)) || base.Player.RequestedFireworkItem != null || base.Player.RequestedRod != null || base.Player.RodSlotToTakeFromPod != -1)
			{
				return typeof(ToolDrawOut);
			}
			float axis = ControlsController.ControlsActions.GetAxis("Mouse ScrollWheel");
			if ((axis > 0f || ControlsController.ControlsActions.NextTool.WasPressed) && base.Player.RequestNextFirework())
			{
				return typeof(ToolDrawOut);
			}
			if ((axis < 0f || ControlsController.ControlsActions.PrevTool.WasPressed) && base.Player.RequestPrevFirework())
			{
				return typeof(ToolDrawOut);
			}
			return null;
		}
	}
}
