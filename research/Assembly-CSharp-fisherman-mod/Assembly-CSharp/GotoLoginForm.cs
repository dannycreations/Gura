using System;
using UnityEngine;

public class GotoLoginForm : MonoBehaviour
{
	public void OnClick()
	{
		SceneController.CallAction(ScenesList.NotLogged, SceneStatuses.GotoLogin, this, null);
	}
}
