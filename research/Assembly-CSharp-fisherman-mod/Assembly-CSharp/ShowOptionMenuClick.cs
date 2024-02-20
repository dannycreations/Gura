using System;
using UnityEngine;

public class ShowOptionMenuClick : MonoBehaviour
{
	public void OnClick()
	{
		SceneController.CallAction(ScenesList.GameMenu, SceneStatuses.GotoOptionMenu, this, null);
	}
}
