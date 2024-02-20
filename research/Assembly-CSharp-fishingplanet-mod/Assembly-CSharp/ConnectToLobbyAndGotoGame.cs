using System;
using System.Collections;
using UnityEngine;

public class ConnectToLobbyAndGotoGame : MonoBehaviour
{
	internal void Awake()
	{
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed += this.OnMoveFailed;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed -= this.OnMoveFailed;
	}

	private void OnMoved(TravelDestination destination)
	{
		base.StartCoroutine(this.LoadPondScene());
	}

	private void OnMoveFailed(Failure moveFailure)
	{
		Debug.LogWarning(moveFailure.FullErrorInfo);
	}

	private IEnumerator LoadPondScene()
	{
		while (!CacheLibrary.AllChachesInited)
		{
			yield return new WaitForSeconds(1f);
		}
		SceneController.CallAction(ScenesList.Loading, SceneStatuses.ConnectedToGlobalMap, this, null);
		yield break;
	}
}
