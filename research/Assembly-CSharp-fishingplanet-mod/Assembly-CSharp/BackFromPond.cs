using System;
using UnityEngine;

public class BackFromPond : MonoBehaviour
{
	internal void Awake()
	{
		this.timer = 0f;
		this._isLoaded = false;
		if (GameFactory.Player != null)
		{
			GameFactory.Player.StartMoveToNewLocation();
		}
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
	}

	internal void Update()
	{
		this.timer += Time.deltaTime;
		if (!this._isLoaded && this.timer >= this.timerMax)
		{
			this._isLoaded = true;
			PhotonConnectionFactory.Instance.MoveToBase();
		}
	}

	private void OnMoved(TravelDestination destination)
	{
		SceneController.CallAction(ScenesList.Loading, SceneStatuses.GotoLobby, this, null);
	}

	private float timer;

	private float timerMax = 1f;

	private bool _isLoaded = true;
}
