using System;
using ObjectModel;
using UnityEngine;

public class HandIdle : PlayerStateBase
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
		base.Player.OnHandMode(true);
		base.Player.PlayAnimation("LureIdle", 1f, 1f, 0f);
		base.Player.ShowSleeves = false;
	}

	protected override Type onUpdate()
	{
		if (!base.Player.IsSailing)
		{
			base.Player.UpdateSelectedBoat();
		}
		if (base.IsOpenMapRequest)
		{
			base.Player.IsTransitionToMap = true;
			return typeof(HandDrawOut);
		}
		if (base.Player.IsSailing && !base.Player.IsBoatFishing)
		{
			base.Player.DestroyChumBall();
			return typeof(PlayerOnBoat);
		}
		if (base.Player.IsCouldBoardBoat())
		{
			return typeof(HandDrawOut);
		}
		if (ControlsController.ControlsActions.EmptyHands.WasReleased || base.Player.IsEmptyHandsMode)
		{
			base.Player.IsEmptyHandsMode = true;
			return typeof(HandDrawOut);
		}
		if (base.Player.RequestedRod != null)
		{
			return typeof(HandDrawOut);
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
			return typeof(HandDrawOut);
		}
		if (ControlsController.ControlsActions.RodPod.WasReleased || (ControlsController.ControlsActions.RodStandCancel.WasLongPressed && flag) || base.Player.IsDrawRodPodRequest)
		{
			base.Player.IsDrawRodPodRequest = false;
			if (base.Player.InitPickUpRodPod() || base.Player.IsOneMoreRodPodPresent())
			{
				base.Player.IsWithRodPodMode = true;
				return typeof(HandDrawOut);
			}
		}
		Chum chum = FeederHelper.FindPreparedChumOnDoll();
		Chum chum2 = FeederHelper.FindPreparedChumInHand();
		if (chum == null && chum2 == null)
		{
			base.Player.IsEmptyHandsMode = true;
			return typeof(HandDrawOut);
		}
		if (chum2 == null)
		{
			PhotonConnectionFactory.Instance.MoveItemOrCombine(chum, null, StoragePlaces.Hands, true);
			return typeof(HandIdleToLoading);
		}
		if (base.IsHandChumLoadingRequired)
		{
			return typeof(HandIdleToLoading);
		}
		if (ControlsController.ControlsActions.Fire1.IsPressed)
		{
			if (this._actionHoldAt < 0f)
			{
				this._actionHoldAt = Time.time;
			}
			else if (Time.time - this._actionHoldAt > 0.15f)
			{
				return typeof(HandThrowIn);
			}
		}
		else
		{
			this._actionHoldAt = -1f;
		}
		if (base.Player.EmptyOnMoveSwitch)
		{
			base.Player.ResetEmptyOnMoveSwitch();
			base.Player.OnDrawOut();
			base.Player.DestroyChumBall();
			return typeof(PlayerEmpty);
		}
		return null;
	}

	private float _actionHoldAt = -1f;
}
