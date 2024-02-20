using System;
using ObjectModel;

public class RodPodIdle : PlayerStateBase
{
	public override bool CantOpenInventory
	{
		get
		{
			return true;
		}
	}

	protected override void onEnter()
	{
		base.ProcessSavedMenuPage();
	}

	protected override Type onUpdate()
	{
		base.Player.UpdateRodPods();
		if (base.Player.RequestedFireworkItem != null)
		{
			base.Player.IsWithRodPodMode = false;
			return typeof(RodPodOut);
		}
		if ((ControlsController.ControlsActions.RodPod.WasReleased || ControlsController.ControlsActions.RodStandCancel.WasLongPressed) && base.Player.InitPickUpRodPod())
		{
			return typeof(RodPodOut);
		}
		if (!base.Player.InFishingZone || !base.Player.IsWithRodPodMode)
		{
			return typeof(RodPodOut);
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
				base.Player.IsWithRodPodMode = false;
				return typeof(RodPodOut);
			}
			base.Player.IsHandThrowMode = false;
		}
		if (ControlsController.ControlsActions.EmptyHands.WasReleased || base.Player.IsEmptyHandsMode || ControlsController.ControlsActions.RodStandCancel.WasClicked)
		{
			base.Player.IsWithRodPodMode = false;
			base.Player.IsEmptyHandsMode = true;
			return typeof(RodPodOut);
		}
		if (ControlsController.ControlsActions.RodStandSubmit.WasPressed)
		{
			if (base.Player.InitTakeRodFromPod(true))
			{
				return typeof(RodPodOut);
			}
			if (base.Player.CuRodPodPutState == RodPodPutState.Good)
			{
				return typeof(PutRodPodIn);
			}
			LogHelper.Error("Impossible to put rod pod here - {0}", new object[] { base.Player.CuRodPodPutState });
		}
		base.Player.UpdateCurRodPod();
		return null;
	}

	protected override void onExit()
	{
		base.Player.ClearRodPodsHighlighting();
	}
}
