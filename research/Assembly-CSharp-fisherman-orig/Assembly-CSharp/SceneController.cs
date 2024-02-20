using System;
using Assets.Scripts.Common.Managers;
using UnityEngine;

public static class SceneController
{
	public static void CallAction(ScenesList currentScene, SceneStatuses currentSceneStatuses, MonoBehaviour sender, object parameters = null)
	{
		if (currentSceneStatuses == SceneStatuses.UnknownError)
		{
			return;
		}
		SceneActionFactory.GetInstance(currentScene).Action(currentSceneStatuses, sender, parameters);
	}
}
