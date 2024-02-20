using System;
using ExitGames.Client.Photon;
using ObjectModel;
using Photon;
using UnityEngine;

internal class PhotonDispatcher : MonoBehaviour
{
	internal void Awake()
	{
		if (PhotonDispatcher.Instance != null && PhotonDispatcher.Instance != this && PhotonDispatcher.Instance.gameObject != null)
		{
			Object.DestroyImmediate(PhotonDispatcher.Instance.gameObject);
		}
		PhotonDispatcher.Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	internal void Update()
	{
		IPhotonServerConnection instance = PhotonConnectionFactory.Instance;
		try
		{
			if (!instance.IsMessageQueueRunning)
			{
				return;
			}
			bool flag = true;
			while (instance.IsMessageQueueRunning && flag)
			{
				AsynchProcessor.ReleaseQueue();
				flag = instance.Peer.DispatchIncomingCommands();
			}
			bool flag2 = true;
			while (instance.IsMessageQueueRunning && flag2)
			{
				flag2 = instance.Peer.SendOutgoingCommands();
			}
			instance.DispatchTimeActions();
			instance.DispatchTravelActions();
			instance.DispatchChatMessages();
			ClientMissionsManager.Instance.UnityUpdateTrackedTasks();
			DateTime? dateTime = this.tokenLastUpdated;
			if (dateTime == null)
			{
				this.tokenLastUpdated = new DateTime?(TimeHelper.UtcTime());
			}
			if (TimeHelper.UtcTime().Subtract(this.tokenLastUpdated.Value).TotalMinutes >= 10.0)
			{
				instance.RefreshSecurityToken();
				this.tokenLastUpdated = new DateTime?(TimeHelper.UtcTime());
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR:\r\n" + ex.Message + "\r\n" + ex.StackTrace);
			PhotonConnectionFactory.Instance.PinError(ex);
		}
		if (Input.GetKeyDown(290))
		{
			this.isConnectionDebugOn = !this.isConnectionDebugOn;
		}
	}

	internal void OnApplicationQuit()
	{
		if (!DisconnectServerAction.IsQuitDisconnect)
		{
			DisconnectServerAction.IsQuitDisconnect = true;
			GracefulDisconnectHandler.Disconnect();
			Application.CancelQuit();
		}
	}

	internal void OnGUI()
	{
		if (!this.isConnectionDebugOn)
		{
			return;
		}
		GUILayout.BeginArea(this.screenRect);
		LoadbalancingPeer peer = PhotonConnectionFactory.Instance.Peer;
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		GUILayout.FlexibleSpace();
		if (peer != null && PhotonConnectionFactory.Instance != null)
		{
			GUILayout.Label(string.Concat(new object[]
			{
				"Server:",
				peer.ServerAddress,
				"\r\nState:",
				peer.PeerState,
				"\r\nOperationsInProgress:",
				OpTimer.OperationsInProgress,
				"\r\nPing:",
				peer.RoundTripTime,
				"ms\r\nAppPing:",
				OpTimer.AvgPing.ToString("#0.000"),
				"/",
				OpTimer.MaxPing.ToString("#0.000"),
				"s\r\nGamePing:",
				OpTimer.GameAvgPing.ToString("#0.000"),
				"/",
				OpTimer.GameMaxPing.ToString("#0.000"),
				"s\r\nFPS:",
				(int)FPSController.GetFps(),
				"\r\nPlayer:",
				PhotonConnectionFactory.Instance.UserName
			}), new GUIStyle
			{
				normal = 
				{
					textColor = Color.black
				}
			}, new GUILayoutOption[0]);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	private DateTime? tokenLastUpdated;

	private const int TokenUpdateTimeout = 10;

	public static PhotonDispatcher Instance;

	private bool isConnectionDebugOn;

	private readonly Rect screenRect = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
}
