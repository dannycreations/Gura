using System;
using System.Collections.Generic;

public class TutorialStep23 : TutorialStepWithPointer
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		GameFactory.Message.IgnoreMessages = false;
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "ForceMouselook", "Move", "RunHotkey" });
		ControlsController.ControlsActions.BlockMouseButtons(true, false, true, true);
		ShowHudElements.Instance.SetCatchedFishActiveReleaseButton(false);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		ControlsController.ControlsActions.UnBlockInput();
		ShowHudElements.Instance.SetCatchedFishActiveReleaseButton(true);
	}
}
