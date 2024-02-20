using System;
using ObjectModel;
using UnityEngine;

public class EventsController : MonoBehaviour
{
	private void Start()
	{
		EventsController.CurrentEvent = null;
	}

	private void Update()
	{
		if (!this._isInited && PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.IsAuthenticated && (PhotonConnectionFactory.Instance.IsConnectedToMaster || PhotonConnectionFactory.Instance.IsConnectedToGameServer))
		{
			this._isInited = true;
			PhotonConnectionFactory.Instance.OnGotCurrentEvent += this.PhotonServerOnGotCurrentEvent;
			PhotonConnectionFactory.Instance.GetCurrentEvent();
		}
	}

	private void PhotonServerOnGotCurrentEvent(MarketingEvent activeEvent)
	{
		PhotonConnectionFactory.Instance.OnGotCurrentEvent -= this.PhotonServerOnGotCurrentEvent;
		EventsController.CurrentEvent = activeEvent;
	}

	private bool _isInited;

	public static MarketingEvent CurrentEvent;
}
