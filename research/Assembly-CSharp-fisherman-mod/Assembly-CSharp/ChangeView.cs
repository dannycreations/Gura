using System;
using UnityEngine;

public class ChangeView : MonoBehaviour
{
	public void ShowExitMenu()
	{
		SceneController.CallAction(ScenesList.GlobalMap, SceneStatuses.GotoExitMenu, this, null);
	}

	public void ShowOptionMenu()
	{
		SceneController.CallAction(ScenesList.GlobalMap, SceneStatuses.GotoOptionMenu, this, null);
	}

	public void ShowInventoryMenu()
	{
		SceneController.CallAction(ScenesList.GlobalMap, SceneStatuses.GotoInventoryMenu, this, null);
	}

	public void ShowShopMenu()
	{
		SceneController.CallAction(ScenesList.GlobalMap, SceneStatuses.GotoShopMenu, this, null);
	}
}
