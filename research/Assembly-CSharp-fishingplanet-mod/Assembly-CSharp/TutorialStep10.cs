using System;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine.UI;

public class TutorialStep10 : TutorialStep
{
	public override void DoStartAction()
	{
		base.DoStartAction();
		this._helper.MenuPrefabsList.topMenuForm.transform.Find("Image/TopMenuLeft/btnMap").GetComponent<Toggle>().isOn = false;
		ControlsController.ControlsActions.BlockKeyboardInput(new List<string> { "Inventory", "InventoryAdditional" });
		ControlsController.ControlsActions.BlockMouseButtons(true, true, true, true);
	}

	public override void DoEndAction()
	{
		base.DoEndAction();
		if (PondControllers.Instance != null && PondControllers.Instance.FirstPerson != null)
		{
			PondControllers.Instance.FirstPerson.GetComponent<CameraWetness>().DisableCameraDrops();
		}
		ControlsController.ControlsActions.UnBlockInput();
	}

	private MenuHelpers _helper = new MenuHelpers();
}
