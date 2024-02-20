using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExitGames.Client.Photon;
using UnityEngine;

internal class NetworkingPeer : LoadbalancingPeer, IPhotonPeerListener
{
	public NetworkingPeer(IPhotonPeerListener listener, string playername, ConnectionProtocol connectionProtocol)
		: base(listener, connectionProtocol)
	{
		base.Listener = this;
		this.lobby = TypedLobby.Default;
		base.LimitOfUnreliableCommands = 40;
		this.externalListener = listener;
		this.PlayerName = playername;
		this.mLocalActor = new PhotonPlayer(true, -1, this.playername);
		this.AddNewPlayer(this.mLocalActor.ID, this.mLocalActor);
		this.rpcShortcuts = new Dictionary<string, int>(PhotonNetwork.PhotonServerSettings.RpcList.Count);
		for (int i = 0; i < PhotonNetwork.PhotonServerSettings.RpcList.Count; i++)
		{
			string text = PhotonNetwork.PhotonServerSettings.RpcList[i];
			this.rpcShortcuts[text] = i;
		}
		this.State = PeerState.PeerCreated;
	}

	protected internal string mAppVersionPun
	{
		get
		{
			return string.Format("{0}_{1}", this.mAppVersion, "1.51");
		}
	}

	public AuthenticationValues CustomAuthenticationValues { get; set; }

	public string MasterServerAddress { get; protected internal set; }

	public string PlayerName
	{
		get
		{
			return this.playername;
		}
		set
		{
			if (string.IsNullOrEmpty(value) || value.Equals(this.playername))
			{
				return;
			}
			if (this.mLocalActor != null)
			{
				this.mLocalActor.name = value;
			}
			this.playername = value;
			if (this.mCurrentGame != null)
			{
				this.SendPlayerName();
			}
		}
	}

	public PeerState State { get; internal set; }

	public Room mCurrentGame
	{
		get
		{
			if (this.mRoomToGetInto != null && this.mRoomToGetInto.isLocalClientInside)
			{
				return this.mRoomToGetInto;
			}
			return null;
		}
	}

	internal Room mRoomToGetInto { get; set; }

	internal RoomOptions mRoomOptionsForCreate { get; set; }

	internal TypedLobby mRoomToEnterLobby { get; set; }

	public PhotonPlayer mLocalActor { get; internal set; }

	public string mGameserver { get; internal set; }

	public TypedLobby lobby { get; set; }

	public int mPlayersOnMasterCount { get; internal set; }

	public int mGameCount { get; internal set; }

	public int mPlayersInRoomsCount { get; internal set; }

	protected internal ServerConnection server { get; private set; }

	public bool IsUsingNameServer { get; protected internal set; }

	public List<Region> AvailableRegions { get; protected internal set; }

	public CloudRegionCode CloudRegion { get; protected internal set; }

	public bool IsAuthorizeSecretAvailable
	{
		get
		{
			return false;
		}
	}

	public override bool Connect(string serverAddress, string applicationName)
	{
		Debug.LogError("Avoid using this directly. Thanks.");
		return false;
	}

	public bool Connect(string serverAddress, ServerConnection type)
	{
		if (PhotonHandler.AppQuits)
		{
			Debug.LogWarning("Ignoring Connect() because app gets closed. If this is an error, check PhotonHandler.AppQuits.");
			return false;
		}
		if (PhotonNetwork.connectionStateDetailed == PeerState.Disconnecting)
		{
			Debug.LogError("Connect() failed. Can't connect while disconnecting (still). Current state: " + PhotonNetwork.connectionStateDetailed);
			return false;
		}
		bool flag = base.Connect(serverAddress, string.Empty);
		if (flag)
		{
			if (type != ServerConnection.NameServer)
			{
				if (type != ServerConnection.MasterServer)
				{
					if (type == ServerConnection.GameServer)
					{
						this.State = PeerState.ConnectingToGameserver;
					}
				}
				else
				{
					this.State = PeerState.ConnectingToMasterserver;
				}
			}
			else
			{
				this.State = PeerState.ConnectingToNameServer;
			}
		}
		return flag;
	}

	public bool ConnectToNameServer()
	{
		if (PhotonHandler.AppQuits)
		{
			Debug.LogWarning("Ignoring Connect() because app gets closed. If this is an error, check PhotonHandler.AppQuits.");
			return false;
		}
		this.IsUsingNameServer = true;
		this.CloudRegion = CloudRegionCode.none;
		if (this.State == PeerState.ConnectedToNameServer)
		{
			return true;
		}
		string text = this.NameServerAddress;
		if (!text.Contains(":"))
		{
			int num = 0;
			NetworkingPeer.ProtocolToNameServerPort.TryGetValue(base.UsedProtocol, out num);
			text = string.Format("{0}:{1}", text, num);
			Debug.Log(string.Concat(new object[]
			{
				"Server to connect to: ",
				text,
				" settings protocol: ",
				PhotonNetwork.PhotonServerSettings.Protocol
			}));
		}
		if (!base.Connect(text, "ns"))
		{
			return false;
		}
		this.State = PeerState.ConnectingToNameServer;
		return true;
	}

	public bool ConnectToRegionMaster(CloudRegionCode region)
	{
		if (PhotonHandler.AppQuits)
		{
			Debug.LogWarning("Ignoring Connect() because app gets closed. If this is an error, check PhotonHandler.AppQuits.");
			return false;
		}
		this.IsUsingNameServer = true;
		this.CloudRegion = region;
		if (this.State == PeerState.ConnectedToNameServer)
		{
			return this.OpAuthenticate(this.mAppId, this.mAppVersionPun, this.PlayerName, this.CustomAuthenticationValues, region.ToString());
		}
		string text = this.NameServerAddress;
		if (!text.Contains(":"))
		{
			int num = 0;
			NetworkingPeer.ProtocolToNameServerPort.TryGetValue(base.UsedProtocol, out num);
			text = string.Format("{0}:{1}", text, num);
		}
		if (!base.Connect(text, "ns"))
		{
			return false;
		}
		this.State = PeerState.ConnectingToNameServer;
		return true;
	}

	public bool GetRegions()
	{
		if (this.server != ServerConnection.NameServer)
		{
			return false;
		}
		bool flag = this.OpGetRegions(this.mAppId);
		if (flag)
		{
			this.AvailableRegions = null;
		}
		return flag;
	}

	public override void Disconnect()
	{
		if (base.PeerState == null)
		{
			if (!PhotonHandler.AppQuits)
			{
			}
			return;
		}
		this.State = PeerState.Disconnecting;
		base.Disconnect();
	}

	private void DisconnectToReconnect()
	{
		ServerConnection server = this.server;
		if (server != ServerConnection.NameServer)
		{
			if (server != ServerConnection.MasterServer)
			{
				if (server == ServerConnection.GameServer)
				{
					this.State = PeerState.DisconnectingFromGameserver;
					base.Disconnect();
				}
			}
			else
			{
				this.State = PeerState.DisconnectingFromMasterserver;
				base.Disconnect();
			}
		}
		else
		{
			this.State = PeerState.DisconnectingFromNameServer;
			base.Disconnect();
		}
	}

