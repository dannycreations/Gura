using System;
using System.Collections.Generic;

public class TutorialStep2_2 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Fire1" });
		ControlsController.ControlsActions.BlockMouseButtons(false, true, true, true);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		ControlsController.ControlsActions.UnBlockInput();
	}
}
