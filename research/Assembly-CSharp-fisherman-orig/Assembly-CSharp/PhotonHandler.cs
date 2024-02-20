﻿using System;
using System.Collections;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

internal class PhotonHandler : MonoBehaviour, IPhotonPeerListener
{
	protected void Awake()
	{
		if (PhotonHandler.SP != null && PhotonHandler.SP != this && PhotonHandler.SP.gameObject != null)
		{
			Object.DestroyImmediate(PhotonHandler.SP.gameObject);
		}
		PhotonHandler.SP = this;
		Object.DontDestroyOnLoad(base.gameObject);
		this.updateInterval = 1000 / PhotonNetwork.sendRate;
		this.updateIntervalOnSerialize = 1000 / PhotonNetwork.sendRateOnSerialize;
		PhotonHandler.StartFallbackSendAckThread();
		SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnLevelWasLoadedNew);
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnLevelWasLoadedNew);
	}

	protected void OnApplicationQuit()
	{
		PhotonHandler.AppQuits = true;
		PhotonHandler.StopFallbackSendAckThread();
	}

	protected void Update()
	{
		if (PhotonNetwork.networkingPeer == null)
		{
			Debug.LogError("NetworkPeer broke!");
			return;
		}
		if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated || PhotonNetwork.connectionStateDetailed == PeerState.Disconnected || PhotonNetwork.offlineMode)
		{
			return;
		}
		if (!PhotonNetwork.isMessageQueueRunning)
		{
			return;
		}
		bool flag = true;
		while (PhotonNetwork.isMessageQueueRunning && flag)
		{
			flag = PhotonNetwork.networkingPeer.DispatchIncomingCommands();
		}
		int num = (int)(Time.realtimeSinceStartup * 1000f);
		if (PhotonNetwork.isMessageQueueRunning && num > this.nextSendTickCountOnSerialize)
		{
			PhotonNetwork.networkingPeer.RunViewUpdate();
			this.nextSendTickCountOnSerialize = num + this.updateIntervalOnSerialize;
			this.nextSendTickCount = 0;
		}
		num = (int)(Time.realtimeSinceStartup * 1000f);
		if (num > this.nextSendTickCount)
		{
			bool flag2 = true;
			while (PhotonNetwork.isMessageQueueRunning && flag2)
			{
				flag2 = PhotonNetwork.networkingPeer.SendOutgoingCommands();
			}
			this.nextSendTickCount = num + this.updateInterval;
		}
	}

	private void OnLevelWasLoadedNew(Scene scene, LoadSceneMode mode)
	{
		PhotonNetwork.networkingPeer.NewSceneLoaded();
		PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(Application.loadedLevelName);
	}

	protected void OnJoinedRoom()
	{
		PhotonNetwork.networkingPeer.LoadLevelIfSynced();
	}

	protected void OnCreatedRoom()
	{
		PhotonNetwork.networkingPeer.SetLevelInPropsIfSynced(Application.loadedLevelName);
	}

	public static void StartFallbackSendAckThread()
	{
		if (PhotonHandler.sendThreadShouldRun)
		{
			return;
		}
		PhotonHandler.sendThreadShouldRun = true;
		SupportClass.CallInBackground(new Func<bool>(PhotonHandler.FallbackSendAckThread), 100, string.Empty);
	}

	public static void StopFallbackSendAckThread()
	{
		PhotonHandler.sendThreadShouldRun = false;
	}

	public static bool FallbackSendAckThread()
	{
		if (PhotonHandler.sendThreadShouldRun && PhotonNetwork.networkingPeer != null)
		{
			PhotonNetwork.networkingPeer.SendAcksOnly();
		}
		return PhotonHandler.sendThreadShouldRun;
	}

	public void DebugReturn(DebugLevel level, string message)
	{
		if (level == 1)
		{
			Debug.LogError(message);
		}
		else if (level == 2)
		{
			Debug.LogWarning(message);
		}
		else if (level == 3 && PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log(message);
		}
		else if (level == 5 && PhotonNetwork.logLevel == PhotonLogLevel.Full)
		{
			Debug.Log(message);
		}
	}

	public void OnOperationResponse(OperationResponse operationResponse)
	{
	}

	public void OnStatusChanged(StatusCode statusCode)
	{
	}

	public void OnEvent(EventData photonEvent)
	{
	}

	internal static CloudRegionCode BestRegionCodeInPreferences
	{
		get
		{
			string @string = PlayerPrefs.GetString("PUNCloudBestRegion", string.Empty);
			if (!string.IsNullOrEmpty(@string))
			{
				return Region.Parse(@string);
			}
			return CloudRegionCode.none;
		}
		set
		{
			if (value == CloudRegionCode.none)
			{
				PlayerPrefs.DeleteKey("PUNCloudBestRegion");
			}
			else
			{
				PlayerPrefs.SetString("PUNCloudBestRegion", value.ToString());
			}
		}
	}

	protected internal static void PingAvailableRegionsAndConnectToBest()
	{
		PhotonHandler.SP.StartCoroutine(PhotonHandler.SP.PingAvailableRegionsCoroutine(true));
	}

	internal IEnumerator PingAvailableRegionsCoroutine(bool connectToBest)
	{
		PhotonHandler.BestRegionCodeCurrently = CloudRegionCode.none;
		while (PhotonNetwork.networkingPeer.AvailableRegions == null)
		{
			if (PhotonNetwork.connectionStateDetailed != PeerState.ConnectingToNameServer && PhotonNetwork.connectionStateDetailed != PeerState.ConnectedToNameServer)
			{
				Debug.LogError("Call ConnectToNameServer to ping available regions.");
				yield break;
			}
			Debug.Log(string.Concat(new object[]
			{
				"Waiting for AvailableRegions. State: ",
				PhotonNetwork.connectionStateDetailed,
				" Server: ",
				PhotonNetwork.Server,
				" PhotonNetwork.networkingPeer.AvailableRegions ",
				PhotonNetwork.networkingPeer.AvailableRegions != null
			}));
			yield return new WaitForSeconds(0.25f);
		}
		if (PhotonNetwork.networkingPeer.AvailableRegions == null || PhotonNetwork.networkingPeer.AvailableRegions.Count == 0)
		{
			Debug.LogError("No regions available. Are you sure your appid is valid and setup?");
			yield break;
		}
		PhotonPingManager pingManager = new PhotonPingManager();
		foreach (Region region in PhotonNetwork.networkingPeer.AvailableRegions)
		{
			PhotonHandler.SP.StartCoroutine(pingManager.PingSocket(region));
		}
		while (!pingManager.Done)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Region best = pingManager.BestRegion;
		PhotonHandler.BestRegionCodeCurrently = best.Code;
		PhotonHandler.BestRegionCodeInPreferences = best.Code;
		Debug.Log(string.Concat(new object[] { "Found best region: ", best.Code, " ping: ", best.Ping, ". Calling ConnectToRegionMaster() is: ", connectToBest }));
		if (connectToBest)
		{
			PhotonNetwork.networkingPeer.ConnectToRegionMaster(best.Code);
		}
		yield break;
	}

	public static PhotonHandler SP;

	public int updateInterval;

	public int updateIntervalOnSerialize;

	private int nextSendTickCount;

	private int nextSendTickCountOnSerialize;

	private static bool sendThreadShouldRun;

	public static bool AppQuits;

	public static Type PingImplementation;

	private const string PlayerPrefsKey = "PUNCloudBestRegion";

	internal static CloudRegionCode BestRegionCodeCurrently = CloudRegionCode.none;
}
