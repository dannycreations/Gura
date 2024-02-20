using System;
using UnityEngine;

public class GotoGlobalMap : MonoBehaviour
{
	private void Start()
	{
		SceneController.CallAction(ScenesList.Loading, SceneStatuses.GotoGame, this, null);
	}
}
