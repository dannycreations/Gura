using System;
using System.Collections.Generic;

public class TutorialStep5 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Fire2", "Move", "RunHotkey" });
		ControlsController.ControlsActions.BlockMouseButtons(true, false, true, true);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		ControlsController.ControlsActions.UnBlockInput();
	}
}
