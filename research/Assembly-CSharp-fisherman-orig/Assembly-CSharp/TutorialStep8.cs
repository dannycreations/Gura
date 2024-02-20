using System;
using System.Collections.Generic;

public class TutorialStep8 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Fire1", "Fire2", "Move", "RunHotkey" });
		ControlsController.ControlsActions.BlockMouseButtons(false, false, true, true);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		ControlsController.ControlsActions.UnBlockInput();
	}
}
