using System;
using UnityEngine;

public class BackFromLogin : MonoBehaviour
{
	public void OnClick()
	{
		SceneController.CallAction(ScenesList.Login, SceneStatuses.Back, this, null);
	}
}
