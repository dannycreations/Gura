using System;
using UnityEngine;

public class BackFromRegistration : MonoBehaviour
{
	public void OnClick()
	{
		SceneController.CallAction(ScenesList.Registration, SceneStatuses.Back, this, null);
	}
}
