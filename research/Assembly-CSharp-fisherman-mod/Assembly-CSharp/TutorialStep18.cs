using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStep18 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Fire1", "Fire2", "Move", "RunHotkey" });
		ControlsController.ControlsActions.BlockMouseButtons(false, false, true, true);
		if (this.ChatHidesPanel != null)
		{
			this.ChatHidesPanel.Hide();
		}
		this.ZoneActivation.transform.position += this.positionOffset;
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		ControlsController.ControlsActions.UnBlockInput();
	}

	public HidesPanel ChatHidesPanel;

	public Transform ZoneActivation;

	public Vector3 positionOffset;
}
