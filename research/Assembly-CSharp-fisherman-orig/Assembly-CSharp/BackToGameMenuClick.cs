using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

public class BackToGameMenuClick : MonoBehaviour
{
	public void OnClick()
	{
		MenuHelpers menuHelpers = new MenuHelpers();
		if (this.currentForm == menuHelpers.MenuPrefabsList.exitMenuForm)
		{
			SceneController.CallAction(ScenesList.GameMenu, SceneStatuses.GotoGameMenuFromExit, this, null);
		}
		if (this.currentForm == menuHelpers.MenuPrefabsList.optionsForm)
		{
			SceneController.CallAction(ScenesList.GameMenu, SceneStatuses.GotoGameMenuFromOption, this, null);
		}
		if (this.currentForm == menuHelpers.MenuPrefabsList.inventoryForm)
		{
			SceneController.CallAction(ScenesList.GlobalMap, SceneStatuses.GotoGameMenuFromInventory, this, null);
		}
		if (this.currentForm == menuHelpers.MenuPrefabsList.shopForm)
		{
			SceneController.CallAction(ScenesList.GlobalMap, SceneStatuses.GotoGameMenuFromShop, this, null);
		}
	}

	public GameObject currentForm;
}
