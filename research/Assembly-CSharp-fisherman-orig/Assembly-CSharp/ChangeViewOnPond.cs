using System;
using UnityEngine;

public class ChangeViewOnPond : MonoBehaviour
{
	public void ShowExitMenu()
	{
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoExitMenu, this, null);
	}

	public void ShowOptionMenu()
	{
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoOptionMenu, this, null);
	}

	public void ShowInventoryMenu()
	{
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoInventoryMenu, this, null);
	}

	public void ShowShopMenu()
	{
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoShopMenu, this, null);
	}

	public void BackFromExitMenu()
	{
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoBackFromExit, this, null);
	}

	public void BackFromOptionMenu()
	{
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoBackFromOption, this, null);
	}

	public void BackFromShop()
	{
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoBackFromShop, this, null);
	}

	public void BackFromInventory()
	{
		SceneController.CallAction(ScenesList.Pond, SceneStatuses.GotoBackFromInventory, this, null);
	}
}
