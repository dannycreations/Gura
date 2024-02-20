using System;
using UnityEngine;

public class ShowExitMenuClick : MonoBehaviour
{
	public void OnClick()
	{
		SceneController.CallAction(ScenesList.GameMenu, SceneStatuses.GotoExitMenu, this, null);
	}
}
