using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStep19 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		if ((double)StaticUserData.RodInHand.Rod.LeaderLength < 0.6)
		{
			StaticUserData.RodInHand.Rod.LeaderLength = 1.27f;
		}
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Fire1", "Fire2", "Move", "RunHotkey" });
		ControlsController.ControlsActions.BlockMouseButtons(false, false, true, true);
		this.PlaceForCastIllumination.SetActive(true);
		if (this.ChatHidesPanel != null)
		{
			this.ChatHidesPanel.Hide();
		}
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		ControlsController.ControlsActions.UnBlockInput();
		this.PlaceForCastIllumination.SetActive(false);
	}

	public override void StepUpdate()
	{
		if (GameFactory.Player != null && GameFactory.Player.IsTackleThrown && GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Hook.transform.position.y < 0f)
		{
			this.PlaceForCastIllumination.SetActive(false);
		}
		else if (GameFactory.Player.Tackle == null || GameFactory.Player.Tackle.Fish == null)
		{
			this.PlaceForCastIllumination.SetActive(true);
		}
	}

	public HidesPanel ChatHidesPanel;

	public GameObject PlaceForCastIllumination;
}
