using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class TutorialStep22 : TutorialStep2
{
	public override void DoStartAction()
	{
		GameFactory.Message.IgnoreMessages = true;
		base.DoStartAction();
		this.PlaceForCastIllumination.SetActive(true);
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Fire1", "Fire2", "Move", "RunHotkey" });
		ControlsController.ControlsActions.BlockMouseButtons(false, false, true, true);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		this.PlaceForCastIllumination.SetActive(false);
		ControlsController.ControlsActions.UnBlockInput();
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

	public Button ReleaseButton;

	public Button TakeButton;
}
