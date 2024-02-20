using System;
using ObjectModel;
using UnityEngine;

public class PlayerEmpty : PlayerStateBase
{
	private bool IsMoving
	{
		get
		{
			return !Mathf.Approximately(ControlsController.ControlsActions.Move.Y, 0f) && !base.Player.IsSailing;
		}
	}

	public override bool CantOpenInventory
	{
		get
		{
			return false;
		}
	}

	protected override void onEnter()
	{
		base.Player.Update3dCharMecanimParameter(TPMMecanimIParameter.ThrowType, 0);
		this._wasMoving = this.IsMoving;
		base.Player.OnEmpty(this._wasMoving);
		base.ProcessSavedMenuPage();
		if (base.Player.FastUseRodPod)
		{
			base.Player.RequestUseRodPod();
		}
	}

	protected override Type onUpdate()
	{
		if (!base.Player.IsSailing)
		{
			base.Player.UpdateSelectedBoat();
		}
		else if (base.Player.CurrentBoat.IsInTransitionState)
		{
			return null;
		}
		if (base.Player.InFishingZone)
		{
			if (this._isZoomed)
			{
				base.Player.ZoomCamera(true);
			}
			if (ControlsController.ControlsActions.CameraZoom.WasPressed)
			{
				this._isZoomed = !this._isZoomed;
				base.Player.ZoomCamera(this._isZoomed);
			}
		}
		else if (this._isZoomed)
		{
			base.Player.ZoomCamera(false);
		}
		if (base.Player.IsRequestedRodDestroying)
		{
			base.Player.DestroyRod(true);
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
		if (!GameFactory.IsRodAssembling && (base.Player.RodSlotToTakeFromPod != -1 || (ControlsController.ControlsActions.RodStandSubmit.WasFastLongPressed && base.Player.InitTakeRodFromPod(true))))
		{
			base.Player.TakingRod.Behaviour.Tackle.Adapter.FinishAttackOnPod(true, false, false, true, 0f);
			base.Player.IsEmptyHandsMode = false;
			return typeof(TakeRodFromPodIn);
		}
		if (!base.Player.IsSailing && base.Player.InFishingZone)
		{
			bool flag3 = base.Player.RodPodToPickUp != null;
			if (base.Player.IsWithRodPodMode || flag3 || ((ControlsController.ControlsActions.RodPod.WasReleased || base.Player.IsDrawRodPodRequest) && base.Player.IsOneMoreRodPodPresent()) || (((ControlsController.ControlsActions.RodStandCancel.WasLongPressed && flag) || base.Player.IsDrawRodPodRequest) && base.Player.InitPickUpRodPod()))
			{
				base.Player.IsDrawRodPodRequest = false;
				flag3 = base.Player.RodPodToPickUp != null;
				if (!TournamentHelper.IS_IN_TOURNAMENT)
				{
					base.Player.IsWithRodPodMode = true;
					base.Player.IsEmptyHandsMode = false;
					base.Player.WasFirstZoneEntered = true;
					return (!flag3) ? typeof(RodPodIn) : typeof(PickUpRodPodIn);
				}
				if (PhotonConnectionFactory.Instance.Profile.Tournament.EquipmentAllowed.AllowRodStands || flag3)
				{
					base.Player.IsWithRodPodMode = true;
					base.Player.IsEmptyHandsMode = false;
					base.Player.WasFirstZoneEntered = true;
					return (!flag3) ? typeof(RodPodIn) : typeof(PickUpRodPodIn);
				}
				base.Player.IsWithRodPodMode = false;
				this.ShowCantUseRodStand(TournamentHelper.IsCompetition);
			}
		}
		if (base.Player.IsTransitionToMap || base.IsOpenMapRequest)
		{
			base.Player.IsTransitionToMap = false;
			return typeof(ShowMap);
		}
		if ((base.Player.IsSailing && !base.Player.IsBoatFishing) || base.Player.IsCouldBoardBoat())
		{
			return typeof(PlayerOnBoat);
		}
		if (base.Player.RequestedFireworkItem != null || (ControlsController.ControlsActions.GetTool.WasPressed && (!flag2 || SettingsManager.InputType != InputModuleManager.InputType.GamePad) && base.Player.RequestFirework()))
		{
			base.Player.RequestedRod = null;
			base.Player.IsEmptyHandsMode = false;
			return typeof(ToolDrawIn);
		}
		if ((ControlsController.ControlsActions.HandThrow.WasReleased || base.Player.IsHandThrowMode) && base.Player.IsReadyForRod)
		{
			base.Player.IsHandThrowMode = true;
			Chum chum = FeederHelper.FindPreparedChumOnDoll();
			Chum chum2 = FeederHelper.FindPreparedChumInHand();
			if (chum != null || chum2 != null)
			{
				base.Player.OnHandMode(true);
				base.Player.IsEmptyHandsMode = false;
				if (chum2 == null)
				{
					PhotonConnectionFactory.Instance.MoveItemOrCombine(chum, null, StoragePlaces.Hands, true);
					return typeof(HandLoadingOut);
				}
				if (base.IsHandChumLoadingRequired)
				{
					return typeof(HandLoadingIdle);
				}
				return typeof(HandDrawIn);
			}
			else
			{
				base.Player.IsHandThrowMode = false;
			}
		}
		if (base.Player.IsReadyForRod || this._rodInitStarted)
		{
			if (this._rodInitStarted && this.AnimationState != null)
			{
				this.AnimationState.time = 0f;
			}
			GameFactory.RodSlot rodSlot = GameFactory.RodSlots[0];
			if (base.Player.RequestedRod != null)
			{
				rodSlot = GameFactory.RodSlots[base.Player.RequestedRod.Slot];
			}
			if (base.Player.RequestedRod != null && !this._rodInitStarted && (rodSlot.IsEmpty || rodSlot.IsInHands))
			{
				base.Player.CreateRequestedRod();
				this._rodInitStarted = true;
				base.PlayDrawInAnimation(0f);
				this.AnimationState.time = 0f;
			}
			else if (!base.Player.RodSlot.IsRodAssembling && !base.Player.RodSlot.PendingServerOp && !StaticUserData.RodInHand.IsRodDisassembled && !base.Player.CurRodSessionSettings.IsOnPod && !base.Player.IsEmptyHandsMode && base.Player.Rod == null && StaticUserData.RodInHand != null && StaticUserData.RodInHand.Rod != null)
			{
				base.Player.RequestedRod = StaticUserData.RodInHand.Rod;
			}
			else if (!GameFactory.IsRodAssembling && !base.Player.RodSlot.PendingServerOp && !StaticUserData.RodInHand.IsRodDisassembled && !base.Player.CurRodSessionSettings.IsOnPod && !base.Player.IsEmptyHandsMode)
			{
				base.Player.OnDrawIn();
				if (base.IsChumLoadingRequired)
				{
					return typeof(FeederLoadingIdle);
				}
				if (base.ChumClearingRequired)
				{
					return typeof(FeederSkipLoading);
				}
				return typeof(PlayerDrawIn);
			}
		}
		if (this._wasMoving != this.IsMoving && !this._rodInitStarted)
		{
			this._wasMoving = this.IsMoving;
			base.Player.PlayAnimation((!this._wasMoving) ? "empty" : "Walk", 1f, 1f, 0f);
		}
		base.Player.UpdateRodPods();
		return null;
	}

	protected override void onExit()
	{
		base.Player.ZoomCamera(false);
		base.Player.ClearRodPodsHighlighting();
		base.Player.ShowSleeves = true;
	}

	private bool _wasMoving;

	private bool _isZoomed;

	private bool _rodInitStarted;
}
