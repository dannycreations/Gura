using System;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;

public class TutorialStep1 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		ShowHudElements.Instance.SetTutorial(true);
		ControlsController.ControlsActions.ResetAndUnblock();
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Move", "RunHotkey" });
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		ControlsController.ControlsActions.UnBlockInput();
	}

	public override bool IsWaitingHintMessage()
	{
		MenuPrefabsList menuPrefabsList = MenuHelpers.Instance.MenuPrefabsList;
		bool flag = menuPrefabsList.loadingFormAS == null || !menuPrefabsList.loadingFormAS.isActive;
		return !flag;
	}
}