	private void LeftLobbyCleanup()
	{
		this.mGameList = new Dictionary<string, RoomInfo>();
		this.mGameListCopy = new RoomInfo[0];
		if (this.insideLobby)
		{
			this.insideLobby = false;
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnLeftLobby, new object[0]);
		}
	}

	private void LeftRoomCleanup()
	{
		bool flag = this.mRoomToGetInto != null;
		bool flag2 = ((this.mRoomToGetInto == null) ? PhotonNetwork.autoCleanUpPlayerObjects : this.mRoomToGetInto.autoCleanUp);
		this.hasSwitchedMC = false;
		this.mRoomToGetInto = null;
		this.mActors = new Dictionary<int, PhotonPlayer>();
		this.mPlayerListCopy = new PhotonPlayer[0];
		this.mOtherPlayerListCopy = new PhotonPlayer[0];
		this.mMasterClient = null;
		this.allowedReceivingGroups = new HashSet<int>();
		this.blockSendingGroups = new HashSet<int>();
		this.mGameList = new Dictionary<string, RoomInfo>();
		this.mGameListCopy = new RoomInfo[0];
		this.isFetchingFriends = false;
		this.ChangeLocalID(-1);
		if (flag2)
		{
			this.LocalCleanupAnythingInstantiated(true);
			PhotonNetwork.manuallyAllocatedViewIds = new List<int>();
		}
		if (flag)
		{
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnLeftRoom, new object[0]);
		}
	}

	protected internal void LocalCleanupAnythingInstantiated(bool destroyInstantiatedGameObjects)
	{
		if (this.tempInstantiationData.Count > 0)
		{
			Debug.LogWarning("It seems some instantiation is not completed, as instantiation data is used. You should make sure instantiations are paused when calling this method. Cleaning now, despite this.");
		}
		if (destroyInstantiatedGameObjects)
		{
			HashSet<GameObject> hashSet = new HashSet<GameObject>();
			foreach (PhotonView photonView in this.photonViewList.Values)
			{
				if (photonView.isRuntimeInstantiated)
				{
					hashSet.Add(photonView.gameObject);
				}
			}
			foreach (GameObject gameObject in hashSet)
			{
				this.RemoveInstantiatedGO(gameObject, true);
			}
		}
		this.tempInstantiationData.Clear();
		PhotonNetwork.lastUsedViewSubId = 0;
		PhotonNetwork.lastUsedViewSubIdStatic = 0;
	}

	private void ReadoutProperties(Hashtable gameProperties, Hashtable pActorProperties, int targetActorNr)
	{
		if (this.mCurrentGame != null && gameProperties != null)
		{
			this.mCurrentGame.CacheProperties(gameProperties);
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonCustomRoomPropertiesChanged, new object[] { gameProperties });
			if (PhotonNetwork.automaticallySyncScene)
			{
				this.LoadLevelIfSynced();
			}
		}
		if (pActorProperties != null && pActorProperties.Count > 0)
		{
			if (targetActorNr > 0)
			{
				PhotonPlayer playerWithID = this.GetPlayerWithID(targetActorNr);
				if (playerWithID != null)
				{
					Hashtable actorPropertiesForActorNr = this.GetActorPropertiesForActorNr(pActorProperties, targetActorNr);
					playerWithID.InternalCacheProperties(actorPropertiesForActorNr);
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerPropertiesChanged, new object[] { playerWithID, actorPropertiesForActorNr });
				}
			}
			else
			{
				foreach (object obj in pActorProperties.Keys)
				{
					int num = (int)obj;
					Hashtable hashtable = (Hashtable)pActorProperties[obj];
					string text = (string)hashtable[byte.MaxValue];
					PhotonPlayer photonPlayer = this.GetPlayerWithID(num);
					if (photonPlayer == null)
					{
						photonPlayer = new PhotonPlayer(false, num, text);
						this.AddNewPlayer(num, photonPlayer);
					}
					photonPlayer.InternalCacheProperties(hashtable);
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerPropertiesChanged, new object[] { photonPlayer, hashtable });
				}
			}
		}
	}

	private void AddNewPlayer(int ID, PhotonPlayer player)
	{
		if (!this.mActors.ContainsKey(ID))
		{
			this.mActors[ID] = player;
			this.RebuildPlayerListCopies();
		}
		else
		{
			Debug.LogError("Adding player twice: " + ID);
		}
	}

	private void RemovePlayer(int ID, PhotonPlayer player)
	{
		this.mActors.Remove(ID);
		if (!player.isLocal)
		{
			this.RebuildPlayerListCopies();
		}
	}

	private void RebuildPlayerListCopies()
	{
		this.mPlayerListCopy = new PhotonPlayer[this.mActors.Count];
		this.mActors.Values.CopyTo(this.mPlayerListCopy, 0);
		List<PhotonPlayer> list = new List<PhotonPlayer>();
		foreach (PhotonPlayer photonPlayer in this.mPlayerListCopy)
		{
			if (!photonPlayer.isLocal)
			{
				list.Add(photonPlayer);
			}
		}
		this.mOtherPlayerListCopy = list.ToArray();
	}

	private void ResetPhotonViewsOnSerialize()
	{
		foreach (PhotonView photonView in this.photonViewList.Values)
		{
			photonView.lastOnSerializeDataSent = null;
		}
	}

	private void HandleEventLeave(int actorID)
	{
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log("HandleEventLeave for player ID: " + actorID);
		}
		if (actorID < 0 || !this.mActors.ContainsKey(actorID))
		{
			Debug.LogError(string.Format("Received event Leave for unknown player ID: {0}", actorID));
			return;
		}
		PhotonPlayer playerWithID = this.GetPlayerWithID(actorID);
		if (playerWithID == null)
		{
			Debug.LogError("HandleEventLeave for player ID: " + actorID + " has no PhotonPlayer!");
		}
		this.CheckMasterClient(actorID);
		if (this.mCurrentGame != null && this.mCurrentGame.autoCleanUp)
		{
			this.DestroyPlayerObjects(actorID, true);
		}
		this.RemovePlayer(actorID, playerWithID);
		NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerDisconnected, new object[] { playerWithID });
	}

	private void CheckMasterClient(int leavingPlayerId)
	{
		bool flag = this.mMasterClient != null && this.mMasterClient.ID == leavingPlayerId;
		bool flag2 = leavingPlayerId > 0;
		if (flag2 && !flag)
		{
			return;
		}
		if (this.mActors.Count <= 1)
		{
			this.mMasterClient = this.mLocalActor;
		}
		else
		{
			int num = int.MaxValue;
			foreach (int num2 in this.mActors.Keys)
			{
				if (num2 < num && num2 != leavingPlayerId)
				{
					num = num2;
				}
			}
			this.mMasterClient = this.mActors[num];
		}
		if (flag2)
		{
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnMasterClientSwitched, new object[] { this.mMasterClient });
		}
	}

	private static int ReturnLowestPlayerId(PhotonPlayer[] players, int playerIdToIgnore)
	{
		if (players == null || players.Length == 0)
		{
			return -1;
		}
		int num = int.MaxValue;
		foreach (PhotonPlayer photonPlayer in players)
		{
			if (photonPlayer.ID != playerIdToIgnore)
			{
				if (photonPlayer.ID < num)
				{
					num = photonPlayer.ID;
				}
			}
		}
		return num;
	}

	protected internal bool SetMasterClient(int playerId, bool sync)
	{
		bool flag = this.mMasterClient != null && this.mMasterClient.ID != playerId;
		if (!flag || !this.mActors.ContainsKey(playerId))
		{
			return false;
		}
		if (sync)
		{
			byte b = 208;
			Hashtable hashtable = new Hashtable();
			hashtable.Add(1, playerId);
			if (!this.OpRaiseEvent(b, hashtable, true, null))
			{
				return false;
			}
		}
		this.hasSwitchedMC = true;
		this.mMasterClient = this.mActors[playerId];
		NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnMasterClientSwitched, new object[] { this.mMasterClient });
		return true;
	}

	private Hashtable GetActorPropertiesForActorNr(Hashtable actorProperties, int actorNr)
	{
		if (actorProperties.ContainsKey(actorNr))
		{
			return (Hashtable)actorProperties[actorNr];
		}
		return actorProperties;
	}

	private PhotonPlayer GetPlayerWithID(int number)
	{
		if (this.mActors != null && this.mActors.ContainsKey(number))
		{
			return this.mActors[number];
		}
		return null;
	}

	private void SendPlayerName()
	{
		if (this.State == PeerState.Joining)
		{
			this.mPlayernameHasToBeUpdated = true;
			return;
		}
		if (this.mLocalActor != null)
		{
			this.mLocalActor.name = this.PlayerName;
			Hashtable hashtable = new Hashtable();
			hashtable[byte.MaxValue] = this.PlayerName;
			if (this.mLocalActor.ID > 0)
			{
				base.OpSetPropertiesOfActor(this.mLocalActor.ID, hashtable, true, 0, null);
				this.mPlayernameHasToBeUpdated = false;
			}
		}
	}

	private void GameEnteredOnGameServer(OperationResponse operationResponse)
	{
		if (operationResponse.ReturnCode != 0)
		{
			byte operationCode = operationResponse.OperationCode;
			if (operationCode != 227)
			{
				if (operationCode != 226)
				{
					if (operationCode == 225)
					{
						if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
						{
							Debug.Log("Join failed on GameServer. Changing back to MasterServer. Msg: " + operationResponse.DebugMessage);
							if (operationResponse.ReturnCode == 32758)
							{
								Debug.Log("Most likely the game became empty during the switch to GameServer.");
							}
						}
						NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonRandomJoinFailed, new object[] { operationResponse.ReturnCode, operationResponse.DebugMessage });
					}
				}
				else
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.Log("Join failed on GameServer. Changing back to MasterServer. Msg: " + operationResponse.DebugMessage);
						if (operationResponse.ReturnCode == 32758)
						{
							Debug.Log("Most likely the game became empty during the switch to GameServer.");
						}
					}
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonJoinRoomFailed, new object[] { operationResponse.ReturnCode, operationResponse.DebugMessage });
				}
			}
			else
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
				{
					Debug.Log("Create failed on GameServer. Changing back to MasterServer. Msg: " + operationResponse.DebugMessage);
				}
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonCreateRoomFailed, new object[] { operationResponse.ReturnCode, operationResponse.DebugMessage });
			}
			this.DisconnectToReconnect();
			return;
		}
		this.State = PeerState.Joined;
		this.mRoomToGetInto.isLocalClientInside = true;
		Hashtable hashtable = (Hashtable)operationResponse[249];
		Hashtable hashtable2 = (Hashtable)operationResponse[248];
		this.ReadoutProperties(hashtable2, hashtable, 0);
		int num = (int)operationResponse[254];
		this.ChangeLocalID(num);
		this.CheckMasterClient(-1);
		if (this.mPlayernameHasToBeUpdated)
		{
			this.SendPlayerName();
		}
		byte operationCode2 = operationResponse.OperationCode;
		if (operationCode2 != 227)
		{
			if (operationCode2 != 226 && operationCode2 != 225)
			{
			}
		}
		else
		{
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom, new object[0]);
		}
	}

	private Hashtable GetLocalActorProperties()
	{
		if (PhotonNetwork.player != null)
		{
			return PhotonNetwork.player.allProperties;
		}
		Hashtable hashtable = new Hashtable();
		hashtable[byte.MaxValue] = this.PlayerName;
		return hashtable;
	}

	public void ChangeLocalID(int newID)
	{
		if (this.mLocalActor == null)
		{
			Debug.LogWarning(string.Format("Local actor is null or not in mActors! mLocalActor: {0} mActors==null: {1} newID: {2}", this.mLocalActor, this.mActors == null, newID));
		}
		if (this.mActors.ContainsKey(this.mLocalActor.ID))
		{
			this.mActors.Remove(this.mLocalActor.ID);
		}
		this.mLocalActor.InternalChangeLocalID(newID);
		this.mActors[this.mLocalActor.ID] = this.mLocalActor;
		this.RebuildPlayerListCopies();
	}

	public bool OpCreateGame(string roomName, RoomOptions roomOptions, TypedLobby typedLobby)
	{
		bool flag = this.server == ServerConnection.GameServer;
		if (!flag)
		{
			this.mRoomOptionsForCreate = roomOptions;
			this.mRoomToGetInto = new Room(roomName, roomOptions);
			this.mRoomToEnterLobby = typedLobby ?? ((!this.insideLobby) ? null : this.lobby);
		}
		this.mLastJoinType = JoinType.CreateGame;
		return base.OpCreateRoom(roomName, roomOptions, this.mRoomToEnterLobby, this.GetLocalActorProperties(), flag);
	}

	public bool OpJoinRoom(string roomName, RoomOptions roomOptions, TypedLobby typedLobby, bool createIfNotExists)
	{
		bool flag = this.server == ServerConnection.GameServer;
		if (!flag)
		{
			this.mRoomOptionsForCreate = roomOptions;
			this.mRoomToGetInto = new Room(roomName, roomOptions);
			this.mRoomToEnterLobby = null;
			if (createIfNotExists)
			{
				this.mRoomToEnterLobby = typedLobby ?? ((!this.insideLobby) ? null : this.lobby);
			}
		}
		this.mLastJoinType = ((!createIfNotExists) ? JoinType.JoinGame : JoinType.JoinOrCreateOnDemand);
		return base.OpJoinRoom(roomName, roomOptions, this.mRoomToEnterLobby, createIfNotExists, this.GetLocalActorProperties(), flag);
	}

	public override bool OpJoinRandomRoom(Hashtable expectedCustomRoomProperties, byte expectedMaxPlayers, Hashtable playerProperties, MatchmakingMode matchingType, TypedLobby typedLobby, string sqlLobbyFilter)
	{
		this.mRoomToGetInto = new Room(null, null);
		this.mRoomToEnterLobby = null;
		this.mLastJoinType = JoinType.JoinRandomGame;
		return base.OpJoinRandomRoom(expectedCustomRoomProperties, expectedMaxPlayers, playerProperties, matchingType, typedLobby, sqlLobbyFilter);
	}

	public virtual bool OpLeave()
	{
		if (this.State != PeerState.Joined)
		{
			Debug.LogWarning("Not sending leave operation. State is not 'Joined': " + this.State);
			return false;
		}
		return this.OpCustom(254, null, true, 0, false);
	}

	public override bool OpRaiseEvent(byte eventCode, object customEventContent, bool sendReliable, RaiseEventOptions raiseEventOptions)
	{
		return !PhotonNetwork.offlineMode && base.OpRaiseEvent(eventCode, customEventContent, sendReliable, raiseEventOptions);
	}

	public void DebugReturn(DebugLevel level, string message)
	{
		this.externalListener.DebugReturn(level, message);
	}

	public void OnOperationResponse(OperationResponse operationResponse)
	{
		if (PhotonNetwork.networkingPeer.State == PeerState.Disconnecting)
		{
			if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
			{
				Debug.Log("OperationResponse ignored while disconnecting. Code: " + operationResponse.OperationCode);
			}
			return;
		}
		if (operationResponse.ReturnCode == 0)
		{
			if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
			{
				Debug.Log(operationResponse.ToString());
			}
		}
		else if (operationResponse.ReturnCode == -3)
		{
			Debug.LogError("Operation " + operationResponse.OperationCode + " could not be executed (yet). Wait for state JoinedLobby or ConnectedToMaster and their callbacks before calling operations. WebRPCs need a server-side configuration. Enum OperationCode helps identify the operation.");
		}
		else if (operationResponse.ReturnCode == 32752)
		{
			Debug.LogError(string.Concat(new object[] { "Operation ", operationResponse.OperationCode, " failed in a server-side plugin. Check the configuration in the Dashboard. Message from server-plugin: ", operationResponse.DebugMessage }));
		}
		else if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.LogError(string.Concat(new object[]
			{
				"Operation failed: ",
				operationResponse.ToStringFull(),
				" Server: ",
				this.server
			}));
		}
		if (operationResponse.Parameters.ContainsKey(221))
		{
			if (this.CustomAuthenticationValues == null)
			{
				this.CustomAuthenticationValues = new AuthenticationValues();
				this.CustomAuthenticationValues.Secret = (string)operationResponse.Parameters[221];
			}
			this.CustomAuthenticationValues.Secret = operationResponse[221] as string;
		}
		byte operationCode = operationResponse.OperationCode;
		switch (operationCode)
		{
		case 219:
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnWebRpcResponse, new object[] { operationResponse });
			break;
		case 220:
		{
			if (operationResponse.ReturnCode == 32767)
			{
				Debug.LogError(string.Format("The appId this client sent is unknown on the server (Cloud). Check settings. If using the Cloud, check account.", new object[0]));
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, new object[] { DisconnectCause.InvalidAuthentication });
				this.State = PeerState.Disconnecting;
				this.Disconnect();
				return;
			}
			string[] array = operationResponse[210] as string[];
			string[] array2 = operationResponse[230] as string[];
			if (array == null || array2 == null || array.Length != array2.Length)
			{
				Debug.LogError("The region arrays from Name Server are not ok. Must be non-null and same length.");
			}
			else
			{
				this.AvailableRegions = new List<Region>(array.Length);
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					if (!string.IsNullOrEmpty(text))
					{
						text = text.ToLower();
						CloudRegionCode cloudRegionCode = Region.Parse(text);
						this.AvailableRegions.Add(new Region
						{
							Code = cloudRegionCode,
							HostAndPort = array2[i]
						});
					}
				}
				if (PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.BestRegion)
				{
					PhotonHandler.PingAvailableRegionsAndConnectToBest();
				}
			}
			break;
		}
		default:
			switch (operationCode)
			{
			case 251:
			{
				Hashtable hashtable = (Hashtable)operationResponse[249];
				Hashtable hashtable2 = (Hashtable)operationResponse[248];
				this.ReadoutProperties(hashtable2, hashtable, 0);
				break;
			}
			case 252:
				break;
			case 253:
				break;
			case 254:
				this.DisconnectToReconnect();
				break;
			default:
				Debug.LogWarning(string.Format("OperationResponse unhandled: {0}", operationResponse.ToString()));
				break;
			}
			break;
		case 222:
		{
			bool[] array3 = operationResponse[1] as bool[];
			string[] array4 = operationResponse[2] as string[];
			if (array3 != null && array4 != null && this.friendListRequested != null && array3.Length == this.friendListRequested.Length)
			{
				List<FriendInfo> list = new List<FriendInfo>(this.friendListRequested.Length);
				for (int j = 0; j < this.friendListRequested.Length; j++)
				{
					list.Insert(j, new FriendInfo
					{
						Name = this.friendListRequested[j],
						Room = array4[j],
						IsOnline = array3[j]
					});
				}
				PhotonNetwork.Friends = list;
			}
			else
			{
				Debug.LogError("FindFriends failed to apply the result, as a required value wasn't provided or the friend list length differed from result.");
			}
			this.friendListRequested = null;
			this.isFetchingFriends = false;
			this.friendListTimestamp = Environment.TickCount;
			if (this.friendListTimestamp == 0)
			{
				this.friendListTimestamp = 1;
			}
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnUpdatedFriendList, new object[0]);
			break;
		}
		case 225:
			if (operationResponse.ReturnCode != 0)
			{
				if (operationResponse.ReturnCode == 32760)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
					{
						Debug.Log("JoinRandom failed: No open game. Calling: OnPhotonRandomJoinFailed() and staying on master server.");
					}
				}
				else if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
				{
					Debug.LogWarning(string.Format("JoinRandom failed: {0}.", operationResponse.ToStringFull()));
				}
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonRandomJoinFailed, new object[] { operationResponse.ReturnCode, operationResponse.DebugMessage });
			}
			else
			{
				string text2 = (string)operationResponse[byte.MaxValue];
				this.mRoomToGetInto.name = text2;
				this.mGameserver = (string)operationResponse[230];
				this.DisconnectToReconnect();
			}
			break;
		case 226:
			if (this.server != ServerConnection.GameServer)
			{
				if (operationResponse.ReturnCode != 0)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.Log(string.Format("JoinRoom failed (room maybe closed by now). Client stays on masterserver: {0}. State: {1}", operationResponse.ToStringFull(), this.State));
					}
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonJoinRoomFailed, new object[0]);
				}
				else
				{
					this.mGameserver = (string)operationResponse[230];
					this.DisconnectToReconnect();
				}
			}
			else
			{
				this.GameEnteredOnGameServer(operationResponse);
			}
			break;
		case 227:
			if (this.server == ServerConnection.GameServer)
			{
				this.GameEnteredOnGameServer(operationResponse);
			}
			else if (operationResponse.ReturnCode != 0)
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
				{
					Debug.LogWarning(string.Format("CreateRoom failed, client stays on masterserver: {0}.", operationResponse.ToStringFull()));
				}
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonCreateRoomFailed, new object[] { operationResponse.ReturnCode, operationResponse.DebugMessage });
			}
			else
			{
				string text3 = (string)operationResponse[byte.MaxValue];
				if (!string.IsNullOrEmpty(text3))
				{
					this.mRoomToGetInto.name = text3;
				}
				this.mGameserver = (string)operationResponse[230];
				this.DisconnectToReconnect();
			}
			break;
		case 228:
			this.State = PeerState.Authenticated;
			this.LeftLobbyCleanup();
			break;
		case 229:
			this.State = PeerState.JoinedLobby;
			this.insideLobby = true;
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnJoinedLobby, new object[0]);
			break;
		case 230:
			if (operationResponse.ReturnCode != 0)
			{
				if (operationResponse.ReturnCode == -2)
				{
					Debug.LogError(string.Format("If you host Photon yourself, make sure to start the 'Instance LoadBalancing' " + base.ServerAddress, new object[0]));
				}
				else if (operationResponse.ReturnCode == 32767)
				{
					Debug.LogError(string.Format("The appId this client sent is unknown on the server (Cloud). Check settings. If using the Cloud, check account.", new object[0]));
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, new object[] { DisconnectCause.InvalidAuthentication });
				}
				else if (operationResponse.ReturnCode == 32755)
				{
					Debug.LogError(string.Format("Custom Authentication failed (either due to user-input or configuration or AuthParameter string format). Calling: OnCustomAuthenticationFailed()", new object[0]));
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnCustomAuthenticationFailed, new object[] { operationResponse.DebugMessage });
				}
				else
				{
					Debug.LogError(string.Format("Authentication failed: '{0}' Code: {1}", operationResponse.DebugMessage, operationResponse.ReturnCode));
				}
				this.State = PeerState.Disconnecting;
				this.Disconnect();
				if (operationResponse.ReturnCode == 32757)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.LogWarning(string.Format("Currently, the limit of users is reached for this title. Try again later. Disconnecting", new object[0]));
					}
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonMaxCccuReached, new object[0]);
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, new object[] { DisconnectCause.MaxCcuReached });
				}
				else if (operationResponse.ReturnCode == 32756)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.LogError(string.Format("The used master server address is not available with the subscription currently used. Got to Photon Cloud Dashboard or change URL. Disconnecting.", new object[0]));
					}
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, new object[] { DisconnectCause.InvalidRegion });
				}
				else if (operationResponse.ReturnCode == 32753)
				{
					if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
					{
						Debug.LogError(string.Format("The authentication ticket expired. You need to connect (and authenticate) again. Disconnecting.", new object[0]));
					}
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, new object[] { DisconnectCause.AuthenticationTicketExpired });
				}
			}
			else if (this.server == ServerConnection.NameServer)
			{
				this.MasterServerAddress = operationResponse[230] as string;
				this.DisconnectToReconnect();
			}
			else if (this.server == ServerConnection.MasterServer)
			{
				if (PhotonNetwork.autoJoinLobby)
				{
					this.State = PeerState.Authenticated;
					this.OpJoinLobby(this.lobby);
				}
				else
				{
					this.State = PeerState.ConnectedToMaster;
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectedToMaster, new object[0]);
				}
			}
			else if (this.server == ServerConnection.GameServer)
			{
				this.State = PeerState.Joining;
				if (this.mLastJoinType == JoinType.JoinGame || this.mLastJoinType == JoinType.JoinRandomGame || this.mLastJoinType == JoinType.JoinOrCreateOnDemand)
				{
					this.OpJoinRoom(this.mRoomToGetInto.name, this.mRoomOptionsForCreate, this.mRoomToEnterLobby, this.mLastJoinType == JoinType.JoinOrCreateOnDemand);
				}
				else if (this.mLastJoinType == JoinType.CreateGame)
				{
					this.OpCreateGame(this.mRoomToGetInto.name, this.mRoomOptionsForCreate, this.mRoomToEnterLobby);
				}
			}
			break;
		}
		this.externalListener.OnOperationResponse(operationResponse);
	}

	protected internal int FriendsListAge
	{
		get
		{
			return (!this.isFetchingFriends && this.friendListTimestamp != 0) ? (Environment.TickCount - this.friendListTimestamp) : 0;
		}
	}

	public override bool OpFindFriends(string[] friendsToFind)
	{
		if (this.isFetchingFriends)
		{
			return false;
		}
		this.friendListRequested = friendsToFind;
		this.isFetchingFriends = true;
		return base.OpFindFriends(friendsToFind);
	}

	public void OnStatusChanged(StatusCode statusCode)
	{
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log(string.Format("OnStatusChanged: {0}", statusCode.ToString()));
		}
		switch (statusCode)
		{
		case 1022:
		case 1023:
		{
			this.State = PeerState.PeerCreated;
			if (this.CustomAuthenticationValues != null)
			{
				this.CustomAuthenticationValues.Secret = null;
			}
			DisconnectCause disconnectCause = statusCode;
			NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, new object[] { disconnectCause });
			break;
		}
		case 1024:
			if (this.State == PeerState.ConnectingToNameServer)
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
				{
					Debug.Log("Connected to NameServer.");
				}
				this.server = ServerConnection.NameServer;
				if (this.CustomAuthenticationValues != null)
				{
					this.CustomAuthenticationValues.Secret = null;
				}
			}
			if (this.State == PeerState.ConnectingToGameserver)
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
				{
					Debug.Log("Connected to gameserver.");
				}
				this.server = ServerConnection.GameServer;
				this.State = PeerState.ConnectedToGameserver;
			}
			if (this.State == PeerState.ConnectingToMasterserver)
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
				{
					Debug.Log("Connected to masterserver.");
				}
				this.server = ServerConnection.MasterServer;
				this.State = PeerState.ConnectedToMaster;
				if (this.IsInitialConnect)
				{
					this.IsInitialConnect = false;
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectedToPhoton, new object[0]);
				}
			}
			base.EstablishEncryption();
			if (this.IsAuthorizeSecretAvailable)
			{
				this.didAuthenticate = this.OpAuthenticate(this.mAppId, this.mAppVersionPun, this.PlayerName, this.CustomAuthenticationValues, this.CloudRegion.ToString());
				if (this.didAuthenticate)
				{
					this.State = PeerState.Authenticating;
				}
			}
			break;
		case 1025:
			this.didAuthenticate = false;
			this.isFetchingFriends = false;
			if (this.server == ServerConnection.GameServer)
			{
				this.LeftRoomCleanup();
			}
			if (this.server == ServerConnection.MasterServer)
			{
				this.LeftLobbyCleanup();
			}
			if (this.State == PeerState.DisconnectingFromMasterserver)
			{
				if (this.Connect(this.mGameserver, ServerConnection.GameServer))
				{
					this.State = PeerState.ConnectingToGameserver;
				}
			}
			else if (this.State == PeerState.DisconnectingFromGameserver || this.State == PeerState.DisconnectingFromNameServer)
			{
				if (this.Connect(this.MasterServerAddress, ServerConnection.MasterServer))
				{
					this.State = PeerState.ConnectingToMasterserver;
				}
			}
			else
			{
				if (this.CustomAuthenticationValues != null)
				{
					this.CustomAuthenticationValues.Secret = null;
				}
				this.State = PeerState.PeerCreated;
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnDisconnectedFromPhoton, new object[0]);
			}
			break;
		case 1026:
			if (this.IsInitialConnect)
			{
				Debug.LogError("Exception while connecting to: " + base.ServerAddress + ". Check if the server is available.");
				if (base.ServerAddress == null || base.ServerAddress.StartsWith("127.0.0.1"))
				{
					Debug.LogWarning("The server address is 127.0.0.1 (localhost): Make sure the server is running on this machine. Android and iOS emulators have their own localhost.");
					if (base.ServerAddress == this.mGameserver)
					{
						Debug.LogWarning("This might be a misconfiguration in the game server config. You need to edit it to a (public) address.");
					}
				}
				this.State = PeerState.PeerCreated;
				DisconnectCause disconnectCause = statusCode;
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, new object[] { disconnectCause });
			}
			else
			{
				this.State = PeerState.PeerCreated;
				DisconnectCause disconnectCause = statusCode;
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, new object[] { disconnectCause });
			}
			this.Disconnect();
			break;
		default:
			switch (statusCode)
			{
			case 1039:
			case 1040:
			case 1042:
			case 1043:
				if (this.IsInitialConnect)
				{
					Debug.LogWarning(string.Concat(new object[] { statusCode, " while connecting to: ", base.ServerAddress, ". Check if the server is available." }));
					DisconnectCause disconnectCause = statusCode;
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnFailedToConnectToPhoton, new object[] { disconnectCause });
				}
				else
				{
					DisconnectCause disconnectCause = statusCode;
					NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnConnectionFail, new object[] { disconnectCause });
				}
				if (this.CustomAuthenticationValues != null)
				{
					this.CustomAuthenticationValues.Secret = null;
				}
				this.Disconnect();
				goto IL_51E;
			case 1048:
				if (this.server == ServerConnection.NameServer)
				{
					this.State = PeerState.ConnectedToNameServer;
					if (!this.didAuthenticate && this.CloudRegion == CloudRegionCode.none)
					{
						this.OpGetRegions(this.mAppId);
					}
				}
				if (!this.didAuthenticate && (!this.IsUsingNameServer || this.CloudRegion != CloudRegionCode.none))
				{
					this.didAuthenticate = this.OpAuthenticate(this.mAppId, this.mAppVersionPun, this.PlayerName, this.CustomAuthenticationValues, this.CloudRegion.ToString());
					if (this.didAuthenticate)
					{
						this.State = PeerState.Authenticating;
					}
				}
				goto IL_51E;
			case 1049:
				Debug.LogError("Encryption wasn't established: " + statusCode + ". Going to authenticate anyways.");
				this.OpAuthenticate(this.mAppId, this.mAppVersionPun, this.PlayerName, this.CustomAuthenticationValues, this.CloudRegion.ToString());
				goto IL_51E;
			}
			Debug.LogError("Received unknown status code: " + statusCode);
			break;
		case 1030:
			break;
		}
		IL_51E:
		this.externalListener.OnStatusChanged(statusCode);
	}

	public void OnEvent(EventData photonEvent)
	{
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log(string.Format("OnEvent: {0}", photonEvent.ToString()));
		}
		int num = -1;
		PhotonPlayer photonPlayer = null;
		if (photonEvent.Parameters.ContainsKey(254))
		{
			num = (int)photonEvent[254];
			if (this.mActors.ContainsKey(num))
			{
				photonPlayer = this.mActors[num];
			}
		}
		byte code = photonEvent.Code;
		switch (code)
		{
		case 201:
		case 206:
		{
			Hashtable hashtable = (Hashtable)photonEvent[245];
			int num2 = (int)hashtable[0];
			short num3 = -1;
			short num4 = 1;
			if (hashtable.ContainsKey(1))
			{
				num3 = (short)hashtable[1];
				num4 = 2;
			}
			short num5 = num4;
			while ((int)num5 < hashtable.Count)
			{
				this.OnSerializeRead(hashtable[num5] as Hashtable, photonPlayer, num2, num3);
				num5 += 1;
			}
			break;
		}
		case 202:
			this.DoInstantiate((Hashtable)photonEvent[245], photonPlayer, null);
			break;
		case 203:
			if (photonPlayer == null || !photonPlayer.isMasterClient)
			{
				Debug.LogError("Error: Someone else(" + photonPlayer + ") then the masterserver requests a disconnect!");
			}
			else
			{
				PhotonNetwork.LeaveRoom();
			}
			break;
		case 204:
		{
			Hashtable hashtable2 = (Hashtable)photonEvent[245];
			int num6 = (int)hashtable2[0];
			PhotonView photonView = null;
			if (this.photonViewList.TryGetValue(num6, out photonView))
			{
				this.RemoveInstantiatedGO(photonView.gameObject, true);
			}
			else if (this.DebugOut >= 1)
			{
				Debug.LogError(string.Concat(new object[] { "Ev Destroy Failed. Could not find PhotonView with instantiationId ", num6, ". Sent by actorNr: ", num }));
			}
			break;
		}
		default:
			switch (code)
			{
			case 226:
				this.mPlayersInRoomsCount = (int)photonEvent[229];
				this.mPlayersOnMasterCount = (int)photonEvent[227];
				this.mGameCount = (int)photonEvent[228];
				break;
			default:
				switch (code)
				{
				case 253:
				{
					int num7 = (int)photonEvent[253];
					Hashtable hashtable3 = null;
					Hashtable hashtable4 = null;
					if (num7 == 0)
					{
						hashtable3 = (Hashtable)photonEvent[251];
					}
					else
					{
						hashtable4 = (Hashtable)photonEvent[251];
					}
					this.ReadoutProperties(hashtable3, hashtable4, num7);
					break;
				}
				case 254:
					this.HandleEventLeave(num);
					break;
				case 255:
				{
					Hashtable hashtable5 = (Hashtable)photonEvent[249];
					if (photonPlayer == null)
					{
						bool flag = this.mLocalActor.ID == num;
						this.AddNewPlayer(num, new PhotonPlayer(flag, num, hashtable5));
						this.ResetPhotonViewsOnSerialize();
					}
					if (num == this.mLocalActor.ID)
					{
						int[] array = (int[])photonEvent[252];
						foreach (int num8 in array)
						{
							if (this.mLocalActor.ID != num8 && !this.mActors.ContainsKey(num8))
							{
								this.AddNewPlayer(num8, new PhotonPlayer(false, num8, string.Empty));
							}
						}
						if (this.mLastJoinType == JoinType.JoinOrCreateOnDemand && this.mLocalActor.ID == 1)
						{
							NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnCreatedRoom, new object[0]);
						}
						NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnJoinedRoom, new object[0]);
					}
					else
					{
						NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnPhotonPlayerConnected, new object[] { this.mActors[num] });
					}
					break;
				}
				default:
					if (photonEvent.Code < 200 && PhotonNetwork.OnEventCall != null)
					{
						object obj = photonEvent[245];
						PhotonNetwork.OnEventCall(photonEvent.Code, obj, num);
					}
					else
					{
						Debug.LogError("Error. Unhandled event: " + photonEvent);
					}
					break;
				}
				break;
			case 228:
				break;
			case 229:
			{
				Hashtable hashtable6 = (Hashtable)photonEvent[222];
				foreach (DictionaryEntry dictionaryEntry in hashtable6)
				{
					string text = (string)dictionaryEntry.Key;
					RoomInfo roomInfo = new RoomInfo(text, (Hashtable)dictionaryEntry.Value);
					if (roomInfo.removedFromList)
					{
						this.mGameList.Remove(text);
					}
					else
					{
						this.mGameList[text] = roomInfo;
					}
				}
				this.mGameListCopy = new RoomInfo[this.mGameList.Count];
				this.mGameList.Values.CopyTo(this.mGameListCopy, 0);
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnReceivedRoomListUpdate, new object[0]);
				break;
			}
			case 230:
			{
				this.mGameList = new Dictionary<string, RoomInfo>();
				Hashtable hashtable7 = (Hashtable)photonEvent[222];
				foreach (DictionaryEntry dictionaryEntry2 in hashtable7)
				{
					string text2 = (string)dictionaryEntry2.Key;
					this.mGameList[text2] = new RoomInfo(text2, (Hashtable)dictionaryEntry2.Value);
				}
				this.mGameListCopy = new RoomInfo[this.mGameList.Count];
				this.mGameList.Values.CopyTo(this.mGameListCopy, 0);
				NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnReceivedRoomListUpdate, new object[0]);
				break;
			}
			}
			break;
		case 207:
		{
			Hashtable hashtable2 = (Hashtable)photonEvent[245];
			int num9 = (int)hashtable2[0];
			if (num9 >= 0)
			{
				this.DestroyPlayerObjects(num9, true);
			}
			else
			{
				if (this.DebugOut >= 3)
				{
					Debug.Log("Ev DestroyAll! By PlayerId: " + num);
				}
				this.DestroyAll(true);
			}
			break;
		}
		case 208:
		{
			Hashtable hashtable2 = (Hashtable)photonEvent[245];
			int num10 = (int)hashtable2[1];
			this.SetMasterClient(num10, false);
			break;
		}
		case 209:
		{
			int[] array3 = (int[])photonEvent.Parameters[245];
			int num11 = array3[0];
			int num12 = array3[1];
			Debug.Log(string.Concat(new object[]
			{
				"Ev OwnershipRequest: ",
				photonEvent.Parameters.ToStringFull(),
				" ViewID: ",
				num11,
				" from: ",
				num12,
				" Time: ",
				Environment.TickCount % 1000
			}));
			PhotonView photonView2 = PhotonView.Find(num11);
			if (photonView2 == null)
			{
				Debug.LogWarning("Can't find PhotonView of incoming OwnershipRequest. ViewId not found: " + num11);
			}
			else
			{
				Debug.Log(string.Concat(new object[]
				{
					"Ev OwnershipRequest PhotonView.ownershipTransfer: ",
					photonView2.ownershipTransfer,
					" .ownerId: ",
					photonView2.ownerId,
					" isOwnerActive: ",
					photonView2.isOwnerActive,
					". This client's player: ",
					PhotonNetwork.player.ToStringFull()
				}));
				switch (photonView2.ownershipTransfer)
				{
				case OwnershipOption.Fixed:
					Debug.LogWarning("Ownership mode == fixed. Ignoring request.");
					break;
				case OwnershipOption.Takeover:
					if (num12 == photonView2.ownerId)
					{
						photonView2.ownerId = num;
					}
					break;
				case OwnershipOption.Request:
					if ((num12 == PhotonNetwork.player.ID || PhotonNetwork.player.isMasterClient) && (photonView2.ownerId == PhotonNetwork.player.ID || (PhotonNetwork.player.isMasterClient && !photonView2.isOwnerActive)))
					{
						NetworkingPeer.SendMonoMessage(PhotonNetworkingMessage.OnOwnershipRequest, new object[] { photonView2, photonPlayer });
					}
					break;
				}
			}
			break;
		}
		case 210:
		{
			int[] array4 = (int[])photonEvent.Parameters[245];
			Debug.Log(string.Concat(new object[]
			{
				"Ev OwnershipTransfer. ViewID ",
				array4[0],
				" to: ",
				array4[1],
				" Time: ",
				Environment.TickCount % 1000
			}));
			int num13 = array4[0];
			int num14 = array4[1];
			PhotonView photonView3 = PhotonView.Find(num13);
			photonView3.ownerId = num14;
			break;
		}
		}
		this.externalListener.OnEvent(photonEvent);
	}

	private void SendVacantViewIds()
	{
		Debug.Log("SendVacantViewIds()");
		List<int> list = new List<int>();
		foreach (PhotonView photonView in this.photonViewList.Values)
		{
			if (!photonView.isOwnerActive)
			{
				list.Add(photonView.viewID);
			}
		}
		Debug.Log("Sending vacant view IDs. Length: " + list.Count);
		this.OpRaiseEvent(211, list.ToArray(), true, null);
	}

	public static void SendMonoMessage(PhotonNetworkingMessage methodString, params object[] parameters)
	{
		HashSet<GameObject> hashSet;
		if (PhotonNetwork.SendMonoMessageTargets != null)
		{
			hashSet = PhotonNetwork.SendMonoMessageTargets;
		}
		else
		{
			hashSet = PhotonNetwork.FindGameObjectsWithComponent(PhotonNetwork.SendMonoMessageTargetType);
		}
		string text = methodString.ToString();
		object obj = ((parameters == null || parameters.Length != 1) ? parameters : parameters[0]);
		foreach (GameObject gameObject in hashSet)
		{
			gameObject.SendMessage(text, obj, 1);
		}
	}

	private bool CheckTypeMatch(ParameterInfo[] methodParameters, Type[] callParameterTypes)
	{
		if (methodParameters.Length < callParameterTypes.Length)
		{
			return false;
		}
		for (int i = 0; i < callParameterTypes.Length; i++)
		{
			Type parameterType = methodParameters[i].ParameterType;
			if (callParameterTypes[i] != null && !parameterType.Equals(callParameterTypes[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal Hashtable SendInstantiate(string prefabName, Vector3 position, Quaternion rotation, int group, int[] viewIDs, object[] data, bool isGlobalObject)
	{
		int num = viewIDs[0];
		Hashtable hashtable = new Hashtable();
		hashtable[0] = prefabName;
		if (position != Vector3.zero)
		{
			hashtable[1] = position;
		}
		if (rotation != Quaternion.identity)
		{
			hashtable[2] = rotation;
		}
		if (group != 0)
		{
			hashtable[3] = group;
		}
		if (viewIDs.Length > 1)
		{
			hashtable[4] = viewIDs;
		}
		if (data != null)
		{
			hashtable[5] = data;
		}
		if (this.currentLevelPrefix > 0)
		{
			hashtable[8] = this.currentLevelPrefix;
		}
		hashtable[6] = base.ServerTimeInMilliSeconds;
		hashtable[7] = num;
		this.OpRaiseEvent(202, hashtable, true, new RaiseEventOptions
		{
			CachingOption = ((!isGlobalObject) ? EventCaching.AddToRoomCache : EventCaching.AddToRoomCacheGlobal)
		});
		return hashtable;
	}

	internal GameObject DoInstantiate(Hashtable evData, PhotonPlayer photonPlayer, GameObject resourceGameObject)
	{
		string text = (string)evData[0];
		int num = (int)evData[6];
		int num2 = (int)evData[7];
		Vector3 vector;
		if (evData.ContainsKey(1))
		{
			vector = (Vector3)evData[1];
		}
		else
		{
			vector = Vector3.zero;
		}
		Quaternion quaternion = Quaternion.identity;
		if (evData.ContainsKey(2))
		{
			quaternion = (Quaternion)evData[2];
		}
		int num3 = 0;
		if (evData.ContainsKey(3))
		{
			num3 = (int)evData[3];
		}
		short num4 = 0;
		if (evData.ContainsKey(8))
		{
			num4 = (short)evData[8];
		}
		int[] array;
		if (evData.ContainsKey(4))
		{
			array = (int[])evData[4];
		}
		else
		{
			array = new int[] { num2 };
		}
		object[] array2;
		if (evData.ContainsKey(5))
		{
			array2 = (object[])evData[5];
		}
		else
		{
			array2 = null;
		}
		if (num3 != 0 && !this.allowedReceivingGroups.Contains(num3))
		{
			return null;
		}
		if (resourceGameObject == null)
		{
			if (!NetworkingPeer.UsePrefabCache || !NetworkingPeer.PrefabCache.TryGetValue(text, out resourceGameObject))
			{
				resourceGameObject = (GameObject)Resources.Load(text, typeof(GameObject));
				if (NetworkingPeer.UsePrefabCache)
				{
					NetworkingPeer.PrefabCache.Add(text, resourceGameObject);
				}
			}
			if (resourceGameObject == null)
			{
				Debug.LogError("PhotonNetwork error: Could not Instantiate the prefab [" + text + "]. Please verify you have this gameobject in a Resources folder.");
				return null;
			}
		}
		PhotonView[] photonViewsInChildren = resourceGameObject.GetPhotonViewsInChildren();
		if (photonViewsInChildren.Length != array.Length)
		{
			throw new Exception("Error in Instantiation! The resource's PhotonView count is not the same as in incoming data.");
		}
		for (int i = 0; i < array.Length; i++)
		{
			photonViewsInChildren[i].viewID = array[i];
			photonViewsInChildren[i].prefix = (int)num4;
			photonViewsInChildren[i].instantiationId = num2;
			photonViewsInChildren[i].isRuntimeInstantiated = true;
		}
		this.StoreInstantiationData(num2, array2);
		GameObject gameObject = Object.Instantiate<GameObject>(resourceGameObject, vector, quaternion);
		for (int j = 0; j < array.Length; j++)
		{
			photonViewsInChildren[j].viewID = 0;
			photonViewsInChildren[j].prefix = -1;
			photonViewsInChildren[j].prefixBackup = -1;
			photonViewsInChildren[j].instantiationId = -1;
			photonViewsInChildren[j].isRuntimeInstantiated = false;
		}
		this.RemoveInstantiationData(num2);
		gameObject.SendMessage(PhotonNetworkingMessage.OnPhotonInstantiate.ToString(), new PhotonMessageInfo(photonPlayer, num, null), 1);
		return gameObject;
	}

	private void StoreInstantiationData(int instantiationId, object[] instantiationData)
	{
		this.tempInstantiationData[instantiationId] = instantiationData;
	}

	public object[] FetchInstantiationData(int instantiationId)
	{
		object[] array = null;
		if (instantiationId == 0)
		{
			return null;
		}
		this.tempInstantiationData.TryGetValue(instantiationId, out array);
		return array;
	}

	private void RemoveInstantiationData(int instantiationId)
	{
		this.tempInstantiationData.Remove(instantiationId);
	}

	public void DestroyPlayerObjects(int playerId, bool localOnly)
	{
		if (playerId <= 0)
		{
			Debug.LogError("Failed to Destroy objects of playerId: " + playerId);
			return;
		}
		if (!localOnly)
		{
			this.OpRemoveFromServerInstantiationsOfPlayer(playerId);
			this.OpCleanRpcBuffer(playerId);
			this.SendDestroyOfPlayer(playerId);
		}
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		foreach (PhotonView photonView in this.photonViewList.Values)
		{
			if (photonView.CreatorActorNr == playerId)
			{
				hashSet.Add(photonView.gameObject);
			}
		}
		foreach (GameObject gameObject in hashSet)
		{
			this.RemoveInstantiatedGO(gameObject, true);
		}
		foreach (PhotonView photonView2 in this.photonViewList.Values)
		{
			if (photonView2.ownerId == playerId)
			{
				photonView2.ownerId = photonView2.CreatorActorNr;
			}
		}
	}

	public void DestroyAll(bool localOnly)
	{
		if (!localOnly)
		{
			this.OpRemoveCompleteCache();
			this.SendDestroyOfAll();
		}
		this.LocalCleanupAnythingInstantiated(true);
	}

	protected internal void RemoveInstantiatedGO(GameObject go, bool localOnly)
	{
		if (go == null)
		{
			Debug.LogError("Failed to 'network-remove' GameObject because it's null.");
			return;
		}
		PhotonView[] componentsInChildren = go.GetComponentsInChildren<PhotonView>(true);
		if (componentsInChildren == null || componentsInChildren.Length <= 0)
		{
			Debug.LogError("Failed to 'network-remove' GameObject because has no PhotonView components: " + go);
			return;
		}
		PhotonView photonView = componentsInChildren[0];
		int creatorActorNr = photonView.CreatorActorNr;
		int instantiationId = photonView.instantiationId;
		if (!localOnly)
		{
			if (!photonView.isMine)
			{
				Debug.LogError("Failed to 'network-remove' GameObject. Client is neither owner nor masterClient taking over for owner who left: " + photonView);
				return;
			}
			if (instantiationId < 1)
			{
				Debug.LogError("Failed to 'network-remove' GameObject because it is missing a valid InstantiationId on view: " + photonView + ". Not Destroying GameObject or PhotonViews!");
				return;
			}
		}
		if (!localOnly)
		{
			this.ServerCleanInstantiateAndDestroy(instantiationId, creatorActorNr, photonView.isRuntimeInstantiated);
		}
		for (int i = componentsInChildren.Length - 1; i >= 0; i--)
		{
			PhotonView photonView2 = componentsInChildren[i];
			if (!(photonView2 == null))
			{
				if (photonView2.instantiationId >= 1)
				{
					this.LocalCleanPhotonView(photonView2);
				}
				if (!localOnly)
				{
					this.OpCleanRpcBuffer(photonView2);
				}
			}
		}
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
		{
			Debug.Log("Network destroy Instantiated GO: " + go.name);
		}
		Object.Destroy(go);
	}

	public int GetInstantiatedObjectsId(GameObject go)
	{
		int num = -1;
		if (go == null)
		{
			Debug.LogError("GetInstantiatedObjectsId() for GO == null.");
			return num;
		}
		PhotonView[] photonViewsInChildren = go.GetPhotonViewsInChildren();
		if (photonViewsInChildren != null && photonViewsInChildren.Length > 0 && photonViewsInChildren[0] != null)
		{
			return photonViewsInChildren[0].instantiationId;
		}
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log("GetInstantiatedObjectsId failed for GO: " + go);
		}
		return num;
	}

	private void ServerCleanInstantiateAndDestroy(int instantiateId, int creatorId, bool isRuntimeInstantiated)
	{
		Hashtable hashtable = new Hashtable();
		hashtable[7] = instantiateId;
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions
		{
			CachingOption = EventCaching.RemoveFromRoomCache,
			TargetActors = new int[] { creatorId }
		};
		this.OpRaiseEvent(202, hashtable, true, raiseEventOptions);
		Hashtable hashtable2 = new Hashtable();
		hashtable2[0] = instantiateId;
		raiseEventOptions = null;
		if (!isRuntimeInstantiated)
		{
			raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.CachingOption = EventCaching.AddToRoomCacheGlobal;
			Debug.Log("Destroying GO as global. ID: " + instantiateId);
		}
		this.OpRaiseEvent(204, hashtable2, true, raiseEventOptions);
	}

	private void SendDestroyOfPlayer(int actorNr)
	{
		Hashtable hashtable = new Hashtable();
		hashtable[0] = actorNr;
		this.OpRaiseEvent(207, hashtable, true, null);
	}

	private void SendDestroyOfAll()
	{
		Hashtable hashtable = new Hashtable();
		hashtable[0] = -1;
		this.OpRaiseEvent(207, hashtable, true, null);
	}

	private void OpRemoveFromServerInstantiationsOfPlayer(int actorNr)
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions
		{
			CachingOption = EventCaching.RemoveFromRoomCache,
			TargetActors = new int[] { actorNr }
		};
		this.OpRaiseEvent(202, null, true, raiseEventOptions);
	}

	protected internal void RequestOwnership(int viewID, int fromOwner)
	{
		Debug.Log(string.Concat(new object[]
		{
			"RequestOwnership(): ",
			viewID,
			" from: ",
			fromOwner,
			" Time: ",
			Environment.TickCount % 1000
		}));
		this.OpRaiseEvent(209, new int[] { viewID, fromOwner }, true, new RaiseEventOptions
		{
			Receivers = ReceiverGroup.All
		});
	}

	protected internal void TransferOwnership(int viewID, int playerID)
	{
		Debug.Log(string.Concat(new object[]
		{
			"TransferOwnership() view ",
			viewID,
			" to: ",
			playerID,
			" Time: ",
			Environment.TickCount % 1000
		}));
		this.OpRaiseEvent(210, new int[] { viewID, playerID }, true, new RaiseEventOptions
		{
			Receivers = ReceiverGroup.All
		});
	}

	public void LocalCleanPhotonView(PhotonView view)
	{
		view.destroyedByPhotonNetworkOrQuit = true;
		this.photonViewList.Remove(view.viewID);
	}

	public PhotonView GetPhotonView(int viewID)
	{
		PhotonView photonView = null;
		this.photonViewList.TryGetValue(viewID, out photonView);
		if (photonView == null)
		{
			PhotonView[] array = Object.FindObjectsOfType(typeof(PhotonView)) as PhotonView[];
			foreach (PhotonView photonView2 in array)
			{
				if (photonView2.viewID == viewID)
				{
					if (photonView2.didAwake)
					{
						Debug.LogWarning("Had to lookup view that wasn't in photonViewList: " + photonView2);
					}
					return photonView2;
				}
			}
		}
		return photonView;
	}

	public void RegisterPhotonView(PhotonView netView)
	{
		if (!Application.isPlaying)
		{
			this.photonViewList = new Dictionary<int, PhotonView>();
			return;
		}
		if (netView.viewID == 0)
		{
			Debug.Log("PhotonView register is ignored, because viewID is 0. No id assigned yet to: " + netView);
			return;
		}
		if (this.photonViewList.ContainsKey(netView.viewID))
		{
			if (!(netView != this.photonViewList[netView.viewID]))
			{
				return;
			}
			Debug.LogError(string.Format("PhotonView ID duplicate found: {0}. New: {1} old: {2}. Maybe one wasn't destroyed on scene load?! Check for 'DontDestroyOnLoad'. Destroying old entry, adding new.", netView.viewID, netView, this.photonViewList[netView.viewID]));
			this.RemoveInstantiatedGO(this.photonViewList[netView.viewID].gameObject, true);
		}
		this.photonViewList.Add(netView.viewID, netView);
		if (PhotonNetwork.logLevel >= PhotonLogLevel.Full)
		{
			Debug.Log("Registered PhotonView: " + netView.viewID);
		}
	}

	public void OpCleanRpcBuffer(int actorNumber)
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions
		{
			CachingOption = EventCaching.RemoveFromRoomCache,
			TargetActors = new int[] { actorNumber }
		};
		this.OpRaiseEvent(200, null, true, raiseEventOptions);
	}

	public void OpRemoveCompleteCacheOfPlayer(int actorNumber)
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions
		{
			CachingOption = EventCaching.RemoveFromRoomCache,
			TargetActors = new int[] { actorNumber }
		};
		this.OpRaiseEvent(0, null, true, raiseEventOptions);
	}

	public void OpRemoveCompleteCache()
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions
		{
			CachingOption = EventCaching.RemoveFromRoomCache,
			Receivers = ReceiverGroup.MasterClient
		};
		this.OpRaiseEvent(0, null, true, raiseEventOptions);
	}

	private void RemoveCacheOfLeftPlayers()
	{
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		dictionary[244] = 0;
		dictionary[247] = 7;
		this.OpCustom(253, dictionary, true, 0, false);
	}

	public void CleanRpcBufferIfMine(PhotonView view)
	{
		if (view.ownerId != this.mLocalActor.ID && !this.mLocalActor.isMasterClient)
		{
			Debug.LogError(string.Concat(new object[] { "Cannot remove cached RPCs on a PhotonView thats not ours! ", view.owner, " scene: ", view.isSceneView }));
			return;
		}
		this.OpCleanRpcBuffer(view);
	}

	public void OpCleanRpcBuffer(PhotonView view)
	{
		Hashtable hashtable = new Hashtable();
		hashtable[0] = view.viewID;
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions
		{
			CachingOption = EventCaching.RemoveFromRoomCache
		};
		this.OpRaiseEvent(200, hashtable, true, raiseEventOptions);
	}

	public void RemoveRPCsInGroup(int group)
	{
		foreach (KeyValuePair<int, PhotonView> keyValuePair in this.photonViewList)
		{
			PhotonView value = keyValuePair.Value;
			if (value.group == group)
			{
				this.CleanRpcBufferIfMine(value);
			}
		}
	}

	public void SetLevelPrefix(short prefix)
	{
		this.currentLevelPrefix = prefix;
	}

	public void SetReceivingEnabled(int group, bool enabled)
	{
		if (group <= 0)
		{
			Debug.LogError("Error: PhotonNetwork.SetReceivingEnabled was called with an illegal group number: " + group + ". The group number should be at least 1.");
			return;
		}
		if (enabled)
		{
			if (!this.allowedReceivingGroups.Contains(group))
			{
				this.allowedReceivingGroups.Add(group);
				byte[] array = new byte[] { (byte)group };
				this.OpChangeGroups(null, array);
			}
		}
		else if (this.allowedReceivingGroups.Contains(group))
		{
			this.allowedReceivingGroups.Remove(group);
			byte[] array2 = new byte[] { (byte)group };
			this.OpChangeGroups(array2, null);
		}
	}

	public void SetReceivingEnabled(int[] enableGroups, int[] disableGroups)
	{
		List<byte> list = new List<byte>();
		List<byte> list2 = new List<byte>();
		if (enableGroups != null)
		{
			foreach (int num in enableGroups)
			{
				if (num <= 0)
				{
					Debug.LogError("Error: PhotonNetwork.SetReceivingEnabled was called with an illegal group number: " + num + ". The group number should be at least 1.");
				}
				else if (!this.allowedReceivingGroups.Contains(num))
				{
					this.allowedReceivingGroups.Add(num);
					list.Add((byte)num);
				}
			}
		}
		if (disableGroups != null)
		{
			foreach (int num2 in disableGroups)
			{
				if (num2 <= 0)
				{
					Debug.LogError("Error: PhotonNetwork.SetReceivingEnabled was called with an illegal group number: " + num2 + ". The group number should be at least 1.");
				}
				else if (list.Contains((byte)num2))
				{
					Debug.LogError("Error: PhotonNetwork.SetReceivingEnabled disableGroups contains a group that is also in the enableGroups: " + num2 + ".");
				}
				else if (this.allowedReceivingGroups.Contains(num2))
				{
					this.allowedReceivingGroups.Remove(num2);
					list2.Add((byte)num2);
				}
			}
		}
		this.OpChangeGroups((list2.Count <= 0) ? null : list2.ToArray(), (list.Count <= 0) ? null : list.ToArray());
	}

	public void SetSendingEnabled(int group, bool enabled)
	{
		if (!enabled)
		{
			this.blockSendingGroups.Add(group);
		}
		else
		{
			this.blockSendingGroups.Remove(group);
		}
	}

	public void SetSendingEnabled(int[] enableGroups, int[] disableGroups)
	{
		if (enableGroups != null)
		{
			foreach (int num in enableGroups)
			{
				if (this.blockSendingGroups.Contains(num))
				{
					this.blockSendingGroups.Remove(num);
				}
			}
		}
		if (disableGroups != null)
		{
			foreach (int num2 in disableGroups)
			{
				if (!this.blockSendingGroups.Contains(num2))
				{
					this.blockSendingGroups.Add(num2);
				}
			}
		}
	}

	public void NewSceneLoaded()
	{
		if (this.loadingLevelAndPausedNetwork)
		{
			this.loadingLevelAndPausedNetwork = false;
			PhotonNetwork.isMessageQueueRunning = true;
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, PhotonView> keyValuePair in this.photonViewList)
		{
			PhotonView value = keyValuePair.Value;
			if (value == null)
			{
				list.Add(keyValuePair.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			int num = list[i];
			this.photonViewList.Remove(num);
		}
		if (list.Count > 0 && PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
		{
			Debug.Log("New level loaded. Removed " + list.Count + " scene view IDs from last level.");
		}
	}

	public void RunViewUpdate()
	{
		if (!PhotonNetwork.connected || PhotonNetwork.offlineMode)
		{
			return;
		}
		if (this.mActors == null || this.mActors.Count <= 1)
		{
			return;
		}
		this.dataPerGroupReliable.Clear();
		this.dataPerGroupUnreliable.Clear();
		foreach (KeyValuePair<int, PhotonView> keyValuePair in this.photonViewList)
		{
			PhotonView value = keyValuePair.Value;
			if (value.synchronization != ViewSynchronization.Off)
			{
				if (value.isMine)
				{
					if (value.gameObject.activeInHierarchy)
					{
						if (!this.blockSendingGroups.Contains(value.group))
						{
							Hashtable hashtable = this.OnSerializeWrite(value);
							if (hashtable != null)
							{
								if (value.synchronization == ViewSynchronization.ReliableDeltaCompressed || value.mixedModeIsReliable)
								{
									if (hashtable.ContainsKey(1) || hashtable.ContainsKey(2))
									{
										if (!this.dataPerGroupReliable.ContainsKey(value.group))
										{
											this.dataPerGroupReliable[value.group] = new Hashtable();
											this.dataPerGroupReliable[value.group][0] = base.ServerTimeInMilliSeconds;
											if (this.currentLevelPrefix >= 0)
											{
												this.dataPerGroupReliable[value.group][1] = this.currentLevelPrefix;
											}
										}
										Hashtable hashtable2 = this.dataPerGroupReliable[value.group];
										hashtable2.Add((short)hashtable2.Count, hashtable);
									}
								}
								else
								{
									if (!this.dataPerGroupUnreliable.ContainsKey(value.group))
									{
										this.dataPerGroupUnreliable[value.group] = new Hashtable();
										this.dataPerGroupUnreliable[value.group][0] = base.ServerTimeInMilliSeconds;
										if (this.currentLevelPrefix >= 0)
										{
											this.dataPerGroupUnreliable[value.group][1] = this.currentLevelPrefix;
										}
									}
									Hashtable hashtable3 = this.dataPerGroupUnreliable[value.group];
									hashtable3.Add((short)hashtable3.Count, hashtable);
								}
							}
						}
					}
				}
			}
		}
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		foreach (KeyValuePair<int, Hashtable> keyValuePair2 in this.dataPerGroupReliable)
		{
			raiseEventOptions.InterestGroup = (byte)keyValuePair2.Key;
			this.OpRaiseEvent(206, keyValuePair2.Value, true, raiseEventOptions);
		}
		foreach (KeyValuePair<int, Hashtable> keyValuePair3 in this.dataPerGroupUnreliable)
		{
			raiseEventOptions.InterestGroup = (byte)keyValuePair3.Key;
			this.OpRaiseEvent(201, keyValuePair3.Value, false, raiseEventOptions);
		}
	}

	private Hashtable OnSerializeWrite(PhotonView view)
	{
		PhotonStream photonStream = new PhotonStream(true, null);
		PhotonMessageInfo photonMessageInfo = new PhotonMessageInfo(this.mLocalActor, base.ServerTimeInMilliSeconds, view);
		view.SerializeView(photonStream, photonMessageInfo);
		if (photonStream.Count == 0)
		{
			return null;
		}
		object[] array = photonStream.data.ToArray();
		if (view.synchronization == ViewSynchronization.UnreliableOnChange)
		{
			if (this.AlmostEquals(array, view.lastOnSerializeDataSent))
			{
				if (view.mixedModeIsReliable)
				{
					return null;
				}
				view.mixedModeIsReliable = true;
				view.lastOnSerializeDataSent = array;
			}
			else
			{
				view.mixedModeIsReliable = false;
				view.lastOnSerializeDataSent = array;
			}
		}
		Hashtable hashtable = new Hashtable();
		hashtable[0] = view.viewID;
		hashtable[1] = array;
		if (view.synchronization == ViewSynchronization.ReliableDeltaCompressed)
		{
			bool flag = this.DeltaCompressionWrite(view, hashtable);
			view.lastOnSerializeDataSent = array;
			if (!flag)
			{
				return null;
			}
		}
		return hashtable;
	}

	private void OnSerializeRead(Hashtable data, PhotonPlayer sender, int networkTime, short correctPrefix)
	{
		int num = (int)data[0];
		PhotonView photonView = this.GetPhotonView(num);
		if (photonView == null)
		{
			Debug.LogWarning(string.Concat(new object[] { "Received OnSerialization for view ID ", num, ". We have no such PhotonView! Ignored this if you're leaving a room. State: ", this.State }));
			return;
		}
		if (photonView.prefix > 0 && (int)correctPrefix != photonView.prefix)
		{
			Debug.LogError(string.Concat(new object[] { "Received OnSerialization for view ID ", num, " with prefix ", correctPrefix, ". Our prefix is ", photonView.prefix }));
			return;
		}
		if (photonView.group != 0 && !this.allowedReceivingGroups.Contains(photonView.group))
		{
			return;
		}
		if (photonView.synchronization == ViewSynchronization.ReliableDeltaCompressed)
		{
			if (!this.DeltaCompressionRead(photonView, data))
			{
				if (PhotonNetwork.logLevel >= PhotonLogLevel.Informational)
				{
					Debug.Log(string.Concat(new object[] { "Skipping packet for ", photonView.name, " [", photonView.viewID, "] as we haven't received a full packet for delta compression yet. This is OK if it happens for the first few frames after joining a game." }));
				}
				return;
			}
			photonView.lastOnSerializeDataReceived = data[1] as object[];
		}
		if (sender.ID != photonView.ownerId && (!photonView.isSceneView || !sender.isMasterClient))
		{
			Debug.Log(string.Concat(new object[] { "Adjusting owner to sender of updates. From: ", photonView.ownerId, " to: ", sender.ID }));
			photonView.ownerId = sender.ID;
		}
		object[] array = data[1] as object[];
		PhotonStream photonStream = new PhotonStream(false, array);
		PhotonMessageInfo photonMessageInfo = new PhotonMessageInfo(sender, networkTime, photonView);
		photonView.DeserializeView(photonStream, photonMessageInfo);
	}

	private bool AlmostEquals(object[] lastData, object[] currentContent)
	{
		if (lastData == null && currentContent == null)
		{
			return true;
		}
		if (lastData == null || currentContent == null || lastData.Length != currentContent.Length)
		{
			return false;
		}
		for (int i = 0; i < currentContent.Length; i++)
		{
			object obj = currentContent[i];
			object obj2 = lastData[i];
			if (!this.ObjectIsSameWithInprecision(obj, obj2))
			{
				return false;
			}
		}
		return true;
	}

	private bool DeltaCompressionWrite(PhotonView view, Hashtable data)
	{
		if (view.lastOnSerializeDataSent == null)
		{
			return true;
		}
		object[] lastOnSerializeDataSent = view.lastOnSerializeDataSent;
		object[] array = data[1] as object[];
		if (array == null)
		{
			return false;
		}
		if (lastOnSerializeDataSent.Length != array.Length)
		{
			return true;
		}
		object[] array2 = new object[array.Length];
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < array2.Length; i++)
		{
			object obj = array[i];
			object obj2 = lastOnSerializeDataSent[i];
			if (this.ObjectIsSameWithInprecision(obj, obj2))
			{
				num++;
			}
			else
			{
				array2[i] = array[i];
				if (obj == null)
				{
					list.Add(i);
				}
			}
		}
		if (num > 0)
		{
			data.Remove(1);
			if (num == array.Length)
			{
				return false;
			}
			data[2] = array2;
			if (list.Count > 0)
			{
				data[3] = list.ToArray();
			}
		}
		return true;
	}

	private bool DeltaCompressionRead(PhotonView view, Hashtable data)
	{
		if (data.ContainsKey(1))
		{
			return true;
		}
		if (view.lastOnSerializeDataReceived == null)
		{
			return false;
		}
		object[] array = data[2] as object[];
		if (array == null)
		{
			return false;
		}
		int[] array2 = data[3] as int[];
		if (array2 == null)
		{
			array2 = new int[0];
		}
		object[] lastOnSerializeDataReceived = view.lastOnSerializeDataReceived;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == null && !array2.Contains(i))
			{
				object obj = lastOnSerializeDataReceived[i];
				array[i] = obj;
			}
		}
		data[1] = array;
		return true;
	}

	private bool ObjectIsSameWithInprecision(object one, object two)
	{
		if (one == null || two == null)
		{
			return one == null && two == null;
		}
		if (!one.Equals(two))
		{
			if (one is Vector3)
			{
				Vector3 vector = (Vector3)one;
				Vector3 vector2 = (Vector3)two;
				if (vector.AlmostEquals(vector2, PhotonNetwork.precisionForVectorSynchronization))
				{
					return true;
				}
			}
			else if (one is Vector2)
			{
				Vector2 vector3 = (Vector2)one;
				Vector2 vector4 = (Vector2)two;
				if (vector3.AlmostEquals(vector4, PhotonNetwork.precisionForVectorSynchronization))
				{
					return true;
				}
			}
			else if (one is Quaternion)
			{
				Quaternion quaternion = (Quaternion)one;
				Quaternion quaternion2 = (Quaternion)two;
				if (quaternion.AlmostEquals(quaternion2, PhotonNetwork.precisionForQuaternionSynchronization))
				{
					return true;
				}
			}
			else if (one is float)
			{
				float num = (float)one;
				float num2 = (float)two;
				if (num.AlmostEquals(num2, PhotonNetwork.precisionForFloatSynchronization))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	protected internal static bool GetMethod(MonoBehaviour monob, string methodType, out MethodInfo mi)
	{
		mi = null;
		if (monob == null || string.IsNullOrEmpty(methodType))
		{
			return false;
		}
		List<MethodInfo> methods = SupportClass.GetMethods(monob.GetType(), null);
		for (int i = 0; i < methods.Count; i++)
		{
			MethodInfo methodInfo = methods[i];
			if (methodInfo.Name.Equals(methodType))
			{
				mi = methodInfo;
				return true;
			}
		}
		return false;
	}

	protected internal void LoadLevelIfSynced()
	{
		if (!PhotonNetwork.automaticallySyncScene || PhotonNetwork.isMasterClient || PhotonNetwork.room == null)
		{
			return;
		}
		if (!PhotonNetwork.room.customProperties.ContainsKey("curScn"))
		{
			return;
		}
		object obj = PhotonNetwork.room.customProperties["curScn"];
		if (obj is int)
		{
			if (Application.loadedLevel != (int)obj)
			{
				PhotonNetwork.LoadLevel((int)obj);
			}
		}
		else if (obj is string && Application.loadedLevelName != (string)obj)
		{
			PhotonNetwork.LoadLevel((string)obj);
		}
	}

	protected internal void SetLevelInPropsIfSynced(object levelId)
	{
		if (!PhotonNetwork.automaticallySyncScene || !PhotonNetwork.isMasterClient || PhotonNetwork.room == null)
		{
			return;
		}
		if (levelId == null)
		{
			Debug.LogError("Parameter levelId can't be null!");
			return;
		}
		if (PhotonNetwork.room.customProperties.ContainsKey("curScn"))
		{
			object obj = PhotonNetwork.room.customProperties["curScn"];
			if (obj is int && Application.loadedLevel == (int)obj)
			{
				return;
			}
			if (obj is string && Application.loadedLevelName.Equals((string)obj))
			{
				return;
			}
		}
		Hashtable hashtable = new Hashtable();
		if (levelId is int)
		{
			hashtable["curScn"] = (int)levelId;
		}
		else if (levelId is string)
		{
			hashtable["curScn"] = (string)levelId;
		}
		else
		{
			Debug.LogError("Parameter levelId must be int or string!");
		}
		PhotonNetwork.room.SetCustomProperties(hashtable);
		this.SendOutgoingCommands();
	}

	public void SetApp(string appId, string gameVersion)
	{
		this.mAppId = appId.Trim();
		if (!string.IsNullOrEmpty(gameVersion))
		{
			this.mAppVersion = gameVersion.Trim();
		}
	}

	public bool WebRpc(string uriPath, object parameters)
	{
		return this.OpCustom(219, new Dictionary<byte, object>
		{
			{ 209, uriPath },
			{ 208, parameters }
		}, true, 0, false);
	}

	protected internal string mAppVersion;

	protected internal string mAppId;

	public AuthModeOption AuthMode;

	private string playername = string.Empty;

	private IPhotonPeerListener externalListener;

	private JoinType mLastJoinType;

	private bool mPlayernameHasToBeUpdated;

	public Dictionary<int, PhotonPlayer> mActors = new Dictionary<int, PhotonPlayer>();

	public PhotonPlayer[] mOtherPlayerListCopy = new PhotonPlayer[0];

	public PhotonPlayer[] mPlayerListCopy = new PhotonPlayer[0];

	public PhotonPlayer mMasterClient;

	public bool hasSwitchedMC;

	public bool requestSecurity = true;

	private Dictionary<Type, List<MethodInfo>> monoRPCMethodsCache = new Dictionary<Type, List<MethodInfo>>();

	public static bool UsePrefabCache = true;

	public static Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

	public Dictionary<string, RoomInfo> mGameList = new Dictionary<string, RoomInfo>();

	public RoomInfo[] mGameListCopy = new RoomInfo[0];

	public bool insideLobby;

	private HashSet<int> allowedReceivingGroups = new HashSet<int>();

	private HashSet<int> blockSendingGroups = new HashSet<int>();

	protected internal Dictionary<int, PhotonView> photonViewList = new Dictionary<int, PhotonView>();

	private readonly Dictionary<int, Hashtable> dataPerGroupReliable = new Dictionary<int, Hashtable>();

	private readonly Dictionary<int, Hashtable> dataPerGroupUnreliable = new Dictionary<int, Hashtable>();

	protected internal short currentLevelPrefix;

	private readonly Dictionary<string, int> rpcShortcuts;

	public bool IsInitialConnect;

	public string NameServerAddress = "ns.exitgamescloud.com";

	public string NameServerAddressHttp = "http://ns.exitgamescloud.com:80/photon/n";

	private static readonly Dictionary<ConnectionProtocol, int> ProtocolToNameServerPort = new Dictionary<ConnectionProtocol, int>
	{
		{ 0, 5058 },
		{ 1, 4533 }
	};

	private bool didAuthenticate;

	private string[] friendListRequested;

	private int friendListTimestamp;

	private bool isFetchingFriends;

	private Dictionary<int, object[]> tempInstantiationData = new Dictionary<int, object[]>();

	protected internal bool loadingLevelAndPausedNetwork;

	protected internal const string CurrentSceneProperty = "curScn";
}
