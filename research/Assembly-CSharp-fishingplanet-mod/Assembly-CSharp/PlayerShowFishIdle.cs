﻿using System;

public class PlayerShowFishIdle : PlayerStateBase
{
	protected override void onEnter()
	{
		ReelTypes reelType = base.Player.ReelType;
		if (reelType != ReelTypes.Spinning)
		{
			if (reelType == ReelTypes.Baitcasting)
			{
				base.Player.PlayAnimation("BaitFishGripIdle", 1f, 1f, 0f);
			}
		}
		else
		{
			base.Player.PlayAnimation("FishGripIdle", 1f, 1f, 0f);
		}
		if (base.Player.Tackle.UnderwaterItem == null)
		{
			ShowHudElements.Instance.ShowCatchedFishWindow(base.Player.IsNewFish);
		}
	}

	protected override Type onUpdate()
	{
		if (!CatchedFishInfoHandler.Instance.IsKeepnet && ((ControlsController.ControlsActions.PHMOpen.IsPressed && !base.Player.IsLookWithFishMode) || base.Player.IsPhotoModeRequested))
		{
			return typeof(PlayerPhotoMode);
		}
		if (!base.Player.Tackle.IsShowing)
		{
			return typeof(PlayerShowFishOut);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.IsNewFish = false;
	}
}
