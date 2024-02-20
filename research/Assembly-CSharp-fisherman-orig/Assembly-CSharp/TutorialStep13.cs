using System;

public class TutorialStep13 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		TutorialStep.Shop.SetActiveToolsMenu(true, this.SelectablesForSkip);
		base.MenuCanvas.sortingOrder = 2;
		ControlsController.ControlsActions.BlockInput(null);
	}

	public override void StepUpdate()
	{
		base.StepUpdate();
		TutorialStep.Shop.SetActiveToolsMenu(true, this.SelectablesForSkip);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		TutorialStep.Shop.SetActiveToolsMenu(false, this.SelectablesForSkip);
		base.MenuCanvas.sortingOrder = 0;
		ControlsController.ControlsActions.UnBlockInput();
	}
}
