using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialStep7 : TutorialStepWithPointer
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		ControlsController.ControlsActions.BlockInput(new List<string> { "ForceMouselook", "Move", "RunHotkey" });
		ControlsController.ControlsActions.BlockMouseButtons(true, false, true, true);
		if (this.keepTransform != null)
		{
		}
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		ControlsController.ControlsActions.UnBlockInput();
	}

	[SerializeField]
	private StartStep7_1Trigger trigger;
}
