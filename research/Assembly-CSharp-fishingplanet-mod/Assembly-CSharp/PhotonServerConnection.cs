using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using BiteEditor;
using CodeStage.AntiCheat.ObscuredTypes;
using ExitGames.Client.Photon;
using Missions;
using Newtonsoft.Json;
using ObjectModel;
using ObjectModel.Common;
using ObjectModel.Serialization;
using ObjectModel.Tournaments;
using Photon.Interfaces;
using Photon.Interfaces.Game;
using Photon.Interfaces.LeaderBoards;
using Photon.Interfaces.Monetization;
using Photon.Interfaces.Profile;
using Photon.Interfaces.SharedMethods;
using Photon.Interfaces.Sys;
using Photon.Interfaces.Tournaments;
using TPM;
using UnityEngine;

public class PhotonServerConnection : IPhotonServerConnection, IPhotonPeerListener
{
	public PhotonServerConnection()
	{
		this.Room = new RoomDesc();
		this.serverTimeCache = new ServerTimeCache();
		for (int i = 1; i < this.gameSlots.Length; i++)
		{
			this.gameSlots[i] = new Game(i);
		}
	}

	public bool IsMessageQueueRunning { get; private set; }

	public bool IsConnectedToMaster { get; private set; }

	public bool IsConnectedToGameServer { get; private set; }

	public bool IsAuthenticated { get; private set; }

	public bool IsSilentMode { get; set; }

	public OperationResponse LastResponse { get; private set; }

	public bool IsChatMessagingOn { get; set; }

	public Profile Profile
	{
		get
		{
			return this.profile;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnDisconnect;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnConnectionFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<StatsCounterType, int> OnStatsUpdated;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int, int> OnExpGained;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ProfileFailure> OnProfileOperationFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Amount> OnMoneyUpdate;

	public ConnectionProtocol Protocol { get; set; }

	public string MasterServerAddress { get; set; }

	public string AppName { get; set; }

	public string Environment { get; private set; }

	public string Email { get; set; }

	public string Password { get; set; }

	public string UserName { get; private set; }

	public bool IsPondStayFinished
	{
		get
		{
			if (this.Profile == null)
			{
				return true;
			}
			if (this.globalVariables == null || !this.globalVariables.ContainsKey("PondDayStart"))
			{
				return true;
			}
			if (this.Profile.PondTimeSpent == null || this.Profile.PondStayTime == null)
			{
				return true;
			}
			if (this.Profile.Tournament != null)
			{
				return false;
			}
			int? pondStayTime = this.Profile.PondStayTime;
			return this.Profile.PondTimeSpent.Value.Days > pondStayTime || (this.Profile.PondTimeSpent.Value.Days == this.Profile.PondStayTime && this.Profile.PondTimeSpent.Value.Hours >= (int)this.globalVariables["PondDayStart"]);
		}
	}

	public void StopPhotonMessageQueue()
	{
		this.IsMessageQueueRunning = false;
		this.backgroundServiceThread = new Thread(delegate
		{
			while (!this.IsMessageQueueRunning)
			{
				try
				{
					if (this.Peer != null)
					{
						this.Peer.SendOutgoingCommands();
					}
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
				}
				Thread.Sleep(200);
			}
		});
		this.backgroundServiceThread.IsBackground = true;
		this.backgroundServiceThread.Start();
	}

	public void StartPhotonMessageQueue()
	{
		this.IsMessageQueueRunning = true;
		this.backgroundServiceThread = null;
	}

	public LoadbalancingPeer Peer { get; private set; }

	public void ConnectToMaster()
	{
		this.connectWithoutAuth = false;
		this.ConnectToMasterInt();
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnConnectedToMaster;

	public void ConnectToMasterNoAuth()
	{
		this.connectWithoutAuth = true;
		this.ConnectToMasterInt();
	}

	private void ConnectToMasterInt()
	{
		if (this.IsConnectedToMaster || this.IsConnectedToGameServer)
		{
			Debug.LogWarning("csri: Already connected!");
			return;
		}
		this.ClearInternalState();
		if (this.Peer != null && this.Peer.PeerState != null && this.Peer.PeerState != 4)
		{
			this.Peer.Disconnect();
			this.Peer.SendOutgoingCommands();
		}
		this.Peer = new LoadbalancingPeer(this, this.Protocol);
		this.SetupPeer();
		OpTimer.Reset();
		this.Peer.Connect(this.MasterServerAddress, this.AppName);
		DebugUtility.Connection.Trace("Connection to master server (" + this.MasterServerAddress + ") requested", new object[0]);
		this.IsMessageQueueRunning = true;
	}

	private void SetupPeer()
	{
		this.Peer.DisconnectTimeout = 25000;
		this.Peer.QuickResendAttempts = 3;
		this.Peer.SentCountAllowance = 14;
	}

	public void Disconnect()
	{
		if (this.Peer != null && this.Peer.PeerState != null)
		{
			if (this.Peer.QueuedOutgoingCommands > 0)
			{
				this.Peer.SendOutgoingCommands();
				Thread.Sleep(100);
			}
			this.Peer.Disconnect();
		}
		this.ClearInternalState();
		if (this.OnDisconnect != null)
		{
			this.OnDisconnect();
		}
	}

	public void Reset()
	{
		if (this.Peer != null)
		{
			if (this.Peer.PeerState == 3)
			{
				Debug.LogError("csri: Can not reset when connected. Use Disconnect instead.");
				return;
			}
			this.Peer = null;
		}
		this.ClearInternalState();
	}

	public void ReuseCaches()
	{
		foreach (Pond pond in CacheLibrary.MapCache.CachedPonds)
		{
			this.pondInfos[pond.PondId] = pond;
		}
		this.globalVariables = CacheLibrary.AssetsCache.GlobalVariables;
	}

	public void GracefulDisconnect()
	{
		Debug.LogWarning("csri: About to perform PreviewQuit");
		if (this.CheckProfile() && this.CheckGameConnectedAndAuthed())
		{
			Debug.Log("Graceful disconnect requested");
			this.PreviewQuit();
			Debug.LogWarning("PreviewQuit done!");
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPreviewQuitComplete;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnPreviewQuitFailed;

	public StatusCode ConnectionState { get; private set; }

	public string LastError { get; private set; }

	public string UserId { get; private set; }

	public bool? IsTempUser { get; private set; }

	public int ProtocolVersion
	{
		get
		{
			return 1100;
		}
	}

	public void Authenticate()
	{
		if (this.isAuthenticating)
		{
			Debug.LogWarning("auth: Currently authenticating");
			return;
		}
		if (string.IsNullOrEmpty(this.Email))
		{
			Debug.LogError("auth: Email is null on authenticate");
			return;
		}
		this.Peer.OpAuthenticate(this.AppName, this.ProtocolVersion.ToString(CultureInfo.InvariantCulture), this.Email, new AuthenticationValues
		{
			Secret = this.Password
		}, null);
		this.isAuthenticating = true;
		this.Password = null;
		OpTimer.Start(230);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnAuthenticated;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnAuthenticationFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnRefreshSecurityTokenFailed;

	public void AuthenticateWithSteam()
	{
		if (this.isAuthenticating)
		{
			Debug.LogWarning("Currently authenticating");
			return;
		}
		if (string.IsNullOrEmpty(this.SteamAuthTicket))
		{
			Debug.LogError("Email is null on authenticate");
			return;
		}
		string text = StaticUserData.SteamId.ToString();
		string @string = ObscuredPrefs.GetString("Email" + text);
		string string2 = ObscuredPrefs.GetString("Password" + text);
		this.Peer.OpAuthenticate(this.AppName, this.ProtocolVersion.ToString(CultureInfo.InvariantCulture), @string, new AuthenticationValues
		{
			Secret = this.SteamAuthTicket,
			AuthType = CustomAuthenticationType.Steam,
			AuthParameters = string2
		}, null);
		this.isAuthenticating = true;
		this.isAuthenticatingWithSteam = true;
		this.Password = null;
		OpTimer.Start(230);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnOperationFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnDuplicateLogin;

	public void RefreshSecurityToken()
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("auth: Not authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(199);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(199);
	}

	public void SimulateReconnect()
	{
		if (!this.IsConnectedToMaster && !this.IsConnectedToGameServer)
		{
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(223);
		operationRequest.Parameters[218] = "TestReconnect";
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(223);
	}

	public void GetProtocolVersion()
	{
		if (!this.IsConnectedToMaster)
		{
			throw new InvalidOperationException("Not connected to master!");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(170);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(170);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnGotProtocolVersion;

	public void RequestProfileOnError(OnGotProfile continuation = null, bool refresh = false)
	{
		if (DateTime.UtcNow.Subtract(this.lastRequestProfileTime).TotalMilliseconds >= 30000.0)
		{
			if (continuation != null)
			{
				OnGotProfile callback = null;
				callback = delegate(Profile p)
				{
					continuation(p);
					this.OnGotProfile -= callback;
				};
				this.OnGotProfile += callback;
			}
			UIHelper.Waiting(true, "Please, Wait");
			this.inventoryRequestNumberRequestProfileOnError = this.inventoryRequestNumber;
			if (refresh)
			{
				this.RefreshProfile();
			}
			else
			{
				this.RequestProfile();
			}
		}
	}

	public void RefreshProfile()
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 224;
		operationRequest.Parameters[250] = this.UserId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 224);
		this.lastRequestProfileTime = DateTime.UtcNow;
	}

	public void RequestProfile()
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 250;
		operationRequest.Parameters[250] = this.UserId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 250);
		this.lastRequestProfileTime = DateTime.UtcNow;
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotProfile OnGotProfile;

	public void RequestOtherPlayerProfile(string userId)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated!");
			return;
		}
		if (this.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase))
		{
			Debug.LogError("This function should be used to get other player profile, not self profile!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 250;
		operationRequest.Parameters[250] = userId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 250);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotProfile OnGotOtherPlayerProfile;

	public void RequestStats()
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 241;
		operationRequest.Parameters[250] = this.UserId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 241);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotStats OnGotStats;

	public void CheckEmailIsUnique(string email)
	{
		if (!this.IsConnectedToMaster && !this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to master!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 244;
		operationRequest.Parameters[248] = email;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 244);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnCheckEmailIsUnique;

	public void CheckUsernameIsUnique(string userName)
	{
		if (!this.IsConnectedToMaster && !this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to master!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 240;
		operationRequest.Parameters[249] = userName;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 240);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnCheckUsernameIsUnique;

	public void RegisterNewAccount(string name, string password, string email, int languageId = 1, string source = null, string extId = null, string promoCode = null)
	{
		if (this.IsAuthenticated || !this.IsConnectedToMaster)
		{
			Debug.LogError("Not authenticated or not connected to master!");
			return;
		}
		if (name.Length < 3)
		{
			throw new InvalidOperationException("User name should be at least 3 characters long");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 249;
		operationRequest.Parameters[249] = name;
		operationRequest.Parameters[247] = password;
		operationRequest.Parameters[248] = email;
		operationRequest.Parameters[198] = languageId;
		if (source != null)
		{
			operationRequest.Parameters[242] = source;
		}
		if (extId != null)
		{
			operationRequest.Parameters[241] = extId;
		}
		if (!string.IsNullOrEmpty(promoCode))
		{
			operationRequest.Parameters[166] = promoCode;
		}
		this.Email = email;
		this.Password = password;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 249);
	}

	public void RegisterTempAccount(string name, int languageId = 1)
	{
		if (this.IsAuthenticated || !this.IsConnectedToMaster)
		{
			Debug.LogError("Not authenticated or not connected to master!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 249;
		operationRequest.Parameters[249] = name;
		operationRequest.Parameters[198] = languageId;
		operationRequest.Parameters[186] = true;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 249);
	}

	public void UpdateProfile(Profile profile2Save)
	{
		if (!this.CheckGameConnectedAndAuthed())
		{
			Debug.LogError("Not authenticated or not connected to game server!");
			return;
		}
		AsynchProcessor.ProcessByteWorkItem(new InputByteWorkItem
		{
			Data = 200,
			ProcessAction = delegate(byte p)
			{
				OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
				operationRequest.Parameters = ProfileSerializationHelper.Serialize(profile2Save);
				operationRequest.Parameters[1] = 248;
				return operationRequest;
			},
			ResultAction = delegate(object r)
			{
				OperationRequest operationRequest2 = (OperationRequest)r;
				this.Peer.OpCustom(operationRequest2, true, 0, true);
				this.RefreshMainProfileValues(operationRequest2.Parameters);
				OpTimer.Start(200, 248);
			}
		});
	}

	public void UpdateProfileSettings(Dictionary<string, string> profileSettings)
	{
		if (!this.CheckIsAuthed())
		{
			Debug.LogError("Not authenticated!");
			return;
		}
		AsynchProcessor.ProcessByteWorkItem(new InputByteWorkItem
		{
			Data = 200,
			ProcessAction = delegate(byte p)
			{
				OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
				operationRequest.Parameters[1] = 231;
				operationRequest.Parameters[10] = CompressHelper.CompressString(JsonConvert.SerializeObject(profileSettings, 0, SerializationHelper.JsonSerializerSettings));
				return operationRequest;
			},
			ResultAction = delegate(object r)
			{
				OperationRequest operationRequest2 = (OperationRequest)r;
				this.Peer.OpCustom(operationRequest2, true, 0, true);
				OpTimer.Start(200, 231);
			}
		});
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnProfileUpdated;

	public void ChangePassword(string oldPassword, string newPassword)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 247;
		operationRequest.Parameters[248] = this.Email;
		operationRequest.Parameters[247] = oldPassword;
		operationRequest.Parameters[243] = newPassword;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 247);
	}

	public void SetLastPin(int pindId)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 216;
		operationRequest.Parameters[161] = pindId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 216);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnChangePassword;

	public void RequestPasswordRecoverOnEmail(string email, string name)
	{
		if (this.IsAuthenticated)
		{
			Debug.LogError("Already authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 239;
		operationRequest.Parameters[248] = email;
		operationRequest.Parameters[249] = name;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 239);
	}

	public void RecoverPassword(string email, string token, string newPassword)
	{
		if (this.IsAuthenticated)
		{
			Debug.LogError("Already authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 238;
		operationRequest.Parameters[248] = email;
		operationRequest.Parameters[247] = token;
		operationRequest.Parameters[243] = newPassword;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 238);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPasswordRecoverRequested;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPasswordRecovered;

	public void MakeTempAccountPersistent(string name, string password, string email)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated!");
			return;
		}
		if (this.IsTempUser == null)
		{
			throw new InvalidOperationException("Player profile is not received!");
		}
		if (!this.IsTempUser.Value)
		{
			throw new InvalidOperationException("User account is already persistent!");
		}
		this.cachedEmail = email;
		this.cachedPassword = password;
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 245;
		operationRequest.Parameters[250] = this.UserId;
		operationRequest.Parameters[249] = name;
		operationRequest.Parameters[247] = password;
		operationRequest.Parameters[248] = email;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		this.RefreshMainProfileValues(operationRequest.Parameters);
		OpTimer.Start(200, 245);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnMakeTempAccountPersistent;

	public void RequestEndOfMissionResult()
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 237;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 237);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnEndOfPeriodResult OnEndOfDayResult;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnEndOfPeriodResult OnEndOfMissionResult;

	public void RequestFishCage()
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 218;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 218);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnRequestFishCage;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnRequestFishCageFailed;

	public void ReleaseFishFromFishCage(global::ObjectModel.Fish fish)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		CaughtFish caughtFish = this.profile.FishCage.Fish.FirstOrDefault(delegate(CaughtFish f)
		{
			Guid? instanceId = f.Fish.InstanceId;
			bool flag = instanceId != null;
			Guid? instanceId2 = fish.InstanceId;
			return flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault());
		});
		if (caughtFish == null || caughtFish.Fish.InstanceId == null)
		{
			Debug.LogError("Fish to release does not exist in cage!");
			return;
		}
		if (!this.profile.FishCage.Cage.Safety)
		{
			Debug.LogError("Cage is damaging fish, release is not possible!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 236;
		operationRequest.Parameters[250] = caughtFish.Fish.InstanceId.ToString();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		this.profile.FishCage.ReleaseFish(caughtFish.Fish.InstanceId.Value);
		OpTimer.Start(200, 236);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Guid> OnReleaseFishFromFishCage;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnReleaseFishFromFishCageFailed;

	public int LevelCap
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return 0;
			}
			return (int)this.globalVariables["LevelCap"];
		}
	}

	public void ChangeName(string newName)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 232;
		operationRequest.Parameters[249] = newName;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 232);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnNameChanged;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnNameChangeFailed;

	public void SkipTutorial()
	{
		if (this.profile != null && this.profile.IsTutorialFinished)
		{
			Debug.LogError("Tutorial is already finished!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 225;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 225);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnTutorialSkipped;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnSkipTutorialFailed;

	public void SetModifier(float value)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 223;
		operationRequest.Parameters[189] = (double)value;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 223);
	}

	public void ResetProfileToDefault()
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 221;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 221);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnResetProfileToDefault;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnResetProfileToDefaultFailed;

	public void SetAvatarUrl(string url)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated");
			return;
		}
		this.Profile.AvatarUrl = url;
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 220;
		operationRequest.Parameters[162] = url;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 220);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnAvatarUrlSet;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnSettingAvatarUrlFailed;

	public void GetPlayerPlacementByExternalId(string externalId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 219;
		operationRequest.Parameters[250] = externalId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 219);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int?, string> OnGotPlayerPlacement;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingPlayerPlacementFailed;

	public void FlagSnowballHit(string targetUserId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 217;
		operationRequest.Parameters[250] = targetUserId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 217);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnSnowballHitFlagged;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnFlagSnowballHitFailed;

	public void GeneratePromoCode()
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to Game or not authenticated");
		}
		if (this.CheckProfile() && !string.IsNullOrEmpty(this.Profile.OwnPromoCode))
		{
			Debug.LogError("Promo Code is already available in profile");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 230;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 230);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPromoCodeGenerated;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGeneratePromoCodeFailed;

	public void CheckInviteIsUnique(string email)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to Game or not authenticated");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 229;
		operationRequest.Parameters[248] = email;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 229);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnInviteIsUnique;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnInviteDuplicated;

	public void CreateInvite(string email, string name)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to Game or not authenticated");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 228;
		operationRequest.Parameters[248] = email;
		operationRequest.Parameters[249] = name;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 228);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnInviteCreated;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnCreateInviteFailed;

	public void CheckPromoCode(string promoCode)
	{
		if (!this.IsConnectedToMaster)
		{
			Debug.LogError("Not connected to Master");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 227;
		operationRequest.Parameters[166] = promoCode;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 227);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPromoCodeValid;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnPromoCodeInvalid;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnReferralReward OnReferralReward;

	private bool LogDoubleInventoryRequests(OperationRequest request)
	{
		bool flag = true;
		DateTime utcNow = DateTime.UtcNow;
		Func<string> func = delegate
		{
			string text = StackTraceUtility.ExtractStackTrace();
			int num;
			if (text != null && (num = text.IndexOf("LogDoubleInventoryRequests(OperationRequest)")) >= 0)
			{
				num = text.IndexOf(')', num + 1);
				if (num > 0)
				{
					num = text.IndexOf(')', num + 1);
				}
				if (num > 0)
				{
					text = text.Substring(num + 1);
				}
			}
			int num2;
			if (text != null && (num2 = text.IndexOf("UnityEngine")) >= 0)
			{
				text = text.Substring(0, num2);
			}
			return text;
		};
		if (utcNow.Subtract(this.lastInventoryRequestTime).TotalMilliseconds < 200.0)
		{
			DebugUtility.Inventory.Important("Double inventory request: {0}", new object[] { this.lastInventoryStackTrace });
			this.PinError("Double inventory request", this.lastInventoryStackTrace + "\n\n---------------------------------------------\n\n" + func());
			flag = false;
		}
		if (flag)
		{
			if (this.inventoryRequestNumber != this.inventoryResponseNumber)
			{
				this.PinError(string.Format("Difference in request/response numbers. [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber), this.lastInventoryStackTrace + "\n\n---------------------------------------------\n\n" + func());
			}
			this.inventoryRequestNumber++;
			request.Parameters[221] = this.inventoryRequestNumber;
		}
		this.lastInventoryStackTrace = func();
		this.lastInventoryRequestTime = utcNow;
		return flag;
	}

	private bool LogInventoryResponse(OperationResponse response)
	{
		if (response.Parameters.ContainsKey(221))
		{
			this.inventoryResponseNumber = (int)response.Parameters[221];
			if (this.inventoryRequestNumberRequestProfileOnError > 0 && this.inventoryResponseNumber <= this.inventoryRequestNumberRequestProfileOnError)
			{
				return false;
			}
		}
		return true;
	}

	private bool LogMissedInventoryItem(InventoryItem item, string parameterName)
	{
		if (item == null || this.profile.Inventory.Contains(item))
		{
			return true;
		}
		DateTime utcNow = DateTime.UtcNow;
		DateTime? dateTime = this.lastRefreshLogMissedInventoryItem;
		if (dateTime == null || utcNow.Subtract(this.lastRefreshLogMissedInventoryItem.Value).TotalMilliseconds > 500.0)
		{
			this.lastRefreshLogMissedInventoryItem = new DateTime?(utcNow);
			if (this.OnInventoryUpdated != null)
			{
				this.OnInventoryUpdated();
			}
			return false;
		}
		this.PinError(string.Format("Missed InventoryItem {0}", parameterName), this.lastInventoryStackTrace);
		return true;
	}

	public void SwapRods(Rod currentRodInHands, Rod rodToHands)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (currentRodInHands != null && rodToHands != null && object.ReferenceEquals(currentRodInHands, rodToHands))
		{
			return;
		}
		if (!this.profile.Inventory.CanSwapRods(currentRodInHands, rodToHands))
		{
			Debug.LogError("Can't SwapRods! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(currentRodInHands, "currentRodInHands"))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(rodToHands, "rodToHands"))
		{
			return;
		}
		operationRequest.Parameters[191] = 16;
		if (currentRodInHands != null)
		{
			operationRequest.Parameters[1] = currentRodInHands.InstanceId.ToString();
			operationRequest.Parameters[14] = currentRodInHands.ItemSubType.ToString();
		}
		if (rodToHands != null)
		{
			operationRequest.Parameters[3] = rodToHands.InstanceId.ToString();
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 16);
		if (rodToHands != null)
		{
			GameFactory.RodSlots[rodToHands.Slot].BeginServerOp();
			this.PendingGameSlotIndex = rodToHands.Slot;
		}
	}

	private void UpdateRodInGameEngine(Rod rod, StoragePlaces priorStorage = StoragePlaces.Storage)
	{
		if (priorStorage == StoragePlaces.Hands && rod.Storage != StoragePlaces.Hands)
		{
			this.CurrentGameSlotIndex = 0;
			this.PendingGameSlotIndex = 0;
		}
		RodTemplate rodTemplate = this.Profile.Inventory.GetRodTemplate(rod);
		if (rod.Storage == StoragePlaces.Hands && rod.Durability > 0 && rodTemplate != RodTemplate.UnEquiped)
		{
			this.CurrentGameSlotIndex = rod.Slot;
			this.PendingGameSlotIndex = rod.Slot;
			GameFactory.RodSlots[this.CurrentGameSlotIndex].FinishServerOp();
		}
		if (rod.Storage == StoragePlaces.Hands && rodTemplate == RodTemplate.UnEquiped)
		{
			this.CurrentGameSlotIndex = rod.Slot;
			this.PendingGameSlotIndex = rod.Slot;
			if (GameFactory.Player != null)
			{
				GameFactory.Player.IsEmptyHandsMode = true;
			}
		}
	}

	public void MoveItemOrCombine(InventoryItem item, InventoryItem parent, StoragePlaces storage, bool moveRelatedItems = true)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (item.Storage == storage)
		{
			return;
		}
		InventoryItem inventoryItem = this.profile.Inventory.CheckMoveIsCombine(item, parent, storage, new InventoryMovementMapInfo());
		if (inventoryItem != null)
		{
			this.CombineItems(item, inventoryItem);
			return;
		}
		if (!this.profile.Inventory.CanMove(item, parent, storage, true))
		{
			Debug.LogError("Can't move item! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(item, "item"))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(parent, "parent"))
		{
			return;
		}
		operationRequest.Parameters[191] = 1;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[14] = item.ItemSubType.ToString();
		if (parent != null)
		{
			operationRequest.Parameters[3] = parent.InstanceId.ToString();
		}
		operationRequest.Parameters[2] = (byte)storage;
		operationRequest.Parameters[5] = item.Slot;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 1);
		if (item is Rod)
		{
			GameFactory.RodSlots[item.Slot].BeginServerOp();
			this.PendingGameSlotIndex = item.Slot;
		}
	}

	public void SplitItem(InventoryItem item, InventoryItem parent, int count, StoragePlaces storage)
	{
		if (!this.CheckGameConnectedAndAuthed())
		{
			return;
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (!this.profile.Inventory.CanSplit(item, count) || !this.profile.Inventory.CanMove(item, parent, storage, true))
		{
			Debug.LogError("Can't split/move items! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		if (item.Count == count)
		{
			this.MoveItemOrCombine(item, parent, storage, true);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(item, "item"))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(parent, "parent"))
		{
			return;
		}
		operationRequest.Parameters[191] = 3;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[14] = item.ItemSubType.ToString();
		if (parent != null)
		{
			operationRequest.Parameters[3] = parent.InstanceId.ToString();
		}
		operationRequest.Parameters[2] = (byte)storage;
		operationRequest.Parameters[4] = count;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 3);
	}

	public void SplitItem(InventoryItem item, InventoryItem parent, float amount, StoragePlaces storage)
	{
		if (!this.CheckGameConnectedAndAuthed())
		{
			return;
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (!this.profile.Inventory.CanSplit(item, amount) || !this.profile.Inventory.CanMove(item, parent, storage, true) || !this.profile.Inventory.CanUseChumOnRod(item as Chum, parent, amount))
		{
			Debug.LogError("Can't split/move items! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(item, "item"))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(parent, "parent"))
		{
			return;
		}
		operationRequest.Parameters[191] = 3;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[14] = item.ItemSubType.ToString();
		if (parent != null)
		{
			operationRequest.Parameters[3] = parent.InstanceId.ToString();
		}
		operationRequest.Parameters[2] = (byte)storage;
		operationRequest.Parameters[13] = amount;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 3);
	}

	public void CombineItems(InventoryItem item, InventoryItem targetItem)
	{
		if (!this.CheckGameConnectedAndAuthed())
		{
			return;
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (targetItem == null)
		{
			throw new ArgumentNullException("targetItem");
		}
		if (!this.profile.Inventory.CanCombine(item, targetItem))
		{
			Debug.LogError("Can't combine items! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(item, "item"))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(targetItem, "targetItem"))
		{
			return;
		}
		operationRequest.Parameters[191] = 4;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[14] = item.ItemSubType.ToString();
		operationRequest.Parameters[3] = targetItem.InstanceId.ToString();
		if (item.IsStockableByAmount)
		{
			operationRequest.Parameters[13] = item.Amount;
		}
		else
		{
			operationRequest.Parameters[4] = item.Count;
			if (item.Count < 1)
			{
				PhotonConnectionFactory.Instance.PinError("InventoryItem.Count can't be 0", global::System.Environment.StackTrace);
			}
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 4);
	}

	public void SubordinateItem(InventoryItem parent, InventoryItem newParent)
	{
		if (!this.CheckGameConnectedAndAuthed())
		{
			return;
		}
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		if (!this.profile.Inventory.CanSubordinate(parent, newParent))
		{
			Debug.LogError("Can't subordinate item! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(parent, "parent"))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(newParent, "newParent"))
		{
			return;
		}
		operationRequest.Parameters[191] = 2;
		operationRequest.Parameters[1] = parent.InstanceId.ToString();
		operationRequest.Parameters[14] = parent.ItemSubType.ToString();
		if (newParent != null)
		{
			operationRequest.Parameters[3] = newParent.InstanceId.ToString();
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 2);
	}

	public void ReplaceItem(InventoryItem item, InventoryItem replacementItem)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (replacementItem == null)
		{
			throw new ArgumentNullException("replacementItem");
		}
		if (!this.profile.Inventory.CanReplace(item, replacementItem))
		{
			Debug.LogError("Can't replace item! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(item, "item"))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(replacementItem, "replacementItem"))
		{
			return;
		}
		operationRequest.Parameters[191] = 5;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[14] = item.ItemSubType.ToString();
		operationRequest.Parameters[3] = replacementItem.InstanceId.ToString();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 5);
	}

	public void SplitItemAndReplace(InventoryItem item, InventoryItem replacementItem, int count)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (replacementItem == null)
		{
			throw new ArgumentNullException("replacementItem");
		}
		if (!this.profile.Inventory.CanSplit(replacementItem, count) || !this.profile.Inventory.CanReplace(item, replacementItem))
		{
			Debug.LogError("Can't SplitItemAndReplace item! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		if (replacementItem.Count == count)
		{
			this.ReplaceItem(item, replacementItem);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(item, "item"))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(replacementItem, "replacementItem"))
		{
			return;
		}
		operationRequest.Parameters[191] = 15;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[14] = item.ItemSubType.ToString();
		operationRequest.Parameters[3] = replacementItem.InstanceId.ToString();
		operationRequest.Parameters[4] = count;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 15);
	}

	public void SplitItemAndReplace(InventoryItem item, InventoryItem replacementItem, float amount)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (replacementItem == null)
		{
			throw new ArgumentNullException("replacementItem");
		}
		if (!this.profile.Inventory.CanSplit(replacementItem, amount) || !this.profile.Inventory.CanReplace(item, replacementItem) || !this.profile.Inventory.CanUseChumOnRod(replacementItem as Chum, item.ParentItem, amount))
		{
			Debug.LogError("Can't SplitItemAndReplace item! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(item, "item"))
		{
			return;
		}
		if (!this.LogMissedInventoryItem(replacementItem, "replacementItem"))
		{
			return;
		}
		operationRequest.Parameters[191] = 15;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[14] = item.ItemSubType.ToString();
		operationRequest.Parameters[3] = replacementItem.InstanceId.ToString();
		operationRequest.Parameters[13] = amount;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 15);
	}

	public void SetActiveQuiverTip(FeederRod rod, int index)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (rod == null)
		{
			throw new ArgumentNullException("rod");
		}
		if (!this.profile.Inventory.CanSetQuiverIndex(rod, index))
		{
			Debug.LogError("Can't SetActiveQuiverTip! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 25;
		operationRequest.Parameters[1] = rod.InstanceId.ToString();
		operationRequest.Parameters[5] = index;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 25);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnActiveQuiverTipSet;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnSetActiveQuiverTipFailed;

	public void PutRodOnStand(int slot, bool isRodOnStand)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 33;
		operationRequest.Parameters[5] = slot;
		operationRequest.Parameters[15] = isRodOnStand;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 33);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPutRodOnStand;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnPutRodOnStandFailed;

	public void BeginFillChum(Chum[] chums)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (chums == null || chums.Length == 0)
		{
			throw new ArgumentNullException("chums");
		}
		foreach (Chum chum in chums)
		{
			if (!this.profile.Inventory.CanBeginFillChum(chum))
			{
				Debug.LogError("Can't BeginFillChum! " + this.profile.Inventory.LastVerificationError);
				return;
			}
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 26;
		operationRequest.Parameters[1] = string.Join(", ", chums.Select((Chum c) => c.InstanceId.ToString()).ToArray<string>());
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 26);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnBeginFillChumDone;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnBeginFillChumFailed;

	public void FinishFillChum(Chum[] chums)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (chums == null || chums.Length == 0)
		{
			throw new ArgumentNullException("chums");
		}
		foreach (Chum chum in chums)
		{
			if (!this.profile.Inventory.CanFinishFillChum(chum))
			{
				Debug.LogError("Can't FinishFillChum! " + this.profile.Inventory.LastVerificationError);
				return;
			}
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 27;
		operationRequest.Parameters[1] = string.Join(", ", chums.Select((Chum c) => c.InstanceId.ToString()).ToArray<string>());
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 27);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Chum[]> OnFinishFillChumDone;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnFinishFillChumFailed;

	public void CancelFillChum(Chum[] chums)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (chums == null || chums.Length == 0)
		{
			throw new ArgumentNullException("chums");
		}
		foreach (Chum chum in chums)
		{
			if (!this.profile.Inventory.CanCancelFillChum(chum))
			{
				Debug.LogError("Can't CancelFillChum! " + this.profile.Inventory.LastVerificationError);
				return;
			}
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 28;
		operationRequest.Parameters[1] = string.Join(", ", chums.Select((Chum c) => c.InstanceId.ToString()).ToArray<string>());
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 28);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Chum[]> OnCancelFillChumDone;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnCancelFillChumFailed;

	public void ThrowChum(Chum[] chums)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (chums == null || chums.Length == 0)
		{
			throw new ArgumentNullException("chums");
		}
		foreach (Chum chum in chums)
		{
			if (!this.profile.Inventory.CanThrowChum(chum))
			{
				Debug.LogError("Can't ThrowChum! " + this.profile.Inventory.LastVerificationError);
				return;
			}
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 34;
		operationRequest.Parameters[1] = string.Join(", ", chums.Select((Chum c) => c.InstanceId.ToString()).ToArray<string>());
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 34);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Chum[]> OnThrowChumDone;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnThrowChumFailed;

	public void PutRodStand(InventoryItem item, bool put)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 35;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[15] = put;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 35);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPutRodStand;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnPutRodStandFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnInventoryMoved;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnInventoryMoveFailure;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnInventoryUpdated;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnInventoryUpdatedFailure;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnInventoryUpdateCancelled;

	public void RaiseInventoryUpdateCancelled()
	{
		if (this.OnInventoryUpdateCancelled != null)
		{
			this.OnInventoryUpdateCancelled();
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnAdsTriggered OnAdsTriggered;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnLoyatyBonusTriggered OnLoyatyBonusTriggered;

	public bool ProfileWasRequested { get; private set; }

	public void RaiseInventoryUpdated(bool profileRequested = false)
	{
		try
		{
			this.ProfileWasRequested = profileRequested;
			if (this.OnInventoryUpdated != null)
			{
				this.OnInventoryUpdated();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("RaiseInventoryUpdated exception: " + ex.Message + "\r\n" + ex.StackTrace);
			throw;
		}
		finally
		{
			this.ProfileWasRequested = false;
		}
	}

	private void HandleInventoryOperationResponse(OperationResponse response)
	{
		InventoryOperationCode? inventoryOperationCode = null;
		if (response.Parameters.ContainsKey(191))
		{
			inventoryOperationCode = new InventoryOperationCode?((byte)response.Parameters[191]);
			OpTimer.End(194, inventoryOperationCode.Value);
		}
		if (!this.LogInventoryResponse(response))
		{
			return;
		}
		if (inventoryOperationCode == 1 || inventoryOperationCode == 4 || inventoryOperationCode == 3 || inventoryOperationCode == 5 || inventoryOperationCode == 2 || inventoryOperationCode == 15 || inventoryOperationCode == 16)
		{
			this.HandleInventoryMovementResponse(response);
			return;
		}
		if (inventoryOperationCode == 9 || inventoryOperationCode == 10 || inventoryOperationCode == 12 || inventoryOperationCode == 11 || inventoryOperationCode == 13 || inventoryOperationCode == 14)
		{
			this.HandleBuoyOperationResponse(response);
			return;
		}
		if (inventoryOperationCode == 17 || inventoryOperationCode == 18 || inventoryOperationCode == 19 || inventoryOperationCode == 20 || inventoryOperationCode == 21 || inventoryOperationCode == 22)
		{
			this.HandleRodSetupOperationResponse(response);
			return;
		}
		if (inventoryOperationCode == 23 || inventoryOperationCode == 24)
		{
			this.HandleChumOperationResponse(response);
			return;
		}
		if (inventoryOperationCode == 29 || inventoryOperationCode == 30 || inventoryOperationCode == 31 || inventoryOperationCode == 32)
		{
			this.HandleChumRecipeOperationResponse(response);
			return;
		}
		if (inventoryOperationCode == 36)
		{
			this.HandleSetShimsCountResponse(response);
			return;
		}
		if (response.ReturnCode == 0)
		{
			Debug.Log("InventoryUpdated response arrived");
			try
			{
				if (inventoryOperationCode == 6)
				{
					if (this.OnSetLeaderLength != null)
					{
						this.OnSetLeaderLength();
					}
				}
				else if (inventoryOperationCode == 7)
				{
					if (this.OnItemConsumed != null)
					{
						this.OnItemConsumed();
					}
				}
				else if (inventoryOperationCode == 8)
				{
					if (this.OnGiftMade != null)
					{
						this.OnGiftMade();
					}
				}
				else if (inventoryOperationCode == 33)
				{
					if (this.OnPutRodOnStand != null)
					{
						this.OnPutRodOnStand();
					}
				}
				else if (inventoryOperationCode == 35)
				{
					if (this.OnPutRodStand != null)
					{
						this.OnPutRodStand();
					}
				}
				else if (inventoryOperationCode == 25)
				{
					this.HandleSetActiveQuiverTipResponse(response);
					if (this.OnActiveQuiverTipSet != null)
					{
						this.OnActiveQuiverTipSet();
					}
				}
				else if (inventoryOperationCode == 26)
				{
					this.HandleBeginFillChumResponse(response);
					if (this.OnBeginFillChumDone != null)
					{
						this.OnBeginFillChumDone();
					}
				}
				else if (inventoryOperationCode == 27)
				{
					Chum[] array = this.HandleFinishFillChumResponse(response);
					if (this.OnFinishFillChumDone != null)
					{
						this.OnFinishFillChumDone(array);
					}
					this.RaiseInventoryUpdated(false);
				}
				else if (inventoryOperationCode == 28)
				{
					Chum[] array2 = this.HandleCancelFillChumResponse(response);
					if (this.OnCancelFillChumDone != null)
					{
						this.OnCancelFillChumDone(array2);
					}
					this.RaiseInventoryUpdated(false);
				}
				else if (inventoryOperationCode == 34)
				{
					Chum[] array3 = this.HandleThrowChumResponse(response);
					if (this.OnThrowChumDone != null)
					{
						this.OnThrowChumDone(array3);
					}
					this.RaiseInventoryUpdated(false);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				this.PinError(string.Format("CLIENT !INVENTORY-UNSYNC! on call {2} [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber, inventoryOperationCode) + ex.Message, this.lastInventoryStackTrace);
				if (!ex.Message.Contains("!INVENTORY-UNSYNC!"))
				{
					throw;
				}
				Failure failure = new Failure(response);
				if (this.OnInventoryUpdatedFailure != null)
				{
					this.OnInventoryUpdatedFailure(failure);
				}
			}
		}
		else
		{
			Failure failure2 = new Failure(response);
			Debug.LogError(failure2.FullErrorInfo);
			this.PinError(string.Format("SERVER !INVENTORY-UNSYNC! on CLIENT call [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber) + failure2.ErrorMessage, this.lastInventoryStackTrace);
			if (inventoryOperationCode == 6)
			{
				if (this.OnSetLeaderLengthFailed != null)
				{
					this.OnSetLeaderLengthFailed(failure2);
				}
			}
			else if (inventoryOperationCode == 7)
			{
				if (this.OnConsumeItemFailed != null)
				{
					this.OnConsumeItemFailed(failure2);
				}
			}
			else if (inventoryOperationCode == 8)
			{
				if (this.OnMakingGiftFailed != null)
				{
					this.OnMakingGiftFailed(failure2);
				}
			}
			else if (inventoryOperationCode == 33)
			{
				if (this.OnPutRodOnStandFailed != null)
				{
					this.OnPutRodOnStandFailed(failure2);
				}
			}
			else if (inventoryOperationCode == 35)
			{
				if (this.OnPutRodStandFailed != null)
				{
					this.OnPutRodStandFailed(failure2);
				}
			}
			else if (inventoryOperationCode == 25)
			{
				if (this.OnSetActiveQuiverTipFailed != null)
				{
					this.OnSetActiveQuiverTipFailed(failure2);
				}
			}
			else if (inventoryOperationCode == 26)
			{
				if (this.OnBeginFillChumFailed != null)
				{
					this.OnBeginFillChumFailed(failure2);
				}
			}
			else if (inventoryOperationCode == 27)
			{
				if (this.OnFinishFillChumFailed != null)
				{
					this.OnFinishFillChumFailed(failure2);
				}
				if (this.OnInventoryUpdatedFailure != null)
				{
					this.OnInventoryUpdatedFailure(failure2);
				}
			}
			else if (inventoryOperationCode == 28)
			{
				if (this.OnCancelFillChumFailed != null)
				{
					this.OnCancelFillChumFailed(failure2);
				}
				if (this.OnInventoryUpdatedFailure != null)
				{
					this.OnInventoryUpdatedFailure(failure2);
				}
			}
			else if (inventoryOperationCode == 34)
			{
				if (this.OnThrowChumFailed != null)
				{
					this.OnThrowChumFailed(failure2);
				}
				if (this.OnInventoryUpdatedFailure != null)
				{
					this.OnInventoryUpdatedFailure(failure2);
				}
			}
		}
	}

	private void HandleInventoryMovementResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			InventoryOperationCode inventoryOperationCode = (byte)response.Parameters[191];
			try
			{
				switch (inventoryOperationCode)
				{
				case 1:
					this.HandleMoveItemResponse(response);
					break;
				case 2:
					this.HandleSubordinateItemResponse(response);
					break;
				case 3:
					if (response.Parameters.ContainsKey(13))
					{
						this.HandleSplitItemResponseAmount(response);
					}
					else
					{
						this.HandleSplitItemResponse(response);
					}
					break;
				case 4:
					if (response.Parameters.ContainsKey(13))
					{
						this.HandleCombineItemResponseAmount(response);
					}
					else
					{
						this.HandleCombineItemResponse(response);
					}
					break;
				case 5:
					this.HandleReplaceItemResponse(response);
					break;
				default:
					if (inventoryOperationCode != 15)
					{
						if (inventoryOperationCode == 16)
						{
							this.HandleSwapRodsResponse(response);
						}
					}
					else if (response.Parameters.ContainsKey(13))
					{
						this.HandleSplitAndReplaceItemResponseAmount(response);
					}
					else
					{
						this.HandleSplitAndReplaceItemResponse(response);
					}
					break;
				}
				if (this.OnInventoryMoved != null)
				{
					this.OnInventoryMoved();
				}
				this.RaiseInventoryUpdated(false);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				this.PinError(string.Format("CLIENT !INVENTORY-UNSYNC! on call {2} [[rq: {0}, rs: {1}]] ", this.inventoryRequestNumber, this.inventoryResponseNumber, inventoryOperationCode) + ex.Message, this.lastInventoryStackTrace);
				if (!ex.Message.Contains("!INVENTORY-UNSYNC!"))
				{
					throw;
				}
				Failure failure = new Failure(response);
				if (this.OnInventoryMoveFailure != null)
				{
					this.OnInventoryMoveFailure();
				}
				if (this.OnInventoryUpdatedFailure != null)
				{
					this.OnInventoryUpdatedFailure(failure);
				}
				this.RequestProfileOnError(null, false);
			}
		}
		else
		{
			Failure failure2 = new Failure(response);
			Debug.LogError(failure2.FullErrorInfo);
			this.PinError(string.Format("SERVER !INVENTORY-UNSYNC! on CLIENT call [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber) + failure2.ErrorMessage, this.lastInventoryStackTrace);
			if (this.OnInventoryMoveFailure != null)
			{
				this.OnInventoryMoveFailure();
			}
			if (this.OnInventoryUpdatedFailure != null)
			{
				this.OnInventoryUpdatedFailure(failure2);
			}
			this.RequestProfileOnError(null, false);
		}
	}

	private void HandleSetActiveQuiverTipResponse(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SetActiveQuiverTip! No item found with id: " + itemInstanceId, new object[0]));
		}
		FeederRod feederRod = inventoryItem as FeederRod;
		int num = (int)response.Parameters[5];
		if (!this.profile.Inventory.CanSetQuiverIndex(feederRod, num))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SetActiveQuiverTip! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		this.profile.Inventory.SetQuiverIndex(feederRod, num);
		this.UpdateRodInGameEngine(feederRod, StoragePlaces.Storage);
	}

	private Chum[] HandleBeginFillChumResponse(OperationResponse response)
	{
		string text = (string)response.Parameters[1];
		Guid[] array = (from s in text.Split(new char[] { ',' })
			select new Guid(s)).ToArray<Guid>();
		List<Chum> list = new List<Chum>();
		Guid[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			Guid itemInstanceId = array2[j];
			InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
			if (inventoryItem == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't BeginFillChum! No item found with id: " + itemInstanceId, new object[0]));
			}
			Chum chum = inventoryItem as Chum;
			if (chum == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't BeginFillChum! Item is not chum: " + itemInstanceId, new object[0]));
			}
			if (!this.profile.Inventory.CanBeginFillChum(chum))
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't BeginFillChum! " + this.profile.Inventory.LastVerificationError, new object[0]));
			}
			list.Add(chum);
		}
		foreach (Chum chum2 in list)
		{
			this.profile.Inventory.BeginFillChum(chum2, new InventoryMovementMapInfo());
		}
		return list.ToArray();
	}

	private Chum[] HandleFinishFillChumResponse(OperationResponse response)
	{
		string text = (string)response.Parameters[1];
		Guid[] array = (from s in text.Split(new char[] { ',' })
			select new Guid(s)).ToArray<Guid>();
		List<Chum> list = new List<Chum>();
		Guid[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			Guid itemInstanceId = array2[j];
			InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
			if (inventoryItem == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't FinishFillChum! No item found with id: " + itemInstanceId, new object[0]));
			}
			Chum chum = inventoryItem as Chum;
			if (chum == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't FinishFillChum! Item is not chum: " + itemInstanceId, new object[0]));
			}
			if (!this.profile.Inventory.CanFinishFillChum(chum))
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't FinishFillChum! " + this.profile.Inventory.LastVerificationError, new object[0]));
			}
			list.Add(chum);
		}
		string text2 = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text2, SerializationHelper.JsonSerializerSettings);
		List<Chum> list2 = new List<Chum>();
		foreach (Chum chum2 in list)
		{
			Chum chum3 = this.profile.Inventory.FinishFillChum(chum2, inventoryMovementMapInfo.GetRelatedInfoOrNew(chum2.InstanceId.Value));
			list2.Add(chum3);
			chum3.WasThrown = false;
		}
		return list2.ToArray();
	}

	private Chum[] HandleCancelFillChumResponse(OperationResponse response)
	{
		string text = (string)response.Parameters[1];
		Guid[] array = (from s in text.Split(new char[] { ',' })
			select new Guid(s)).ToArray<Guid>();
		List<Chum> list = new List<Chum>();
		Guid[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			Guid itemInstanceId = array2[j];
			InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
			if (inventoryItem == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't CancelFillChum! No item found with id: " + itemInstanceId, new object[0]));
			}
			Chum chum = inventoryItem as Chum;
			if (chum == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't CancelFillChum! Item is not chum: " + itemInstanceId, new object[0]));
			}
			if (!this.profile.Inventory.CanCancelFillChum(chum))
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't CancelFillChum! " + this.profile.Inventory.LastVerificationError, new object[0]));
			}
			list.Add(chum);
		}
		foreach (Chum chum2 in list)
		{
			this.profile.Inventory.CancelFillChum(chum2);
			chum2.WasThrown = false;
		}
		return list.ToArray();
	}

	private Chum[] HandleThrowChumResponse(OperationResponse response)
	{
		string text = (string)response.Parameters[1];
		Guid[] array = (from s in text.Split(new char[] { ',' })
			select new Guid(s)).ToArray<Guid>();
		List<Chum> list = new List<Chum>();
		Guid[] array2 = array;
		for (int j = 0; j < array2.Length; j++)
		{
			Guid itemInstanceId = array2[j];
			InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
			if (inventoryItem == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't ThrowChum! No item found with id: " + itemInstanceId, new object[0]));
			}
			Chum chum = inventoryItem as Chum;
			if (chum == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't ThrowChum! Item is not chum: " + itemInstanceId, new object[0]));
			}
			if (!this.profile.Inventory.CanThrowChum(chum))
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't ThrowChum! " + this.profile.Inventory.LastVerificationError, new object[0]));
			}
			list.Add(chum);
		}
		string text2 = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text2, SerializationHelper.JsonSerializerSettings);
		List<Chum> list2 = new List<Chum>();
		foreach (Chum chum2 in list)
		{
			Chum chum3 = this.profile.Inventory.ThrowChum(chum2, inventoryMovementMapInfo.GetRelatedInfoOrNew(chum2.InstanceId.Value));
			chum3.ThrownChumInstanceId = chum2.InstanceId;
			list2.Add(chum3);
		}
		return list2.ToArray();
	}

	private void HandleMoveItemResponse(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't move item! No item found with id: " + itemInstanceId, new object[0]));
		}
		InventoryItem inventoryItem2 = null;
		if (response.Parameters.ContainsKey(3))
		{
			Guid parentItemId = new Guid((string)response.Parameters[3]);
			inventoryItem2 = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == parentItemId);
			if (inventoryItem2 == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't move item! No parent item found with id: " + parentItemId, new object[0]));
			}
		}
		StoragePlaces storagePlaces = (StoragePlaces)response.Parameters[2];
		int num = 0;
		if (response.Parameters.ContainsKey(5))
		{
			num = (int)response.Parameters[5];
		}
		if (!this.profile.Inventory.CanMove(inventoryItem, inventoryItem2, storagePlaces, true))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't move item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		string text = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
		StoragePlaces storage = inventoryItem.Storage;
		this.profile.Inventory.MoveItem(inventoryItem, inventoryItem2, storagePlaces, inventoryMovementMapInfo, true, true);
		if (storagePlaces == StoragePlaces.Doll || storagePlaces == StoragePlaces.Hands)
		{
			inventoryItem.Slot = num;
		}
		Rod rod = (inventoryItem as Rod) ?? (inventoryItem2 as Rod);
		if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, storage);
		}
		Debug.Log("Item moved locally");
		this.UpdateFishCage(inventoryItem, false);
	}

	private void HandleSplitItemResponse(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! No item found with id: " + itemInstanceId, new object[0]));
		}
		if (inventoryItem.IsStockableByAmount)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item IsStockableByAmount!", new object[0]));
		}
		InventoryItem inventoryItem2 = null;
		if (response.Parameters.ContainsKey(3))
		{
			Guid parentItemId = new Guid((string)response.Parameters[3]);
			inventoryItem2 = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == parentItemId);
			if (inventoryItem2 == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! No parent item found with id: " + parentItemId, new object[0]));
			}
		}
		int num = (int)response.Parameters[4];
		if (num < 1)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! count can't be 0 and negative", new object[0]));
		}
		if (num > inventoryItem.Count)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! Client is asking to split more item count than exist", new object[0]));
		}
		StoragePlaces storagePlaces = (StoragePlaces)response.Parameters[2];
		if (!this.profile.Inventory.CanSplit(inventoryItem, num) || !this.profile.Inventory.CanMove(inventoryItem, inventoryItem2, storagePlaces, true))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		string text = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
		InventoryItem inventoryItem3 = this.profile.Inventory.SplitItem(inventoryItem, num, inventoryMovementMapInfo);
		Debug.Log("Item split locally");
		if (inventoryItem is Line && ((Line)inventoryItem).Length < 10.0)
		{
			this.profile.Inventory.RemoveItem(inventoryItem, false);
			Debug.Log("Line removed locally: Split");
		}
		this.profile.Inventory.MoveItem(inventoryItem3, inventoryItem2, storagePlaces, true, true);
		Debug.Log("Item moved locally: Split");
		this.UpdateFishCage(inventoryItem3, false);
		Rod rod = (inventoryItem3 as Rod) ?? (inventoryItem3.ParentItem as Rod);
		if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, StoragePlaces.Storage);
		}
	}

	private void HandleSplitItemResponseAmount(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! No item found with id: " + itemInstanceId, new object[0]));
		}
		if (!inventoryItem.IsStockableByAmount)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item not IsStockableByAmount!", new object[0]));
		}
		InventoryItem inventoryItem2 = null;
		if (response.Parameters.ContainsKey(3))
		{
			Guid parentItemId = new Guid((string)response.Parameters[3]);
			inventoryItem2 = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == parentItemId);
			if (inventoryItem2 == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! No parent item found with id: " + parentItemId, new object[0]));
			}
		}
		float num = (float)response.Parameters[13];
		if (num < 0f)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! amount can't be negative", new object[0]));
		}
		if (num > inventoryItem.Amount)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! Client is asking to split more item amount than exist", new object[0]));
		}
		StoragePlaces storagePlaces = (StoragePlaces)response.Parameters[2];
		if (!this.profile.Inventory.CanSplit(inventoryItem, num) || !this.profile.Inventory.CanMove(inventoryItem, inventoryItem2, storagePlaces, true) || !this.profile.Inventory.CanUseChumOnRod(inventoryItem as Chum, inventoryItem2, num))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		string text = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
		InventoryItem inventoryItem3 = this.profile.Inventory.SplitItem(inventoryItem, num, inventoryMovementMapInfo);
		Debug.Log("Item split locally");
		if (inventoryItem is Line && ((Line)inventoryItem).Length < 10.0)
		{
			this.profile.Inventory.RemoveItem(inventoryItem, false);
			Debug.Log("Line removed locally: Split");
		}
		this.profile.Inventory.MoveItem(inventoryItem3, inventoryItem2, storagePlaces, true, true);
		Debug.Log("Item moved locally: Split");
		this.UpdateFishCage(inventoryItem3, false);
		Rod rod = (inventoryItem3 as Rod) ?? (inventoryItem3.ParentItem as Rod);
		if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, StoragePlaces.Storage);
		}
	}

	private void HandleCombineItemResponse(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! No item found with id: " + itemInstanceId, new object[0]));
		}
		if (inventoryItem.IsStockableByAmount)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item IsStockableByAmount!", new object[0]));
		}
		int num = inventoryItem.Count;
		object obj;
		if (response.Parameters.TryGetValue(4, out obj))
		{
			num = (int)obj;
		}
		if (num > inventoryItem.Count)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! Client is asking to combine more item count than exist", new object[0]));
		}
		if (!response.Parameters.ContainsKey(3))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! Client is asking to combine item without target item", new object[0]));
		}
		Guid parentItemId = new Guid((string)response.Parameters[3]);
		InventoryItem inventoryItem2 = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == parentItemId);
		if (inventoryItem2 == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! No target item found with id: " + parentItemId, new object[0]));
		}
		if (!this.profile.Inventory.CanCombine(inventoryItem, inventoryItem2))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		string text = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
		InventoryItem parentItem = inventoryItem.ParentItem;
		bool flag = this.profile.Inventory.CombineItem(inventoryItem, inventoryItem2, num, inventoryMovementMapInfo, null, true);
		Debug.Log("Item combined locally");
		if (flag)
		{
			this.profile.Inventory.RemoveItem(inventoryItem, false);
			Debug.Log("Item removed locally: Combine");
		}
		Rod rod = parentItem as Rod;
		if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, StoragePlaces.Storage);
		}
	}

	private void HandleCombineItemResponseAmount(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! No item found with id: " + itemInstanceId, new object[0]));
		}
		if (!inventoryItem.IsStockableByAmount)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item not IsStockableByAmount!", new object[0]));
		}
		float num = inventoryItem.Amount;
		object obj;
		if (response.Parameters.TryGetValue(13, out obj))
		{
			num = (float)obj;
		}
		if (num > inventoryItem.Amount)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! Client is asking to combine more item amount than exist", new object[0]));
		}
		if (!response.Parameters.ContainsKey(3))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! Client is asking to combine item without target item", new object[0]));
		}
		Guid parentItemId = new Guid((string)response.Parameters[3]);
		InventoryItem inventoryItem2 = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == parentItemId);
		if (inventoryItem2 == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! No target item found with id: " + parentItemId, new object[0]));
		}
		if (!this.profile.Inventory.CanCombine(inventoryItem, inventoryItem2))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't combine item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		string text = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
		InventoryItem parentItem = inventoryItem.ParentItem;
		bool flag = this.profile.Inventory.CombineItem(inventoryItem, inventoryItem2, num, inventoryMovementMapInfo, null, true);
		Debug.Log("Item combined locally");
		if (flag)
		{
			this.profile.Inventory.RemoveItem(inventoryItem, false);
			Debug.Log("Item removed locally: Combine");
		}
		Rod rod = parentItem as Rod;
		if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, StoragePlaces.Storage);
		}
	}

	private void HandleSubordinateItemResponse(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't subordinate item! No item found with id: " + itemInstanceId, new object[0]));
		}
		InventoryItem inventoryItem2 = null;
		if (response.Parameters.ContainsKey(3))
		{
			Guid newParentId = new Guid((string)response.Parameters[3]);
			inventoryItem2 = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == newParentId);
			if (inventoryItem2 == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Client is asking to subordinate non-existent inventory item: " + newParentId, new object[0]));
			}
		}
		if (inventoryItem2 == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Client is asking to subordinate with non-existent inventory item", new object[0]));
		}
		if (!this.profile.Inventory.CanSubordinate(inventoryItem, inventoryItem2))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't subordinate item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		this.profile.Inventory.SubordinateItem(inventoryItem, inventoryItem2);
		Debug.Log("Item subordinated locally");
		Rod rod = inventoryItem2 as Rod;
		if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, StoragePlaces.Storage);
		}
	}

	private void HandleReplaceItemResponse(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't replace item! No item found with id: " + itemInstanceId, new object[0]));
		}
		InventoryItem inventoryItem2 = null;
		if (response.Parameters.ContainsKey(3))
		{
			Guid newParentId = new Guid((string)response.Parameters[3]);
			inventoryItem2 = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == newParentId);
			if (inventoryItem2 == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Client is asking to replace non-existent inventory item: " + newParentId, new object[0]));
			}
		}
		if (inventoryItem2 == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Client is asking to replace with non-existent inventory item", new object[0]));
		}
		if (!this.profile.Inventory.CanReplace(inventoryItem, inventoryItem2))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't replace item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		string text = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
		this.profile.Inventory.ReplaceItem(inventoryItem, inventoryItem2, inventoryMovementMapInfo, true);
		Debug.Log("Item replaced locally");
		this.UpdateFishCage(inventoryItem2, false);
		Rod rod = (inventoryItem2 as Rod) ?? (inventoryItem2.ParentItem as Rod);
		if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, StoragePlaces.Storage);
		}
	}

	private void HandleSplitAndReplaceItemResponse(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SplitAndReplace item! No item found with id: " + itemInstanceId, new object[0]));
		}
		if (inventoryItem.IsStockableByAmount)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item IsStockableByAmount!", new object[0]));
		}
		InventoryItem inventoryItem2 = null;
		if (response.Parameters.ContainsKey(3))
		{
			Guid newParentId = new Guid((string)response.Parameters[3]);
			inventoryItem2 = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == newParentId);
			if (inventoryItem2 == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Client is asking to SplitAndReplace non-existent inventory item: " + newParentId, new object[0]));
			}
		}
		if (inventoryItem2 == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Client is asking to SplitAndReplace with non-existent inventory item", new object[0]));
		}
		int num = (int)response.Parameters[4];
		if (num > inventoryItem2.Count)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SplitAndReplace item! Client is asking to split more item count than exist", new object[0]));
		}
		if (!this.profile.Inventory.CanSplit(inventoryItem2, num) || !this.profile.Inventory.CanReplace(inventoryItem, inventoryItem2))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SplitAndReplace item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		string text = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
		InventoryItem inventoryItem3 = this.profile.Inventory.SplitItemAndReplace(inventoryItem, inventoryItem2, num, inventoryMovementMapInfo);
		Debug.Log("Item splited and replaced locally");
		if (inventoryItem2 is Line && ((Line)inventoryItem2).Length < 10.0)
		{
			this.profile.Inventory.RemoveItem(inventoryItem2, false);
			Debug.Log("Line removed locally: Split");
		}
		this.UpdateFishCage(inventoryItem3, false);
		Rod rod = (inventoryItem3 as Rod) ?? (inventoryItem3.ParentItem as Rod);
		if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, StoragePlaces.Storage);
		}
	}

	private void HandleSplitAndReplaceItemResponseAmount(OperationResponse response)
	{
		Guid itemInstanceId = new Guid((string)response.Parameters[1]);
		InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == itemInstanceId);
		if (inventoryItem == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SplitAndReplace item! No item found with id: " + itemInstanceId, new object[0]));
		}
		if (!inventoryItem.IsStockableByAmount)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't split item not IsStockableByAmount!", new object[0]));
		}
		InventoryItem inventoryItem2 = null;
		if (response.Parameters.ContainsKey(3))
		{
			Guid newParentId = new Guid((string)response.Parameters[3]);
			inventoryItem2 = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == newParentId);
			if (inventoryItem2 == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Client is asking to SplitAndReplace non-existent inventory item: " + newParentId, new object[0]));
			}
		}
		if (inventoryItem2 == null)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Client is asking to SplitAndReplace with non-existent inventory item", new object[0]));
		}
		float num = (float)response.Parameters[13];
		if (num > inventoryItem2.Amount)
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SplitAndReplace item! Client is asking to split more item amount than exist", new object[0]));
		}
		if (!this.profile.Inventory.CanSplit(inventoryItem2, num) || !this.profile.Inventory.CanReplace(inventoryItem, inventoryItem2) || !this.profile.Inventory.CanUseChumOnRod(inventoryItem2 as Chum, inventoryItem.ParentItem, num))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SplitAndReplace item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		string text = CompressHelper.DecompressString((byte[])response.Parameters[12]);
		InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
		InventoryItem inventoryItem3 = this.profile.Inventory.SplitItemAndReplace(inventoryItem, inventoryItem2, num, inventoryMovementMapInfo);
		Debug.Log("Item splited and replaced locally");
		if (inventoryItem2 is Line && ((Line)inventoryItem2).Length < 10.0)
		{
			this.profile.Inventory.RemoveItem(inventoryItem2, false);
			Debug.Log("Line removed locally: Split");
		}
		this.UpdateFishCage(inventoryItem3, false);
		Rod rod = (inventoryItem3 as Rod) ?? (inventoryItem3.ParentItem as Rod);
		if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, StoragePlaces.Storage);
		}
	}

	private void HandleSwapRodsResponse(OperationResponse response)
	{
		Rod rod = null;
		Rod rod2 = null;
		if (response.Parameters.ContainsKey(1))
		{
			Guid guid = new Guid((string)response.Parameters[1]);
			rod = (Rod)this.Profile.Inventory.GetItem(guid);
			if (rod == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SwapRods! No item found with id: " + guid, new object[0]));
			}
		}
		if (response.Parameters.ContainsKey(3))
		{
			Guid guid2 = new Guid((string)response.Parameters[3]);
			rod2 = (Rod)this.Profile.Inventory.GetItem(guid2);
			if (rod2 == null)
			{
				throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SwapRods! No replacement item found with id: " + guid2, new object[0]));
			}
		}
		if (!this.profile.Inventory.CanSwapRods(rod, rod2))
		{
			throw new InvalidOperationException(string.Format("!INVENTORY-UNSYNC! Can't SwapRods item! " + this.profile.Inventory.LastVerificationError, new object[0]));
		}
		this.profile.Inventory.SwapRods(rod, rod2);
		if (rod2 != null)
		{
			this.UpdateRodInGameEngine(rod2, StoragePlaces.Doll);
		}
		else if (rod != null)
		{
			this.UpdateRodInGameEngine(rod, StoragePlaces.Hands);
		}
		Debug.Log("Item moved locally");
		this.UpdateFishCage(rod, false);
	}

	private void HandleRepairResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (response.Parameters.ContainsKey(194))
			{
				string[] array = ((string)response[194]).Split(new char[] { ',' });
				if (array.Length == 1 && response.Parameters.ContainsKey(188) && response.Parameters.ContainsKey(187) && response.Parameters.ContainsKey(183))
				{
					Guid guid = new Guid(array[0]);
					float num = (float)response[188];
					string text = (string)response[187];
					int num2 = (int)response[183];
					InventoryItem item = this.profile.Inventory.GetItem(guid);
					if (item == null)
					{
						Debug.LogError("Item to repair is absent in inventory");
						return;
					}
					this.profile.Inventory.Repair(item, num2);
					if (this.profile != null)
					{
						this.profile.IncrementBalance(text, (double)(-(double)num));
					}
				}
			}
			this.RaiseInventoryUpdated(false);
		}
		else
		{
			Debug.LogErrorFormat("Repair failed. Code: {0} Message: {1}", new object[]
			{
				Enum.GetName(typeof(ErrorCode), response.ReturnCode),
				response.DebugMessage
			});
			if (this.OnInventoryUpdatedFailure != null)
			{
				this.OnInventoryUpdatedFailure(new Failure(response));
			}
		}
	}

	public void GetItemCategories(bool topLevelOnly = true)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(190);
		operationRequest.Parameters[193] = topLevelOnly;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(190);
	}

	public void GetItemsCountForCategories(int? pondId = null)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(175);
		if (pondId != null)
		{
			operationRequest.Parameters[194] = pondId.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(175);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<List<CategoryItemsCount>> OnGotItemsCountForCategories;

	public void GetGlobalItemsFromCategory(int[] categoryIds, bool allItems = false)
	{
		if (categoryIds.Length == 0)
		{
			Debug.LogError("categoryIds should not be empty");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(189);
		operationRequest.Parameters[194] = categoryIds;
		operationRequest.Parameters[193] = allItems;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(189);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotItems OnGotItemsForCategory;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingItemsForCategoryFailed;

	public void GetLocalItemsFromCategory(int[] categoryIds, int pondId)
	{
		if (categoryIds.Length == 0)
		{
			Debug.LogError("categoryIds should not be empty");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(188);
		operationRequest.Parameters[194] = categoryIds;
		operationRequest.Parameters[184] = pondId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(188);
	}

	public void GetItemsByIds(int[] ids, int subscriberId, bool useGlobalContext = false)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(173);
		if (ids != null)
		{
			operationRequest.Parameters[194] = ids;
		}
		operationRequest.Parameters[244] = subscriberId;
		if (useGlobalContext)
		{
			operationRequest.Parameters[193] = true;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(173);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotItemsSubscribed OnGotItems;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingItemsFailed;

	public void BuyItem(InventoryItem item, int? itemCount = null)
	{
		this.BuyItemInt(item, itemCount, null);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<InventoryItem> OnItemBought;

	public void BuyItemAndSendToBase(InventoryItem item, int? itemCount = null)
	{
		this.BuyItemInt(item, itemCount, new bool?(true));
	}

	private void BuyItemInt(InventoryItem item, int? itemCount = null, bool? sendToBase = null)
	{
		if (!this.CheckProfile())
		{
			return;
		}
		if (item.MinLevel > this.profile.Level)
		{
			Debug.LogError("Item level is greater than player level");
			return;
		}
		if (!this.profile.CanBuyItemWithoutSendingToStorage(item) && (sendToBase == null || !sendToBase.Value))
		{
			Debug.LogWarning("No place to buy new item to");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(187);
		operationRequest.Parameters[194] = item.ItemId;
		if (itemCount != null)
		{
			operationRequest.Parameters[183] = itemCount;
		}
		if (sendToBase != null && sendToBase.Value)
		{
			operationRequest.Parameters[182] = true;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(187);
	}

	public void DestroyItem(InventoryItem item)
	{
		if (!this.profile.Inventory.CanDestroyItem(item))
		{
			Debug.LogError("Item can't be destroyed");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(186);
		if (!this.LogDoubleInventoryRequests(operationRequest))
		{
			return;
		}
		operationRequest.Parameters[194] = item.InstanceId.ToString();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(186);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<InventoryItem> OnItemDestroyed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<TransactionFailure> OnTransactionFailed;

	public void BuyProduct(StoreProduct product, int? count = null)
	{
		if (!this.CheckProfile())
		{
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(132);
		operationRequest.Parameters[194] = product.ProductId;
		if (count != null)
		{
			operationRequest.Parameters[183] = count;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(132);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnProductBought OnProductBought;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnBuyProductFailed;

	public void BuyLicense(ShopLicense license, int term)
	{
		if (!this.CheckProfile())
		{
			return;
		}
		if (license.MinLevel > this.profile.Level)
		{
			Debug.LogError("License level is greater than player level");
			return;
		}
		if (!this.profile.ValidateBuyLicense(license.LicenseId))
		{
			Debug.LogWarning("Can't buy license. Player might already have unlimited license of this type.");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(180);
		operationRequest.Parameters[194] = license.LicenseId;
		operationRequest.Parameters[183] = term;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(180);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<PlayerLicense> OnLicenseBought;

	public void DestroyLicense(PlayerLicense license)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(179);
		operationRequest.Parameters[194] = license.InstanceId.ToString();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(179);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<PlayerLicense> OnLicenseDestroyed;

	public void GetBrands()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(161);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(161);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotBrands OnGotBrands;

	public void RepairItem(InventoryItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (this.CurrentPondId != null)
		{
			Debug.LogError("Repair for money is not available on pond!");
			return;
		}
		if (!this.CheckProfile())
		{
			return;
		}
		int num;
		float num2;
		string text;
		this.profile.Inventory.PreviewRepair(item, out num, out num2, out text);
		if (num == 0)
		{
			Debug.LogError("Nothing to repair!");
			return;
		}
		if ((double)num2 > this.profile.GetBalance(text))
		{
			Debug.LogError("Not enough money to repair!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(185);
		operationRequest.Parameters[194] = item.InstanceId.ToString();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(185);
	}

	public void SetLeaderLength(Rod rod)
	{
		if (rod == null)
		{
			throw new ArgumentNullException("rod");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 6;
		operationRequest.Parameters[1] = rod.InstanceId.ToString();
		operationRequest.Parameters[7] = rod.LeaderLength;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 6);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnSetLeaderLength;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnSetLeaderLengthFailed;

	public void ConsumeItem(InventoryItem item, int itemCount)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.Log("Not connected to game server or not authenticated");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 7;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[4] = itemCount;
		operationRequest.Parameters[8] = GameFactory.Player.transform.position.ToPoint3();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 7);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnItemConsumed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnConsumeItemFailed;

	public void MakeGift(Player receiver, InventoryItem item, int itemCount)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.Log("Not connected to game server or not authenticated");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 8;
		operationRequest.Parameters[1] = item.InstanceId.ToString();
		operationRequest.Parameters[4] = itemCount;
		operationRequest.Parameters[9] = receiver.UserId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 8);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnGiftMade;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnMakingGiftFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Player, IEnumerable<InventoryItem>> OnItemGifted;

	public void RodSetupSaveNew(int slot, string name)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (!this.profile.Inventory.CanSaveNewSetup(slot, name))
		{
			Debug.LogError("Can't save new setup! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 17;
		operationRequest.Parameters[2] = slot;
		operationRequest.Parameters[3] = name;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 17);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnSaveNewRodSetup OnSaveNewRodSetup;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnSaveNewRodSetupFailure;

	public void RodSetupRemove(RodSetup setup)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (setup == null)
		{
			throw new ArgumentNullException("setup");
		}
		if (!this.profile.Inventory.CanRemoveSetup(setup))
		{
			Debug.LogError("Can't remove setup! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 18;
		operationRequest.Parameters[1] = setup.InstanceId.ToString();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 18);
		Debug.Log("RodSetup removed locally");
		this.profile.Inventory.RemoveSetup(setup);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnRemoveRodSetup OnRodSetupRemove;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnRodSetupRemoveFailure;

	public void RodSetupRename(RodSetup setup, string name)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (setup == null)
		{
			throw new ArgumentNullException("setup");
		}
		if (!this.profile.Inventory.CanRenameSetup(setup, name))
		{
			Debug.LogError("Can't rename setup! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 19;
		operationRequest.Parameters[1] = setup.InstanceId.ToString();
		operationRequest.Parameters[3] = name;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 19);
		Debug.Log("RodSetup renamed locally");
		this.profile.Inventory.RenameSetup(setup, name);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnRenameRodSetup OnRodSetupRename;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnRodSetupRenameFailure;

	public void RodSetupEquip(RodSetup setup, int slot)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (setup == null)
		{
			throw new ArgumentNullException("setup");
		}
		if (!this.profile.Inventory.CanEquipSetup(setup, slot))
		{
			Debug.LogError("Can't equip setup! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 20;
		operationRequest.Parameters[1] = setup.InstanceId.ToString();
		operationRequest.Parameters[2] = slot;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 20);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnEquipRodSetup OnRodSetupEquip;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnRodSetupEquipFailure;

	public void RodSetupShare(RodSetup setup, string receiver)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (setup == null)
		{
			throw new ArgumentNullException("setup");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 21;
		operationRequest.Parameters[1] = setup.InstanceId.ToString();
		operationRequest.Parameters[4] = receiver;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 21);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnShareRodSetup OnSharedRodSetup;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnShareRodSetupFailure;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnReceiveNewRodSetup OnReceiveNewRodSetup;

	public void RodSetupAdd(RodSetup setup)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (!this.profile.Inventory.CanAddSetup(setup))
		{
			Debug.LogError("Can't save new setup! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 22;
		operationRequest.Parameters[192] = CompressHelper.CompressString(JsonConvert.SerializeObject(setup, SerializationHelper.JsonSerializerSettings));
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 22);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnSaveNewRodSetup OnAddRodSetup;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnAddRodSetupFailure;

	private void HandleRodSetupOperationResponse(OperationResponse response)
	{
		InventoryOperationCode? inventoryOperationCode = null;
		if (response.Parameters.ContainsKey(191))
		{
			inventoryOperationCode = new InventoryOperationCode?((byte)response.Parameters[191]);
		}
		if (response.ReturnCode == 0)
		{
			try
			{
				if (inventoryOperationCode == 17)
				{
					string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
					RodSetup rodSetup = JsonConvert.DeserializeObject<RodSetup>(text, SerializationHelper.JsonSerializerSettings);
					this.profile.Inventory.AddSetup(rodSetup);
					if (this.OnSaveNewRodSetup != null)
					{
						this.OnSaveNewRodSetup(rodSetup);
					}
				}
				else if (inventoryOperationCode == 18)
				{
					Guid instanceId2 = new Guid((string)response.Parameters[1]);
					RodSetup rodSetup2 = this.profile.InventoryRodSetups.FirstOrDefault((RodSetup s) => s.InstanceId == instanceId2);
					if (this.OnRodSetupRemove != null)
					{
						this.OnRodSetupRemove(rodSetup2);
					}
				}
				else if (inventoryOperationCode == 19)
				{
					Guid instanceId3 = new Guid((string)response.Parameters[1]);
					RodSetup rodSetup3 = this.profile.InventoryRodSetups.FirstOrDefault((RodSetup s) => s.InstanceId == instanceId3);
					if (this.OnRodSetupRename != null)
					{
						this.OnRodSetupRename(rodSetup3);
					}
				}
				else if (inventoryOperationCode == 20)
				{
					string text2 = CompressHelper.DecompressString((byte[])response.Parameters[192]);
					InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text2, SerializationHelper.JsonSerializerSettings);
					Guid instanceId = new Guid((string)response.Parameters[1]);
					int num = (int)response.Parameters[2];
					RodSetup rodSetup4 = this.profile.InventoryRodSetups.FirstOrDefault((RodSetup s) => s.InstanceId == instanceId);
					Debug.Log("RodSetup equipped locally");
					this.profile.Inventory.EquipSetup(rodSetup4, num, inventoryMovementMapInfo);
					if (this.OnRodSetupEquip != null)
					{
						this.OnRodSetupEquip(rodSetup4);
					}
				}
				else if (inventoryOperationCode == 21)
				{
					string text3 = CompressHelper.DecompressString((byte[])response.Parameters[192]);
					RodSetup rodSetup5 = JsonConvert.DeserializeObject<RodSetup>(text3, SerializationHelper.JsonSerializerSettings);
					string text4 = (string)response.Parameters[4];
					if (this.OnSharedRodSetup != null)
					{
						this.OnSharedRodSetup(rodSetup5, text4);
					}
				}
				if (inventoryOperationCode == 22)
				{
					string text5 = CompressHelper.DecompressString((byte[])response.Parameters[192]);
					RodSetup rodSetup6 = JsonConvert.DeserializeObject<RodSetup>(text5, SerializationHelper.JsonSerializerSettings);
					this.profile.Inventory.AddSetup(rodSetup6);
					if (this.OnAddRodSetup != null)
					{
						this.OnAddRodSetup(rodSetup6);
					}
				}
				this.RaiseInventoryUpdated(false);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				this.PinError(string.Format("CLIENT !INVENTORY-UNSYNC! on call {2} [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber, inventoryOperationCode) + ex.Message, this.lastInventoryStackTrace);
				if (!ex.Message.Contains("!INVENTORY-UNSYNC!"))
				{
					throw;
				}
				Failure failure = new Failure(response);
				if (this.OnInventoryUpdatedFailure != null)
				{
					this.OnInventoryUpdatedFailure(failure);
				}
				if (inventoryOperationCode == 20)
				{
					this.RequestProfileOnError(null, false);
				}
			}
		}
		else
		{
			Failure failure2 = new Failure(response);
			Debug.LogError(failure2.FullErrorInfo);
			this.PinError(string.Format("SERVER !INVENTORY-UNSYNC! on CLIENT call [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber) + failure2.ErrorMessage, this.lastInventoryStackTrace);
			if (inventoryOperationCode == 17)
			{
				if (this.OnSaveNewRodSetupFailure != null)
				{
					this.OnSaveNewRodSetupFailure(failure2);
				}
			}
			else if (inventoryOperationCode == 18)
			{
				if (this.OnRodSetupRemoveFailure != null)
				{
					this.OnRodSetupRemoveFailure(failure2);
				}
			}
			else if (inventoryOperationCode == 19 && this.OnRodSetupRenameFailure != null)
			{
				this.OnRodSetupRenameFailure(failure2);
			}
			if (inventoryOperationCode == 20)
			{
				if (this.OnRodSetupEquipFailure != null)
				{
					this.OnRodSetupEquipFailure(failure2);
				}
			}
			else if (inventoryOperationCode == 21 && this.OnShareRodSetupFailure != null)
			{
				this.OnShareRodSetupFailure(failure2);
			}
			if (inventoryOperationCode == 22 && this.OnAddRodSetupFailure != null)
			{
				this.OnAddRodSetupFailure(failure2);
			}
			if (this.OnInventoryUpdatedFailure != null)
			{
				this.OnInventoryUpdatedFailure(failure2);
			}
			if (inventoryOperationCode == 20)
			{
				this.RequestProfileOnError(null, false);
			}
		}
	}

	public void MixChum(Chum chum)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (chum == null)
		{
			throw new ArgumentNullException("chum");
		}
		if (!this.profile.Inventory.CanMixChum(chum))
		{
			Debug.LogError("Can't mix chum! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		this.profile.Inventory.MixingChum = chum;
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 23;
		operationRequest.Parameters[192] = CompressHelper.CompressString(JsonConvert.SerializeObject(chum, SerializationHelper.JsonSerializerSettings));
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 23);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Chum> OnChumMix;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnChumMixFailure;

	public void RenameChum(Chum chum, string name)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (chum == null)
		{
			throw new ArgumentNullException("chum");
		}
		if (!this.profile.Inventory.CanRenameChum(chum, name))
		{
			Debug.LogError("Can't rename chum! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 24;
		operationRequest.Parameters[194] = chum.InstanceId.ToString();
		operationRequest.Parameters[244] = name;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 24);
		Debug.Log("Chum renamed locally");
		this.profile.Inventory.RenameChum(chum, name);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Chum> OnChumRename;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnChumRenameFailure;

	public void ChumRecipeShare(Chum recipe, string receiver)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (recipe == null)
		{
			throw new ArgumentNullException("recipe");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 32;
		operationRequest.Parameters[1] = recipe.InstanceId.ToString();
		operationRequest.Parameters[9] = receiver;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 32);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnShareChumRecipe OnChumRecipeShare;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnChumRecipeShareFailure;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnReceiveNewChumRecipe OnReceiveNewChumRecipe;

	private void HandleChumOperationResponse(OperationResponse response)
	{
		InventoryOperationCode? inventoryOperationCode = null;
		if (response.Parameters.ContainsKey(191))
		{
			inventoryOperationCode = new InventoryOperationCode?((byte)response.Parameters[191]);
		}
		if (response.ReturnCode == 0)
		{
			try
			{
				if (inventoryOperationCode == 23)
				{
					string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
					InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
					Chum chum = this.profile.Inventory.MixChum(this.profile.Inventory.MixingChum, inventoryMovementMapInfo);
					Debug.Log("Chum mixed locally");
					if (this.OnChumMix != null)
					{
						this.OnChumMix(chum);
					}
					this.profile.Inventory.MixingChum = null;
				}
				else if (inventoryOperationCode == 24)
				{
					Guid instanceId = new Guid((string)response.Parameters[194]);
					Chum chum2 = (Chum)this.profile.Inventory.FirstOrDefault((InventoryItem e) => e.InstanceId == instanceId);
					if (this.OnChumRename != null)
					{
						this.OnChumRename(chum2);
					}
				}
				this.RaiseInventoryUpdated(false);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				this.PinError(string.Format("CLIENT !INVENTORY-UNSYNC! on call {2} [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber, inventoryOperationCode) + ex.Message, this.lastInventoryStackTrace);
				if (!ex.Message.Contains("!INVENTORY-UNSYNC!"))
				{
					throw;
				}
				Failure failure = new Failure(response);
				if (this.OnInventoryUpdatedFailure != null)
				{
					this.OnInventoryUpdatedFailure(failure);
				}
			}
		}
		else
		{
			Failure failure2 = new Failure(response);
			Debug.LogError(failure2.FullErrorInfo);
			this.PinError(string.Format("SERVER !INVENTORY-UNSYNC! on CLIENT call [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber) + failure2.ErrorMessage, this.lastInventoryStackTrace);
			if (inventoryOperationCode == 23)
			{
				if (this.OnChumMixFailure != null)
				{
					this.OnChumMixFailure(failure2);
				}
				this.profile.Inventory.MixingChum = null;
			}
			else if (inventoryOperationCode == 24 && this.OnChumRenameFailure != null)
			{
				this.OnChumRenameFailure(failure2);
			}
			if (this.OnInventoryUpdatedFailure != null)
			{
				this.OnInventoryUpdatedFailure(failure2);
			}
		}
	}

	public void ChumRecipeSaveNew(Chum chum)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (chum == null)
		{
			throw new ArgumentNullException("chum");
		}
		if (!this.profile.Inventory.CanSaveNewChumRecipe(chum))
		{
			Debug.LogError("Can't save ChumRecipe! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		this.profile.Inventory.MixingChum = chum;
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 29;
		operationRequest.Parameters[192] = CompressHelper.CompressString(JsonConvert.SerializeObject(chum, SerializationHelper.JsonSerializerSettings));
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 29);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Chum> OnChumRecipeSaveNew;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnChumRecipeSaveNewFailure;

	public void ChumRecipeRename(Chum chum, string name)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (chum == null)
		{
			throw new ArgumentNullException("chum");
		}
		if (!this.profile.Inventory.CanRenameChumRecipe(chum, name))
		{
			Debug.LogError("Can't rename ChumRecipe! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 30;
		operationRequest.Parameters[194] = chum.InstanceId.ToString();
		operationRequest.Parameters[244] = name;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 30);
		Debug.Log("ChumRecipe renamed locally");
		this.profile.Inventory.RenameChumRecipe(chum, name);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Chum> OnChumRecipeRename;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnChumRecipeRenameFailure;

	public void ChumRecipeRemove(Chum chum)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		if (chum == null)
		{
			throw new ArgumentNullException("chum");
		}
		if (!this.profile.Inventory.CanRemoveChumRecipe(chum))
		{
			Debug.LogError("Can't remove ChumRecipe! " + this.profile.Inventory.LastVerificationError);
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 31;
		operationRequest.Parameters[194] = chum.InstanceId.ToString();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 31);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Chum> OnChumRecipeRemove;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnChumRecipeRemoveFailure;

	private void HandleChumRecipeOperationResponse(OperationResponse response)
	{
		InventoryOperationCode? inventoryOperationCode = null;
		if (response.Parameters.ContainsKey(191))
		{
			inventoryOperationCode = new InventoryOperationCode?((byte)response.Parameters[191]);
		}
		if (response.ReturnCode == 0)
		{
			try
			{
				if (inventoryOperationCode == 29)
				{
					string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
					InventoryMovementMapInfo inventoryMovementMapInfo = JsonConvert.DeserializeObject<InventoryMovementMapInfo>(text, SerializationHelper.JsonSerializerSettings);
					Debug.Log("ChumRecipe saved locally");
					this.profile.Inventory.SaveNewChumRecipe(this.profile.Inventory.MixingChum, inventoryMovementMapInfo);
					if (this.OnChumRecipeSaveNew != null)
					{
						this.OnChumRecipeSaveNew(this.profile.Inventory.MixingChum);
					}
					this.profile.Inventory.MixingChum = null;
				}
				else if (inventoryOperationCode == 30)
				{
					Guid instanceId2 = new Guid((string)response.Parameters[194]);
					Chum chum = this.profile.ChumRecipes.FirstOrDefault((Chum e) => e.InstanceId == instanceId2);
					if (this.OnChumRecipeRename != null)
					{
						this.OnChumRecipeRename(chum);
					}
				}
				else if (inventoryOperationCode == 31)
				{
					Guid instanceId = new Guid((string)response.Parameters[194]);
					Chum chum2 = this.profile.ChumRecipes.FirstOrDefault((Chum e) => e.InstanceId == instanceId);
					Debug.Log("ChumRecipe removed locally");
					this.profile.Inventory.RemoveChumRecipe(chum2);
					if (this.OnChumRecipeRemove != null)
					{
						this.OnChumRecipeRemove(chum2);
					}
				}
				else if (inventoryOperationCode == 32)
				{
					string text2 = CompressHelper.DecompressString((byte[])response.Parameters[192]);
					Chum chum3 = JsonConvert.DeserializeObject<Chum>(text2, SerializationHelper.JsonSerializerSettings);
					string text3 = (string)response.Parameters[9];
					if (this.OnChumRecipeShare != null)
					{
						this.OnChumRecipeShare(chum3, text3);
					}
				}
				this.RaiseInventoryUpdated(false);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				this.PinError(string.Format("CLIENT !INVENTORY-UNSYNC! on call {2} [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber, inventoryOperationCode) + ex.Message, this.lastInventoryStackTrace);
				if (!ex.Message.Contains("!INVENTORY-UNSYNC!"))
				{
					throw;
				}
				Failure failure = new Failure(response);
				if (this.OnInventoryUpdatedFailure != null)
				{
					this.OnInventoryUpdatedFailure(failure);
				}
			}
		}
		else
		{
			Failure failure2 = new Failure(response);
			Debug.LogError(failure2.FullErrorInfo);
			this.PinError(string.Format("SERVER !INVENTORY-UNSYNC! on CLIENT call [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber) + failure2.ErrorMessage, this.lastInventoryStackTrace);
			if (inventoryOperationCode == 29)
			{
				if (this.OnChumRecipeSaveNewFailure != null)
				{
					this.OnChumRecipeSaveNewFailure(failure2);
				}
				this.profile.Inventory.MixingChum = null;
			}
			else if (inventoryOperationCode == 30)
			{
				if (this.OnChumRecipeRenameFailure != null)
				{
					this.OnChumRecipeRenameFailure(failure2);
				}
			}
			else if (inventoryOperationCode == 31)
			{
				if (this.OnChumRecipeRemoveFailure != null)
				{
					this.OnChumRecipeRemoveFailure(failure2);
				}
			}
			else if (inventoryOperationCode == 32 && this.OnChumRecipeShareFailure != null)
			{
				this.OnChumRecipeShareFailure(failure2);
			}
			if (this.OnInventoryUpdatedFailure != null)
			{
				this.OnInventoryUpdatedFailure(failure2);
			}
		}
	}

	public int? CurrentPondId { get; private set; }

	public int? CurrentTournamentId { get; private set; }

	public TimeSpan? TournamentRemainingTime { get; private set; }

	public DateTime? TournamentRemainingTimeReceived { get; private set; }

	public bool IsInRoom { get; private set; }

	public RoomDesc Room { get; private set; }

	public bool IsOnBase { get; private set; }

	public int MaxRoomCapacity { get; private set; }

	public bool LockConnection()
	{
		if (!this.isConnectionLocked)
		{
			this.isConnectionLocked = true;
			DebugUtility.Travel.Trace("Travel action: Lock", new object[0]);
			return this.isConnectionLocked;
		}
		return false;
	}

	public void ReleaseConnection()
	{
		this.isConnectionLocked = false;
		DebugUtility.Travel.Trace("Travel action: Release", new object[0]);
	}

	public void DispatchTravelActions()
	{
		if (this.cachedActionQueue.Count == 0)
		{
			return;
		}
		if (this.LockConnection())
		{
			this.processingTime = 0f;
			while (this.cachedActionQueue.Count > 0)
			{
				Action action = this.cachedActionQueue.Dequeue();
				action();
			}
		}
		else
		{
			this.processingTime += Time.deltaTime;
			if (this.processingTime > 30f)
			{
				this.ReleaseConnection();
			}
		}
	}

	private void ExecuteTravelAction(Action action)
	{
		if (this.LockConnection())
		{
			action();
		}
		else
		{
			Debug.Log("Travel action deferred");
			this.cachedActionQueue.Enqueue(action);
		}
	}

	public void MoveToBase()
	{
		this.ExecuteTravelAction(new Action(this.MoveToBaseInt));
	}

	private void MoveToBaseInt()
	{
		if (!this.CheckIsAuthed())
		{
			return;
		}
		if (this.isMovingToBase)
		{
			Debug.LogError("Already moving to Base!");
			return;
		}
		if (this.IsOnBase && this.CurrentPondId != null)
		{
			this.InternalMoveToBase();
			return;
		}
		if (this.IsOnBase)
		{
			Debug.LogWarning("Already on Base!");
			this.Moved(TravelDestination.Base);
			return;
		}
		DebugUtility.Travel.Trace("MoveToBaseInt", new object[0]);
		this.CurrentPondId = null;
		this.CurrentTournamentId = null;
		this.IsInRoom = false;
		this.Room.Clear();
		DebugUtility.RoomIdIssue.Trace("Clear RoomId", new object[0]);
		this.isMovingToBase = true;
		this.raiseJoinErrorEvents = true;
		if (this.IsConnectedToMaster)
		{
			this.Peer.OpJoinLobby(null);
			OpTimer.Start(229);
			return;
		}
		this.DisconnectToReconnect(this.MasterServerAddress, null, null);
	}

	public void MoveToPond(int pondId, int? days, bool? hasMovedByCar = null)
	{
		this.ExecuteTravelAction(delegate
		{
			this.MoveToPondInt(pondId, days, hasMovedByCar);
		});
	}

	private void MoveToPondInt(int pondId, int? days, bool? hasMovedByCar)
	{
		if (!this.CheckIsAuthed())
		{
			return;
		}
		if (this.CurrentPondId == pondId)
		{
			Debug.LogWarning("Already at pond " + pondId);
			return;
		}
		if (pondId == 0)
		{
			Debug.LogError("Can't use MoveToPond as MoveToBase!");
			return;
		}
		this.CurrentPondId = null;
		this.CurrentTournamentId = null;
		this.pondToEnter = new int?(pondId);
		this.daysToStay = days;
		this.isMovingByCar = hasMovedByCar;
		if (!this.IsOnBase)
		{
			this.MoveToBaseInt();
			return;
		}
		this.IsOnBase = false;
		this.InternalChangePond();
	}

	public void MoveToRoom(int pondId, string roomName = null)
	{
		this.ExecuteTravelAction(delegate
		{
			this.MoveToRoomInt(pondId, roomName);
		});
	}

	private void MoveToRoomInt(int pondId, string roomName)
	{
		if (!this.CheckIsAuthed())
		{
			return;
		}
		this.isRoomCreatedOnMaster = false;
		if (this.CurrentPondId == null || this.CurrentPondId == 0)
		{
			Debug.LogError("Not on pond to move to room!");
			return;
		}
		if (pondId == 0)
		{
			Debug.LogError("Can't use MoveToRoom as MoveToBase!");
			return;
		}
		if (this.CurrentPondId != pondId)
		{
			Debug.LogErrorFormat("You are currently on pond {0}, but moving to room on pond {1}!", new object[] { this.CurrentPondId, pondId });
			return;
		}
		if (roomName == "random")
		{
			string text = string.Format("___kocha ::MoveToRoomInt roomName == SharedConsts.RandomRoomName pondId:{0} roomName:{1}", pondId, roomName);
			LogHelper.Error("{0} StackTrace:{1}", new object[]
			{
				text,
				global::System.Environment.StackTrace
			});
			PhotonConnectionFactory.Instance.PinError(text, global::System.Environment.StackTrace);
			roomName = null;
		}
		string text2 = roomName ?? "random";
		if (this.CurrentPondId == pondId && this.IsInRoom)
		{
			int? currentTournamentId = this.CurrentTournamentId;
			int valueOrDefault = currentTournamentId.GetValueOrDefault();
			int? num = this.tournamentToEnter;
			if (valueOrDefault == num.GetValueOrDefault() && currentTournamentId != null == (num != null) && this.priorRoomName == text2)
			{
				this.Moved(TravelDestination.Room);
				return;
			}
		}
		DebugUtility.Travel.Trace("MoveToRoomInt", new object[0]);
		this.priorRoomName = text2;
		this.raiseJoinErrorEvents = roomName != "top";
		this.isMovingToRoom = true;
		this.IsInRoom = false;
		this.Room.Clear();
		DebugUtility.RoomIdIssue.Trace("Clear RoomId", new object[0]);
		this.IsOnBase = false;
		this.pondToEnter = new int?(pondId);
		this.roomToEnter = roomName;
		DebugUtility.RoomIdIssue.Trace("Set roomName = {0}", new object[] { roomName });
		if (this.IsConnectedToMaster)
		{
			this.Peer.OpJoinLobby(null);
			OpTimer.Start(229);
			return;
		}
		this.DisconnectToReconnect(this.MasterServerAddress, null, null);
	}

	public void MoveToTournamentRoom(int pondId, int tournamentId)
	{
		this.ExecuteTravelAction(delegate
		{
			this.MoveToTournamentRoomInt(pondId, tournamentId);
		});
	}

	private void MoveToTournamentRoomInt(int pondId, int tournamentId)
	{
		if (!this.CheckIsAuthed())
		{
			return;
		}
		if (this.CurrentPondId == null || this.CurrentPondId == 0)
		{
			Debug.LogError("Not on pond to move to room!");
			return;
		}
		if (pondId == 0)
		{
			Debug.LogError("Can't use MoveToRoom as MoveToBase!");
			return;
		}
		if (this.CurrentPondId != pondId)
		{
			Debug.LogErrorFormat("You are currently on pond {0}, but moving to room on pond {1}!", new object[] { this.CurrentPondId, pondId });
			return;
		}
		if (this.CurrentPondId == pondId && this.IsInRoom && this.CurrentTournamentId == tournamentId)
		{
			this.Moved(TravelDestination.Room);
			return;
		}
		DebugUtility.Travel.Trace("MoveToTournamentRoomInt", new object[0]);
		this.raiseJoinErrorEvents = true;
		this.isMovingToRoom = true;
		this.IsInRoom = false;
		this.Room.Clear();
		DebugUtility.RoomIdIssue.Trace("Clear RoomId", new object[0]);
		this.IsOnBase = false;
		this.pondToEnter = new int?(pondId);
		this.tournamentToEnter = new int?(tournamentId);
		this.roomToEnter = null;
		DebugUtility.RoomIdIssue.Trace("Clear roomToEnter", new object[0]);
		if (this.IsConnectedToMaster)
		{
			this.Peer.OpJoinLobby(null);
			OpTimer.Start(229);
			return;
		}
		this.DisconnectToReconnect(this.MasterServerAddress, null, null);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMoved OnMoved;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnMoveFailed;

	private void Moved(TravelDestination destination)
	{
		this.ReleaseConnection();
		if (this.OnMoved != null)
		{
			this.OnMoved(destination);
		}
	}

	private void MoveFailed(Failure failure)
	{
		this.ReleaseConnection();
		if (this.OnMoveFailed != null)
		{
			this.OnMoveFailed(failure);
		}
	}

	public void ProlongPondStay(int days)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connectd to game server or not authenticated");
			return;
		}
		if (!this.CheckProfile())
		{
			Debug.LogError("No profile!");
			return;
		}
		if (this.CurrentPondId == null)
		{
			Debug.LogError("Not on pond!");
			return;
		}
		int? num = this.daysToAdd;
		if (num != null)
		{
			Debug.LogError("Prolong had already been initiated!");
			return;
		}
		if (this.Profile.PondStayTime != null && this.Profile.PondStayTime.Value + days > 180)
		{
			Debug.LogError("Can not stay longer!");
			return;
		}
		this.daysToAdd = new int?(days);
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 242;
		operationRequest.Parameters[181] = days;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 242);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ProlongInfo> OnPondStayProlonged;

	private void ValidateRoomCreation(string tag)
	{
		if (this.IsConnectedToMaster)
		{
			this.isRoomCreatedOnMaster = true;
			return;
		}
		if (this.IsConnectedToGameServer && !this.isRoomCreatedOnMaster)
		{
			string text = "Room was not created on Master while it is being created on Game";
			Debug.LogError(text + ", tag: " + tag);
			PhotonConnectionFactory.Instance.PinError(text, tag);
		}
	}

	public void GetAvailablePonds(int countryId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(198);
		operationRequest.Parameters[194] = countryId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(198);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<IEnumerable<PondBrief>> OnGotAvailablePonds;

	public void GetAvailableLocations(int? pondId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(197);
		if (pondId != null)
		{
			operationRequest.Parameters[194] = pondId;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(197);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<IEnumerable<LocationBrief>> OnGotAvailableLocations;

	public void GetPondInfo(int pondId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(196);
		operationRequest.Parameters[194] = pondId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(196);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Pond> OnGotPondInfo;

	public void GetPondWeather(int pondId, int days = 1)
	{
		MonoBehaviour.print("get pond weather called");
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			return;
		}
		MonoBehaviour.print("get pond weather performes");
		OperationRequest operationRequest;
		if (days == 1)
		{
			operationRequest = PhotonServerConnection.CreateOperationRequest(178);
		}
		else
		{
			operationRequest = PhotonServerConnection.CreateOperationRequest(174);
			operationRequest.Parameters[188] = days;
		}
		operationRequest.Parameters[194] = pondId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(operationRequest.OperationCode);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotPondWeather OnGotPondWeather;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotPondWeather OnGotPondWeatherForecast;

	public void GetAllPondsWeather()
	{
		MonoBehaviour.print("get all pond weather called");
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			return;
		}
		MonoBehaviour.print("get all pond weather performes");
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(122);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(operationRequest.OperationCode);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotAllPondWeather OnGotAllPondWeather;

	public void GetLocationInfo(int locationId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(195);
		operationRequest.Parameters[194] = locationId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(195);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Location> OnGotLocationInfo;

	public void GetLicenses(int? stateId = null, int? pondId = null, int? regionId = null)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(181);
		if (stateId != null)
		{
			operationRequest.Parameters[194] = stateId.Value;
		}
		if (pondId != null)
		{
			operationRequest.Parameters[184] = pondId.Value;
		}
		if (regionId != null)
		{
			operationRequest.Parameters[219] = regionId.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(181);
	}

	public void GetStates(int countryId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(182);
		operationRequest.Parameters[194] = countryId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(182);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<IEnumerable<State>> OnGotStates;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingStatesFailed;

	public void PreviewChangeResidenceState(int stateId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(163);
		operationRequest.Parameters[194] = stateId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(163);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ChangeResidenceInfo> OnPreviewResidenceChange;

	public void ChangeResidenceState(int stateId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(162);
		operationRequest.Parameters[194] = stateId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(162);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ChangeResidenceInfo> OnResidenceChanged;

	public void GetMinTravelCost()
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(160);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(160);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> OnGotMinTravelCost;

	public void FinishTutorial()
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(159);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(159);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnTutorialFinished;

	public void GetPondStats(int? pondId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(152);
		if (pondId != null)
		{
			operationRequest.Parameters[184] = pondId.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(152);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotPondStats OnGotPondStats;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingPondStatsFailed;

	public void GetRoomsPopulation(int pondId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(151);
		operationRequest.Parameters[188] = pondId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(151);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotRoomsPopulation OnGotRoomsPopulation;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingRoomsPopulationFailed;

	public void GetCurrentRoomPopulation()
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(148);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(148);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnPlayersAction OnGotCurrentRoomPopulation;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingCurrenRoomPopulationFailed;

	public void GetOwnRoom()
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 226;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 226);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<string> OnGotOwnRoom;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingOwnRoomFailed;

	public void GetInteractiveObjects(int pondId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(150);
		operationRequest.Parameters[184] = pondId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(150);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotInteractiveObjects OnGotInteractiveObjects;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingInteractiveObjectsFailed;

	public void InteractWithObject(InteractiveObject obj)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		if (this.lastInteractedObject != null)
		{
			Debug.LogError("Already interacting with object!");
			return;
		}
		this.lastInteractedObject = obj;
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(149);
		operationRequest.Parameters[194] = obj.ObjectId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(149);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnInteractedWithObject OnInteractedWithObject;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnInteractWithObjectFailed;

	public IDictionary<int, Pond> PondInfos
	{
		get
		{
			return this.pondInfos;
		}
	}

	public void GetFishList(int[] fishIds)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		string text = string.Join(",", fishIds.Select((int x) => x.ToString()).ToArray<string>());
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(143);
		operationRequest.Parameters[245] = fishIds;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(143);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotFishList OnGotFishList;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnErrorGettingFishList;

	public void GetFishCategories()
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(142);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(142);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotFishCategories OnGotFishCategories;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnErrorGettingFishCategories;

	public void RentBoat(int boatId, int daysCount)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		if (this.CheckProfile() && this.Profile.PondId == null)
		{
			Debug.LogError("Not on pond to rent a boat!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(133);
		operationRequest.Parameters[194] = boatId;
		operationRequest.Parameters[183] = daysCount;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(133);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnBoatRented;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnErrorRentingBoat;

	public void OutdateRent()
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(124);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(124);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnOutdateRent;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnErrorOutdateRent;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<short[]> OnBoatsRentFinished = delegate
	{
	};

	private void HandleRentBoatResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			Failure failure = new Failure(response);
			Debug.LogError(failure.FullErrorInfo);
			if (this.OnErrorRentingBoat != null)
			{
				this.OnErrorRentingBoat(failure);
			}
			return;
		}
		if (!response.Parameters.ContainsKey(192))
		{
			Failure failure2 = new Failure
			{
				ErrorMessage = "Boat not rented!"
			};
			if (this.OnErrorRentingBoat != null)
			{
				this.OnErrorRentingBoat(failure2);
			}
			return;
		}
		string text = CompressHelper.DecompressString((byte[])response[192]);
		InventoryItem[] array = JsonConvert.DeserializeObject<InventoryItem[]>(text, SerializationHelper.JsonSerializerSettings);
		this.profile.AddBoatRent(array);
		if (this.OnBoatRented != null)
		{
			this.OnBoatRented();
		}
	}

	private void HandleOutdateRentResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			Failure failure = new Failure(response);
			Debug.LogError(failure.FullErrorInfo);
			if (this.OnErrorOutdateRent != null)
			{
				this.OnErrorOutdateRent(failure);
			}
			return;
		}
		if (this.OnOutdateRent != null)
		{
			this.OnOutdateRent();
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<PondStayFinish> OnPondStayFinish;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnIncomingChatMessage OnIncomingChatMessage;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnIncomingChatMessage OnIncomingChannelMessage;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnIncognitoChanged OnIncognitoChanged;

	public void SendChatMessage(string recepient, string message, string channel = null, string group = null, string data = null, bool isOffline = false, DateTime? expiration = null)
	{
		if (!this.CheckGameConnectedAndAuthed() || !this.CheckProfile())
		{
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(221);
		operationRequest.Parameters[199] = this.UserName;
		operationRequest.Parameters[200] = this.UserId;
		operationRequest.Parameters[160] = this.Profile.Level;
		operationRequest.Parameters[147] = this.Profile.Rank;
		operationRequest.Parameters[145] = this.Profile.ExternalId;
		operationRequest.Parameters[198] = recepient;
		operationRequest.Parameters[162] = channel;
		operationRequest.Parameters[197] = group;
		operationRequest.Parameters[196] = message;
		operationRequest.Parameters[195] = data;
		operationRequest.Parameters[176] = isOffline;
		if (expiration != null)
		{
			operationRequest.Parameters[177] = expiration.Value.Ticks;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(221);
	}

	private void SendChatMessageConfirmation(string messageId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(146);
		operationRequest.Parameters[175] = messageId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	public void RefreshFriendsDetails()
	{
		if (!this.CheckProfile())
		{
			return;
		}
		if (this.Profile.Friends == null || this.Profile.Friends.Count == 0)
		{
			Debug.LogWarning("Nothing to refresh!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 235;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 235);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnFriendsDetailsRefreshed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnFriendsDetailsRefreshFailed;

	public void FindPlayersByName(string name)
	{
		if (!this.CheckGameConnectedAndAuthed())
		{
			return;
		}
		if (name.Length < 3)
		{
			throw new InvalidOperationException("Player name mask is too short");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(177);
		operationRequest.Parameters[211] = name;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(177);
	}

	public void FindPlayersByEmail(string email)
	{
		if (!this.CheckGameConnectedAndAuthed())
		{
			return;
		}
		if (email.Length < 5)
		{
			throw new InvalidOperationException("Email is too short");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(177);
		operationRequest.Parameters[194] = email;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(177);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnPlayersAction OnFindPlayers;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnFindPlayersFailed;

	public int FriendsCount
	{
		get
		{
			int num;
			if (this.profile == null || this.profile.Friends == null)
			{
				num = 0;
			}
			else
			{
				num = this.profile.Friends.Count((Player f) => f.Status != FriendStatus.Ignore);
			}
			return num;
		}
	}

	public void RequestFriendship(Player player)
	{
		if (!this.CheckProfile())
		{
			return;
		}
		Player player2 = null;
		if (this.profile.Friends != null)
		{
			player2 = this.profile.Friends.FirstOrDefault((Player f) => f.UserId.Equals(player.UserId, StringComparison.InvariantCultureIgnoreCase));
			if (player2 != null && player2.Status == FriendStatus.Friend)
			{
				Debug.LogErrorFormat("Player {0} is already your friend. Can't request friendship again!", new object[] { player.UserName });
				return;
			}
			if (this.FriendsCount >= 100)
			{
				Debug.LogErrorFormat("Maximum limit of 100 players reached!", new object[0]);
				return;
			}
		}
		player.Status = FriendStatus.FriendshipRequested;
		if (player2 == null)
		{
			this.InternalAddFriend(player);
		}
		else
		{
			player2.Status = FriendStatus.FriendshipRequested;
		}
		this.SendChatMessage(player.UserId, null, null, null, new string('F', 1), true, new DateTime?(DateTime.Now.ToUniversalTime().AddDays(99.0)));
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFriendshipRequest OnFriendshipRequest;

	public void UnFriend(Player player, bool ignore)
	{
		if (!this.CheckProfile())
		{
			return;
		}
		Player player2 = null;
		for (int i = 0; i < this.profile.Friends.Count; i++)
		{
			Player player3 = this.profile.Friends[i];
			if (player3.UserId == player.UserId)
			{
				player2 = player3;
				break;
			}
		}
		if (this.profile.Friends == null || player2 == null)
		{
			Debug.LogErrorFormat("Player {0} is not your friend. Can't remove it from friends!", new object[] { player.UserName });
			return;
		}
		if (ignore)
		{
			player2.Status = FriendStatus.Ignore;
			this.SendChatMessage(player.UserId, null, null, null, new string('e', 1), true, new DateTime?(DateTime.Now.ToUniversalTime().AddDays(99.0)));
			if (this.ignoreAsUnfriend)
			{
				this.ignoreAsUnfriend = false;
				if (this.OnChatCommandSucceeded != null)
				{
					this.OnChatCommandSucceeded(new ChatCommandInfo
					{
						Command = 2,
						Username = player2.UserName
					});
				}
			}
		}
		else
		{
			this.InternalRemoveFriend(player.UserId, false);
			this.SendChatMessage(player.UserId, null, null, null, new string('E', 1), true, new DateTime?(DateTime.Now.ToUniversalTime().AddDays(99.0)));
		}
		Guid guid = new Guid(player2.UserId);
		this.profile.RemoveBuoyShareRequestsOfPlayer(guid);
		this.RefreshFriendsDetails();
	}

	public void IgnorePlayer(Player player)
	{
		if (this.FindFriend(player.UserName) != null)
		{
			this.ignoreAsUnfriend = true;
			this.UnFriend(player, true);
		}
		else
		{
			this.SendChatCommand(2, player.UserName);
		}
	}

	public void UnIgnorePlayer(Player player)
	{
		this.SendChatCommand(3, player.UserName);
	}

	private Player FindFriend(string playerName)
	{
		for (int i = 0; i < this.Profile.Friends.Count; i++)
		{
			Player player = this.Profile.Friends[i];
			if (player.UserName.Equals(playerName, StringComparison.InvariantCultureIgnoreCase) && player.Status == FriendStatus.Friend)
			{
				return player;
			}
		}
		return null;
	}

	public void ReplyToFriendshipRequest(Player player, FriendshipAction reaction)
	{
		if (!this.CheckProfile())
		{
			return;
		}
		if (player.Status != FriendStatus.FriendshipRequest)
		{
			Debug.LogErrorFormat("Player {0} has not requested a friendship to reply to request", new object[] { player.UserName });
			return;
		}
		char c = 'N';
		if (reaction != FriendshipAction.Confirm)
		{
			if (reaction != FriendshipAction.Cancel)
			{
				if (reaction == FriendshipAction.Ignore)
				{
					c = 'I';
					player.Status = FriendStatus.Ignore;
					this.InternalAddFriend(player);
				}
			}
			else
			{
				c = 'N';
				this.InternalRemoveFriend(player.UserId, true);
			}
		}
		else
		{
			c = 'Y';
			player.Status = FriendStatus.Friend;
			this.InternalAddFriend(player);
		}
		this.SendChatMessage(player.UserId, null, null, null, string.Concat(new object[] { "Answer", 'F', c, player.UserName }), true, new DateTime?(DateTime.Now.ToUniversalTime().AddDays(99.0)));
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFriendAction OnFriendshipAdded;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFriendAction OnFriendshipRemoved;

	public void GetChatPlayersCount()
	{
		if (!this.IsConnectedToGameServer)
		{
			throw new InvalidOperationException("Not connected to Game server");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(171);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(171);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotChatPlayersCount OnGotChatPlayersCount;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingChatPlayersCountFailed;

	public void SendChatCommand(ChatCommands command, string username)
	{
		if (!this.IsConnectedToGameServer)
		{
			throw new InvalidOperationException("Not connected to Game server");
		}
		if (!string.IsNullOrEmpty(username) && this.Profile != null && username.Equals(this.Profile.Name, StringComparison.InvariantCultureIgnoreCase))
		{
			Debug.LogError("Can't run chat command for self!");
			return;
		}
		if (command == 3 && this.profile != null && this.profile.Friends != null && this.profile.Friends.All((Player p) => !p.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase)))
		{
			Debug.LogError("User to unIgnore is not currently ignored!");
			return;
		}
		if (command == 2)
		{
			Player player = this.FindFriend(username);
			if (player != null)
			{
				this.ignoreAsUnfriend = true;
				this.UnFriend(player, true);
				return;
			}
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(156);
		operationRequest.Parameters[244] = command;
		if (!string.IsNullOrEmpty(username))
		{
			operationRequest.Parameters[211] = username;
		}
		else if (command == 2 || command == 1 || command == 3)
		{
			Debug.LogError("Username should be specified!");
			return;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(156);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnChatCommandSucceeded OnChatCommandSucceeded;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnChatCommandFailed;

	public void AddFriendsByExternalIds(string source, IEnumerable<string> newFriendsList)
	{
		if (!this.CheckGameConnectedAndAuthed())
		{
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 234;
		operationRequest.Parameters[242] = source;
		operationRequest.Parameters[175] = newFriendsList.ToArray<string>();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 234);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnPlayersAction OnFriendsAddedByExternalIds;

	public void SyncFriendsByExternalIds(string source, IEnumerable<string> friendExternalIds)
	{
		if (!this.CheckGameConnectedAndAuthed())
		{
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 222;
		operationRequest.Parameters[242] = source;
		operationRequest.Parameters[175] = friendExternalIds.ToArray<string>();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 222);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnPlayersAction OnFriendsSyncedByExternalIds;

	public void JoinChatChannel(string channelId)
	{
		this.SendChatMessage(null, null, channelId, null, "join", false, new DateTime?(this.ServerUtcNow.AddMinutes(30.0)));
	}

	public void LeaveChatChannel(string channelId)
	{
		this.SendChatMessage(null, null, channelId, null, "leave", false, new DateTime?(this.ServerUtcNow.AddMinutes(30.0)));
	}

	public void SendChatMessageToChannel(string channelId, string message)
	{
		this.SendChatMessage(null, message, channelId, null, null, false, new DateTime?(this.ServerUtcNow.AddMinutes(30.0)));
	}

	public void ExpireChatChannel(string channelId, DateTime expiration)
	{
		this.SendChatMessage(null, null, channelId, null, "expire", false, new DateTime?(expiration));
	}

	public Game Game
	{
		get
		{
			return this.gameSlots[this.CurrentGameSlotIndex];
		}
	}

	public Game PendingGame
	{
		get
		{
			return this.gameSlots[this.PendingGameSlotIndex];
		}
	}

	public int CurrentGameSlotIndex { get; private set; }

	public int PendingGameSlotIndex { get; private set; }

	public Game GetGameSlot(int index)
	{
		return this.gameSlots[index];
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGameActionResult OnGameActionResult;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGameEvent OnGameEvent;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGameHint OnGameHint;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnLicensePenalty OnLicensePenalty;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnLicensePenaltyWarning OnLicensePenaltyWarning;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnItemBroken OnItemBroken;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnItemLost OnItemLost;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnItemLost OnChumLost;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnItemLost OnWearedItemLostOnDeequip;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnItemGained OnItemGained;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnRodUnequiped OnRodUnequiped;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnBaitLost OnBaitLost;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnBaitReplenished OnBaitReplenished;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnNeedClientReset OnNeedClientReset;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnRodCantBeUsed OnRodCantBeUsed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<LevelInfo> OnLevelGained;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<AchivementInfo> OnAchivementGained;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BonusInfo> OnBonusGained;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<HashSet<ushort>> OnFBAZonesUpdate;

	public void GetSounderFish(Point3 position)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(125);
		operationRequest.Parameters[188] = position;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(125);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<float[]> OnGotSounderFish;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingSounderFishFailed;

	private void HandleGetSounderFishResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			float[] array = SerializationHelper.DeserializeSounderInfo((byte[])response.Parameters[245]);
			if (this.OnGotSounderFish != null)
			{
				this.OnGotSounderFish(array);
			}
		}
		else
		{
			Failure failure = new Failure(response);
			Debug.LogError(failure.FullErrorInfo);
			if (this.OnGettingSounderFishFailed != null)
			{
				this.OnGettingSounderFishFailed(failure);
			}
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotWhatsNewList OnGotWhatsNewList;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingWhatsNewListFailed;

	public void GetAbTestSelection(int[] testIds)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 8;
		operationRequest.Parameters[194] = testIds;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(167, 8);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<Dictionary<int, bool>> OnGotAbTestSelection;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingAbTestSelectionFailed;

	public void SpawnFish(int slot, FishName fishName, FishForm fishForm, float? fishWeight = null, float? biteTime = null, bool? hookResult = null)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 9;
		operationRequest.Parameters[218] = slot;
		operationRequest.Parameters[194] = (int)fishName;
		operationRequest.Parameters[191] = (int)fishForm;
		if (fishWeight != null)
		{
			operationRequest.Parameters[244] = fishWeight;
		}
		if (hookResult != null)
		{
			operationRequest.Parameters[193] = hookResult;
		}
		if (biteTime != null)
		{
			operationRequest.Parameters[188] = biteTime;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(167, 9);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<XboxGoldAccountNeedReason> OnXboxGoldAccountNeeded;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event ServerCachesRefreshedHandler ServerCachesRefreshed;

	public void GetAllPondInfos(int countryId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 1;
		operationRequest.Parameters[194] = countryId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 1);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotAllPondInfos OnGotAllPondInfos;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingAllPondInfosFailed;

	public void GetAllPondLevels(int countryId)
	{
		if (!this.IsConnectedToGameServer)
		{
			Debug.LogError("Not connected to game server!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 2;
		operationRequest.Parameters[194] = countryId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 2);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotAllPondLevels OnGotAllPondLevels;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingAllPondLevelsFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPondLevelInvalidated;

	public void GetInventoryItemAssets()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 3;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 3);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotInventoryItemAssets OnGotInventoryItemAssets;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingInventoryItemAssetsFailed;

	public void GetFishAssets()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 4;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 4);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotFishAssets OnGotFishAssets;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingFishAssetsFailed;

	public void GetFishBrief()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 9;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 9);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotFishBrief OnGotFishBrief;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingFishBriefFailed;

	public void GetGlobalVariables()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 5;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 5);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotGlobalVariables OnGotGlobalVariables;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingGlobalVariablesFailed;

	public void GetBoats()
	{
		MonoBehaviour.print("get boats called");
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 6;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 6);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotBoats OnGotBoats;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingBoatsFailed;

	private void HandleClientCacheResponse(OperationResponse response)
	{
		ClientCacheOperationCode clientCacheOperationCode = (ClientCacheOperationCode)response.Parameters[191];
		OpTimer.End(140, clientCacheOperationCode);
		if (response.ReturnCode == 0)
		{
			switch (clientCacheOperationCode)
			{
			case 1:
				AsynchProcessor.ProcessDictionaryWorkItem(new InputDictionaryWorkItem
				{
					Data = response.Parameters,
					ProcessAction = (Dictionary<byte, object> parameters) => TravelSerializationHelper.DeserializePonds(response.Parameters),
					ResultAction = delegate(object r)
					{
						List<Pond> list5 = (List<Pond>)r;
						foreach (Pond pond in list5)
						{
							this.pondInfos[pond.PondId] = pond;
						}
						if (this.OnGotAllPondInfos != null)
						{
							this.OnGotAllPondInfos(list5);
						}
					}
				});
				break;
			case 2:
				if (this.OnGotAllPondLevels != null)
				{
					List<PondLevelInfo> list = TravelSerializationHelper.DeserializePondLevels(response.Parameters);
					this.OnGotAllPondLevels(list);
				}
				break;
			case 3:
			{
				string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
				List<ItemAssetInfo> list2 = JsonConvert.DeserializeObject<List<ItemAssetInfo>>(text);
				if (this.OnGotInventoryItemAssets != null)
				{
					this.OnGotInventoryItemAssets(list2);
				}
				break;
			}
			case 4:
			{
				string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
				List<FishAssetInfo> list3 = JsonConvert.DeserializeObject<List<FishAssetInfo>>(text);
				if (this.OnGotFishAssets != null)
				{
					this.OnGotFishAssets(list3);
				}
				break;
			}
			case 5:
			{
				Hashtable hashtable = (Hashtable)response.Parameters[245];
				this.globalVariables = hashtable;
				if (this.OnGotGlobalVariables != null)
				{
					this.OnGotGlobalVariables(hashtable);
				}
				break;
			}
			case 6:
				if (this.OnGotBoats != null)
				{
					this.DeserializeListAsync<BoatDesc>(response, delegate(IEnumerable<BoatDesc> result)
					{
						if (this.OnGotBoats != null)
						{
							this.OnGotBoats(result);
						}
					}, false);
				}
				break;
			case 7:
				this.HandleGetProductsResponse(response);
				break;
			case 8:
				this.HandleGetProductCategoriesResponse(response);
				break;
			case 9:
			{
				string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
				List<FishBrief> list4 = JsonConvert.DeserializeObject<List<FishBrief>>(text);
				if (this.OnGotFishBrief != null)
				{
					this.OnGotFishBrief(list4);
				}
				break;
			}
			case 10:
			{
				string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
				InventoryItemBrief[] array = JsonConvert.DeserializeObject<InventoryItemBrief[]>(text);
				if (this.OnGotItemBriefs != null)
				{
					this.OnGotItemBriefs(array);
				}
				break;
			}
			}
		}
		else
		{
			ProfileFailure profileFailure = new ProfileFailure(response);
			Debug.LogError(profileFailure.FullErrorInfo);
			switch (clientCacheOperationCode)
			{
			case 1:
				if (this.OnGettingAllPondInfosFailed != null)
				{
					this.OnGettingAllPondInfosFailed(profileFailure);
				}
				break;
			case 2:
				if (this.OnGettingAllPondLevelsFailed != null)
				{
					this.OnGettingAllPondLevelsFailed(profileFailure);
				}
				break;
			case 3:
				if (this.OnGettingInventoryItemAssetsFailed != null)
				{
					this.OnGettingInventoryItemAssetsFailed(profileFailure);
				}
				break;
			case 4:
				if (this.OnGettingFishAssetsFailed != null)
				{
					this.OnGettingFishAssetsFailed(profileFailure);
				}
				break;
			case 5:
				if (this.OnGettingGlobalVariablesFailed != null)
				{
					this.OnGettingGlobalVariablesFailed(profileFailure);
				}
				break;
			case 6:
				if (this.OnGettingBoatsFailed != null)
				{
					this.OnGettingBoatsFailed(profileFailure);
				}
				break;
			case 9:
				if (this.OnGettingFishBriefFailed != null)
				{
					this.OnGettingFishBriefFailed(profileFailure);
				}
				break;
			case 10:
				if (this.OnErrorGettingItemBriefs != null)
				{
					this.OnErrorGettingItemBriefs(profileFailure);
				}
				break;
			}
		}
	}

	public float MaxBonusExp
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return 0f;
			}
			return (float)this.globalVariables["MaxBonusExp"];
		}
	}

	public float MaxPenaltyExp
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return 0f;
			}
			return (float)this.globalVariables["MaxPenaltyExp"];
		}
	}

	public int ChangeNameCost
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return 0;
			}
			return (int)this.globalVariables["ChangeNameCost"];
		}
	}

	public int PondDayStart
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return 0;
			}
			return (int)this.globalVariables["PondDayStart"];
		}
	}

	public int DefaultInventoryCapacity
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return 0;
			}
			return (int)this.globalVariables["DefaultInventoryCapacity"];
		}
	}

	public bool IsCert
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return false;
			}
			return this.globalVariables.ContainsKey("IsCert") && (bool)this.globalVariables["IsCert"];
		}
	}

	public bool HasSilverMultiplayerPriv
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return false;
			}
			return this.globalVariables.ContainsKey("HasSilverMultiplayerPriv") && (bool)this.globalVariables["HasSilverMultiplayerPriv"];
		}
	}

	public bool IsItalianOn
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return false;
			}
			return this.globalVariables.ContainsKey("IsItalianOn") && (bool)this.globalVariables["IsItalianOn"];
		}
	}

	public bool IsPredatorFishOn
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return false;
			}
			return this.globalVariables.ContainsKey("IsPredatorFishOn") && (bool)this.globalVariables["IsPredatorFishOn"];
		}
	}

	public bool IsTest
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return false;
			}
			return this.globalVariables.ContainsKey("IsTest") && (bool)this.globalVariables["IsTest"];
		}
	}

	public bool IsUgcOn
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return false;
			}
			return this.globalVariables.ContainsKey("IsUgcOn") && (bool)this.globalVariables["IsUgcOn"];
		}
	}

	public bool IsFreeRoamingOn
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return false;
			}
			return this.globalVariables.ContainsKey("IsFreeRoamingOn") && (bool)this.globalVariables["IsFreeRoamingOn"];
		}
	}

	public bool IsSavePlayerStateOn
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return false;
			}
			return this.globalVariables.ContainsKey("IsSavePlayerStateOn") && (bool)this.globalVariables["IsSavePlayerStateOn"];
		}
	}

	public bool UgcSilverAccCanJoin
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available");
				return false;
			}
			return this.globalVariables.ContainsKey("UgcSilverAccCanJoin") && (bool)this.globalVariables["UgcSilverAccCanJoin"];
		}
	}

	public bool IsFPSShowOn
	{
		get
		{
			return this.globalVariables != null && this.globalVariables.ContainsKey("IsFPSShowOn") && (bool)this.globalVariables["IsFPSShowOn"];
		}
	}

	public DateTime ServerUtcNow
	{
		get
		{
			return this.serverTimeCache.ServerUtcNow;
		}
	}

	public void DispatchTimeActions()
	{
		this.serverTimeCache.Update();
	}

	public void GetServerTime(int callerId)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(147);
		operationRequest.Parameters[188] = callerId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(147);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotServerTime OnGotServerTime;

	public void GetTime()
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(184);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(184);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTime OnGotTime;

	public void MoveTimeForward(int? hours)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(183);
		if (hours == null)
		{
			operationRequest.Parameters[193] = true;
		}
		else
		{
			operationRequest.Parameters[188] = hours;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(183);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTime OnMovedTimeForward;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnMovingTimeForwardFailed;

	public DateTime? PreviewMoveTimeForward(bool hasPremium, int? hours)
	{
		if (!this.CheckProfile())
		{
			Debug.LogError("Profile is not available!");
			return new DateTime?(DateTime.MinValue);
		}
		if (this.Profile.PondTimeSpent == null)
		{
			Debug.LogError("Pond time is not available!");
			return new DateTime?(DateTime.MinValue);
		}
		if (this.globalVariables == null)
		{
			Debug.LogError("Global variables are not available!");
			return new DateTime?(DateTime.MinValue);
		}
		TimeSpan value = this.Profile.PondTimeSpent.Value;
		TimeSpan timeSpan;
		if (hours == null)
		{
			int num = (int)this.globalVariables["PondDayStart"];
			timeSpan = value.MoveToNextDay(num);
		}
		else
		{
			timeSpan = value.RoundToNextHour().AddHours((double)(hours.Value - 1));
		}
		float num2;
		if (hasPremium)
		{
			num2 = (float)this.globalVariables["TimeForwardPremiumCooldown"];
		}
		else
		{
			num2 = (float)this.globalVariables["TimeForwardCooldown"];
		}
		return TimeRewinder.GetMoveTimeForwardCooldownTime(num2, value, timeSpan);
	}

	public int RemoveTimeForwardCooldownCost
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available!");
				return 1;
			}
			return (int)this.globalVariables["TimeForwardCooldownRemoveCost"];
		}
	}

	public float PremiumTimeForwardCooldownDiff
	{
		get
		{
			if (this.globalVariables == null)
			{
				Debug.LogError("Global variables are not available!");
				return 2f;
			}
			return (float)this.globalVariables["TimeForwardCooldown"] / (float)this.globalVariables["TimeForwardPremiumCooldown"];
		}
	}

	public void GetTimeForwardCooldown()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(224);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(224);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTimeForwardCooldown OnGotTimeForwardCooldown;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTimeForwardCooldownFailed;

	public void RemoveTimeForwardCooldown()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(137);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(137);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnRemovedTimeForwardCooldown;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnRemovingTimeForwardCooldownFailed;

	private void HandleServerTimeResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			return;
		}
		int num = (int)response.Parameters[188];
		DateTime dateTime = new DateTime((long)response.Parameters[181]);
		if (this.OnGotServerTime != null)
		{
			this.OnGotServerTime(num, dateTime);
		}
	}

	private void HandleGetTimeResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			return;
		}
		TimeSpan timeSpan = new TimeSpan((long)response.Parameters[181]);
		if (this.Profile != null)
		{
			this.Profile.PondTimeSpent = new TimeSpan?(timeSpan);
		}
		foreach (Chum chum in this.profile.Inventory.OfType<Chum>())
		{
			chum.SetPondTimeSpent(timeSpan);
		}
		if (response.Parameters.ContainsKey(188))
		{
			this.TournamentRemainingTimeReceived = new DateTime?(DateTime.UtcNow);
			this.TournamentRemainingTime = new TimeSpan?(new TimeSpan((long)response.Parameters[188]));
		}
		if (this.OnGotTime != null)
		{
			this.OnGotTime(timeSpan);
		}
	}

	private void HandleMoveTimeResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			Failure failure = new Failure(response);
			if (this.OnMovingTimeForwardFailed != null)
			{
				this.OnMovingTimeForwardFailed(failure);
			}
			return;
		}
		TimeSpan timeSpan = new TimeSpan((long)response.Parameters[181]);
		if (this.Profile != null)
		{
			this.Profile.PondTimeSpent = new TimeSpan?(timeSpan);
		}
		if (response.Parameters.ContainsKey(188))
		{
			this.TournamentRemainingTimeReceived = new DateTime?(DateTime.UtcNow);
			this.TournamentRemainingTime = new TimeSpan?(new TimeSpan((long)response.Parameters[188]));
		}
		if (this.OnMovedTimeForward != null)
		{
			this.OnMovedTimeForward(timeSpan);
		}
	}

	private void HandleGetTimeForwardCooldownResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			Failure failure = new Failure(response);
			if (this.OnGettingTimeForwardCooldownFailed != null)
			{
				this.OnGettingTimeForwardCooldownFailed(failure);
			}
			return;
		}
		DateTime dateTime = new DateTime((long)response.Parameters[188]);
		if (this.OnGotServerTime != null)
		{
			this.OnGotServerTime(int.MinValue, dateTime);
		}
		DateTime? dateTime2 = null;
		if (response.Parameters.ContainsKey(181))
		{
			dateTime2 = new DateTime?(new DateTime((long)response.Parameters[181]));
		}
		if (this.OnGotTimeForwardCooldown != null)
		{
			this.OnGotTimeForwardCooldown(dateTime2);
		}
	}

	private void HandleRemoveTimeForwardCooldownResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			Failure failure = new Failure(response);
			if (this.OnRemovingTimeForwardCooldownFailed != null)
			{
				this.OnRemovingTimeForwardCooldownFailed(failure);
			}
			return;
		}
		if (this.CheckProfile())
		{
			this.Profile.IncrementBalance("GC", (double)(-(double)this.RemoveTimeForwardCooldownCost));
		}
		if (this.OnRemovedTimeForwardCooldown != null)
		{
			this.OnRemovedTimeForwardCooldown();
		}
	}

	public void DebugFishing(string message)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(172);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = 4;
		operationRequest.Parameters[188] = "Fishing";
		operationRequest.Parameters[196] = message;
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	public void DebugInventory(string message)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(172);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = 4;
		operationRequest.Parameters[188] = "Inventory";
		operationRequest.Parameters[196] = message;
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	public void DebugInventoryContent()
	{
		if (!this.CheckProfile())
		{
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(172);
		string inventoryAsString = this.Profile.Inventory.GetInventoryAsString();
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = 4;
		operationRequest.Parameters[188] = "InventoryContent";
		operationRequest.Parameters[196] = inventoryAsString;
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	public void DebugSecurity(string message)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(172);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = 4;
		operationRequest.Parameters[188] = "Security";
		operationRequest.Parameters[196] = message;
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	public void DebugClient(byte flagIndex, string message)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(172);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = 4;
		operationRequest.Parameters[188] = "Client";
		operationRequest.Parameters[245] = flagIndex;
		operationRequest.Parameters[196] = message;
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	public void PinFps(float fps)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("PinFps - Not authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(172);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = 1;
		operationRequest.Parameters[188] = fps;
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	public void PinError(string message, string stackTrace)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(172);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = 3;
		operationRequest.Parameters[196] = message;
		if (stackTrace == null)
		{
			stackTrace = string.Empty;
		}
		operationRequest.Parameters[188] = stackTrace;
		if ((this.IsConnectedToMaster || this.IsConnectedToGameServer) && this.IsAuthenticated && this.errorsCache.Count == 0)
		{
			this.Peer.OpCustom(operationRequest, true, 0, true);
		}
		else
		{
			this.errorsCache.Add(operationRequest);
		}
	}

	private void FlushErrorsCache()
	{
		if (this.errorsCache.Count == 0)
		{
			return;
		}
		foreach (OperationRequest operationRequest in this.errorsCache)
		{
			this.Peer.OpCustom(operationRequest, true, 0, true);
		}
		this.errorsCache.Clear();
	}

	public void PinError(Exception ex)
	{
		this.PinError(ex.GetType().Name + ": " + ex.Message, ex.StackTrace);
	}

	public void PinSysInfo(SysInfo info)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(172);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = 2;
		operationRequest.Parameters[188] = JsonConvert.SerializeObject(info, 0, SerializationHelper.JsonSerializerSettings);
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	public void SaveTelemetryInfo(TelemetryCode code, string message)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(155);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = code;
		operationRequest.Parameters[188] = message;
		if (this.IsConnectedToGameServer && this.IsAuthenticated && this.errorsCache.Count == 0)
		{
			this.Peer.OpCustom(operationRequest, true, 0, true);
		}
		else
		{
			this.errorsCache.Add(operationRequest);
		}
	}

	public void FlushTelemetryCache()
	{
		if (this.telemetryCache.Count == 0)
		{
			return;
		}
		foreach (OperationRequest operationRequest in this.telemetryCache)
		{
			this.Peer.OpCustom(operationRequest, true, 0, true);
		}
		this.telemetryCache.Clear();
	}

	public void SaveBinaryData(byte[] data)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(155);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = 3;
		operationRequest.Parameters[188] = data;
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	public void DebugBiteSystemEvents(bool enabled)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(131);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[193] = enabled;
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnDebugBiteSystemEvents;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnDebugBiteSystemEventsFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ChumPiece> OnChumPieceUpdated;

	private void HandleDebugBiteSystemEventsResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnDebugBiteSystemEvents != null)
			{
				this.OnDebugBiteSystemEvents();
			}
		}
		else
		{
			Failure failure = new Failure(response);
			Debug.LogError(failure.FullErrorInfo);
			if (this.OnDebugBiteSystemEventsFailed != null)
			{
				this.OnDebugBiteSystemEventsFailed(failure);
			}
		}
	}

	public void GetTopPlayers(TopScope scope, TopPlayerOrder order)
	{
		if (scope == 2 && this.profile.PondId == null)
		{
			Debug.LogError("Not on pond to request pond tops");
			return;
		}
		if (scope == 4)
		{
			Debug.LogError("Invalid scope for top players");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(169);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[173] = 1;
		operationRequest.Parameters[172] = scope;
		operationRequest.Parameters[171] = order;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(169);
	}

	public void GetTopFish(TopScope scope, TopFishOrder order, int? pondId, int? fishCategoryId, int[] fishId = null)
	{
		if (scope == 2 && this.profile.PondId == null)
		{
			Debug.LogError("Not on pond to request pond tops");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(169);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[173] = 2;
		operationRequest.Parameters[172] = scope;
		operationRequest.Parameters[171] = order;
		if (scope != 2 && pondId != null)
		{
			operationRequest.Parameters[184] = pondId.Value;
		}
		if (fishCategoryId != null)
		{
			operationRequest.Parameters[194] = fishCategoryId.Value;
		}
		if (fishId != null)
		{
			operationRequest.Parameters[244] = fishId;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(169);
	}

	public void GetTopTournamentPlayers(TopScope scope, TopTournamentKind tournamentKind)
	{
		if (scope == 2 || scope == 4 || scope == 3)
		{
			Debug.LogError("Invalid scope for top tournament players");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(169);
		operationRequest.Parameters = new Dictionary<byte, object>();
		operationRequest.Parameters[173] = 3;
		operationRequest.Parameters[172] = scope;
		operationRequest.Parameters[171] = tournamentKind;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(169);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotLeaderboards OnGotLeaderboards;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingLeaderboardsFailed;

	public void GetTournamentSeries()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 7;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 7);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentSeries OnGotTournamentSeries;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTournamentSeriesFailed;

	public void GetTournamentSerieInstances()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 17;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 17);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentSerieInstances OnGotTournamentSerieInstances;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTournamentSerieInstancesFailed;

	public void GetTournamentGrid()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 8;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 8);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentGrid OnGotTournamentGrid;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTournamentGridFailed;

	public void GetTournaments(TournamentKinds? kind, int? pondId, DateTime? start = null, DateTime? end = null)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 9;
		if (kind != null)
		{
			operationRequest.Parameters[27] = kind.Value;
		}
		if (pondId != null)
		{
			operationRequest.Parameters[22] = pondId.Value;
		}
		if (start != null)
		{
			operationRequest.Parameters[40] = start.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		}
		if (end != null)
		{
			operationRequest.Parameters[41] = end.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 9);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentsByKind OnGotTournaments;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTournamentsFailed;

	public void GetTournament(int tournamentId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 13;
		operationRequest.Parameters[21] = tournamentId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 13);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournament OnGotTournament;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTournamentFailed;

	public void GetTournamentWeather(int tournamentId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 15;
		operationRequest.Parameters[21] = tournamentId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 15);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotPondWeather OnGotTournamentWeather;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTournamentWeatherFailed;

	public void GetTournamentRewards(int tournamentId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 14;
		operationRequest.Parameters[21] = tournamentId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 14);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentRewards OnGotTournamentRewards;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTournamentRewardsFailed;

	public void GetOpenTournaments(int? kindId, int? pondId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 1;
		if (kindId != null)
		{
			operationRequest.Parameters[27] = kindId.Value;
		}
		if (pondId != null)
		{
			operationRequest.Parameters[22] = pondId.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 1);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournaments OnGotOpenTournaments;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingOpenTournamentsFailed;

	public void GetMyTournaments(int? pondId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 2;
		if (pondId != null)
		{
			operationRequest.Parameters[22] = pondId.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 2);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournaments OnGotMyTournaments;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingMyTournamentsFailed;

	public void GetPlayerTournaments(int? pondId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 23;
		if (pondId != null)
		{
			operationRequest.Parameters[22] = pondId.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 23);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournaments OnGotPlayerTournaments;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingPlayerTournamentsFailed;

	public void GetTournamentParticipationHistory(int? tournamentTemplateId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 6;
		if (tournamentTemplateId != null)
		{
			operationRequest.Parameters[21] = tournamentTemplateId.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 6);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentResults OnGotTournamentParticipationHistory;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTournamentParticipationHistoryFailed;

	public void RegisterForTournament(int tournamentId, bool removeOtherRegistrations)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 3;
		operationRequest.Parameters[21] = tournamentId;
		operationRequest.Parameters[44] = removeOtherRegistrations;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 3);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnRegisteredForTournament;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnRegisterForTournamentFailed;

	public void RegisterForTournamentSerie(int serieInstanceId, bool removeOtherRegistrations)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 18;
		operationRequest.Parameters[34] = serieInstanceId;
		operationRequest.Parameters[44] = removeOtherRegistrations;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 18);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnRegisteredForTournamentSerie;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnRegisterForTournamentSerieFailed;

	public void UnregisterFromTournament(int tournamentId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 22;
		operationRequest.Parameters[21] = tournamentId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 22);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnUnregisteredFromTournament;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnUnregisterFromTournamentFailed;

	public void PreviewTournamentStart()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 4;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 4);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnTournamentStartPreview;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnPreviewTournamentStartFailed;

	public void GetIntermediateTournamentResult(int? tournamentId = null)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 10;
		if (tournamentId != null)
		{
			operationRequest.Parameters[21] = tournamentId.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 10);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentResults OnGotTournamentIntermediateResult;

	public void GetFinalTournamentResult(int tournamentId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 11;
		operationRequest.Parameters[21] = tournamentId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 11);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentResults OnGotTournamentFinalResult;

	public void GetIntermediateTournamentTeamResult(int? tournamentId = null)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 19;
		if (tournamentId != null)
		{
			operationRequest.Parameters[21] = tournamentId.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 19);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentTeamResults OnGotIntermediateTournamentTeamResult;

	public void GetFinalTournamentTeamResult(int tournamentId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 20;
		operationRequest.Parameters[21] = tournamentId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 20);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentTeamResults OnGotFinalTournamentTeamResult;

	public void GetSecondaryTournamentResult(int tournamentId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 16;
		operationRequest.Parameters[21] = tournamentId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 16);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentSecondaryResult OnGotTournamentSecondaryResult;

	public void EndTournament(bool prematurely)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 12;
		operationRequest.Parameters[44] = prematurely;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 12);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnTournamentEnded;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnTournamentEndFailed;

	public void GetTournamentSerieStats()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(168);
		operationRequest.Parameters[1] = 21;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 21);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotTournamentSerieStats OnGotTournamentSerieStats;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingTournamentSerieStatsFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnTournamentScoringStarted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnStartTournamentScoringFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnTournamentStarted OnTournamentStarted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnTournamentCancelled OnTournamentCancelled;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ErrorCode> OnTournamentWrongTackle;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnTournamentUpdate OnTournamentUpdate;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnTournamentTimeEnded OnTournamentTimeEnded;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFishSold OnFishSold;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnTournamentResult OnTournamentResult;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnLicensesGained OnLicensesGained;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnTournamentLogoRequested;

	public void SetAddProps(int languageId, string country = null, bool requestProfile = false)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 1;
		operationRequest.Parameters[2] = languageId;
		if (country != null)
		{
			operationRequest.Parameters[4] = country;
		}
		if (requestProfile)
		{
			operationRequest.Parameters[88] = true;
		}
		this.languageIdToSet = languageId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(167, 1);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnSetAddPropsComplete;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnSetAddPropsFailed;

	public void GetWhatsNewList()
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 2;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(167, 2);
	}

	public void WhatsNewShown(List<WhatsNewItem> items)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 10;
		operationRequest.Parameters[154] = string.Join(", ", items.Select((WhatsNewItem i) => i.ItemId.ToString()).ToArray<string>());
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(167, 10);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnWhatsNewShown;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnWhatsNewShownFailed;

	public void WhatsNewClicked(WhatsNewItem item)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 11;
		operationRequest.Parameters[194] = item.ItemId;
		operationRequest.Parameters[188] = item.Title;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(167, 11);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnWhatsNewClicked;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnWhatsNewClickedFailed;

	public void SetTutorialStep(string stepId)
	{
		if (!this.CheckProfile())
		{
			Debug.LogError("Profile is not availalbe yet");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 3;
		operationRequest.Parameters[3] = stepId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(167, 3);
	}

	public void GetCurrentEvent()
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 4;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(167, 4);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotCurrentEvent OnGotCurrentEvent;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingCurrentEventFailed;

	public void CaptureActionInStats(string action, string id = null, string categoryId = null, string subCategoryId = null)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(145);
		operationRequest.Parameters[188] = ((action.Length <= 32) ? action : action.Substring(0, 32));
		if (!string.IsNullOrEmpty(id))
		{
			operationRequest.Parameters[194] = ((id.Length <= 16) ? id : id.Substring(0, 16));
		}
		if (!string.IsNullOrEmpty(categoryId))
		{
			operationRequest.Parameters[152] = ((categoryId.Length <= 16) ? categoryId : categoryId.Substring(0, 16));
		}
		if (!string.IsNullOrEmpty(subCategoryId))
		{
			operationRequest.Parameters[154] = ((subCategoryId.Length <= 16) ? subCategoryId : subCategoryId.Substring(0, 16));
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFarmRebootTriggered OnFarmRebootTriggered;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnFarmRebootCanceled;

	public void GetLatestEula()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 5;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(168, 5);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotLatestEula OnGotLatestEula;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingLatestEulaFailed;

	public void SignEula(int version)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
		operationRequest.Parameters[1] = 6;
		operationRequest.Parameters[194] = version;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(167, 6);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnEulaSigned;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnEulaSignFailed;

	public bool CreateSupportTicket(string title, string description, Stream attachment)
	{
		if (!this.IsAuthenticated)
		{
			Debug.LogError("Not authenticated");
			return false;
		}
		if (title == null)
		{
			Debug.LogError("Ticket title was not supplied");
			return false;
		}
		if (title.Length > 256)
		{
			Debug.LogError("Ticket title is too long");
			return false;
		}
		if (description == null)
		{
			Debug.LogError("Ticket description was not supplied");
			return false;
		}
		if (title.Length > 4096)
		{
			Debug.LogError("Ticket description is too long");
			return false;
		}
		if (attachment != null && attachment.Length > 2097152L)
		{
			Debug.LogError("Attachment is too big");
			return false;
		}
		AsynchProcessor.ProcessByteWorkItem(new InputByteWorkItem
		{
			Data = 7,
			ProcessAction = delegate(byte p)
			{
				OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(167);
				operationRequest.Parameters[1] = p;
				operationRequest.Parameters[5] = title;
				operationRequest.Parameters[6] = description;
				if (attachment != null)
				{
					operationRequest.Parameters[7] = SerializationHelper.SerializeStream(attachment);
				}
				return operationRequest;
			},
			ResultAction = delegate(object r)
			{
				OperationRequest operationRequest2 = (OperationRequest)r;
				this.Peer.OpCustom(operationRequest2, true, 0, true);
				OpTimer.Start(167, 7);
			}
		});
		return true;
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnSupportTicketCreated;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnCreateSupportTicketFailed;

	public void RaiseUpdateCharacter(Package package)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated || !this.IsInRoom)
		{
			return;
		}
		Hashtable hashtable = CharacterInfo.ToHashtable(this.UserId, CharacterEventType.Update, package);
		if (hashtable != null)
		{
			this.Peer.OpRaiseEvent(161, hashtable, true, RaiseEventOptions.Default);
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnCharacterEvent OnCharacterCreated = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnCharacterEvent OnCharacterDestroyed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnCharacterUpdateEvent OnCharacterUpdated;

	public void CheckForProductSales()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(153);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(153);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnCheckedForProductSales OnCheckedForProductSales;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnCheckForProductSalesFailed;

	public void GetProduct(int productId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(165);
		operationRequest.Parameters[194] = productId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(165);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<StoreProduct> OnGotProduct;

	public void GetProducts(int? typeId = null)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 7;
		if (typeId != null)
		{
			operationRequest.Parameters[194] = typeId.Value;
		}
		PhotonServerConnection.AddPlatformParamsForProducts(operationRequest);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 7);
	}

	public void GetProducts(int[] typeIds)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 7;
		operationRequest.Parameters[154] = string.Join(",", typeIds.Select((int id) => id.ToString()).ToArray<string>());
		PhotonServerConnection.AddPlatformParamsForProducts(operationRequest);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 7);
	}

	public void GetAllItemBriefs()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 10;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 10);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<InventoryItemBrief[]> OnGotItemBriefs;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnErrorGettingItemBriefs;

	public void GetProductCategories()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(140);
		operationRequest.Parameters[191] = 8;
		PhotonServerConnection.AddPlatformParamsForProducts(operationRequest);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(140, 8);
	}

	public void GetOffers()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(136);
		operationRequest.Parameters[191] = 0;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(136, 0);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotOffers OnGotOffers;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGetOffersFailed;

	public void CompleteOffer(int offerId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(136);
		operationRequest.Parameters[191] = 1;
		operationRequest.Parameters[194] = offerId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(136, 1);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnCompleteOffer OnCompleteOffer;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnCompleteOfferFailed;

	private static void AddPlatformParamsForProducts(OperationRequest request)
	{
	}

	private void HandleCheckForProductSales(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnCheckedForProductSales != null)
			{
				string text = (string)response.Parameters[192];
				List<SaleFlags> list = JsonConvert.DeserializeObject<List<SaleFlags>>(text, SerializationHelper.JsonSerializerSettings);
				this.OnCheckedForProductSales(list);
			}
		}
		else
		{
			Debug.LogErrorFormat("CheckForProductSales failed. Code: {0} Message: {1}", new object[]
			{
				Enum.GetName(typeof(ErrorCode), response.ReturnCode),
				response.DebugMessage
			});
			if (this.OnCheckForProductSalesFailed != null)
			{
				this.OnCheckForProductSalesFailed(new Failure(response));
			}
		}
	}

	private void HandleGetProductResponse(OperationResponse response)
	{
		string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
		StoreProduct storeProduct = JsonConvert.DeserializeObject<StoreProduct>(text, SerializationHelper.JsonSerializerSettings);
		if (this.OnGotProduct != null)
		{
			this.OnGotProduct(storeProduct);
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotProducts OnGotProducts;

	private void HandleGetProductsResponse(OperationResponse response)
	{
		InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
		inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
		inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
		{
			string text = CompressHelper.DecompressString(p);
			return JsonConvert.DeserializeObject<List<StoreProduct>>(text, SerializationHelper.JsonSerializerSettings);
		};
		inputByteArrayWorkItem.ResultAction = delegate(object r)
		{
			List<StoreProduct> list = (List<StoreProduct>)r;
			if (this.OnGotProducts != null)
			{
				this.OnGotProducts(list);
			}
		};
		AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotProductCategories OnGotProductCategories;

	private void HandleGetProductCategoriesResponse(OperationResponse response)
	{
		InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
		inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
		inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
		{
			string text = CompressHelper.DecompressString(p);
			return JsonConvert.DeserializeObject<List<ProductCategory>>(text, SerializationHelper.JsonSerializerSettings);
		};
		inputByteArrayWorkItem.ResultAction = delegate(object r)
		{
			List<ProductCategory> list = (List<ProductCategory>)r;
			if (this.OnGotProductCategories != null)
			{
				this.OnGotProductCategories(list);
			}
		};
		AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
	}

	public void GivePsProduct(int productId, string foreignProductId, int count, DateTime expireDate)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(141);
		operationRequest.Parameters[219] = productId;
		operationRequest.Parameters[194] = foreignProductId;
		if (count == 0)
		{
			operationRequest.Parameters[245] = expireDate.Ticks;
		}
		else
		{
			operationRequest.Parameters[183] = count;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(141);
	}

	private void HandleGivePsProductsResponse(OperationResponse response)
	{
		Debug.Log("PS: HandleGivePsProductsResponse enter");
		if (response.ReturnCode == 0 && response.Parameters.ContainsKey(192))
		{
			Debug.Log("PS: HandleGivePsProductsResponse - Ok");
			try
			{
				if (response.Parameters.ContainsKey(192))
				{
					string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
					ProfileProduct profileProduct = JsonConvert.DeserializeObject<ProfileProduct>(text, SerializationHelper.JsonSerializerSettings);
					int num = (int)response.Parameters[183];
					DateTime? dateTime = null;
					if (response.Parameters.ContainsKey(245))
					{
						dateTime = new DateTime?(new DateTime((long)response.Parameters[245], DateTimeKind.Utc));
					}
					bool flag = (bool)response.Parameters[193];
					bool flag2 = (bool)response.Parameters[244];
					if (flag && dateTime != null)
					{
						Debug.Log("PS: Subscription end date updated to: " + dateTime.Value);
						List<LevelLockRemoval> list;
						this.Profile.UpdateSubscriptionEndDate(profileProduct, dateTime.Value, out list);
						if (this.OnProductDelivered != null)
						{
							this.OnProductDelivered(profileProduct, num, flag2);
						}
					}
					else
					{
						Debug.Log(string.Concat(new object[] { "PS: Delivering new product - #", profileProduct.ProductId, " (", profileProduct.Name, ")" }));
						for (int i = 0; i < num; i++)
						{
							List<LevelLockRemoval> list;
							this.profile.PutProductToProfile(profileProduct, out list, dateTime);
						}
						if (profileProduct.Items != null && profileProduct.Items.Length > 0)
						{
							this.profile.Inventory.ResolveParents();
							this.RaiseInventoryUpdated(false);
						}
						if (this.OnProductDelivered != null)
						{
							this.OnProductDelivered(profileProduct, num, flag2);
						}
						if (profileProduct.PondsUnlocked != null && profileProduct.PondsUnlocked.Length > 0 && this.OnPondLevelInvalidated != null)
						{
							this.OnPondLevelInvalidated();
						}
					}
					Debug.Log("PS: HandleGivePsProductsResponse done");
				}
				else
				{
					Debug.LogWarning("PS: Giving product skipped, product is already given...");
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("PS: HandleGivePsProductsResponse exception: " + ex.Message + "\r\n" + ex.StackTrace);
				if (this.OnErrorGivingProduct != null)
				{
					this.OnErrorGivingProduct(new Failure
					{
						ErrorMessage = ex.Message
					});
				}
			}
		}
		else
		{
			Failure failure = new Failure(response);
			Debug.LogError("PS: HandleGivePsProductsResponse - " + failure.FullErrorInfo);
			if (this.OnErrorGivingProduct != null)
			{
				this.OnErrorGivingProduct(failure);
			}
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnErrorGivingProduct;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnProductDelivered OnProductDelivered;

	public void GiveXboxProduct(string foreignProductId, int count)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(129);
		operationRequest.Parameters[194] = foreignProductId;
		operationRequest.Parameters[183] = count;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(129);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<string> OnXboxProductTranComplete;

	private void HandleGiveXboxProductsResponse(OperationResponse response)
	{
		Debug.Log("XBox: HandleGiveXboxProductsResponse enter");
		if (response.ReturnCode == 0 && response.Parameters.ContainsKey(192))
		{
			Debug.Log("XBox: HandleGiveXboxProductsResponse - Ok");
			try
			{
				string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
				ProfileProduct profileProduct = JsonConvert.DeserializeObject<ProfileProduct>(text, SerializationHelper.JsonSerializerSettings);
				int num = (int)response.Parameters[183];
				string text2 = (string)response.Parameters[245];
				Debug.Log(string.Concat(new object[] { "XBox: Delivering new product - #", profileProduct.ProductId, " (", profileProduct.Name, ")" }));
				for (int i = 0; i < num; i++)
				{
					this.profile.PutProductToProfile(profileProduct);
				}
				if (profileProduct.Items != null && profileProduct.Items.Length > 0)
				{
					this.profile.Inventory.ResolveParents();
					this.RaiseInventoryUpdated(false);
				}
				if (this.OnProductDelivered != null)
				{
					this.OnProductDelivered(profileProduct, num, true);
				}
				if (this.OnXboxProductTranComplete != null)
				{
					this.OnXboxProductTranComplete(text2);
				}
				if (profileProduct.PondsUnlocked != null && profileProduct.PondsUnlocked.Length > 0 && this.OnPondLevelInvalidated != null)
				{
					this.OnPondLevelInvalidated();
				}
				Debug.Log("XBox: HandleGiveXboxProductsResponse done");
			}
			catch (Exception ex)
			{
				Debug.LogError("XBox: HandleGiveXboxProductsResponse exception: " + ex.Message + "\r\n" + ex.StackTrace);
				if (this.OnErrorGivingProduct != null)
				{
					this.OnErrorGivingProduct(new Failure
					{
						ErrorMessage = ex.Message
					});
				}
			}
		}
		else
		{
			Failure failure = new Failure(response);
			Debug.LogError("XBox: HandleGiveXboxProductsResponse - " + failure.FullErrorInfo);
			if (this.OnErrorGivingProduct != null)
			{
				this.OnErrorGivingProduct(failure);
			}
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<OperationResponse> OnOpCompleted;

	public void DeleteAbsentSubscriptions(string[] entitlmentIds)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(134);
		string text = JsonConvert.SerializeObject(entitlmentIds, 0, SerializationHelper.JsonSerializerSettings);
		operationRequest.Parameters[192] = CompressHelper.CompressString(text);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(134);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnAbsentSubscriptionsDeleted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnErrorDeletingAbsentSubscriptions;

	private void HandleDeleteAbsentSubscriptionsResponse(OperationResponse response)
	{
		Debug.Log("PS: DeleteAbsentSubscriptions enter");
		if (response.ReturnCode == 0)
		{
			if (response.Parameters.ContainsKey(192))
			{
				string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
				List<Subscription> list = JsonConvert.DeserializeObject<List<Subscription>>(text, SerializationHelper.JsonSerializerSettings);
				this.Profile.DeleteSubscriptions(list);
				Debug.Log("PS: Got deleted subscription(s)");
				if (this.OnPondLevelInvalidated != null)
				{
					this.OnPondLevelInvalidated();
				}
			}
			Debug.Log("PS: DeleteAbsentSubscriptions - Ok");
			if (this.OnAbsentSubscriptionsDeleted != null)
			{
				this.OnAbsentSubscriptionsDeleted();
			}
		}
		else
		{
			Debug.Log("PS: DeleteAbsentSubscriptions - Error");
			if (this.OnErrorDeletingAbsentSubscriptions != null)
			{
				this.OnErrorDeletingAbsentSubscriptions(new Failure(response));
			}
		}
	}

	public string BuyMoneyUrl(int productId)
	{
		return PaymentHelper.GetBuyMoneyUrl(this.profile.LanguageId, this.securityToken, productId);
	}

	public string BuySubscriptionUrl(int productId)
	{
		return PaymentHelper.GetBuySubscriptionUrl(this.profile.LanguageId, this.securityToken, productId);
	}

	public void StartSteamTransaction(int productId, string ipAddress)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(158);
		operationRequest.Parameters[194] = productId;
		operationRequest.Parameters[188] = ipAddress;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(158);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnSteamTransactionStarted OnSteamTransactionStarted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnStartSteamTransactionFailed;

	private void HandleStartSteamTransactionResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			Failure failure = new Failure(response);
			if (this.OnStartSteamTransactionFailed != null)
			{
				this.OnStartSteamTransactionFailed(failure);
			}
			return;
		}
		if (this.OnSteamTransactionStarted != null)
		{
			string text = (string)response.Parameters[194];
			this.OnSteamTransactionStarted(text);
		}
	}

	public void FinalizeSteamTransaction(string orderId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(157);
		operationRequest.Parameters[194] = orderId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(157);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnFinalizeSteamTransactionFailed;

	private void HandleFinalizeSteamTransactionResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			Failure failure = new Failure(response);
			if (this.OnFinalizeSteamTransactionFailed != null)
			{
				this.OnFinalizeSteamTransactionFailed(failure);
			}
			return;
		}
		if (response.Parameters.ContainsKey(192))
		{
			string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
			ProfileProduct profileProduct = JsonConvert.DeserializeObject<ProfileProduct>(text, SerializationHelper.JsonSerializerSettings);
			this.profile.PutProductToProfile(profileProduct);
			if (profileProduct.Items != null && profileProduct.Items.Length > 0)
			{
				this.profile.Inventory.ResolveParents();
				this.RaiseInventoryUpdated(false);
			}
			if (this.OnProductDelivered != null)
			{
				this.OnProductDelivered(profileProduct, 1, true);
			}
			if (profileProduct.PondsUnlocked != null && profileProduct.PondsUnlocked.Length > 0 && this.OnPondLevelInvalidated != null)
			{
				this.OnPondLevelInvalidated();
			}
		}
		else
		{
			Failure failure2 = new Failure(response)
			{
				ErrorMessage = "Error finalizing transaction. Look at player's Trade log for details."
			};
			if (this.OnFinalizeSteamTransactionFailed != null)
			{
				this.OnFinalizeSteamTransactionFailed(failure2);
			}
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnSubscriptionEnded;

	public void ChangeGameScreen(GameScreenType gameScreen, GameScreenTabType gameScreenTab = GameScreenTabType.Undefined, int? categoryId = null, int? itemId = null, int[] childCategoryIds = null, string categoryElementId = null, string[] categoryElementsPath = null)
	{
		if (MissionsController.Instance != null)
		{
			MissionsController.Instance.ChangeGameScreen(gameScreen, gameScreenTab, categoryId, itemId, childCategoryIds, categoryElementId, categoryElementsPath);
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 7;
		operationRequest.Parameters[193] = (byte)gameScreen;
		operationRequest.Parameters[188] = (byte)gameScreenTab;
		if (categoryId != null)
		{
			operationRequest.Parameters[152] = categoryId.Value;
		}
		if (itemId != null)
		{
			operationRequest.Parameters[194] = itemId.Value;
		}
		if (childCategoryIds != null)
		{
			operationRequest.Parameters[154] = string.Join(",", childCategoryIds.Select((int id) => id.ToString()).ToArray<string>());
		}
		if (categoryElementId != null)
		{
			operationRequest.Parameters[240] = categoryElementId;
		}
		if (categoryElementsPath != null)
		{
			operationRequest.Parameters[230] = string.Join(",", categoryElementsPath);
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 7);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGameScreenChanged GameScreenChanged;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure ChangeGameScreenFailed;

	public void ChangeSelectedElement(GameElementType type, string value, int? itemId = null)
	{
		if (MissionsController.Instance != null)
		{
			MissionsController.Instance.ChangeSelectedElement(type, value, itemId);
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 15;
		operationRequest.Parameters[193] = (byte)type;
		if (!string.IsNullOrEmpty(value))
		{
			operationRequest.Parameters[188] = value;
		}
		if (itemId != null)
		{
			operationRequest.Parameters[194] = itemId;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 15);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnSelectedElementChanged SelectedElementChanged;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure ChangeSelectedElementFailed;

	public void ChangeSwitch(GameSwitchType type, bool value, int? slot)
	{
		if (MissionsController.Instance != null)
		{
			MissionsController.Instance.ChangeSwitch(type, value);
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 16;
		operationRequest.Parameters[193] = (byte)type;
		operationRequest.Parameters[188] = value;
		if (slot != null)
		{
			operationRequest.Parameters[150] = slot;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 16);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnSwitchChanged SwitchChanged;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure ChangeSwitchFailed;

	public void ChangeIndicator(GameIndicatorType type, int value)
	{
		if (MissionsController.Instance != null)
		{
			MissionsController.Instance.ChangeIndicator(type, value);
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 8;
		operationRequest.Parameters[193] = (byte)type;
		operationRequest.Parameters[188] = value;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 8);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnIndicatorChanged IndicatorChanged;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure ChangeIndicatorFailed;

	public void SendGameAction(GameActionType type)
	{
		if (MissionsController.Instance != null)
		{
			MissionsController.Instance.SendGameAction(type);
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 17;
		operationRequest.Parameters[193] = (byte)type;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 17);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGameActionSent GameActionSent;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure SendGameActionFailed;

	public void SendPlayerPositionAndRotation(Vector3 playerPosition, Quaternion playerRotation, Vector3? targetPoint)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 14;
		Vector3 vector = playerPosition;
		Vector3 eulerAngles = playerRotation.eulerAngles;
		float[] array = new float[] { vector.x, vector.y, vector.z, eulerAngles.x, eulerAngles.y, eulerAngles.z };
		byte[] array2 = new byte[array.Length * 4];
		Buffer.BlockCopy(array, 0, array2, 0, array2.Length);
		operationRequest.Parameters[245] = array2;
		if (targetPoint != null)
		{
			float[] array3 = new float[]
			{
				targetPoint.Value.x,
				targetPoint.Value.y,
				targetPoint.Value.z
			};
			byte[] array4 = new byte[array3.Length * 4];
			Buffer.BlockCopy(array3, 0, array4, 0, array4.Length);
			operationRequest.Parameters[193] = array4;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 14);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action PlayerPositionAndRotationSent;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure SendPlayerPositionAndRotationFailed;

	public void RestartArchivedMission(int missionId, bool activate)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 9;
		operationRequest.Parameters[194] = missionId;
		operationRequest.Parameters[193] = activate;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 9);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnActiveMissionSet ArchivedMissionRestarted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure RestartArchivedMissionFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMissionHintsReceived MissionHintsReceived;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMissionProgressReceived MissionProgressReceived;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> ActiveMissionChanged;

	public void GetMissionsStarted()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 1;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 1);
	}

	public void GetMissionsCompleted()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 2;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 2);
	}

	public void GetMissionsArchived()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 3;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 3);
	}

	public void GetMissionsFailed()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 4;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 4);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMissionsListReceived MissionsListReceived;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure GetMissionsListFailed;

	public void GetActiveMission()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 6;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 6);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMissionGet ActiveMissionGet;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure ActiveMissionGetFailed;

	public void GetMission(int missionId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 13;
		operationRequest.Parameters[194] = missionId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 13);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMissionGet MissionGet;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure MissionGetFailed;

	public void SetActiveMission(int? missionId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 5;
		if (missionId != null)
		{
			operationRequest.Parameters[194] = missionId;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 5);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnActiveMissionSet ActiveMissionSet;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure ActiveMissionSetFailed;

	public void DisableMissionActivation()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 12;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 12);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action MissionActivationDisabled;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure DisableMissionActivationFailed;

	public void GetMissionInteractiveObject(int missionId, string resourceKey)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 10;
		operationRequest.Parameters[194] = missionId;
		operationRequest.Parameters[244] = resourceKey;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 10);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotInteractiveObject OnGotInteractiveObject;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure GetInteractiveObjectFailed;

	public void SendMissionInteraction(int missionId, string resourceKey, string interaction, object argument)
	{
		if (!ClientMissionsManager.Instance.CanInteract(missionId, resourceKey, interaction))
		{
			Debug.LogError("Can't interact with interactive object!");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 11;
		operationRequest.Parameters[194] = missionId;
		operationRequest.Parameters[244] = resourceKey;
		operationRequest.Parameters[196] = interaction;
		if (argument != null)
		{
			operationRequest.Parameters[195] = argument;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 11);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnSendMissionInteraction MissionInteractionSent;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure SendMissionInteractionFailed;

	public void OnMissionHintsReceived(List<HintMessage> messages)
	{
		if (this.MissionHintsReceived != null)
		{
			this.MissionHintsReceived(messages);
		}
	}

	public void CompleteMissionClientCondition(int missionId, int taskId, string resourceKey, bool completed, string progress)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 18;
		operationRequest.Parameters[194] = missionId;
		operationRequest.Parameters[219] = taskId;
		operationRequest.Parameters[244] = resourceKey;
		operationRequest.Parameters[193] = completed;
		operationRequest.Parameters[218] = progress;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 18);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnCompleteMissionClientCondition MissionClientConditionCompleted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure CompleteMissionClientConditionFailed;

	public void ForceProcessMissions()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(130);
		operationRequest.Parameters[191] = 19;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(130, 19);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnForceProcessMissions;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnForceProcessMissionsFailed;

	private void HandleItemsUpdatedEvents(IDictionary parameters, int rodSlot)
	{
		string text = (string)parameters[6];
		bool flag = parameters.Contains(10) && (bool)parameters[10];
		string[] array = text.Split(new char[] { '|' });
		string[] array2 = array;
		int i = 0;
		while (i < array2.Length)
		{
			string text2 = array2[i];
			string[] array3 = text2.Split(new char[] { ',' });
			Guid guid = new Guid(array3[0]);
			string text3 = array3[1];
			string text4 = array3[2];
			InventoryItem item = this.profile.Inventory.GetItem(guid);
			if (item == null)
			{
				throw new InvalidOperationException(string.Format("Unable to update {1} property of item #{0} as it does not exists", guid, text3));
			}
			if (text3 == null)
			{
				goto IL_18D;
			}
			if (!(text3 == "RodUnequiped"))
			{
				if (!(text3 == "ItemLost"))
				{
					if (!(text3 == "Storage"))
					{
						goto IL_18D;
					}
					this.profile.Inventory.MoveItem(item, null, (StoragePlaces)byte.Parse(text4), true, true);
				}
				else
				{
					this.profile.Inventory.RemoveItem(item, false);
					if (this.OnItemLost != null)
					{
						this.OnItemLost(item);
					}
					if (item is Chum && this.OnChumLost != null)
					{
						this.OnChumLost(item);
					}
				}
			}
			else if (this.OnRodUnequiped != null)
			{
				this.OnRodUnequiped(item);
			}
			IL_19D:
			if (this.OnItemBroken != null && item is FishCage && text3 == "Durability" && int.Parse(text4) == 0)
			{
				this.OnItemBroken(BrokenTackleType.FishCage, 0);
			}
			if (item is Bait && item.ParentItem != null && text3 == "Count")
			{
				if (item.Count == 0 && this.OnBaitLost != null)
				{
					this.OnBaitLost(flag, rodSlot);
				}
				else if (item.Count == 1 && this.OnBaitReplenished != null)
				{
					this.OnBaitReplenished(rodSlot);
				}
			}
			i++;
			continue;
			IL_18D:
			item.SetProperty(text3, text4);
			goto IL_19D;
		}
		Debug.Log("ItemsUpdated event arrived");
		this.RaiseInventoryUpdated(false);
	}

	private void HandleMissionEvent(MissionsResponse missionsResponse)
	{
		if (!string.IsNullOrEmpty(missionsResponse.PrototypeMessagesToSend))
		{
			List<HintMessage> list = JsonConvert.DeserializeObject<List<HintMessage>>(missionsResponse.PrototypeMessagesToSend, SerializationHelper.MissionHintSerializerSettings);
			foreach (IGrouping<int, HintMessage> grouping in from m in list
				group m by m.MissionId)
			{
				MissionClientConfiguration missionClientConfiguration;
				if (!this.missionClientConfigurationCahce.TryGetValue(grouping.Key, out missionClientConfiguration))
				{
					missionClientConfiguration = new MissionClientConfiguration
					{
						MissionId = grouping.Key
					};
					this.missionClientConfigurationCahce.Add(grouping.Key, missionClientConfiguration);
				}
				missionClientConfiguration.VisualHints = grouping.ToDictionary((HintMessage m) => m.MessageId, (HintMessage m) => m);
			}
		}
		List<HintMessage> list2 = null;
		List<HintMessage> processBeforeActiveMissionChanged = null;
		List<HintMessage> list3 = null;
		if (!string.IsNullOrEmpty(missionsResponse.MessagesToSend))
		{
			HashSet<string> messagesUsed = new HashSet<string>();
			list2 = JsonConvert.DeserializeObject<List<HintMessage>>(missionsResponse.MessagesToSend, SerializationHelper.MissionHintSerializerSettings).Select(delegate(HintMessage message)
			{
				MissionClientConfiguration missionClientConfiguration2;
				HintMessage hintMessage4;
				if (message.IsCompact && this.missionClientConfigurationCahce.TryGetValue(message.MissionId, out missionClientConfiguration2) && missionClientConfiguration2.VisualHints.TryGetValue(message.MessageId, out hintMessage4))
				{
					if (messagesUsed.Contains(hintMessage4.MessageId))
					{
						hintMessage4 = hintMessage4.CloneForClient();
					}
					else
					{
						messagesUsed.Add(hintMessage4.MessageId);
					}
					hintMessage4.EventType = message.EventType;
					hintMessage4.Strings = message.Strings;
					return hintMessage4;
				}
				return message;
			}).ToList<HintMessage>();
			int currentMissionId = ClientMissionsManager.Instance.CurrentTrackedMissionId;
			processBeforeActiveMissionChanged = list2.Where((HintMessage m) => m.MissionId == currentMissionId && m.InteractiveObjectId == null && !m.IsCancelMissionEvent()).ToList<HintMessage>();
			list3 = list2.Where((HintMessage m) => !processBeforeActiveMissionChanged.Contains(m)).ToList<HintMessage>();
		}
		if (!string.IsNullOrEmpty(missionsResponse.CompleteMessages))
		{
			List<Dictionary<byte, object>> list4 = JsonConvert.DeserializeObject<List<Dictionary<byte, object>>>(missionsResponse.CompleteMessages, SerializationHelper.JsonSerializerSettings);
			foreach (Dictionary<byte, object> dictionary in list4)
			{
				this.HandleMissionReward(dictionary);
			}
		}
		if (list2 != null)
		{
			foreach (HintMessage hintMessage in list2.Where((HintMessage m) => m.EventType == MissionEventType.MissionFailed && m.TimeToComplete != null))
			{
				if (this.OnMissionTimedOut != null)
				{
					this.OnMissionTimedOut(hintMessage.MissionId, hintMessage.MissionName, hintMessage.ImageId);
				}
			}
		}
		if (processBeforeActiveMissionChanged != null && processBeforeActiveMissionChanged.Any<HintMessage>())
		{
			ClientMissionsManager.Instance.ProcessMessages(processBeforeActiveMissionChanged);
		}
		if (!string.IsNullOrEmpty(missionsResponse.ProgressMessages))
		{
			List<MissionTaskOnClient> list5 = JsonConvert.DeserializeObject<List<MissionTaskOnClient>>(missionsResponse.ProgressMessages, SerializationHelper.JsonSerializerSettings);
			if (list5 != null && list5.Any<MissionTaskOnClient>())
			{
				ClientMissionsManager.Instance.UpdateCurrentMissionTasks(list5);
				if (this.MissionProgressReceived != null)
				{
					this.MissionProgressReceived(list5);
				}
			}
		}
		if (!string.IsNullOrEmpty(missionsResponse.TaskTrackingStopMessages))
		{
			List<MissionTaskTrackedOnClient> list6 = JsonConvert.DeserializeObject<List<MissionTaskTrackedOnClient>>(missionsResponse.TaskTrackingStopMessages, SerializationHelper.JsonSerializerSettings);
			if (list6 != null && list6.Any<MissionTaskTrackedOnClient>())
			{
				foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient in list6)
				{
					ClientMissionsManager.Instance.StopTracking(missionTaskTrackedOnClient);
				}
			}
		}
		if (!string.IsNullOrEmpty(missionsResponse.TaskTrackingStartMessages))
		{
			List<MissionTaskTrackedOnClient> list7 = JsonConvert.DeserializeObject<List<MissionTaskTrackedOnClient>>(missionsResponse.TaskTrackingStartMessages, SerializationHelper.JsonSerializerSettings);
			if (list7 != null && list7.Any<MissionTaskTrackedOnClient>())
			{
				foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient2 in list7)
				{
					ClientMissionsManager.Instance.StartTracking(missionTaskTrackedOnClient2);
				}
			}
		}
		if (missionsResponse.ActiveMissionId != null && this.ActiveMissionChanged != null)
		{
			this.ActiveMissionChanged(missionsResponse.ActiveMissionId.Value);
		}
		if (list3 != null && list3.Any<HintMessage>())
		{
			ClientMissionsManager.Instance.ProcessMessages(list3);
		}
		if (list2 != null)
		{
			foreach (HintMessage hintMessage2 in list2.Where((HintMessage m) => m.IsCancelMissionEvent()))
			{
				if (this.OnMissionTrackStopped != null)
				{
					this.OnMissionTrackStopped(hintMessage2.MissionId);
				}
			}
			foreach (HintMessage hintMessage3 in list2.Where((HintMessage m) => m.IsStartMissionEvent()))
			{
				if (this.OnMissionTrackStarted != null)
				{
					this.OnMissionTrackStarted(hintMessage3.MissionId);
				}
			}
		}
	}

	private void HandleMissionReward(Dictionary<byte, object> eventData)
	{
		AchivementInfo achivementInfo = new AchivementInfo();
		if (eventData.ContainsKey(187))
		{
			this.ParceAndApplyMoneyReward(eventData, out achivementInfo.Amount1, out achivementInfo.Amount2);
		}
		if (eventData.ContainsKey(170))
		{
			achivementInfo.Experience = Convert.ToInt32(eventData[170]);
		}
		if (eventData.ContainsKey(156))
		{
			string text = (string)eventData[156];
			if (!string.IsNullOrEmpty(text))
			{
				achivementInfo.ItemRewards = JsonConvert.DeserializeObject<ItemReward[]>(text, SerializationHelper.JsonSerializerSettings);
			}
		}
		if (eventData.ContainsKey(155))
		{
			string text2 = (string)eventData[155];
			if (!string.IsNullOrEmpty(text2))
			{
				achivementInfo.ProductReward = JsonConvert.DeserializeObject<ProductReward[]>(text2, SerializationHelper.JsonSerializerSettings);
			}
		}
		if (eventData.ContainsKey(153))
		{
			string text3 = (string)eventData[153];
			if (!string.IsNullOrEmpty(text3))
			{
				achivementInfo.LicenseReward = JsonConvert.DeserializeObject<LicenseRef[]>(text3, SerializationHelper.JsonSerializerSettings);
			}
		}
		int num = 0;
		if (eventData.ContainsKey(194))
		{
			num = Convert.ToInt32(eventData[194]);
		}
		string text4 = (string)eventData[180];
		string text5 = (string)eventData[164];
		int? num2 = null;
		if (eventData.ContainsKey(178))
		{
			num2 = new int?(Convert.ToInt32(eventData[178]));
		}
		if (eventData.ContainsKey(179))
		{
			string text6 = (string)eventData[179];
		}
		bool flag = false;
		if (eventData.ContainsKey(163))
		{
			flag = (bool)eventData[163];
		}
		if (!flag && this.OnMissionReward != null)
		{
			this.OnMissionReward(num, text4, text5, num2, achivementInfo);
		}
	}

	private void HandleMissionOperationResponse(OperationResponse response)
	{
		MissionOperationCode subOpCode = (byte)response.Parameters[191];
		OpTimer.End(130, subOpCode);
		if (response.ReturnCode == 0)
		{
			switch (subOpCode)
			{
			case 1:
			case 2:
			case 3:
			case 4:
				if (this.MissionsListReceived != null)
				{
					this.DeserializeListAsync<MissionOnClient>(response, delegate(IEnumerable<MissionOnClient> result)
					{
						if (this.MissionsListReceived != null)
						{
							this.MissionsListReceived(subOpCode, result.ToList<MissionOnClient>());
						}
					}, false);
				}
				break;
			case 5:
			{
				int? num = (int?)response[194];
				if (this.ActiveMissionSet != null)
				{
					this.ActiveMissionSet(num);
				}
				break;
			}
			case 6:
			{
				MissionOnClient missionOnClient = null;
				if (response.Parameters.ContainsKey(192))
				{
					byte[] array = (byte[])response[192];
					missionOnClient = JsonConvert.DeserializeObject<MissionOnClient>(CompressHelper.DecompressString(array), SerializationHelper.JsonSerializerSettings);
				}
				if (this.ActiveMissionGet != null)
				{
					this.ActiveMissionGet(missionOnClient);
				}
				break;
			}
			case 7:
			{
				GameScreenType gameScreenType = (GameScreenType)response.Parameters[193];
				GameScreenTabType gameScreenTabType = (GameScreenTabType)response.Parameters[188];
				if (this.GameScreenChanged != null)
				{
					this.GameScreenChanged(gameScreenType, gameScreenTabType);
				}
				break;
			}
			case 8:
			{
				GameIndicatorType gameIndicatorType = (GameIndicatorType)response.Parameters[193];
				int num2 = (int)response.Parameters[188];
				if (this.IndicatorChanged != null)
				{
					this.IndicatorChanged(gameIndicatorType, num2);
				}
				break;
			}
			case 9:
			{
				int? num3 = (int?)response[194];
				if (this.ArchivedMissionRestarted != null)
				{
					this.ArchivedMissionRestarted(num3);
				}
				break;
			}
			case 10:
			{
				byte[] array2 = (byte[])response[192];
				InteractiveObjectResource interactiveObjectResource = JsonConvert.DeserializeObject<InteractiveObjectResource>(CompressHelper.DecompressString(array2), SerializationHelper.JsonSerializerSettings);
				int num4 = (int)response[194];
				string text = (string)response[244];
				if (this.OnGotInteractiveObject != null)
				{
					this.OnGotInteractiveObject(num4, text, interactiveObjectResource);
				}
				ClientMissionsManager.Instance.InteractiveObjectGot(num4, text, interactiveObjectResource);
				break;
			}
			case 11:
			{
				int num5 = (int)response[194];
				string text2 = (string)response[244];
				string text3 = (string)response[196];
				if (this.MissionInteractionSent != null)
				{
					this.MissionInteractionSent(num5, text2, text3);
				}
				ClientMissionsManager.Instance.OnInteracted(num5, text2, text3);
				break;
			}
			case 12:
				if (this.MissionActivationDisabled != null)
				{
					this.MissionActivationDisabled();
				}
				break;
			case 13:
			{
				MissionOnClient missionOnClient2 = null;
				if (response.Parameters.ContainsKey(192))
				{
					byte[] array3 = (byte[])response[192];
					missionOnClient2 = JsonConvert.DeserializeObject<MissionOnClient>(CompressHelper.DecompressString(array3), SerializationHelper.JsonSerializerSettings);
				}
				if (this.MissionGet != null)
				{
					this.MissionGet(missionOnClient2);
				}
				break;
			}
			case 14:
				if (this.PlayerPositionAndRotationSent != null)
				{
					this.PlayerPositionAndRotationSent();
				}
				break;
			case 15:
			{
				GameElementType gameElementType = (GameElementType)response.Parameters[193];
				string text4 = null;
				if (response.Parameters.ContainsKey(188))
				{
					text4 = (string)response.Parameters[188];
				}
				int? num6 = null;
				if (response.Parameters.ContainsKey(194))
				{
					num6 = new int?((int)response.Parameters[194]);
				}
				if (this.SelectedElementChanged != null)
				{
					this.SelectedElementChanged(gameElementType, text4, num6);
				}
				break;
			}
			case 16:
			{
				GameSwitchType gameSwitchType = (GameSwitchType)response.Parameters[193];
				bool flag = (bool)response.Parameters[188];
				if (this.SwitchChanged != null)
				{
					this.SwitchChanged(gameSwitchType, flag);
				}
				break;
			}
			case 17:
			{
				GameActionType gameActionType = (GameActionType)response.Parameters[193];
				if (this.GameActionSent != null)
				{
					this.GameActionSent(gameActionType);
				}
				break;
			}
			case 18:
			{
				int num7 = (int)response[194];
				int num8 = (int)response[219];
				string text5 = (string)response[244];
				bool flag2 = (bool)response[193];
				string text6 = (string)response[218];
				if (this.MissionClientConditionCompleted != null)
				{
					this.MissionClientConditionCompleted(num7, num8, text5, flag2, text6);
				}
				ClientMissionsManager.Instance.MissionClientConditionCompleted(num7, num8, text5, flag2, text6);
				break;
			}
			case 19:
				if (this.OnForceProcessMissions != null)
				{
					this.OnForceProcessMissions();
				}
				break;
			}
		}
		else
		{
			Failure failure = new Failure(response);
			Debug.LogError(failure.FullErrorInfo);
			switch (subOpCode)
			{
			case 1:
			case 2:
			case 3:
			case 4:
				if (this.GetMissionsListFailed != null)
				{
					this.GetMissionsListFailed(failure);
				}
				break;
			case 5:
				if (this.ActiveMissionSetFailed != null)
				{
					this.ActiveMissionSetFailed(failure);
				}
				break;
			case 6:
				if (this.ActiveMissionGetFailed != null)
				{
					this.ActiveMissionGetFailed(failure);
				}
				break;
			case 7:
				if (this.ChangeGameScreenFailed != null)
				{
					this.ChangeGameScreenFailed(failure);
				}
				break;
			case 8:
				if (this.ChangeIndicatorFailed != null)
				{
					this.ChangeIndicatorFailed(failure);
				}
				break;
			case 9:
				if (this.RestartArchivedMissionFailed != null)
				{
					this.RestartArchivedMissionFailed(failure);
				}
				break;
			case 10:
				if (this.GetInteractiveObjectFailed != null)
				{
					this.GetInteractiveObjectFailed(failure);
				}
				break;
			case 11:
				if (this.SendMissionInteractionFailed != null)
				{
					this.SendMissionInteractionFailed(failure);
				}
				break;
			case 12:
				if (this.DisableMissionActivationFailed != null)
				{
					this.DisableMissionActivationFailed(failure);
				}
				break;
			case 13:
				if (this.MissionGetFailed != null)
				{
					this.MissionGetFailed(failure);
				}
				break;
			case 14:
				if (this.SendPlayerPositionAndRotationFailed != null)
				{
					this.SendPlayerPositionAndRotationFailed(failure);
				}
				break;
			case 15:
				if (this.ChangeSelectedElementFailed != null)
				{
					this.ChangeSelectedElementFailed(failure);
				}
				break;
			case 16:
				if (this.ChangeSwitchFailed != null)
				{
					this.ChangeSwitchFailed(failure);
				}
				break;
			case 17:
				if (this.SendGameActionFailed != null)
				{
					this.SendGameActionFailed(failure);
				}
				break;
			case 18:
				if (this.CompleteMissionClientConditionFailed != null)
				{
					this.CompleteMissionClientConditionFailed(failure);
				}
				break;
			case 19:
				if (this.OnForceProcessMissionsFailed != null)
				{
					this.OnForceProcessMissionsFailed(failure);
				}
				break;
			}
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMissionReward OnMissionReward;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMissionFailed OnMissionTimedOut;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMissionTracked OnMissionTrackStarted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMissionTracked OnMissionTrackStopped;

	public void SendGameAction(GameActionCode actionCode, Hashtable actionData = null)
	{
		if (!this.IsInRoom)
		{
			Debug.LogWarningFormat("An attempt to send GameAction before Room initialized: {0}", new object[] { actionCode });
			return;
		}
		if (!this.IsGameResumed && actionCode != 255 && actionCode != 254 && actionCode != 253 && actionCode != 17 && actionCode != 18 && actionCode != 22)
		{
			Debug.LogWarningFormat("An attempt to send GameAction before Game resumed: {0}", new object[] { actionCode });
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(193);
		operationRequest.Parameters[186] = actionCode;
		if (actionData != null)
		{
			operationRequest.Parameters[185] = actionData;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(193, actionCode, actionCode != 3 && actionCode != 7 && actionCode != 5);
	}

	private void UpdateFishCage(InventoryItem itemMoved = null, bool isNew = false)
	{
		if (itemMoved == null)
		{
			FishCage fishCage = (FishCage)this.profile.Inventory.FirstOrDefault((InventoryItem i) => i is FishCage && i.Storage == StoragePlaces.Doll);
			if (fishCage != null)
			{
				if (this.profile.FishCage == null)
				{
					this.profile.FishCage = new FishCageContents();
				}
				this.profile.FishCage.Cage = fishCage;
			}
		}
		else if (itemMoved is FishCage && itemMoved.Storage == StoragePlaces.Doll)
		{
			if (this.profile.FishCage == null)
			{
				this.profile.FishCage = new FishCageContents();
			}
			if (this.profile.FishCage.Cage != itemMoved)
			{
				this.profile.FishCage.Clear();
			}
			this.profile.FishCage.Cage = (FishCage)itemMoved;
		}
		else if (itemMoved is FishCage && !isNew && itemMoved.Storage != StoragePlaces.Doll)
		{
			this.profile.FishCage = null;
		}
	}

	public void DebugReturn(DebugLevel level, string message)
	{
		if (level == 1)
		{
			this.LastError = message;
			Debug.LogError("ERROR:\r\n" + message);
		}
		else if (level == 2)
		{
			Debug.LogError(message);
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

	public void OnEvent(EventData eventData)
	{
		PhotonServerConnection.<OnEvent>c__AnonStorey2D <OnEvent>c__AnonStorey2D = new PhotonServerConnection.<OnEvent>c__AnonStorey2D();
		<OnEvent>c__AnonStorey2D.eventData = eventData;
		<OnEvent>c__AnonStorey2D.$this = this;
		EventCode code = <OnEvent>c__AnonStorey2D.eventData.Code;
		switch (code)
		{
		case 137:
		{
			byte[] array = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionRevertMessage userCompetitionRevertMessage = JsonConvert.DeserializeObject<UserCompetitionRevertMessage>(CompressHelper.DecompressString(array), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionReverted != null)
			{
				this.OnUserCompetitionReverted(userCompetitionRevertMessage);
			}
			break;
		}
		case 138:
		{
			string text = CompressHelper.DecompressString((byte[])<OnEvent>c__AnonStorey2D.eventData.Parameters[192]);
			if (this.ServerCachesRefreshed != null)
			{
				this.ServerCachesRefreshed(text.Split(new char[] { ',' }));
			}
			break;
		}
		case 139:
		{
			byte[] array2 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionReviewMessage userCompetitionReviewMessage = JsonConvert.DeserializeObject<UserCompetitionReviewMessage>(CompressHelper.DecompressString(array2), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionApprovedOnReview != null)
			{
				this.OnUserCompetitionApprovedOnReview(userCompetitionReviewMessage);
			}
			break;
		}
		case 140:
		{
			byte[] array3 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionReviewMessage userCompetitionReviewMessage2 = JsonConvert.DeserializeObject<UserCompetitionReviewMessage>(CompressHelper.DecompressString(array3), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionDeclinedOnReview != null)
			{
				this.OnUserCompetitionDeclinedOnReview(userCompetitionReviewMessage2);
			}
			break;
		}
		case 141:
		{
			byte[] array4 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionStartMessage userCompetitionStartMessage = JsonConvert.DeserializeObject<UserCompetitionStartMessage>(CompressHelper.DecompressString(array4), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionStarted != null)
			{
				this.OnUserCompetitionStarted(userCompetitionStartMessage);
			}
			break;
		}
		case 142:
		{
			byte[] array5 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionCancellationMessage userCompetitionCancellationMessage = JsonConvert.DeserializeObject<UserCompetitionCancellationMessage>(CompressHelper.DecompressString(array5), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionCancelled != null)
			{
				this.OnUserCompetitionCancelled(userCompetitionCancellationMessage);
			}
			break;
		}
		case 143:
		{
			byte[] array6 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionPromotionMessage userCompetitionPromotionMessage = JsonConvert.DeserializeObject<UserCompetitionPromotionMessage>(CompressHelper.DecompressString(array6), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionPromotion != null)
			{
				this.OnUserCompetitionPromotion(userCompetitionPromotionMessage);
			}
			break;
		}
		case 144:
		{
			byte[] array7 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionReviewMessage userCompetitionReviewMessage3 = JsonConvert.DeserializeObject<UserCompetitionReviewMessage>(CompressHelper.DecompressString(array7), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionRemovedOnReview != null)
			{
				this.OnUserCompetitionRemovedOnReview(userCompetitionReviewMessage3);
			}
			break;
		}
		case 145:
		{
			byte[] array8 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionTeamExchangeResponseMessage userCompetitionTeamExchangeResponseMessage = JsonConvert.DeserializeObject<UserCompetitionTeamExchangeResponseMessage>(CompressHelper.DecompressString(array8), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionTeamExchangeResponse != null)
			{
				this.OnUserCompetitionTeamExchangeResponse(userCompetitionTeamExchangeResponseMessage);
			}
			break;
		}
		case 146:
		{
			byte[] array9 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionTeamExchangeMessage userCompetitionTeamExchangeMessage = JsonConvert.DeserializeObject<UserCompetitionTeamExchangeMessage>(CompressHelper.DecompressString(array9), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionTeamExchange != null)
			{
				this.OnUserCompetitionTeamExchange(userCompetitionTeamExchangeMessage);
			}
			break;
		}
		case 147:
		{
			byte[] array10 = (byte[])<OnEvent>c__AnonStorey2D.eventData[192];
			<OnEvent>c__AnonStorey2D.json = CompressHelper.DecompressString(array10);
			if (this.OnLoyatyBonusTriggered != null)
			{
				this.OnLoyatyBonusTriggered(JsonConvert.DeserializeObject<LoyaltyBonus>(<OnEvent>c__AnonStorey2D.json));
			}
			break;
		}
		case 148:
		case 155:
		case 189:
		case 190:
		case 191:
		case 192:
		case 193:
		case 194:
		case 195:
			this.HandleGameEvent(<OnEvent>c__AnonStorey2D.eventData);
			break;
		case 149:
		{
			string text2 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[194];
			int num = (int)<OnEvent>c__AnonStorey2D.eventData.Parameters[188];
			StatsCounterType statsCounterType = (StatsCounterType)Enum.Parse(typeof(StatsCounterType), text2);
			if (this.profile != null && this.profile.Stats != null)
			{
				this.Profile.Stats.OverrideCounterValue(statsCounterType, num);
			}
			if (this.OnStatsUpdated != null)
			{
				this.OnStatsUpdated(statsCounterType, num);
			}
			break;
		}
		case 150:
		case 197:
			if (this.profile != null)
			{
				int num2 = ((!<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(150)) ? 0 : ((int)<OnEvent>c__AnonStorey2D.eventData.Parameters[150]));
				this.HandleItemsUpdatedEvents(<OnEvent>c__AnonStorey2D.eventData.Parameters, num2);
				if (code == 150 && this.OnNeedClientReset != null)
				{
					this.OnNeedClientReset(num2);
				}
			}
			break;
		case 151:
		{
			string text3 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[11];
			string text4 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[192];
			BuoySetting buoySetting = JsonConvert.DeserializeObject<BuoySetting>(text4, SerializationHelper.JsonSerializerSettings);
			this.Profile.AddBuoyShareRequest(buoySetting, false);
			if (this.OnReceiveBuoyShareRequest != null)
			{
				this.OnReceiveBuoyShareRequest(buoySetting);
			}
			break;
		}
		case 153:
		{
			byte[] array11 = (byte[])<OnEvent>c__AnonStorey2D.eventData[149];
			string text5 = CompressHelper.DecompressString(array11);
			MissionsResponse missionsResponse = JsonConvert.DeserializeObject<MissionsResponse>(text5, SerializationHelper.JsonSerializerSettings);
			this.HandleMissionEvent(missionsResponse);
			break;
		}
		case 154:
		{
			string text6 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[5];
			string text7 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[3];
			string text8 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[192];
			RodSetup rodSetup = JsonConvert.DeserializeObject<RodSetup>(text8, SerializationHelper.JsonSerializerSettings);
			if (this.Profile.Inventory.RodSetupsCount < this.Profile.Inventory.CurrentRodSetupCapacity)
			{
				this.Profile.Inventory.AddSetup(rodSetup);
			}
			if (this.OnReceiveNewRodSetup != null)
			{
				this.OnReceiveNewRodSetup(rodSetup, text6);
			}
			break;
		}
		case 156:
		{
			bool flag = (bool)<OnEvent>c__AnonStorey2D.eventData[193];
			bool flag2 = (bool)<OnEvent>c__AnonStorey2D.eventData[244];
			AchivementInfo achivementInfo = new AchivementInfo();
			this.ParceAndApplyMoneyReward(<OnEvent>c__AnonStorey2D.eventData.Parameters, out achivementInfo.Amount1, out achivementInfo.Amount2);
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(156))
			{
				string text9 = (string)<OnEvent>c__AnonStorey2D.eventData[156];
				if (!string.IsNullOrEmpty(text9))
				{
					achivementInfo.ItemRewards = JsonConvert.DeserializeObject<ItemReward[]>(text9, SerializationHelper.JsonSerializerSettings);
				}
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(155))
			{
				string text10 = (string)<OnEvent>c__AnonStorey2D.eventData[155];
				if (!string.IsNullOrEmpty(text10))
				{
					achivementInfo.ProductReward = JsonConvert.DeserializeObject<ProductReward[]>(text10, SerializationHelper.JsonSerializerSettings);
				}
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(153))
			{
				string text11 = (string)<OnEvent>c__AnonStorey2D.eventData[153];
				if (!string.IsNullOrEmpty(text11))
				{
					achivementInfo.LicenseReward = JsonConvert.DeserializeObject<LicenseRef[]>(text11, SerializationHelper.JsonSerializerSettings);
				}
			}
			if (this.OnReferralReward != null)
			{
				this.OnReferralReward(flag, flag2, achivementInfo);
			}
			break;
		}
		case 157:
		{
			bool flag3 = (bool)<OnEvent>c__AnonStorey2D.eventData[193];
			if (flag3)
			{
				long num3 = (long)<OnEvent>c__AnonStorey2D.eventData[245];
				int num4 = (int)<OnEvent>c__AnonStorey2D.eventData[188];
				if (this.OnFarmRebootTriggered != null)
				{
					this.OnFarmRebootTriggered(new TimeSpan(num3), num4);
				}
			}
			else if (this.OnFarmRebootCanceled != null)
			{
				this.OnFarmRebootCanceled();
			}
			break;
		}
		case 158:
		{
			byte[] array10 = (byte[])<OnEvent>c__AnonStorey2D.eventData[192];
			<OnEvent>c__AnonStorey2D.json = CompressHelper.DecompressString(array10);
			if (this.OnAdsTriggered != null)
			{
				this.OnAdsTriggered(JsonConvert.DeserializeObject<TargetedAdSlide[]>(<OnEvent>c__AnonStorey2D.json));
			}
			break;
		}
		case 159:
			if (this.OnTournamentLogoRequested != null)
			{
				this.OnTournamentLogoRequested();
			}
			break;
		case 160:
			if (this.profile != null)
			{
				byte[] array12 = (byte[])<OnEvent>c__AnonStorey2D.eventData[192];
				<OnEvent>c__AnonStorey2D.json = CompressHelper.DecompressString(array12);
				List<PlayerLicense> list = JsonConvert.DeserializeObject<List<PlayerLicense>>(<OnEvent>c__AnonStorey2D.json, SerializationHelper.JsonSerializerSettings);
				<OnEvent>c__AnonStorey2D.announce = <OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(193) && (bool)<OnEvent>c__AnonStorey2D.eventData.Parameters[193];
				foreach (PlayerLicense playerLicense in list)
				{
					PlayerLicense playerLicense2 = this.profile.AddLicense(playerLicense);
					if (playerLicense2.End != null && playerLicense.End != null)
					{
						DateTime? end = playerLicense2.End;
						bool flag4 = end != null;
						DateTime? end2 = playerLicense.End;
						if ((flag4 & (end2 != null)) && end.GetValueOrDefault() < end2.GetValueOrDefault())
						{
							playerLicense2.End = playerLicense.End;
						}
					}
				}
				if (this.OnLicensesGained != null)
				{
					this.OnLicensesGained(list, <OnEvent>c__AnonStorey2D.announce);
				}
			}
			break;
		case 161:
		{
			if (!StaticUserData.IS_TPM_ENABLED)
			{
				return;
			}
			Hashtable hashtable = (Hashtable)<OnEvent>c__AnonStorey2D.eventData[245];
			CharacterEventType characterEventType = (CharacterEventType)((byte)hashtable[1]);
			CharacterInfo characterInfo = CharacterInfo.FromHashtable(hashtable);
			if (characterEventType == CharacterEventType.Update && this.OnCharacterUpdated != null && characterInfo.Package != null)
			{
				this.OnCharacterUpdated(characterInfo);
			}
			break;
		}
		case 162:
		{
			TournamentCancelInfo tournamentCancelInfo = new TournamentCancelInfo();
			tournamentCancelInfo.TournamentId = (int)<OnEvent>c__AnonStorey2D.eventData[21];
			tournamentCancelInfo.Name = (string)<OnEvent>c__AnonStorey2D.eventData[30];
			tournamentCancelInfo.FeeReturned = (int)<OnEvent>c__AnonStorey2D.eventData[31];
			tournamentCancelInfo.Currency = (string)<OnEvent>c__AnonStorey2D.eventData[32];
			if (this.CheckProfile())
			{
				this.Profile.IncrementBalance(tournamentCancelInfo.Currency, (double)tournamentCancelInfo.FeeReturned);
			}
			if (this.OnTournamentCancelled != null)
			{
				this.OnTournamentCancelled(tournamentCancelInfo);
			}
			break;
		}
		case 163:
		{
			ErrorCode errorCode = (short)<OnEvent>c__AnonStorey2D.eventData[188];
			if (errorCode == null)
			{
				byte[] array10 = (byte[])<OnEvent>c__AnonStorey2D.eventData.Parameters[192];
				<OnEvent>c__AnonStorey2D.json = CompressHelper.DecompressString(array10);
				ProfileTournament profileTournament = JsonConvert.DeserializeObject<ProfileTournament>(<OnEvent>c__AnonStorey2D.json, SerializationHelper.JsonSerializerSettings);
				this.profile.Tournament = profileTournament;
				this.profile.StartTournamentTime();
				if (this.OnTournamentScoringStarted != null)
				{
					this.OnTournamentScoringStarted();
				}
			}
			else if (this.OnStartTournamentScoringFailed != null)
			{
				this.OnStartTournamentScoringFailed(new Failure
				{
					ErrorCode = errorCode,
					ErrorMessage = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[196]
				});
			}
			break;
		}
		case 164:
		{
			byte[] array13 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionInvitationResponseMessage userCompetitionInvitationResponseMessage = JsonConvert.DeserializeObject<UserCompetitionInvitationResponseMessage>(CompressHelper.DecompressString(array13), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionInvitationResponse != null)
			{
				this.OnUserCompetitionInvitationResponse(userCompetitionInvitationResponseMessage);
			}
			break;
		}
		case 165:
		{
			byte[] array14 = (byte[])<OnEvent>c__AnonStorey2D.eventData[10];
			UserCompetitionInvitationMessage userCompetitionInvitationMessage = JsonConvert.DeserializeObject<UserCompetitionInvitationMessage>(CompressHelper.DecompressString(array14), SerializationHelper.JsonSerializerSettings);
			if (this.OnUserCompetitionInvitation != null)
			{
				this.OnUserCompetitionInvitation(userCompetitionInvitationMessage);
			}
			break;
		}
		case 166:
		{
			HashSet<ushort> hashSet = JsonConvert.DeserializeObject<HashSet<ushort>>(CompressHelper.DecompressString((byte[])<OnEvent>c__AnonStorey2D.eventData.Parameters[192]));
			if (this.OnFBAZonesUpdate != null)
			{
				this.OnFBAZonesUpdate(hashSet);
			}
			break;
		}
		case 168:
		{
			<OnEvent>c__AnonStorey2D.announce = <OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(193) && (bool)<OnEvent>c__AnonStorey2D.eventData.Parameters[193];
			byte[] array10 = (byte[])<OnEvent>c__AnonStorey2D.eventData[192];
			ProfileProduct profileProduct = JsonConvert.DeserializeObject<ProfileProduct>(CompressHelper.DecompressString(array10), SerializationHelper.JsonSerializerSettings);
			if (this.profile != null)
			{
				this.profile.PutProductToProfile(profileProduct);
				if (profileProduct.Items != null && profileProduct.Items.Length > 0)
				{
					this.profile.Inventory.ResolveParents(profileProduct.Items);
					this.RaiseInventoryUpdated(false);
				}
			}
			if (this.OnProductDelivered != null)
			{
				this.OnProductDelivered(profileProduct, 1, <OnEvent>c__AnonStorey2D.announce);
			}
			if (profileProduct.PondsUnlocked != null && profileProduct.PondsUnlocked.Length > 0 && this.OnPondLevelInvalidated != null)
			{
				this.OnPondLevelInvalidated();
			}
			break;
		}
		case 169:
			if (this.profile != null)
			{
				this.profile.SubscriptionId = null;
				this.profile.SubscriptionEndDate = null;
				this.profile.ExpMultiplier = null;
			}
			if (this.OnSubscriptionEnded != null)
			{
				this.OnSubscriptionEnded();
			}
			break;
		case 170:
		{
			TournamentUpdateInfo tournamentUpdateInfo = new TournamentUpdateInfo();
			tournamentUpdateInfo.TournamentId = (int)<OnEvent>c__AnonStorey2D.eventData[21];
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(23))
			{
				tournamentUpdateInfo.Score = (float)<OnEvent>c__AnonStorey2D.eventData[23];
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(33))
			{
				tournamentUpdateInfo.SecondaryScore = (float)<OnEvent>c__AnonStorey2D.eventData[33];
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(24))
			{
				tournamentUpdateInfo.PredictedScore = (float)<OnEvent>c__AnonStorey2D.eventData[24];
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(26))
			{
				tournamentUpdateInfo.Place = (int)<OnEvent>c__AnonStorey2D.eventData[26];
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(28))
			{
				tournamentUpdateInfo.PredictedPlace = (int)<OnEvent>c__AnonStorey2D.eventData[28];
			}
			if (this.OnTournamentUpdate != null)
			{
				this.OnTournamentUpdate(tournamentUpdateInfo);
			}
			break;
		}
		case 171:
		{
			string text12 = CompressHelper.DecompressString((byte[])<OnEvent>c__AnonStorey2D.eventData[10]);
			EndTournamentTimeResult endTournamentResult = JsonConvert.DeserializeObject<EndTournamentTimeResult>(text12, SerializationHelper.JsonSerializerSettings);
			Action processTournamentTimeEnded = delegate
			{
				if (endTournamentResult.Tournament != null)
				{
					<OnEvent>c__AnonStorey2D.$this.profile.Tournament = endTournamentResult.Tournament;
				}
				bool flag6 = <OnEvent>c__AnonStorey2D.$this.profile.Tournament != null;
				if (flag6)
				{
					<OnEvent>c__AnonStorey2D.$this.profile.Tournament.IsEnded = true;
				}
				EndTournamentSellFishInfo sellFishInfo = endTournamentResult.SellFishInfo;
				if (<OnEvent>c__AnonStorey2D.$this.profile != null)
				{
					if (sellFishInfo.FishGold != null)
					{
						<OnEvent>c__AnonStorey2D.$this.profile.IncrementBalance("GC", (double)sellFishInfo.FishGold.Value);
					}
					if (sellFishInfo.FishSilver != null)
					{
						<OnEvent>c__AnonStorey2D.$this.profile.IncrementBalance("SC", (double)sellFishInfo.FishSilver.Value);
					}
				}
				if (flag6 && <OnEvent>c__AnonStorey2D.$this.profile.Tournament.KindId != 4 && <OnEvent>c__AnonStorey2D.$this.OnFishSold != null)
				{
					OnFishSold onFishSold = <OnEvent>c__AnonStorey2D.$this.OnFishSold;
					int num10 = ((sellFishInfo.FishList == null) ? 0 : sellFishInfo.FishList.Count);
					int? fishGold2 = sellFishInfo.FishGold;
					int num11 = ((fishGold2 == null) ? 0 : fishGold2.Value);
					int? fishSilver2 = sellFishInfo.FishSilver;
					onFishSold(num10, num11, (fishSilver2 == null) ? 0 : fishSilver2.Value);
				}
				if (sellFishInfo.FishList != null && <OnEvent>c__AnonStorey2D.$this.profile != null && <OnEvent>c__AnonStorey2D.$this.profile.FishCage != null)
				{
					<OnEvent>c__AnonStorey2D.$this.profile.FishCage.Clear();
					<OnEvent>c__AnonStorey2D.$this.profile.FishCage.Fish.AddRange(sellFishInfo.FishList);
				}
				if (<OnEvent>c__AnonStorey2D.$this.OnTournamentTimeEnded != null)
				{
					<OnEvent>c__AnonStorey2D.$this.OnTournamentTimeEnded(endTournamentResult);
				}
				<OnEvent>c__AnonStorey2D.$this.profile.EndTournamentTime(true);
				if (<OnEvent>c__AnonStorey2D.$this.profile != null && <OnEvent>c__AnonStorey2D.$this.profile.FishCage != null)
				{
					<OnEvent>c__AnonStorey2D.$this.profile.FishCage.Clear();
				}
			};
			if (this.receiveProfileInProgress)
			{
				OnGotProfile gotProfile2 = null;
				gotProfile2 = delegate(Profile p)
				{
					<OnEvent>c__AnonStorey2D.$this.OnGotProfile -= gotProfile2;
					processTournamentTimeEnded();
				};
				this.OnGotProfile += gotProfile2;
			}
			else
			{
				processTournamentTimeEnded();
			}
			break;
		}
		case 172:
		{
			ErrorCode errorCode2 = (short)<OnEvent>c__AnonStorey2D.eventData[188];
			if (this.OnTournamentWrongTackle != null)
			{
				this.OnTournamentWrongTackle(errorCode2);
			}
			break;
		}
		case 173:
		{
			Action processProfileRestoredAfterTournament = delegate
			{
				TimeSpan timeSpan = new TimeSpan((long)<OnEvent>c__AnonStorey2D.eventData.Parameters[181]);
				if (<OnEvent>c__AnonStorey2D.$this.Profile != null)
				{
					<OnEvent>c__AnonStorey2D.$this.Profile.PondTimeSpent = new TimeSpan?(timeSpan);
				}
				if (<OnEvent>c__AnonStorey2D.$this.OnGotTime != null)
				{
					<OnEvent>c__AnonStorey2D.$this.OnGotTime(timeSpan);
				}
				WeatherDesc[] array17 = TravelSerializationHelper.DeserializePondWeather(<OnEvent>c__AnonStorey2D.eventData.Parameters);
				if (<OnEvent>c__AnonStorey2D.$this.OnGotPondWeather != null)
				{
					<OnEvent>c__AnonStorey2D.$this.OnGotPondWeather(array17);
				}
				byte[] array18 = (byte[])<OnEvent>c__AnonStorey2D.eventData.Parameters[193];
				string text23 = CompressHelper.DecompressString(array18);
				List<CaughtFish> list3 = JsonConvert.DeserializeObject<List<CaughtFish>>(text23, SerializationHelper.JsonSerializerSettings);
				<OnEvent>c__AnonStorey2D.$this.UpdateFishCage(null, false);
				if (<OnEvent>c__AnonStorey2D.$this.profile.FishCage != null)
				{
					<OnEvent>c__AnonStorey2D.$this.profile.FishCage.Fish.Clear();
					<OnEvent>c__AnonStorey2D.$this.profile.FishCage.Fish.AddRange(list3);
				}
				if (<OnEvent>c__AnonStorey2D.$this.OnRequestFishCage != null)
				{
					<OnEvent>c__AnonStorey2D.$this.OnRequestFishCage();
				}
			};
			if (this.receiveProfileInProgress)
			{
				OnGotProfile gotProfile = null;
				gotProfile = delegate(Profile p)
				{
					<OnEvent>c__AnonStorey2D.$this.OnGotProfile -= gotProfile;
					processProfileRestoredAfterTournament();
				};
				this.OnGotProfile += gotProfile;
			}
			else
			{
				processProfileRestoredAfterTournament();
			}
			break;
		}
		case 174:
		{
			BonusInfo bonusInfo = new BonusInfo
			{
				Days = (int)<OnEvent>c__AnonStorey2D.eventData[194],
				AllDailyBonuses = JsonConvert.DeserializeObject<Reward[]>((string)<OnEvent>c__AnonStorey2D.eventData[192])
			};
			this.ParceAndApplyMoneyReward(<OnEvent>c__AnonStorey2D.eventData.Parameters, out bonusInfo.Amount1, out bonusInfo.Amount2);
			if (this.OnBonusGained != null)
			{
				this.OnBonusGained(bonusInfo);
			}
			break;
		}
		case 175:
		{
			CaughtFish caughtFish = JsonConvert.DeserializeObject<CaughtFish>((string)<OnEvent>c__AnonStorey2D.eventData.Parameters[188]);
			this.profile.FishCage.Fish.Add(caughtFish);
			if (this.CheckProfile() && <OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(194))
			{
				Guid outdatedFish = new Guid((string)<OnEvent>c__AnonStorey2D.eventData.Parameters[194]);
				CaughtFish caughtFish2 = this.Profile.FishCage.Fish.FirstOrDefault((CaughtFish f) => f.Fish.InstanceId == outdatedFish);
				if (caughtFish2 != null)
				{
					caughtFish2.Fish.TournamentScore = null;
					caughtFish2.Fish.TournamentSecondaryScore = null;
				}
			}
			break;
		}
		case 176:
		{
			PeriodStats periodStats = this.ParseMissionResult(<OnEvent>c__AnonStorey2D.eventData.Parameters);
			short[] array15 = null;
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(193))
			{
				array15 = (short[])<OnEvent>c__AnonStorey2D.eventData.Parameters[193];
			}
			int? fishGold = periodStats.FishGold;
			if (fishGold != null && periodStats.FishGold != 0 && periodStats.FishGold > 0)
			{
				this.profile.IncrementBalance("GC", (double)periodStats.FishGold.Value);
			}
			int? fishSilver = periodStats.FishSilver;
			if (fishSilver != null && periodStats.FishSilver != 0 && periodStats.FishSilver > 0)
			{
				this.profile.IncrementBalance("SC", (double)periodStats.FishSilver.Value);
			}
			if (this.OnEndOfDayResult != null)
			{
				this.OnEndOfDayResult(periodStats);
			}
			if (this.profile.FishCage != null && PhotonConnectionFactory.Instance.Profile.Tournament == null)
			{
				this.profile.FishCage.Clear();
			}
			if (array15 != null)
			{
				this.OnBoatsRentFinished(array15);
			}
			break;
		}
		case 177:
			this.HandleLocalEvent(<OnEvent>c__AnonStorey2D.eventData);
			break;
		case 178:
		{
			AchivementInfo achivementInfo2 = new AchivementInfo();
			achivementInfo2.AchivementName = (string)<OnEvent>c__AnonStorey2D.eventData[180];
			achivementInfo2.AchivementDesc = (string)<OnEvent>c__AnonStorey2D.eventData[164];
			achivementInfo2.AchivementStageName = (string)<OnEvent>c__AnonStorey2D.eventData[179];
			achivementInfo2.AchivementStageCount = (int)<OnEvent>c__AnonStorey2D.eventData[165];
			achivementInfo2.ShowDelay = (float)<OnEvent>c__AnonStorey2D.eventData[163];
			AchivementInfo achivementInfo3 = achivementInfo2;
			int? num5 = (int?)<OnEvent>c__AnonStorey2D.eventData.Parameters[178];
			achivementInfo3.ImageId = ((num5 == null) ? 0 : num5.Value);
			AchivementInfo achivementInfo4 = achivementInfo2;
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(170))
			{
				achivementInfo4.Experience = (int)<OnEvent>c__AnonStorey2D.eventData[170];
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(194))
			{
				achivementInfo4.ExternalAchievementId = (string)<OnEvent>c__AnonStorey2D.eventData[194];
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(156))
			{
				achivementInfo4.ItemRewards = JsonConvert.DeserializeObject<ItemReward[]>((string)<OnEvent>c__AnonStorey2D.eventData[156]);
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(155))
			{
				achivementInfo4.ProductReward = JsonConvert.DeserializeObject<ProductReward[]>((string)<OnEvent>c__AnonStorey2D.eventData[155]);
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(153))
			{
				achivementInfo4.LicenseReward = JsonConvert.DeserializeObject<LicenseRef[]>((string)<OnEvent>c__AnonStorey2D.eventData[153]);
			}
			this.ParceAndApplyMoneyReward(<OnEvent>c__AnonStorey2D.eventData.Parameters, out achivementInfo4.Amount1, out achivementInfo4.Amount2);
			if (this.OnAchivementGained != null)
			{
				this.OnAchivementGained(achivementInfo4);
			}
			break;
		}
		case 179:
		{
			LevelInfo levelInfo = new LevelInfo
			{
				Level = (int)<OnEvent>c__AnonStorey2D.eventData[169],
				IsLevel = (bool)<OnEvent>c__AnonStorey2D.eventData[193],
				ExpToThisLevel = (long)<OnEvent>c__AnonStorey2D.eventData[245],
				ExpToNextLevel = (long)<OnEvent>c__AnonStorey2D.eventData[244]
			};
			this.ParceAndApplyMoneyReward(<OnEvent>c__AnonStorey2D.eventData.Parameters, out levelInfo.Amount1, out levelInfo.Amount2);
			if (this.profile != null)
			{
				if (levelInfo.IsLevel)
				{
					this.profile.Level = levelInfo.Level;
				}
				else
				{
					this.profile.Rank = levelInfo.Level;
				}
				if (levelInfo.IsLevel)
				{
					this.profile.ExpToThisLevel = levelInfo.ExpToThisLevel;
					this.profile.ExpToNextLevel = levelInfo.ExpToNextLevel;
				}
				else
				{
					this.profile.ExpToThisRank = levelInfo.ExpToThisLevel;
					this.profile.ExpToNextRank = levelInfo.ExpToNextLevel;
				}
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(192))
			{
				byte[] array16 = (byte[])<OnEvent>c__AnonStorey2D.eventData.Parameters[192];
				string text13 = CompressHelper.DecompressString(array16);
				List<InventoryItem> list2 = JsonConvert.DeserializeObject<List<InventoryItem>>(text13, SerializationHelper.JsonSerializerSettings);
				this.SetItemsDurability(list2);
				levelInfo.GlobalItemsForLevel = list2;
			}
			if (this.OnLevelGained != null)
			{
				this.OnLevelGained(levelInfo);
			}
			break;
		}
		case 180:
		{
			int num6 = (int)<OnEvent>c__AnonStorey2D.eventData[188];
			int num7 = (int)<OnEvent>c__AnonStorey2D.eventData[194];
			if (this.profile != null)
			{
				this.profile.Experience += (long)num6;
				this.profile.RankExperience += (long)num7;
			}
			if (this.OnExpGained != null)
			{
				this.OnExpGained(num6, num7);
			}
			break;
		}
		case 181:
		{
			HintInfo hintInfo = new HintInfo();
			hintInfo.Code = (byte)<OnEvent>c__AnonStorey2D.eventData[193];
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(195))
			{
				hintInfo.Message = CompressHelper.DecompressString((byte[])<OnEvent>c__AnonStorey2D.eventData[195]);
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(196))
			{
				hintInfo.Message = (string)<OnEvent>c__AnonStorey2D.eventData[196];
			}
			if (<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(150))
			{
				hintInfo.RodSlot = (int)<OnEvent>c__AnonStorey2D.eventData[150];
			}
			if (this.OnGameHint != null)
			{
				this.OnGameHint(hintInfo);
			}
			break;
		}
		case 182:
		{
			int num8 = ((!<OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(150)) ? 0 : ((int)<OnEvent>c__AnonStorey2D.eventData.Parameters[150]));
			MultiRodsErrorCode multiRodsErrorCode = (byte)<OnEvent>c__AnonStorey2D.eventData.Parameters[146];
			if (this.OnRodCantBeUsed != null)
			{
				this.OnRodCantBeUsed(num8, multiRodsErrorCode);
			}
			switch (multiRodsErrorCode)
			{
			case 1:
			{
				if (GameFactory.Player != null)
				{
					GameFactory.Player.StartMoveToNewLocation();
				}
				Rod rod2 = this.AssociateRodInHands();
				if (rod2 != null)
				{
					this.UpdateRodInGameEngine(rod2, StoragePlaces.Storage);
					GameFactory.Player.SaveTakeRodRequest(rod2, false);
				}
				break;
			}
			case 2:
			{
				Rod rodInSlot = this.profile.Inventory.GetRodInSlot(num8);
				if (rodInSlot != null)
				{
					StaticUserData.RodInHand.RefreshRod(rodInSlot.InstanceId);
				}
				break;
			}
			case 3:
			{
				Rod rod = this.profile.Inventory.GetRodInSlot(num8);
				this.RequestProfileOnError(delegate(Profile p)
				{
					if (rod != null)
					{
						StaticUserData.RodInHand.RefreshRod(rod.InstanceId);
					}
				}, false);
				break;
			}
			case 4:
			case 5:
			case 6:
				this.RequestProfileOnError(delegate(Profile p)
				{
					Rod rod3 = <OnEvent>c__AnonStorey2D.$this.AssociateRodInHands();
					if (rod3 != null)
					{
						<OnEvent>c__AnonStorey2D.$this.UpdateRodInGameEngine(rod3, StoragePlaces.Storage);
						StaticUserData.RodInHand.RefreshRod(rod3.InstanceId);
					}
				}, false);
				break;
			}
			break;
		}
		case 183:
		{
			string text14 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[11];
			string text15 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[14];
			string text16 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[192];
			Chum chum = JsonConvert.DeserializeObject<Chum>(text16, SerializationHelper.JsonSerializerSettings);
			if (this.Profile.Inventory.ChumRecipesCount < this.Profile.Inventory.CurrentChumRecipesCapacity)
			{
				InventoryMovementMapInfo inventoryMovementMapInfo = new InventoryMovementMapInfo
				{
					InstanceId = chum.InstanceId
				};
				chum.InstanceId = null;
				this.Profile.Inventory.SaveNewChumRecipe(chum, inventoryMovementMapInfo);
			}
			if (this.OnReceiveNewChumRecipe != null)
			{
				this.OnReceiveNewChumRecipe(chum, text14);
			}
			break;
		}
		case 184:
		{
			string text17 = (string)<OnEvent>c__AnonStorey2D.eventData[244];
			string text18 = (string)<OnEvent>c__AnonStorey2D.eventData[196];
			ServerTraceHelper.Log(this.UserId, text17, text18);
			break;
		}
		case 185:
		{
			string text19 = (string)<OnEvent>c__AnonStorey2D.eventData.Parameters[192];
			ChumPiece chumPiece = JsonConvert.DeserializeObject<ChumPiece>(text19);
			if (this.OnChumPieceUpdated != null)
			{
				this.OnChumPieceUpdated(chumPiece);
			}
			break;
		}
		case 186:
		{
			short num9 = (short)<OnEvent>c__AnonStorey2D.eventData[244];
			bool flag5 = <OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(193) && (bool)<OnEvent>c__AnonStorey2D.eventData[193];
			PondStayFinish pondStayFinish = new PondStayFinish
			{
				Reason = num9,
				ForcePlayerToLeavePond = flag5
			};
			if (this.OnPondStayFinish != null)
			{
				this.OnPondStayFinish(pondStayFinish);
			}
			break;
		}
		case 187:
		case 188:
		{
			object obj = <OnEvent>c__AnonStorey2D.eventData[194];
			PlayerLicense playerLicense3 = null;
			if (obj is int)
			{
				string text20 = (string)<OnEvent>c__AnonStorey2D.eventData[168];
				string text21 = (string)<OnEvent>c__AnonStorey2D.eventData[167];
				playerLicense3 = new PlayerLicense
				{
					LicenseId = (int)obj,
					Name = text20,
					Desc = text21
				};
			}
			else if (this.profile.Licenses != null && obj is string)
			{
				Guid guid = new Guid((string)obj);
				foreach (PlayerLicense playerLicense4 in this.profile.Licenses)
				{
					if (playerLicense4.InstanceId == guid)
					{
						playerLicense3 = playerLicense4;
					}
				}
			}
			LicenseBreakingInfo licenseBreakingInfo = new LicenseBreakingInfo
			{
				Value = (int)((float)<OnEvent>c__AnonStorey2D.eventData[188]),
				Currency = (string)<OnEvent>c__AnonStorey2D.eventData[187],
				License = playerLicense3
			};
			if (code == 187)
			{
				this.Profile.IncrementBalance(licenseBreakingInfo.Currency, (double)(-(double)licenseBreakingInfo.Value));
				if (this.OnLicensePenalty != null)
				{
					this.OnLicensePenalty(licenseBreakingInfo);
				}
			}
			else if (this.OnLicensePenaltyWarning != null)
			{
				this.OnLicensePenaltyWarning(licenseBreakingInfo);
			}
			break;
		}
		case 196:
		{
			Amount amount = new Amount
			{
				Value = (int)<OnEvent>c__AnonStorey2D.eventData[188],
				Currency = (string)<OnEvent>c__AnonStorey2D.eventData[187]
			};
			if (this.profile != null)
			{
				this.profile.IncrementBalance(amount);
			}
			if (this.OnMoneyUpdate != null)
			{
				this.OnMoneyUpdate(amount);
			}
			break;
		}
		case 198:
			if (this.profile != null)
			{
				AsynchProcessor.ProcessByteArrayWorkItem(new InputByteArrayWorkItem
				{
					Data = (byte[])<OnEvent>c__AnonStorey2D.eventData[192],
					ProcessAction = delegate(byte[] p)
					{
						<OnEvent>c__AnonStorey2D.json = CompressHelper.DecompressString(p);
						return JsonConvert.DeserializeObject<List<InventoryItem>>(<OnEvent>c__AnonStorey2D.json, SerializationHelper.JsonSerializerSettings);
					},
					ResultAction = delegate(object r)
					{
						List<InventoryItem> list4 = (List<InventoryItem>)r;
						<OnEvent>c__AnonStorey2D.announce = <OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(193) && (bool)<OnEvent>c__AnonStorey2D.eventData.Parameters[193];
						bool flag7 = <OnEvent>c__AnonStorey2D.eventData.Parameters.ContainsKey(218) && (bool)<OnEvent>c__AnonStorey2D.eventData.Parameters[218];
						if (flag7)
						{
							if (<OnEvent>c__AnonStorey2D.$this.OnItemGained != null)
							{
								<OnEvent>c__AnonStorey2D.$this.OnItemGained(list4, <OnEvent>c__AnonStorey2D.announce);
							}
						}
						else
						{
							foreach (InventoryItem inventoryItem2 in list4)
							{
								<OnEvent>c__AnonStorey2D.$this.profile.Inventory.AddItem(inventoryItem2);
								<OnEvent>c__AnonStorey2D.$this.UpdateFishCage(inventoryItem2, true);
							}
							<OnEvent>c__AnonStorey2D.$this.profile.Inventory.ResolveParents(list4);
							if (<OnEvent>c__AnonStorey2D.$this.OnItemGained != null)
							{
								<OnEvent>c__AnonStorey2D.$this.OnItemGained(list4, <OnEvent>c__AnonStorey2D.announce);
							}
							<OnEvent>c__AnonStorey2D.$this.RaiseInventoryUpdated(false);
						}
					}
				});
			}
			break;
		case 199:
			if (this.profile != null)
			{
				string text22 = (string)<OnEvent>c__AnonStorey2D.eventData[194];
				using (IEnumerator<Guid> enumerator3 = (from id in text22.Split(new char[] { ',' })
					select new Guid(id)).GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Guid instanceId = enumerator3.Current;
						InventoryItem inventoryItem = this.profile.Inventory.FirstOrDefault((InventoryItem i) => i.InstanceId == instanceId);
						if (inventoryItem != null)
						{
							this.profile.Inventory.RemoveItem(inventoryItem, false);
							if (inventoryItem is Chum && StaticUserData.RodInHand != null && StaticUserData.RodInHand.Rod != null && inventoryItem.ParentItemInstanceId != null && StaticUserData.RodInHand.Rod.InstanceId == inventoryItem.ParentItemInstanceId.Value)
							{
								StaticUserData.RodInHand.RefreshRod(StaticUserData.RodInHand.Rod.InstanceId);
							}
							if (this.OnItemLost != null)
							{
								this.OnItemLost(inventoryItem);
							}
						}
					}
				}
			}
			break;
		case 200:
			this.HandleChatMessageEvent(<OnEvent>c__AnonStorey2D.eventData);
			break;
		case 224:
			if (this.OnDuplicateLogin != null)
			{
				this.OnDuplicateLogin();
			}
			this.PreviewDisconnect();
			this.Disconnect();
			break;
		case 225:
			this.Disconnect();
			break;
		case 254:
			this.HandleLeaveEvent(<OnEvent>c__AnonStorey2D.eventData);
			break;
		case 255:
			this.HandleJoinEvent(<OnEvent>c__AnonStorey2D.eventData);
			break;
		}
	}

	private void ParceAndApplyMoneyReward(Dictionary<byte, object> pars, out Amount amount1, out Amount amount2)
	{
		double[] array = (double[])pars[188];
		string[] array2 = (string[])pars[187];
		if (array[0] > 0.0 && !string.IsNullOrEmpty(array2[0]))
		{
			amount1 = new Amount
			{
				Currency = array2[0],
				Value = (int)array[0]
			};
			if (this.profile != null)
			{
				this.profile.IncrementBalance(amount1);
			}
		}
		else
		{
			amount1 = null;
		}
		if (array[1] > 0.0 && !string.IsNullOrEmpty(array2[1]))
		{
			amount2 = new Amount
			{
				Currency = array2[1],
				Value = (int)array[1]
			};
			if (this.profile != null)
			{
				this.profile.IncrementBalance(amount2);
			}
		}
		else
		{
			amount2 = null;
		}
	}

	private PeriodStats ParseMissionResult(Dictionary<byte, object> response)
	{
		return JsonConvert.DeserializeObject<PeriodStats>((string)response[192], SerializationHelper.JsonSerializerSettings);
	}

	public void OnOperationResponse(OperationResponse response)
	{
		this.LastResponse = response;
		OperationCode operationCode = response.OperationCode;
		if (operationCode != 200 && operationCode != 194 && operationCode != 140 && operationCode != 193 && operationCode != 167 && operationCode != 130 && operationCode != 168 && operationCode != 136 && operationCode != 176)
		{
			OpTimer.End(operationCode);
		}
		if (this.OnOpCompleted != null)
		{
			this.OnOpCompleted(response);
		}
		switch (operationCode)
		{
		case 122:
			if (response.ReturnCode == 0)
			{
				Dictionary<int, WeatherDesc[]> dictionary = TravelSerializationHelper.DeserializeAllPondWeather(response.Parameters);
				if (this.OnGotAllPondWeather != null)
				{
					this.OnGotAllPondWeather(dictionary);
				}
			}
			else
			{
				this.HandleOperationFailure("GetAllPondsWeather", response);
			}
			break;
		case 124:
			this.HandleOutdateRentResponse(response);
			break;
		case 125:
			this.HandleGetSounderFishResponse(response);
			break;
		case 129:
			this.HandleGiveXboxProductsResponse(response);
			break;
		case 130:
			this.HandleMissionOperationResponse(response);
			break;
		case 131:
			this.HandleDebugBiteSystemEventsResponse(response);
			break;
		case 132:
			this.HandleBuyProductResponse(response);
			break;
		case 133:
			this.HandleRentBoatResponse(response);
			break;
		case 134:
			this.HandleDeleteAbsentSubscriptionsResponse(response);
			break;
		case 135:
			this.HandleGetReferralRewardsResponse(response);
			break;
		case 136:
			this.HandlePremiumShopOperationResponse(response);
			break;
		case 137:
			this.HandleRemoveTimeForwardCooldownResponse(response);
			break;
		case 138:
			this.HandleUnlockPondResponse(response);
			break;
		case 139:
			this.HandleGetPondUnlocksResponse(response);
			break;
		case 140:
			this.HandleClientCacheResponse(response);
			break;
		case 141:
			this.HandleGivePsProductsResponse(response);
			break;
		case 142:
			this.HandleGetFishCategoriesResponse(response);
			break;
		case 143:
			this.HandleGetFishResponse(response);
			break;
		case 144:
			this.HandleExchangeCurrencyResponse(response);
			break;
		case 147:
			this.HandleServerTimeResponse(response);
			break;
		case 148:
			this.HandleGetRoomPopulationResponse(response);
			break;
		case 149:
			this.HandleInteractWithObjectResponse(response);
			break;
		case 150:
			this.HandleGetInteractiveObjectsResponse(response);
			break;
		case 151:
			this.HandleGetRoomsPopulationResponse(response);
			break;
		case 152:
			this.HandleGetPondStatsResponse(response);
			break;
		case 153:
			this.HandleCheckForProductSales(response);
			break;
		case 156:
			this.HandleSendChatCommandResponse(response);
			break;
		case 157:
			this.HandleFinalizeSteamTransactionResponse(response);
			break;
		case 158:
			this.HandleStartSteamTransactionResponse(response);
			break;
		case 159:
			this.HandleFinishTutorialResponse(response);
			break;
		case 160:
			this.HandleGetMinTravelCost(response);
			break;
		case 161:
			this.HandleGetBrandsResponse(response);
			break;
		case 162:
		case 163:
			this.HandleTravelResponse(response);
			break;
		case 164:
			this.HandleGetProductsResponse(response);
			break;
		case 165:
			this.HandleGetProductResponse(response);
			break;
		case 166:
			this.DoOnPreviewDisconnectComplete();
			break;
		case 167:
			this.HandleSysOperationResponse(response);
			break;
		case 168:
			this.HandleTournamentOperationResponse(response);
			break;
		case 169:
			this.HandleGetTopsResponse(response);
			break;
		case 170:
			this.HandleGetProtocolVersion(response);
			break;
		case 171:
			this.HandleGetChatPlayersCount(response);
			break;
		case 172:
			if (response.ReturnCode != 0)
			{
				Debug.LogError(response.DebugMessage);
			}
			break;
		case 173:
			this.HandleGetItemsResponse(response);
			break;
		case 174:
			if (response.ReturnCode == 0)
			{
				WeatherDesc[] array = TravelSerializationHelper.DeserializePondWeather(response.Parameters);
				if (this.OnGotPondWeatherForecast != null)
				{
					this.OnGotPondWeatherForecast(array);
				}
			}
			else
			{
				this.HandleOperationFailure("GetWeather", response);
			}
			break;
		case 175:
			this.HandleGetItemCountForCategoriesResponse(response);
			break;
		case 176:
			this.UGCHandleOperationResponse(response);
			break;
		case 177:
			this.HandleFindPlayersResponse(response);
			break;
		case 178:
			if (response.ReturnCode == 0)
			{
				WeatherDesc[] array2 = TravelSerializationHelper.DeserializePondWeather(response.Parameters);
				if (this.OnGotPondWeather != null)
				{
					this.OnGotPondWeather(array2);
				}
			}
			else
			{
				this.HandleOperationFailure("GetWeather", response);
			}
			break;
		case 179:
			this.HandleDestroyLicenseResponse(response);
			break;
		case 180:
			this.HandleBuyLicenseResponse(response);
			break;
		case 181:
			this.HandleGetLicenses(response);
			break;
		case 182:
			this.HandleGetStates(response);
			break;
		case 183:
			this.HandleMoveTimeResponse(response);
			break;
		case 184:
			this.HandleGetTimeResponse(response);
			break;
		case 185:
			this.HandleRepairResponse(response);
			break;
		case 186:
			this.HandleDestroyItemResponse(response);
			break;
		case 187:
			this.HandleBuyItemResponse(response);
			break;
		case 188:
			this.HandleGetLocalItemsResponse(response);
			break;
		case 189:
			this.HandleGetGlobalItemsResponse(response);
			break;
		case 190:
			this.HandleGetItemCategoriesResponse(response);
			break;
		case 193:
			this.HandleGameAction(response);
			break;
		case 194:
			this.HandleInventoryOperationResponse(response);
			break;
		case 195:
			if (response.ReturnCode == 0)
			{
				Location location = TravelSerializationHelper.DeserializeLocation(response.Parameters);
				if (this.OnGotLocationInfo != null)
				{
					this.OnGotLocationInfo(location);
				}
			}
			else
			{
				this.HandleOperationFailure("GetLocationInfo", response);
			}
			break;
		case 196:
			if (response.ReturnCode == 0)
			{
				Pond pond = TravelSerializationHelper.DeserializePond(response.Parameters);
				this.pondInfos[pond.PondId] = pond;
				if (this.OnGotPondInfo != null)
				{
					this.OnGotPondInfo(pond);
				}
			}
			else
			{
				this.HandleOperationFailure("GetPondInfo", response);
			}
			break;
		case 197:
			if (response.ReturnCode == 0)
			{
				List<LocationBrief> list = TravelSerializationHelper.DeserializeLocationBriefs(response.Parameters);
				if (this.OnGotAvailableLocations != null)
				{
					this.OnGotAvailableLocations(list);
				}
			}
			else
			{
				this.HandleOperationFailure("GetAvailablePonds", response);
			}
			break;
		case 198:
			if (response.ReturnCode == 0)
			{
				List<PondBrief> list2 = TravelSerializationHelper.DeserializePondBriefs(response.Parameters);
				if (this.OnGotAvailablePonds != null)
				{
					this.OnGotAvailablePonds(list2);
				}
			}
			else
			{
				this.HandleOperationFailure("GetAvailablePonds", response);
			}
			break;
		case 199:
			if (response.ReturnCode == 0)
			{
				this.securityToken = (string)response.Parameters[221];
			}
			else if (this.OnRefreshSecurityTokenFailed != null)
			{
				this.OnRefreshSecurityTokenFailed(new Failure(response));
			}
			break;
		case 200:
			this.HandleProfileOperationResponse(response);
			break;
		case 224:
			this.HandleGetTimeForwardCooldownResponse(response);
			break;
		case 225:
			this.HandleJoinRandomGameResponse(response);
			break;
		case 226:
			this.HandleJoinGameResponse(response);
			break;
		case 227:
			this.HandleCreateGameResponse(response);
			break;
		case 229:
			this.HandleJoinLobbyResponse(response);
			break;
		case 230:
			this.HandleAuthenticateResponse(response);
			break;
		}
		if (response.ReturnCode == 0 && response.Parameters.ContainsKey(149))
		{
			byte[] array3 = (byte[])response[149];
			string text = CompressHelper.DecompressString(array3);
			MissionsResponse missionsResponse = JsonConvert.DeserializeObject<MissionsResponse>(text, SerializationHelper.JsonSerializerSettings);
			this.HandleMissionEvent(missionsResponse);
		}
	}

	private void HandleOperationFailure(string opName, OperationResponse response)
	{
		Debug.LogErrorFormat("{0} failed. Code: {1} Message: {2}", new object[]
		{
			opName,
			Enum.GetName(typeof(ErrorCode), response.ReturnCode),
			response.DebugMessage
		});
		if (this.OnOperationFailed != null)
		{
			this.OnOperationFailed(new Failure(response));
		}
	}

	private void InternalAuthenticate()
	{
		if (!string.IsNullOrEmpty(this.securityToken))
		{
			this.ReAuthenticate();
		}
		else if (!string.IsNullOrEmpty(this.SteamAuthTicket))
		{
			this.AuthenticateWithSteam();
		}
		else if (!string.IsNullOrEmpty(this.Email) && !string.IsNullOrEmpty(this.Password))
		{
			this.Authenticate();
		}
	}

	private void HandleBuyItemResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			string text = (string)response[192];
			InventoryItem inventoryItem = (InventoryItem)JsonConvert.DeserializeObject(text, SerializationHelper.JsonSerializerSettings);
			int num = (int)response[183];
			bool flag = (bool)response[182];
			if (this.profile != null)
			{
				this.profile.Inventory.JoinItem(inventoryItem, (!flag) ? null : new StoragePlaces?(StoragePlaces.Storage));
				if (inventoryItem.PriceGold != null && inventoryItem.PriceGold != 0.0)
				{
					this.profile.IncrementBalance("GC", -inventoryItem.PriceGold.Value * (double)num);
				}
				else if (inventoryItem.PriceSilver != null && inventoryItem.PriceSilver != 0.0)
				{
					this.profile.IncrementBalance("SC", -inventoryItem.PriceSilver.Value * (double)num);
				}
				this.UpdateFishCage(inventoryItem, true);
			}
			if (this.OnItemBought != null)
			{
				this.OnItemBought(inventoryItem);
			}
			this.RaiseInventoryUpdated(false);
		}
		else
		{
			this.HandleTransactionFailure("BuyItem", response);
		}
	}

	private void HandleDestroyItemResponse(OperationResponse response)
	{
		if (!this.LogInventoryResponse(response))
		{
			return;
		}
		if (response.ReturnCode == 0)
		{
			Guid guid = new Guid((string)response.Parameters[194]);
			int? num = null;
			object obj;
			if (response.Parameters.TryGetValue(183, out obj))
			{
				num = (int?)obj;
			}
			InventoryItem inventoryItem = null;
			if (this.profile != null)
			{
				foreach (InventoryItem inventoryItem2 in this.profile.Inventory)
				{
					if (inventoryItem2.InstanceId == guid)
					{
						inventoryItem = inventoryItem2;
						break;
					}
				}
				if (inventoryItem != null)
				{
					if (num == null || inventoryItem.Count <= num)
					{
						this.profile.Inventory.RemoveItem(inventoryItem, false);
					}
					else
					{
						inventoryItem.Count -= num.Value;
					}
				}
			}
			if (this.OnItemDestroyed != null)
			{
				this.OnItemDestroyed(inventoryItem);
			}
			this.RaiseInventoryUpdated(false);
		}
		else
		{
			this.HandleTransactionFailure("DestroyItem", response);
		}
	}

	private void HandleBuyProductResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			Debug.Log("HandleBuyProductResponse - Ok");
			try
			{
				string text = CompressHelper.DecompressString((byte[])response.Parameters[192]);
				ProfileProduct profileProduct = JsonConvert.DeserializeObject<ProfileProduct>(text, SerializationHelper.JsonSerializerSettings);
				int num = (int)response.Parameters[183];
				Debug.Log(string.Concat(new object[] { "Delivering new product - #", profileProduct.ProductId, " (", profileProduct.Name, ")" }));
				for (int i = 0; i < num; i++)
				{
					this.profile.PutProductToProfile(profileProduct);
				}
				if (profileProduct.Items != null && profileProduct.Items.Length > 0)
				{
					this.profile.Inventory.ResolveParents();
					this.RaiseInventoryUpdated(false);
				}
				if (this.OnProductBought != null)
				{
					this.OnProductBought(profileProduct, num);
				}
				if (profileProduct.PondsUnlocked != null && profileProduct.PondsUnlocked.Length > 0 && this.OnPondLevelInvalidated != null)
				{
					this.OnPondLevelInvalidated();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("HandleGiveProductsResponse exception: " + ex.Message + "\r\n" + ex.StackTrace);
			}
		}
		else if (this.OnBuyProductFailed != null)
		{
			this.OnBuyProductFailed(new Failure(response));
		}
	}

	private void HandleBuyLicenseResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			string text = (string)response[192];
			PlayerLicense playerLicense = JsonConvert.DeserializeObject<PlayerLicense>(text, SerializationHelper.JsonSerializerSettings);
			if (this.profile != null)
			{
				this.profile.AddLicense(playerLicense);
				if (playerLicense.Cost != null)
				{
					this.profile.IncrementBalance(playerLicense.Currency, -playerLicense.Cost.Value);
				}
			}
			if (this.OnLicenseBought != null)
			{
				this.OnLicenseBought(playerLicense);
			}
		}
		else
		{
			this.HandleTransactionFailure("BuyLicense", response);
		}
	}

	private void HandleDestroyLicenseResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			Guid guid = new Guid((string)response.Parameters[194]);
			PlayerLicense playerLicense = null;
			if (this.profile != null)
			{
				foreach (PlayerLicense playerLicense2 in this.profile.Licenses)
				{
					if (playerLicense2.InstanceId == guid)
					{
						playerLicense = playerLicense2;
						break;
					}
				}
				if (playerLicense != null)
				{
					this.profile.Licenses.Remove(playerLicense);
				}
			}
			if (this.OnLicenseDestroyed != null)
			{
				this.OnLicenseDestroyed(playerLicense);
			}
		}
		else
		{
			this.HandleTransactionFailure("DestroyLicense", response);
		}
	}

	private void HandleGetBrandsResponse(OperationResponse response)
	{
		InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
		inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
		inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
		{
			string text = CompressHelper.DecompressString(p);
			return JsonConvert.DeserializeObject<List<Brand>>(text, SerializationHelper.JsonSerializerSettings);
		};
		inputByteArrayWorkItem.ResultAction = delegate(object r)
		{
			List<Brand> list = (List<Brand>)r;
			if (this.OnGotBrands != null)
			{
				this.OnGotBrands(list);
			}
		};
		AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
	}

	private void HandleTransactionFailure(string opName, OperationResponse response)
	{
		Debug.LogErrorFormat("{0} failed. Code: {1} Message: {2}", new object[]
		{
			opName,
			Enum.GetName(typeof(ErrorCode), response.ReturnCode),
			response.DebugMessage
		});
		if (this.OnTransactionFailed != null)
		{
			this.OnTransactionFailed(new TransactionFailure(response));
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotItemCategories OnGotItemCategories;

	private void HandleGetItemCategoriesResponse(OperationResponse response)
	{
		AsynchProcessor.ProcessByteWorkItem(new InputByteWorkItem
		{
			Data = 192,
			ProcessAction = delegate(byte p)
			{
				string text = (string)response.Parameters[p];
				return JsonConvert.DeserializeObject<List<InventoryCategory>>(text, SerializationHelper.JsonSerializerSettings);
			},
			ResultAction = delegate(object r)
			{
				List<InventoryCategory> list = (List<InventoryCategory>)r;
				if (this.OnGotItemCategories != null)
				{
					this.OnGotItemCategories(list);
				}
			}
		});
	}

	public void SetBuoy(string name, Point3 position, CaughtFish fish)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to game server or not authenticated");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 9;
		operationRequest.Parameters[244] = ((name.Length <= 64) ? name : name.Substring(0, 64));
		operationRequest.Parameters[245] = position;
		if (fish != null)
		{
			operationRequest.Parameters[192] = JsonConvert.SerializeObject(fish, 0, SerializationHelper.JsonSerializerSettings);
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 9);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BuoySetting> OnBuoySet;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnBuoySettingFailed;

	public void TakeBuoy(int buoyId)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to game server or not authenticated");
			return;
		}
		if (this.profile.Buoys == null)
		{
			Debug.LogError("There are no buoys to take");
			return;
		}
		if (this.profile.Buoys.FirstOrDefault((BuoySetting b) => b.BuoyId == buoyId) == null)
		{
			Debug.LogError("Buoy to take #" + buoyId + " does not exists");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 10;
		operationRequest.Parameters[194] = buoyId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 10);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> OnBuoyTaken;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnBuoyTakingFailed;

	public void RenameBuoy(int buoyId, string name)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to game server or not authenticated");
			return;
		}
		if (this.profile.Buoys == null)
		{
			Debug.LogError("There are no buoys to rename");
			return;
		}
		if (this.profile.Buoys.FirstOrDefault((BuoySetting b) => b.BuoyId == buoyId) == null)
		{
			Debug.LogError("Buoy to rename #" + buoyId + " does not exists");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 11;
		operationRequest.Parameters[194] = buoyId;
		operationRequest.Parameters[244] = ((name.Length <= 64) ? name : name.Substring(0, 64));
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 10);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnBuoyRenamed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnBuoyRenamingFailed;

	public void ShareBuoy(int buoyId, string receiver)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to game server or not authenticated");
			return;
		}
		if (this.profile.Buoys == null)
		{
			Debug.LogError("There are no buoys to share");
			return;
		}
		BuoySetting buoySetting = this.profile.Buoys.FirstOrDefault((BuoySetting b) => b.BuoyId == buoyId);
		if (buoySetting == null)
		{
			Debug.LogError("Buoy to share #" + buoyId + " does not exists");
			return;
		}
		if (buoySetting.Position == null)
		{
			Debug.LogError("Buoy #" + buoySetting.BuoyId + " is not set");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 12;
		operationRequest.Parameters[194] = buoyId;
		operationRequest.Parameters[9] = receiver;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 12);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnBuoyShared;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnBuoySharingFailed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnReceiveBuoyShareRequest OnReceiveBuoyShareRequest;

	public void AcceptSharedBuoy(int buoyRequestId)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to game server or not authenticated");
			return;
		}
		if (this.profile.BuoyShareRequests == null)
		{
			Debug.LogError("There are no buoys shared requests");
			return;
		}
		if (this.profile.BuoyShareRequests.FirstOrDefault((BuoySetting b) => b.BuoyId == buoyRequestId) == null)
		{
			Debug.LogError("Buoy share request to accept #" + buoyRequestId + " does not exists");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 13;
		operationRequest.Parameters[194] = buoyRequestId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 13);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnBuoyShareAccepted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnBuoyShareAcceptingFailed;

	public void DeclineSharedBuoy(int buoyRequestId)
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to game server or not authenticated");
			return;
		}
		if (this.profile.BuoyShareRequests == null)
		{
			Debug.LogError("There are no buoys shared requests");
			return;
		}
		if (this.profile.BuoyShareRequests.FirstOrDefault((BuoySetting b) => b.BuoyId == buoyRequestId) == null)
		{
			Debug.LogError("Buoy share request to decline #" + buoyRequestId + " does not exists");
			return;
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 14;
		operationRequest.Parameters[194] = buoyRequestId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 14);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnBuoyShareDeclined;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnBuoyShareDecliningFailed;

	private void HandleBuoyOperationResponse(OperationResponse response)
	{
		if (this.profile.Buoys == null)
		{
			this.profile.Buoys = new List<BuoySetting>();
		}
		byte b2 = (byte)response[191];
		if (response.ReturnCode == 0)
		{
			try
			{
				if (b2 == 9)
				{
					string text = (string)response[192];
					BuoySetting buoySetting = JsonConvert.DeserializeObject<BuoySetting>(text, SerializationHelper.JsonSerializerSettings);
					this.profile.Buoys.Add(buoySetting);
					if (this.OnBuoySet != null)
					{
						this.OnBuoySet(buoySetting);
					}
				}
				else if (b2 == 10)
				{
					int buoyId2 = (int)response[194];
					BuoySetting buoySetting2 = this.profile.Buoys.FirstOrDefault((BuoySetting b) => b.BuoyId == buoyId2);
					if (buoySetting2 != null)
					{
						this.profile.Buoys.Remove(buoySetting2);
					}
					if (this.OnBuoyTaken != null)
					{
						this.OnBuoyTaken(buoyId2);
					}
				}
				else if (b2 == 11)
				{
					int buoyId = (int)response[194];
					string text2 = (string)response[244];
					BuoySetting buoySetting3 = this.profile.Buoys.FirstOrDefault((BuoySetting b) => b.BuoyId == buoyId);
					if (buoySetting3 != null)
					{
						buoySetting3.Name = text2;
					}
					if (this.OnBuoyRenamed != null)
					{
						this.OnBuoyRenamed();
					}
				}
				else if (b2 == 12)
				{
					if (this.OnBuoyShared != null)
					{
						this.OnBuoyShared();
					}
				}
				else if (b2 == 13)
				{
					int num = (int)response[194];
					string text3 = (string)response[192];
					BuoySetting buoySetting4 = JsonConvert.DeserializeObject<BuoySetting>(text3, SerializationHelper.JsonSerializerSettings);
					BuoySetting buoySetting5 = this.profile.FindBuoyShareRequest(num);
					this.profile.RemoveBuoyShareRequest(buoySetting5);
					this.profile.AddBuoy(buoySetting4, false);
					if (this.OnBuoyShareAccepted != null)
					{
						this.OnBuoyShareAccepted();
					}
				}
				else if (b2 == 14)
				{
					int num2 = (int)response[194];
					BuoySetting buoySetting6 = this.profile.FindBuoyShareRequest(num2);
					this.profile.RemoveBuoyShareRequest(buoySetting6);
					if (this.OnBuoyShareDeclined != null)
					{
						this.OnBuoyShareDeclined();
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message);
				this.PinError(string.Format("CLIENT !INVENTORY-UNSYNC! on call {2} [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber, b2) + ex.Message, this.lastInventoryStackTrace);
				throw;
			}
		}
		else
		{
			Failure failure = new Failure(response);
			Debug.LogError(failure.FullErrorInfo);
			this.PinError(string.Format("SERVER !INVENTORY-UNSYNC! on CLIENT call [[rq: {0}, rs: {1}]]", this.inventoryRequestNumber, this.inventoryResponseNumber) + failure.ErrorMessage, this.lastInventoryStackTrace);
			if (b2 == 9)
			{
				if (this.OnBuoySettingFailed != null)
				{
					this.OnBuoySettingFailed(failure);
				}
			}
			else if (b2 == 10)
			{
				if (this.OnBuoyTakingFailed != null)
				{
					this.OnBuoyTakingFailed(failure);
				}
			}
			else if (b2 == 11)
			{
				if (this.OnBuoyRenamingFailed != null)
				{
					this.OnBuoyRenamingFailed(failure);
				}
			}
			else if (b2 == 12)
			{
				if (this.OnBuoySharingFailed != null)
				{
					this.OnBuoySharingFailed(failure);
				}
			}
			else if (b2 == 13)
			{
				if (this.OnBuoyShareAcceptingFailed != null)
				{
					this.OnBuoyShareAcceptingFailed(failure);
				}
			}
			else if (b2 == 14 && this.OnBuoyShareDecliningFailed != null)
			{
				this.OnBuoyShareDecliningFailed(failure);
			}
			this.RequestProfileOnError(null, false);
		}
	}

	public void SetShimsCount(Waggler waggler, int shimsCount)
	{
		if (waggler == null)
		{
			throw new ArgumentNullException("waggler");
		}
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(194);
		operationRequest.Parameters[191] = 36;
		operationRequest.Parameters[1] = waggler.InstanceId.ToString();
		operationRequest.Parameters[4] = shimsCount;
		waggler.ShimsCount = shimsCount;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(194, 36);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnShimsCountSet;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnSetShimsCountFailed;

	private void HandleSetShimsCountResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnShimsCountSet != null)
			{
				this.OnShimsCountSet();
			}
			return;
		}
		if (this.OnSetShimsCountFailed != null)
		{
			this.OnSetShimsCountFailed(new Failure(response));
		}
	}

	private void HandleGetItemCountForCategoriesResponse(OperationResponse response)
	{
		string text = (string)response.Parameters[192];
		List<CategoryItemsCount> list = JsonConvert.DeserializeObject<List<CategoryItemsCount>>(text, SerializationHelper.JsonSerializerSettings);
		if (this.OnGotItemsCountForCategories != null)
		{
			this.OnGotItemsCountForCategories(list);
		}
	}

	private void HandleTravelResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			this.HandleOperationFailure("Travel", response);
			return;
		}
		string text = (string)response.Parameters[192];
		ChangeResidenceInfo changeResidenceInfo = JsonConvert.DeserializeObject<ChangeResidenceInfo>(text, SerializationHelper.JsonSerializerSettings);
		byte operationCode = response.OperationCode;
		if (operationCode != 163)
		{
			if (operationCode == 162)
			{
				if (this.profile != null)
				{
					if (changeResidenceInfo.Amount > 0)
					{
						this.profile.IncrementBalance(changeResidenceInfo.Currency, (double)(-(double)changeResidenceInfo.Amount));
					}
					this.profile.HomeState = changeResidenceInfo.NewStateId;
					this.profile.HomeStateName = changeResidenceInfo.NewStateName;
				}
				if (this.OnResidenceChanged != null)
				{
					this.OnResidenceChanged(changeResidenceInfo);
				}
			}
		}
		else if (this.OnPreviewResidenceChange != null)
		{
			this.OnPreviewResidenceChange(changeResidenceInfo);
		}
	}

	private void HandleGetMinTravelCost(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			this.HandleOperationFailure("GetMinTravelCost", response);
			return;
		}
		int num = (int)response.Parameters[188];
		if (this.OnGotMinTravelCost != null)
		{
			this.OnGotMinTravelCost(num);
		}
	}

	private void HandleFinishTutorialResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			this.HandleOperationFailure("FinishTutorial", response);
			return;
		}
		if (this.profile != null)
		{
			this.profile.IsTutorialFinished = true;
		}
		if (this.OnTutorialFinished != null)
		{
			this.OnTutorialFinished();
		}
	}

	private void HandleGetGlobalItemsResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			if (this.OnGettingItemsForCategoryFailed != null)
			{
				this.OnGettingItemsForCategoryFailed(new Failure(response));
			}
			return;
		}
		InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
		inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
		inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
		{
			string text = CompressHelper.DecompressString(p);
			return JsonConvert.DeserializeObject<List<InventoryItem>>(text, SerializationHelper.JsonSerializerSettings);
		};
		inputByteArrayWorkItem.ResultAction = delegate(object r)
		{
			List<InventoryItem> list = (List<InventoryItem>)r;
			this.SetItemsDurability(list);
			if (this.OnGotItemsForCategory != null)
			{
				this.OnGotItemsForCategory(list);
			}
		};
		AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
	}

	private void HandleGetLocalItemsResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			if (this.OnGettingItemsForCategoryFailed != null)
			{
				this.OnGettingItemsForCategoryFailed(new Failure(response));
			}
			return;
		}
		InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
		inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
		inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
		{
			string text = CompressHelper.DecompressString(p);
			return JsonConvert.DeserializeObject<List<InventoryItem>>(text, SerializationHelper.JsonSerializerSettings);
		};
		inputByteArrayWorkItem.ResultAction = delegate(object r)
		{
			List<InventoryItem> list = (List<InventoryItem>)r;
			this.SetItemsDurability(list);
			if (this.OnGotItemsForCategory != null)
			{
				this.OnGotItemsForCategory(list);
			}
		};
		AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
	}

	private void HandleGetPondStatsResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnGotPondStats != null)
			{
				byte[] array = (byte[])response.Parameters[192];
				string text = CompressHelper.DecompressString(array);
				List<PondStat> list = JsonConvert.DeserializeObject<List<PondStat>>(text, SerializationHelper.JsonSerializerSettings);
				this.OnGotPondStats(list);
			}
		}
		else if (this.OnGettingPondStatsFailed != null)
		{
			this.OnGettingPondStatsFailed(new Failure(response));
		}
	}

	private void HandleGetFishResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnGotFishList != null)
			{
				this.DeserializeListAsync<global::ObjectModel.Fish>(response, delegate(IEnumerable<global::ObjectModel.Fish> result)
				{
					if (this.OnGotFishList != null)
					{
						this.OnGotFishList(result);
					}
				}, false);
			}
		}
		else if (this.OnErrorGettingFishList != null)
		{
			this.OnErrorGettingFishList(new Failure(response));
		}
	}

	private void HandleGetFishCategoriesResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnGotFishCategories != null)
			{
				byte[] array = (byte[])response.Parameters[192];
				string text = CompressHelper.DecompressString(array);
				List<FishCategory> list = JsonConvert.DeserializeObject<List<FishCategory>>(text, SerializationHelper.JsonSerializerSettings);
				this.OnGotFishCategories(list);
			}
		}
		else if (this.OnErrorGettingFishCategories != null)
		{
			this.OnErrorGettingFishCategories(new Failure(response));
		}
	}

	private List<RoomPopulation> friendsRooms { get; set; }

	private void HandleGetRoomsPopulationResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (response.Parameters.ContainsKey(192))
			{
				InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
				inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
				inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
				{
					string text = CompressHelper.DecompressString(p);
					return JsonConvert.DeserializeObject<List<RoomPopulation>>(text, SerializationHelper.JsonSerializerSettings);
				};
				inputByteArrayWorkItem.ResultAction = delegate(object r)
				{
					this.friendsRooms = (List<RoomPopulation>)r;
					if (this.friendsRooms != null)
					{
						for (int i = 0; i < this.friendsRooms.Count; i++)
						{
							RoomPopulation roomPopulation = this.friendsRooms[i];
							for (int j = 0; j < roomPopulation.FriendList.Length; j++)
							{
								string text2 = roomPopulation.FriendList[j];
								Player player = null;
								for (int k = 0; k < this.Profile.Friends.Count; k++)
								{
									Player player2 = this.Profile.Friends[k];
									if (player2.UserId.Equals(text2, StringComparison.InvariantCultureIgnoreCase))
									{
										player = player2;
										break;
									}
								}
								if (player != null)
								{
									player.RoomId = roomPopulation.RoomId;
								}
							}
						}
					}
					if (this.OnGotRoomsPopulation != null)
					{
						this.OnGotRoomsPopulation(this.friendsRooms);
					}
				};
				AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
			}
		}
		else if (this.OnGettingRoomsPopulationFailed != null)
		{
			this.OnGettingRoomsPopulationFailed(new Failure(response));
		}
	}

	private void HandleGetRoomPopulationResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (response.Parameters.ContainsKey(192))
			{
				InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
				inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
				inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
				{
					string text = CompressHelper.DecompressString(p);
					return JsonConvert.DeserializeObject<List<Player>>(text, SerializationHelper.JsonSerializerSettings);
				};
				inputByteArrayWorkItem.ResultAction = delegate(object r)
				{
					List<Player> list = (List<Player>)r;
					if (this.OnGotCurrentRoomPopulation != null)
					{
						this.OnGotCurrentRoomPopulation(list);
					}
				};
				AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
			}
		}
		else if (this.OnGettingCurrenRoomPopulationFailed != null)
		{
			this.OnGettingCurrenRoomPopulationFailed(new Failure(response));
		}
	}

	private void HandleGetInteractiveObjectsResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnGotInteractiveObjects != null)
			{
				byte[] array = (byte[])response.Parameters[192];
				string text = CompressHelper.DecompressString(array);
				List<InteractiveObject> list = JsonConvert.DeserializeObject<List<InteractiveObject>>(text, SerializationHelper.JsonSerializerSettings);
				this.OnGotInteractiveObjects(list);
			}
		}
		else if (this.OnGettingInteractiveObjectsFailed != null)
		{
			this.OnGettingInteractiveObjectsFailed(new Failure(response));
		}
	}

	private void HandleInteractWithObjectResponse(OperationResponse response)
	{
		InteractiveObject interactiveObject = this.lastInteractedObject;
		this.lastInteractedObject = null;
		if (response.ReturnCode == 0)
		{
			if (interactiveObject != null)
			{
				this.Profile.Stats.UpdateInteractStats(interactiveObject.ObjectId, interactiveObject.Config.Frequency == InteractFreq.Daily);
			}
			InventoryItem[] array = null;
			if (response.Parameters.ContainsKey(192))
			{
				byte[] array2 = (byte[])response.Parameters[192];
				string text = CompressHelper.DecompressString(array2);
				array = JsonConvert.DeserializeObject<Inventory>(text, SerializationHelper.JsonSerializerSettings).ToArray();
				if (this.CheckProfile())
				{
					foreach (InventoryItem inventoryItem in array)
					{
						this.Profile.Inventory.JoinItem(inventoryItem, null);
					}
				}
			}
			int? num = null;
			string text2 = null;
			if (response.Parameters.ContainsKey(187) && response.Parameters.ContainsKey(188))
			{
				num = new int?((int)response.Parameters[188]);
				text2 = (string)response.Parameters[187];
				this.Profile.IncrementBalance(text2, (double)num.Value);
			}
			if (this.OnInteractedWithObject != null)
			{
				this.OnInteractedWithObject(array, num, text2);
			}
		}
		else if (this.OnInteractWithObjectFailed != null)
		{
			this.OnInteractWithObjectFailed(new Failure(response));
		}
	}

	private void SetItemsDurability(IEnumerable<InventoryItem> items)
	{
		if (items != null)
		{
			foreach (InventoryItem inventoryItem in items)
			{
				if (inventoryItem.MaxDurability != null)
				{
					inventoryItem.Durability = inventoryItem.MaxDurability.Value;
				}
			}
		}
	}

	private void HandleGetItemsResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			Failure standardFailure = PhotonServerConnection.GetStandardFailure(response);
			if (this.OnGettingItemsFailed != null)
			{
				this.OnGettingItemsFailed(standardFailure);
			}
			return;
		}
		if (this.OnGotItems != null)
		{
			int subscriberId = 0;
			if (response.Parameters.ContainsKey(244))
			{
				subscriberId = (int)response.Parameters[244];
			}
			InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
			inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
			inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
			{
				string text = CompressHelper.DecompressString(p);
				return JsonConvert.DeserializeObject<List<InventoryItem>>(text, SerializationHelper.JsonSerializerSettings);
			};
			inputByteArrayWorkItem.ResultAction = delegate(object r)
			{
				List<InventoryItem> list = (List<InventoryItem>)r;
				if (this.OnGotItems != null)
				{
					this.OnGotItems(list, subscriberId);
				}
			};
			AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotLicenses OnGotLicenses;

	private void HandleGetLicenses(OperationResponse response)
	{
		AsynchProcessor.ProcessByteWorkItem(new InputByteWorkItem
		{
			Data = 192,
			ProcessAction = delegate(byte p)
			{
				byte[] array = (byte[])response.Parameters[p];
				string text = CompressHelper.DecompressString(array);
				return JsonConvert.DeserializeObject<List<ShopLicense>>(text, SerializationHelper.JsonSerializerSettings);
			},
			ResultAction = delegate(object r)
			{
				List<ShopLicense> list = (List<ShopLicense>)r;
				if (this.OnGotLicenses != null)
				{
					this.OnGotLicenses(list);
				}
			}
		});
	}

	private void HandleGetStates(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			this.DeserializeListAsync<State>(response, delegate(IEnumerable<State> result)
			{
				if (this.OnGotStates != null)
				{
					this.OnGotStates(result);
				}
			}, false);
		}
		else if (this.OnGettingStatesFailed != null)
		{
			this.OnGettingStatesFailed(new Failure(response));
		}
	}

	private void HandleGameAction(OperationResponse response)
	{
		GameActionCode gameActionCode = 0;
		if (response.Parameters.ContainsKey(186))
		{
			gameActionCode = (GameActionCode)response.Parameters[186];
		}
		OpTimer.End(193, gameActionCode, gameActionCode != 3 && gameActionCode != 7 && gameActionCode != 5);
		GameActionResult gameActionResult = new GameActionResult(response)
		{
			ActionCode = gameActionCode
		};
		if (response.ReturnCode == 0)
		{
			object obj;
			if (response.Parameters.TryGetValue(150, out obj))
			{
				gameActionResult.RodSlot = (int)obj;
			}
			object obj2;
			if (response.Parameters.TryGetValue(185, out obj2))
			{
				gameActionResult.ActionData = (Hashtable)obj2;
			}
			if (gameActionCode == 11 || (gameActionResult.ActionData != null && gameActionResult.ActionData.ContainsKey("b") && (bool)gameActionResult.ActionData["b"]))
			{
				gameActionResult.AnyBreaks = true;
			}
			if (gameActionResult.ActionData.ContainsKey(6))
			{
				this.HandleItemsUpdatedEvents(gameActionResult.ActionData, gameActionResult.RodSlot);
			}
			gameActionResult.WearInfo = GameActionAdapter.DecodeAndApplyWearInfo(gameActionResult, this.Profile);
			if (gameActionResult.WearInfo.RodUnequiped != null && this.OnRodUnequiped != null)
			{
				this.OnRodUnequiped(gameActionResult.WearInfo.RodUnequiped);
			}
			BrokenTackleType? brokenItem = gameActionResult.WearInfo.BrokenItem;
			if (brokenItem != null && this.OnItemBroken != null)
			{
				this.OnItemBroken(gameActionResult.WearInfo.BrokenItem.Value, gameActionResult.RodSlot);
			}
			if (gameActionCode != 255)
			{
				if (gameActionCode == 254)
				{
					this.IsGameResumed = false;
				}
			}
			else
			{
				this.IsGameResumed = true;
			}
			GameActionAdapter.DecodeFishForce(gameActionResult);
		}
		if (this.OnGameActionResult != null)
		{
			this.OnGameActionResult(gameActionResult);
		}
	}

	private void HandleGameEvent(EventData eventData)
	{
		GameEvent gameEvent = new GameEvent
		{
			EventCode = eventData.Code
		};
		object obj;
		if (eventData.Parameters.TryGetValue(150, out obj))
		{
			gameEvent.RodSlot = (int)obj;
		}
		object obj2;
		if (eventData.Parameters.TryGetValue(185, out obj2))
		{
			gameEvent.RawEventData = (Hashtable)obj2;
		}
		object obj3;
		if (gameEvent.RawEventData != null && gameEvent.RawEventData.TryGetValue("fish", out obj3) && obj3 != null)
		{
			gameEvent.Fish = JsonConvert.DeserializeObject<global::ObjectModel.Fish>((string)obj3, SerializationHelper.JsonSerializerSettings);
		}
		object obj4;
		if (gameEvent.RawEventData != null && gameEvent.RawEventData.TryGetValue("item", out obj4) && obj4 != null)
		{
			gameEvent.Item = JsonConvert.DeserializeObject<List<InventoryItem>>((string)obj4, SerializationHelper.JsonSerializerSettings).First<InventoryItem>();
		}
		if (this.OnGameEvent != null)
		{
			this.OnGameEvent(gameEvent);
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotProfile OnRegisterUser;

	public void UnsubscribeProfileEvents()
	{
		if (this.profile == null)
		{
			return;
		}
		if (this.profile.MissionsContext != null)
		{
			GameCacheAdapter gameCacheAdapter = this.profile.MissionsContext.GetGameCacheAdapter();
			gameCacheAdapter.UnsubscribeEvents();
		}
		if (this.profile.Inventory != null)
		{
			this.profile.Inventory.OnInventoryChange -= this.ProfileInventory_OnInventoryChange;
		}
	}

	public void SubscribeProfileEvents()
	{
		if (this.profile == null)
		{
			return;
		}
		if (this.profile.Inventory != null)
		{
			this.profile.Inventory.OnInventoryChange += this.ProfileInventory_OnInventoryChange;
		}
	}

	private void ProfileInventory_OnInventoryChange(InventoryChange change)
	{
		if (change.Destroyed && change.Broken && this.OnWearedItemLostOnDeequip != null)
		{
			this.OnWearedItemLostOnDeequip(change.Item);
		}
	}

	private void InitMissionsManager()
	{
		this.profile.InitMissionsContext();
		GameCacheAdapter cacheAdapter = this.profile.MissionsContext.GetGameCacheAdapter();
		this.profile.MissionsContext.SetCheckMonitoredDependencies(delegate(string dependency, bool affectProcessing)
		{
			if (dependency == "PondId")
			{
				cacheAdapter.OnPondChanged();
			}
			return ClientMissionsManager.Instance.CheckMonitoredDependencies(dependency, affectProcessing);
		});
		cacheAdapter.SubscribeEvents();
		cacheAdapter.GetPond = (int pondId, int languageId) => CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.PondId == pondId);
		cacheAdapter.GetInventoryCategory = (int categoryId, int languageId) => StaticUserData.AllCategories.FirstOrDefault((InventoryCategory c) => c.CategoryId == categoryId);
		cacheAdapter.GetInventory = (int itemId, int languageId) => CacheLibrary.ItemsCache.GetItemFromCache(itemId);
		cacheAdapter.GetInventoryItemsInGlobalShop = () => CacheLibrary.GlobalShopCacheInstance.GetAllItemsImmediate().ToArray();
		cacheAdapter.GetInventoryItemsInLocalShop = (int pondId) => CacheLibrary.LocalCacheInstance.GetAllItemsImmediate().ToArray();
		this.profile.MissionsContext.ForceAllPropertiesToRecalculate();
	}

	private void HandleProfileOperationResponse(OperationResponse response)
	{
		ProfileSubOperationCode profileSubOperationCode = (ProfileSubOperationCode)response.Parameters[1];
		OpTimer.End(200, profileSubOperationCode);
		if (profileSubOperationCode == 243)
		{
			this.HandlePondUpdateResponse(response);
			return;
		}
		if (profileSubOperationCode == 242)
		{
			this.HandleProlongPondStayResponse(response);
			return;
		}
		if (response.ReturnCode == 0)
		{
			switch (profileSubOperationCode)
			{
			case 217:
				if (this.OnSnowballHitFlagged != null)
				{
					this.OnSnowballHitFlagged();
				}
				break;
			case 218:
				if (response.Parameters.ContainsKey(178))
				{
					byte[] array = (byte[])response[178];
					string text = CompressHelper.DecompressString(array);
					List<CaughtFish> list = JsonConvert.DeserializeObject<List<CaughtFish>>(text, SerializationHelper.JsonSerializerSettings);
					this.UpdateFishCage(null, false);
					if (this.profile.FishCage != null)
					{
						this.profile.FishCage.Fish.Clear();
						this.profile.FishCage.Fish.AddRange(list);
					}
				}
				if (this.OnRequestFishCage != null)
				{
					this.OnRequestFishCage();
				}
				break;
			case 219:
			{
				int num = 0;
				if (response.Parameters.ContainsKey(185))
				{
					num = (int)response.Parameters[185];
				}
				string text2 = null;
				if (response.Parameters.ContainsKey(164))
				{
					text2 = (string)response.Parameters[164];
				}
				if (this.OnGotPlayerPlacement != null)
				{
					this.OnGotPlayerPlacement(new int?(num), text2);
				}
				break;
			}
			case 220:
				if (this.OnAvatarUrlSet != null)
				{
					this.OnAvatarUrlSet();
				}
				break;
			case 221:
				if (this.OnResetProfileToDefault != null)
				{
					this.OnResetProfileToDefault();
				}
				break;
			case 222:
				if (response.Parameters.ContainsKey(10))
				{
					string text3 = CompressHelper.DecompressString((byte[])response.Parameters[10]);
					List<Player> list2 = JsonConvert.DeserializeObject<List<Player>>(text3);
					this.profile.Friends = list2;
					if (this.OnFriendsSyncedByExternalIds != null)
					{
						this.OnFriendsSyncedByExternalIds(list2);
					}
				}
				break;
			case 224:
			case 250:
			{
				string text4 = (string)response.Parameters[250];
				if (this.UserId.Equals(text4, StringComparison.InvariantCultureIgnoreCase))
				{
					this.RefreshMainProfileValues(response.Parameters);
					this.receiveProfileInProgress = true;
					InputDictionaryWorkItem inputDictionaryWorkItem = new InputDictionaryWorkItem();
					inputDictionaryWorkItem.Data = response.Parameters;
					inputDictionaryWorkItem.ProcessAction = new Func<Dictionary<byte, object>, object>(ProfileSerializationHelper.Deserialize);
					inputDictionaryWorkItem.ResultAction = delegate(object r)
					{
						this.UnsubscribeProfileEvents();
						Profile profile = this.profile;
						this.profile = (Profile)r;
						this.profile.Init();
						LogHelper.Log("User Connected: {0} {1}", new object[]
						{
							this.profile.Name,
							this.profile.UserId
						});
						foreach (InventoryItem inventoryItem in this.profile.Inventory)
						{
							Chum chum = inventoryItem as Chum;
							if (chum != null)
							{
								chum.UpdateIngredients(false);
							}
						}
						this.SubscribeProfileEvents();
						this.InitMissionsManager();
						this.UpdateFishCage(null, false);
						this.InheritProperiesFromPriorProfile(profile);
						this.inventoryResponseNumber = this.inventoryRequestNumber;
						if (this.OnGotProfile != null)
						{
							this.OnGotProfile(this.profile);
						}
						this.RaiseInventoryUpdated(true);
						UIHelper.Waiting(false, null);
						this.receiveProfileInProgress = false;
					};
					inputDictionaryWorkItem.ErrorAction = delegate
					{
						this.receiveProfileInProgress = false;
					};
					AsynchProcessor.ProcessDictionaryWorkItem(inputDictionaryWorkItem);
				}
				else
				{
					InputDictionaryWorkItem inputDictionaryWorkItem = new InputDictionaryWorkItem();
					inputDictionaryWorkItem.Data = response.Parameters;
					inputDictionaryWorkItem.ProcessAction = new Func<Dictionary<byte, object>, object>(ProfileSerializationHelper.Deserialize);
					inputDictionaryWorkItem.ResultAction = delegate(object r)
					{
						Profile profile2 = (Profile)r;
						profile2.Init();
						if (this.OnGotOtherPlayerProfile != null)
						{
							this.OnGotOtherPlayerProfile(profile2);
						}
					};
					AsynchProcessor.ProcessDictionaryWorkItem(inputDictionaryWorkItem);
				}
				break;
			}
			case 225:
				if (this.profile != null)
				{
					this.profile.IsTutorialFinished = true;
				}
				if (this.OnTutorialSkipped != null)
				{
					this.OnTutorialSkipped();
				}
				break;
			case 226:
				if (this.OnGotOwnRoom != null)
				{
					string text5 = null;
					if (response.Parameters.ContainsKey(164))
					{
						text5 = (string)response.Parameters[164];
					}
					this.OnGotOwnRoom(text5);
				}
				break;
			case 227:
				if (this.OnPromoCodeValid != null)
				{
					this.OnPromoCodeValid();
				}
				break;
			case 228:
				if (this.OnInviteCreated != null)
				{
					this.OnInviteCreated();
				}
				break;
			case 229:
				if (this.OnInviteIsUnique != null)
				{
					this.OnInviteIsUnique();
				}
				break;
			case 230:
				if (response.Parameters.ContainsKey(167))
				{
					string text6 = (string)response.Parameters[167];
					if (this.CheckProfile())
					{
						this.Profile.OwnPromoCode = text6;
					}
				}
				if (this.OnPromoCodeGenerated != null)
				{
					this.OnPromoCodeGenerated();
				}
				break;
			case 232:
				if (response.Parameters.ContainsKey(249) && this.CheckProfile())
				{
					this.Profile.Name = (string)response.Parameters[249];
					this.UserName = this.Profile.Name;
				}
				if (this.OnNameChanged != null)
				{
					this.OnNameChanged();
				}
				break;
			case 233:
				if (this.OnPreviewQuitComplete != null)
				{
					this.OnPreviewQuitComplete();
				}
				break;
			case 234:
				if (response.Parameters.ContainsKey(10))
				{
					string text7 = CompressHelper.DecompressString((byte[])response.Parameters[10]);
					List<Player> list3 = JsonConvert.DeserializeObject<List<Player>>(text7);
					if (this.profile.Friends == null)
					{
						this.profile.Friends = new List<Player>();
					}
					this.profile.Friends.AddRange(list3);
					if (this.OnFriendsAddedByExternalIds != null)
					{
						this.OnFriendsAddedByExternalIds(list3);
					}
				}
				break;
			case 235:
				if (response.Parameters.ContainsKey(10))
				{
					string text8 = CompressHelper.DecompressString((byte[])response.Parameters[10]);
					List<Player> list4 = JsonConvert.DeserializeObject<List<Player>>(text8);
					for (int i = 0; i < list4.Count; i++)
					{
						Player player = list4[i];
						Player player2 = null;
						bool flag = false;
						for (int j = 0; j < this.profile.Friends.Count; j++)
						{
							player2 = this.profile.Friends[j];
							if (player2.UserId.Equals(player.UserId, StringComparison.InvariantCultureIgnoreCase))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							player2.UserName = player.UserName;
							player2.AvatarBID = player.AvatarBID;
							player2.AvatarUrl = player.AvatarUrl;
							player2.IsOnline = player.IsOnline;
							player2.Level = player.Level;
							player2.Rank = player.Rank;
							player2.PondId = player.PondId;
							player2.HasPremium = player.HasPremium;
							player2.RoomId = player.RoomId;
						}
					}
				}
				if (this.OnFriendsDetailsRefreshed != null)
				{
					this.OnFriendsDetailsRefreshed();
				}
				break;
			case 236:
			{
				Guid guid = new Guid((string)response.Parameters[250]);
				if (response.Parameters.ContainsKey(241))
				{
					Guid renewedFish = new Guid((string)response.Parameters[241]);
					CaughtFish caughtFish = this.Profile.FishCage.Fish.FirstOrDefault((CaughtFish f) => f.Fish.InstanceId == renewedFish);
					if (caughtFish != null)
					{
						if (response.Parameters.ContainsKey(188))
						{
							caughtFish.Fish.TournamentScore = new float?((float)response.Parameters[188]);
						}
						if (response.Parameters.ContainsKey(189))
						{
							caughtFish.Fish.TournamentSecondaryScore = new float?((float)response.Parameters[189]);
						}
					}
				}
				if (this.OnReleaseFishFromFishCage != null)
				{
					this.OnReleaseFishFromFishCage(guid);
				}
				break;
			}
			case 237:
			{
				PeriodStats periodStats = this.ParseMissionResult(response.Parameters);
				if (this.OnEndOfMissionResult != null)
				{
					this.OnEndOfMissionResult(periodStats);
				}
				break;
			}
			case 238:
				if (this.OnPasswordRecovered != null)
				{
					this.OnPasswordRecovered();
				}
				break;
			case 239:
				if (this.OnPasswordRecoverRequested != null)
				{
					this.OnPasswordRecoverRequested();
				}
				break;
			case 240:
				if (this.OnCheckUsernameIsUnique != null)
				{
					this.OnCheckUsernameIsUnique(true);
				}
				break;
			case 241:
			{
				InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
				inputByteArrayWorkItem.Data = (byte[])response.Parameters[180];
				inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
				{
					string text9 = CompressHelper.DecompressString(p);
					return JsonConvert.DeserializeObject<PlayerStats>(text9);
				};
				inputByteArrayWorkItem.ResultAction = delegate(object r)
				{
					PlayerStats playerStats = (PlayerStats)r;
					if (this.profile != null)
					{
						this.profile.Stats = playerStats;
					}
					if (this.OnGotStats != null)
					{
						this.OnGotStats(playerStats);
					}
				};
				AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
				break;
			}
			case 244:
				if (this.OnCheckEmailIsUnique != null)
				{
					this.OnCheckEmailIsUnique(true);
				}
				break;
			case 245:
				this.Email = this.cachedEmail;
				this.Password = this.cachedPassword;
				this.cachedPassword = null;
				if (this.OnMakeTempAccountPersistent != null)
				{
					this.OnMakeTempAccountPersistent();
				}
				this.DisconnectToReconnect(this.MasterServerAddress, null, null);
				break;
			case 247:
				if (this.OnChangePassword != null)
				{
					this.OnChangePassword();
				}
				break;
			case 248:
				if (this.OnProfileUpdated != null)
				{
					this.OnProfileUpdated();
				}
				break;
			case 249:
			{
				this.RefreshMainProfileValues(response.Parameters);
				this.SaveSteamSecondaryAuth(response.Parameters);
				this.RestoreSteamAuthTicket();
				this.InternalAuthenticate();
				InputDictionaryWorkItem inputDictionaryWorkItem = new InputDictionaryWorkItem();
				inputDictionaryWorkItem.Data = response.Parameters;
				inputDictionaryWorkItem.ProcessAction = new Func<Dictionary<byte, object>, object>(ProfileSerializationHelper.Deserialize);
				inputDictionaryWorkItem.ResultAction = delegate(object r)
				{
					this.UnsubscribeProfileEvents();
					this.profile = (Profile)r;
					this.profile.Init();
					this.SubscribeProfileEvents();
					this.InitMissionsManager();
					this.UpdateFishCage(null, false);
					this.IsAuthenticated = true;
					if (this.OnRegisterUser != null)
					{
						this.OnRegisterUser(this.profile);
					}
				};
				AsynchProcessor.ProcessDictionaryWorkItem(inputDictionaryWorkItem);
				break;
			}
			}
		}
		else
		{
			ProfileFailure profileFailure = new ProfileFailure(response);
			if (profileSubOperationCode != 217)
			{
				Debug.LogError(profileFailure.FullErrorInfo);
			}
			switch (profileSubOperationCode)
			{
			case 217:
				if (this.OnFlagSnowballHitFailed != null)
				{
					this.OnFlagSnowballHitFailed(profileFailure);
				}
				Debug.LogError("You have already targeted this player!");
				break;
			case 218:
				if (this.OnRequestFishCageFailed != null)
				{
					this.OnRequestFishCageFailed(profileFailure);
				}
				break;
			case 219:
				if (this.OnGettingPlayerPlacementFailed != null)
				{
					this.OnGettingPlayerPlacementFailed(profileFailure);
				}
				break;
			case 220:
				if (this.OnSettingAvatarUrlFailed != null)
				{
					this.OnSettingAvatarUrlFailed(profileFailure);
				}
				break;
			case 221:
				if (this.OnResetProfileToDefaultFailed != null)
				{
					this.OnResetProfileToDefaultFailed(profileFailure);
				}
				break;
			default:
				this.HandleProfileFailure("Profile", response);
				break;
			case 225:
				if (this.OnSkipTutorialFailed != null)
				{
					this.OnSkipTutorialFailed(profileFailure);
				}
				break;
			case 226:
				if (this.OnGettingOwnRoomFailed != null)
				{
					this.OnGettingOwnRoomFailed(profileFailure);
				}
				break;
			case 227:
				if (this.OnPromoCodeInvalid != null)
				{
					this.OnPromoCodeInvalid(profileFailure);
				}
				break;
			case 228:
				if (this.OnCreateInviteFailed != null)
				{
					this.OnCreateInviteFailed(profileFailure);
				}
				break;
			case 229:
				if (this.OnInviteDuplicated != null)
				{
					this.OnInviteDuplicated(profileFailure);
				}
				break;
			case 230:
				if (this.OnGeneratePromoCodeFailed != null)
				{
					this.OnGeneratePromoCodeFailed(profileFailure);
				}
				break;
			case 232:
				if (this.OnNameChangeFailed != null)
				{
					this.OnNameChangeFailed(profileFailure);
				}
				break;
			case 233:
				if (this.OnPreviewQuitFailed != null)
				{
					this.OnPreviewQuitFailed(profileFailure);
				}
				break;
			case 235:
				if (this.OnFriendsDetailsRefreshFailed != null)
				{
					this.OnFriendsDetailsRefreshFailed(profileFailure);
				}
				break;
			case 236:
				if (this.OnReleaseFishFromFishCageFailed != null)
				{
					this.OnReleaseFishFromFishCageFailed(profileFailure);
				}
				break;
			case 240:
				if (this.OnCheckUsernameIsUnique != null)
				{
					this.OnCheckUsernameIsUnique(false);
				}
				break;
			case 244:
				if (this.OnCheckEmailIsUnique != null)
				{
					this.OnCheckEmailIsUnique(false);
				}
				break;
			}
		}
	}

	private void InheritProperiesFromPriorProfile(Profile priorProfile)
	{
		if (this.profile != null && priorProfile != null)
		{
			this.profile.PondStayTime = priorProfile.PondStayTime;
			this.profile.PondTimeSpent = priorProfile.PondTimeSpent;
			this.profile.PondId = priorProfile.PondId;
			this.profile.PondStartTime = priorProfile.PondStartTime;
			this.profile.IsTravelingByCar = priorProfile.IsTravelingByCar;
		}
	}

	private void HandlePondUpdateResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			this.ResetMovingState();
			this.HandleMoveFailure("PondUpdate", response, false);
			return;
		}
		this.isMovingToBase = false;
		int? num = this.pondToEnter;
		if (num == null)
		{
			this.UpdateToBaseState();
			this.Moved(TravelDestination.Base);
			ClientMissionsManager instance = ClientMissionsManager.Instance;
			int? pondId = this.Profile.PondId;
			instance.ArrivedToPond((pondId == null) ? 0 : pondId.Value);
			return;
		}
		if (this.profile != null && response.Parameters.ContainsKey(183) && response.Parameters.ContainsKey(182))
		{
			string text = (string)response[182];
			int num2 = (int)response[183];
			this.profile.IncrementBalance(text, (double)(-(double)num2));
			if (this.OnMoneyUpdate != null)
			{
				this.OnMoneyUpdate(new Amount
				{
					Value = -num2,
					Currency = text
				});
			}
		}
		this.MaxRoomCapacity = (int)response[168];
		this.UpdateToPondState();
		this.Moved(TravelDestination.Pond);
		ClientMissionsManager instance2 = ClientMissionsManager.Instance;
		int? pondId2 = this.Profile.PondId;
		instance2.ArrivedToPond((pondId2 == null) ? 0 : pondId2.Value);
	}

	private void HandleProlongPondStayResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			DebugUtility.Travel.Trace("HandleProlongPondStayResponse (Failed: {0})", new object[] { response.ReturnCode });
			this.ResetMovingState();
			DebugUtility.RoomIdIssue.Trace("Clear roomToEnter", new object[0]);
			this.HandleMoveFailure("ProlongPondStay", response, false);
			return;
		}
		if (this.profile != null)
		{
			Profile profile = this.profile;
			int? pondStayTime = profile.PondStayTime;
			bool flag = pondStayTime != null;
			int? num = this.daysToAdd;
			profile.PondStayTime = ((!(flag & (num != null))) ? null : new int?(pondStayTime.GetValueOrDefault() + num.GetValueOrDefault()));
			this.daysToAdd = null;
			if (response.Parameters.ContainsKey(183) && response.Parameters.ContainsKey(182))
			{
				string text = (string)response[182];
				int num2 = (int)response[183];
				this.profile.IncrementBalance(text, (double)(-(double)num2));
				if (this.OnPondStayProlonged != null)
				{
					this.OnPondStayProlonged(new ProlongInfo
					{
						Cost = (float)num2,
						Currency = text
					});
				}
			}
		}
	}

	private void HandleMoveFailure(string opName, OperationResponse response, bool isFatal = false)
	{
		Debug.LogErrorFormat("{0} failed. Code: {1} Message: {2}", new object[]
		{
			opName,
			Enum.GetName(typeof(ErrorCode), response.ReturnCode),
			response.DebugMessage
		});
		Failure failure;
		if (response.OperationCode == 200)
		{
			failure = new ProfileFailure(response)
			{
				SubOperation = (ProfileSubOperationCode)response[1]
			};
		}
		else
		{
			failure = new Failure(response);
		}
		this.MoveFailed(failure);
		if (isFatal)
		{
			this.Disconnect();
		}
	}

	private void HandleProfileFailure(string opName, OperationResponse response)
	{
		Debug.LogErrorFormat("{0} failed. Code: {1} Message: {2}", new object[]
		{
			opName,
			Enum.GetName(typeof(ErrorCode), response.ReturnCode),
			response.DebugMessage
		});
		ProfileFailure profileFailure = new ProfileFailure(response);
		if (response.Parameters.ContainsKey(1))
		{
			profileFailure.SubOperation = (ProfileSubOperationCode)response[1];
		}
		Debug.LogError(profileFailure.FullErrorInfo);
		if (this.OnProfileOperationFailed != null)
		{
			this.OnProfileOperationFailed(profileFailure);
		}
	}

	public void OnStatusChanged(StatusCode statusCode)
	{
		this.ConnectionState = statusCode;
		if (statusCode == 1024)
		{
			this.Peer.EstablishEncryption();
			if (this.Peer.ServerAddress == this.MasterServerAddress)
			{
				this.IsConnectedToMaster = true;
				this.IsConnectedToGameServer = false;
			}
			else
			{
				this.IsConnectedToMaster = false;
				this.IsConnectedToGameServer = true;
			}
		}
		else if (statusCode == 1048)
		{
			if (this.Peer.ServerAddress == this.MasterServerAddress)
			{
				this.FlushErrorsCache();
				if (this.connectWithoutAuth)
				{
					this.connectWithoutAuth = false;
				}
				else
				{
					this.InternalAuthenticate();
				}
				if (this.OnConnectedToMaster != null)
				{
					this.OnConnectedToMaster();
				}
			}
			else
			{
				this.ReAuthenticate();
			}
		}
		else if (statusCode == 1049)
		{
			this.Reset();
			if (this.OnConnectionFailed != null)
			{
				this.OnConnectionFailed();
			}
		}
		else if (statusCode == 1023 || statusCode == 1022)
		{
			this.Reset();
			if (this.OnConnectionFailed != null)
			{
				this.OnConnectionFailed();
			}
		}
		else if (statusCode == 1025 || statusCode == 1043 || statusCode == 1042 || statusCode == 1026 || statusCode == 1039 || statusCode == 1030)
		{
			this.IsConnectedToMaster = false;
			this.IsConnectedToGameServer = false;
			this.Room.RoomId = null;
			this.UserId = null;
			this.UserName = null;
			this.isAuthenticating = false;
			this.isAuthenticatingWithSteam = false;
			if (!this.isDisconnectToReconnect)
			{
				this.ClearInternalState();
				if (this.OnDisconnect != null)
				{
					this.OnDisconnect();
				}
			}
			this.isDisconnectToReconnect = false;
		}
	}

	private void HandleAuthenticateResponse(OperationResponse operationResponse)
	{
		this.isAuthenticating = false;
		bool flag = this.isAuthenticatingWithSteam;
		this.isAuthenticatingWithSteam = false;
		if (operationResponse.ReturnCode == 0)
		{
			this.IsAuthenticated = true;
			if (this.OnAuthenticated != null)
			{
				this.OnAuthenticated();
			}
			if (this.IsConnectedToMaster)
			{
				this.UserId = (string)operationResponse.Parameters[225];
				this.UserName = (string)operationResponse.Parameters[211];
				this.securityToken = (string)operationResponse.Parameters[221];
				LogHelper.Log("ConnectedToMaster UserId:{0}", new object[] { this.UserId });
				if (operationResponse.Parameters.ContainsKey(188) && operationResponse.Parameters.ContainsKey(245))
				{
					string text = (string)operationResponse.Parameters[188];
					string text2 = (string)operationResponse.Parameters[245];
					string text3 = StaticUserData.SteamId.ToString();
					ObscuredPrefs.SetString("Email" + text3, text);
					ObscuredPrefs.SetString("Password" + text3, text2);
				}
				else if (flag && this.OnPrimarySteamAuthenticationFailed != null)
				{
					this.OnPrimarySteamAuthenticationFailed();
				}
				if (operationResponse.Parameters.ContainsKey(194))
				{
					this.Environment = (string)operationResponse.Parameters[194];
				}
				if (this.OnConnectedToMaster != null)
				{
					this.OnConnectedToMaster();
				}
				int? num = this.pondToEnter;
				if (num != null && this.isMovingToRoom)
				{
					this.MoveToRoomInt(this.pondToEnter.Value, this.roomToEnter);
				}
				else
				{
					int? num2 = this.pondToEnter;
					if (num2 == null && this.isMovingToBase)
					{
						this.Peer.OpJoinLobby(null);
						OpTimer.Start(229);
					}
				}
			}
			else if (this.IsConnectedToGameServer)
			{
				this.UserId = (string)operationResponse.Parameters[225];
				this.UserName = (string)operationResponse.Parameters[211];
				this.FlushTelemetryCache();
				if (this.cachedGameAction == "join" && !string.IsNullOrEmpty(this.Room.RoomId))
				{
					DebugUtility.RoomIdIssue.Trace("Join Room {0}", new object[] { this.Room.RoomId });
					bool flag2 = false;
					if (this.friendsRooms != null && this.friendsRooms.Count > 0)
					{
						foreach (RoomPopulation roomPopulation in this.friendsRooms)
						{
							if (roomPopulation.RoomId == this.Room.RoomId)
							{
								this.Peer.OpJoinRoom(this.Room.RoomId, (!roomPopulation.IsPrivate) ? this.GetRoomOptions() : this.GetFriendsOnlyRoomOptions(), null, false, null, true);
								OpTimer.Start(226);
								flag2 = true;
								break;
							}
						}
					}
					if (!flag2)
					{
						this.Peer.OpJoinRoom(this.Room.RoomId, this.GetRoomOptions(), null, false, null, true);
						OpTimer.Start(226);
					}
				}
				else if (this.cachedGameAction == "create" || (this.cachedGameAction == "join" && string.IsNullOrEmpty(this.Room.RoomId)))
				{
					if (!(this.cachedGameAction == "join") || string.IsNullOrEmpty(this.Room.RoomId))
					{
					}
					if (string.IsNullOrEmpty(this.Room.RoomId))
					{
						this.GenerateNewRoomId();
					}
					if (this.roomToEnter == "friends")
					{
						this.Peer.OpCreateRoom(this.Room.RoomId, this.GetFriendsOnlyRoomOptions(), null, null, true);
						OpTimer.Start(227);
						this.ValidateRoomCreation("HandleAuthenticateResponse.FriendsOnlyRoomName");
					}
					else if (this.roomToEnter == "private")
					{
						this.Peer.OpCreateRoom(this.Room.RoomId, this.GetPrivateRoomOptions(), null, null, true);
						OpTimer.Start(227);
						this.ValidateRoomCreation("HandleAuthenticateResponse.PrivateRoomName");
					}
					else
					{
						this.Peer.OpCreateRoom(this.Room.RoomId, this.GetRoomOptions(), null, null, true);
						OpTimer.Start(227);
						this.ValidateRoomCreation("HandleAuthenticateResponse." + this.cachedGameAction + "." + this.roomToEnter);
					}
				}
			}
		}
		else
		{
			Debug.LogErrorFormat("auth: Authentication failed. Code: {0} Message: {1}", new object[]
			{
				Enum.GetName(typeof(ErrorCode), operationResponse.ReturnCode),
				operationResponse.DebugMessage
			});
			if (this.OnAuthenticationFailed != null)
			{
				this.OnAuthenticationFailed(new Failure(operationResponse));
			}
		}
	}

	private void HandleJoinLobbyResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.roomToEnter == null)
			{
				LoadbalancingPeer peer = this.Peer;
				Hashtable joinRandomRoomOptions = this.GetJoinRandomRoomOptions();
				int? num = this.pondToEnter;
				peer.OpJoinRandomRoom(joinRandomRoomOptions, (byte)((num == null || !this.isMovingToRoom) ? 20 : this.MaxRoomCapacity), null, MatchmakingMode.FillRoom, TypedLobby.Default, null);
				OpTimer.Start(225);
			}
			else if (this.roomToEnter == "friends")
			{
				this.GenerateNewRoomId();
				this.Peer.OpCreateRoom(this.Room.RoomId, this.GetFriendsOnlyRoomOptions(), null, null, true);
				OpTimer.Start(227);
				this.ValidateRoomCreation("HandleJoinLobbyResponse.FriendsOnlyRoomName");
			}
			else if (this.roomToEnter == "private")
			{
				this.GenerateNewRoomId();
				this.Peer.OpCreateRoom(this.Room.RoomId, this.GetPrivateRoomOptions(), null, null, true);
				OpTimer.Start(227);
				this.ValidateRoomCreation("HandleJoinLobbyResponse.PrivateRoomName");
			}
			else if (this.roomToEnter == "top")
			{
				RoomOptions roomOptions = new RoomOptions
				{
					customRoomProperties = this.GetJoinRandomRoomOptions()
				};
				this.Peer.OpJoinRoom(this.roomToEnter, roomOptions, TypedLobby.Default, false, null, false);
				OpTimer.Start(226);
			}
			else
			{
				this.Peer.OpJoinRoom(this.roomToEnter, null, TypedLobby.Default, false, null, false);
				OpTimer.Start(226);
			}
		}
		else
		{
			this.isMovingToRoom = false;
			Debug.LogErrorFormat("csri: JoinLobby failed. Code: {0} Message: {1}", new object[]
			{
				Enum.GetName(typeof(ErrorCode), response.ReturnCode),
				response.DebugMessage
			});
			this.HandleMoveFailure("JoinLobby", response, true);
		}
	}

	private void HandleJoinRandomGameResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0 && !string.IsNullOrEmpty((string)response[255]))
		{
			string text = (string)response[230];
			string text2 = (string)response[byte.MaxValue];
			this.Room.RoomId = text2;
			this.DisconnectToReconnect(text, text2, "join");
		}
		else if (response.ReturnCode == 32760 || (response.ReturnCode == 0 && string.IsNullOrEmpty((string)response[255])))
		{
			this.GenerateNewRoomId();
			this.Peer.OpCreateRoom(this.Room.RoomId, this.GetRoomOptions(), null, null, true);
			OpTimer.Start(227);
			this.ValidateRoomCreation("HandleJoinRandomGameResponse.NoMatchFound");
		}
		else
		{
			if (this.raiseJoinErrorEvents)
			{
				if (response.ReturnCode == 32765 && this.OnRoomIsFull != null)
				{
					this.OnRoomIsFull();
				}
				else if (this.OnUnableToEnterRoom != null)
				{
					this.OnUnableToEnterRoom(new Failure(response));
				}
			}
			this.GenerateNewRoomId();
			this.Peer.OpCreateRoom(this.Room.RoomId, this.GetRoomOptions(), null, null, true);
			OpTimer.Start(227);
			this.ValidateRoomCreation("HandleJoinRandomGameResponse." + Enum.GetName(typeof(ErrorCode), response.ReturnCode));
		}
	}

	private void HandleJoinGameResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.IsConnectedToMaster)
			{
				string text = (string)response[230];
				if (response.Parameters.ContainsKey(255))
				{
					this.roomToEnter = (string)response[byte.MaxValue];
				}
				string text2 = this.roomToEnter;
				this.DisconnectToReconnect(text, text2, "join");
			}
			else if (this.IsConnectedToGameServer)
			{
				int? num = this.pondToEnter;
				if (num == null)
				{
					this.InternalMoveToBase();
					return;
				}
				int? num2 = this.pondToEnter;
				if (num2 != null && !this.isMovingToRoom)
				{
					this.InternalChangePond();
					return;
				}
				this.UpdateToInRoomState(response);
				this.ParceMultipleActorsProperties(response);
				if (this.OnCharacterCreated != null)
				{
					foreach (Player player in this.Room.Players)
					{
						this.OnCharacterCreated(player);
					}
				}
				this.Moved(TravelDestination.Room);
			}
		}
		else
		{
			Debug.LogErrorFormat("csri: JoinGame failed. Code: {0} Message: {1}", new object[]
			{
				Enum.GetName(typeof(ErrorCode), response.ReturnCode),
				response.DebugMessage
			});
			if (this.raiseJoinErrorEvents)
			{
				if (response.ReturnCode == 32765 && this.OnRoomIsFull != null)
				{
					this.OnRoomIsFull();
				}
				else if (this.OnUnableToEnterRoom != null)
				{
					this.OnUnableToEnterRoom(new Failure(response));
				}
			}
			if (this.IsConnectedToMaster)
			{
				this.GenerateNewRoomId();
				this.Peer.OpCreateRoom(this.Room.RoomId, this.GetRoomOptions(), null, null, true);
				OpTimer.Start(227);
				this.ValidateRoomCreation("HandleJoinGameResponse." + Enum.GetName(typeof(ErrorCode), response.ReturnCode));
			}
			else if (this.IsConnectedToGameServer)
			{
				this.GenerateNewRoomId();
				this.DisconnectToReconnect(this.MasterServerAddress, null, null);
			}
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnRoomIsFull;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnUnableToEnterRoom;

	private void HandleCreateGameResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			if (this.IsConnectedToMaster)
			{
				string text = (string)response[230];
				string text2 = (string)response[byte.MaxValue];
				this.DisconnectToReconnect(text, text2, "create");
			}
			else if (this.IsConnectedToGameServer)
			{
				int? num = this.pondToEnter;
				if (num == null)
				{
					this.InternalMoveToBase();
					return;
				}
				int? num2 = this.pondToEnter;
				if (num2 != null && !this.isMovingToRoom)
				{
					this.InternalChangePond();
					return;
				}
				this.UpdateToInRoomState(response);
				this.Moved(TravelDestination.Room);
			}
		}
		else
		{
			this.ResetMovingState();
			Debug.LogErrorFormat("csri: CreateGame failed. Code: {0} Message: {1}", new object[]
			{
				Enum.GetName(typeof(ErrorCode), response.ReturnCode),
				response.DebugMessage
			});
			this.HandleMoveFailure("CreateGame", response, true);
		}
	}

	private void InternalMoveToBase()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 243;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 243);
	}

	private void InternalChangePond()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
		operationRequest.Parameters[1] = 243;
		operationRequest.Parameters[185] = this.pondToEnter;
		int? num = this.daysToStay;
		if (num != null)
		{
			operationRequest.Parameters[181] = this.daysToStay.Value;
		}
		bool? flag = this.isMovingByCar;
		if (flag != null)
		{
			operationRequest.Parameters[184] = this.isMovingByCar.Value;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(200, 243);
	}

	private void ResetMovingState()
	{
		this.isMovingToRoom = false;
		this.isMovingToBase = false;
		this.pondToEnter = null;
		this.tournamentToEnter = null;
		this.daysToStay = null;
		this.isMovingByCar = null;
		this.roomToEnter = null;
		this.daysToAdd = null;
	}

	private void UpdateToBaseState()
	{
		this.CurrentPondId = null;
		this.CurrentTournamentId = null;
		this.TournamentRemainingTime = null;
		this.IsInRoom = false;
		this.Room.Clear();
		this.IsOnBase = true;
		if (this.profile != null)
		{
			this.profile.PondId = null;
			this.profile.IsTravelingByCar = null;
			this.profile.PondStartTime = null;
			this.profile.PondStayTime = null;
			this.profile.PondTimeSpent = null;
		}
		this.ResetMovingState();
		this.ResetGameState();
	}

	private void UpdateToPondState()
	{
		this.CurrentPondId = this.pondToEnter;
		this.CurrentTournamentId = null;
		this.TournamentRemainingTime = null;
		this.IsInRoom = false;
		this.Room.Clear();
		this.IsOnBase = false;
		if (this.profile != null)
		{
			int? num = this.pondToEnter;
			if (num != null)
			{
				this.profile.PondId = new int?(this.pondToEnter.Value);
				this.profile.IsTravelingByCar = this.isMovingByCar;
				this.profile.PondStayTime = this.daysToStay;
			}
		}
		this.ResetMovingState();
		this.ResetGameState();
	}

	private void UpdateToInRoomState(OperationResponse response)
	{
		this.CurrentPondId = this.pondToEnter;
		this.CurrentTournamentId = this.tournamentToEnter;
		this.TournamentRemainingTime = null;
		this.IsInRoom = true;
		this.Room.ClearPlayers();
		this.IsOnBase = false;
		if (this.profile != null)
		{
			int? num = this.pondToEnter;
			if (num != null)
			{
				this.profile.PondId = new int?(this.pondToEnter.Value);
				if (this.profile.PondTimeSpent == null)
				{
					this.profile.PondTimeSpent = new TimeSpan?(default(TimeSpan));
				}
			}
		}
		this.ParseRoomOptionsToRoom(response);
		this.ResetMovingState();
		this.ResetGameState();
		this.internalChatMessageQueue.Clear();
	}

	private void ParseRoomOptionsToRoom(OperationResponse response)
	{
		Hashtable hashtable = (Hashtable)response.Parameters[248];
		object obj;
		if (hashtable.TryGetValue(253, out obj))
		{
			this.Room.IsOpen = (bool)obj;
		}
		else
		{
			this.Room.IsOpen = true;
		}
		object obj2;
		if (hashtable.TryGetValue(254, out obj2))
		{
			this.Room.IsVisible = (bool)obj2;
		}
		else
		{
			this.Room.IsVisible = true;
		}
		object obj3;
		if (hashtable.TryGetValue(255, out obj3))
		{
			this.Room.MaxPlayers = (int)((byte)obj3);
		}
		else
		{
			this.Room.MaxPlayers = 0;
		}
		object obj4;
		if (hashtable.TryGetValue("pond", out obj4))
		{
			this.Room.PondId = (int)obj4;
		}
		else
		{
			this.Room.PondId = 0;
		}
		object obj5;
		if (hashtable.TryGetValue("lng", out obj5))
		{
			this.Room.LanguageId = (int)obj5;
		}
		else
		{
			this.Room.LanguageId = 0;
		}
		object obj6;
		if (hashtable.TryGetValue("p", out obj6))
		{
			this.Room.IsPrivate = (int)obj6 == 1;
		}
		else
		{
			this.Room.IsPrivate = false;
		}
		object obj7;
		if (hashtable.TryGetValue("t", out obj7))
		{
			this.Room.IsTournament = (int)obj7 == 1;
		}
		else
		{
			this.Room.IsTournament = false;
		}
		object obj8;
		if (hashtable.TryGetValue("ti", out obj8))
		{
			this.Room.TournamentId = (int)obj8;
		}
		else
		{
			this.Room.TournamentId = 0;
		}
	}

	private void ResetGameState()
	{
		if (this.CurrentPondId != null && this.CurrentPondId != 0 && this.IsInRoom)
		{
			this.AssociateRodInHands();
			this.gameSlots[this.CurrentGameSlotIndex] = new Game(this.CurrentGameSlotIndex);
		}
		else
		{
			this.CurrentGameSlotIndex = 0;
			this.PendingGameSlotIndex = 0;
		}
	}

	private Rod AssociateRodInHands()
	{
		Rod rod = (Rod)this.Profile.Inventory.FirstOrDefault((InventoryItem i) => i.ItemType == ItemTypes.Rod && i.Storage == StoragePlaces.Hands && i is Rod && i.Durability > 0);
		if (rod != null && this.Profile.Inventory.GetRodTemplate(rod) != RodTemplate.UnEquiped)
		{
			this.CurrentGameSlotIndex = rod.Slot;
			this.PendingGameSlotIndex = rod.Slot;
			GameFactory.RodSlots[this.CurrentGameSlotIndex].FinishServerOp();
			return rod;
		}
		return null;
	}

	private void GenerateNewRoomId()
	{
		this.Room.RoomId = Guid.NewGuid().ToString().Replace("-", string.Empty)
			.Substring(1, 16);
		DebugUtility.RoomIdIssue.Trace("Set RoomId = {0} - GenerateNewRoomId", new object[] { this.Room.RoomId });
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnPrimarySteamAuthenticationFailed;

	private void HandleFindPlayersResponse(OperationResponse response)
	{
		Failure standardFailure = PhotonServerConnection.GetStandardFailure(response);
		if (response.ReturnCode == 0)
		{
			if (response.Parameters.ContainsKey(192))
			{
				InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
				inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
				inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
				{
					string text = CompressHelper.DecompressString(p);
					return JsonConvert.DeserializeObject<List<Player>>(text, SerializationHelper.JsonSerializerSettings);
				};
				inputByteArrayWorkItem.ResultAction = delegate(object r)
				{
					List<Player> list = (List<Player>)r;
					if (this.OnFindPlayers != null)
					{
						this.OnFindPlayers(list);
					}
				};
				AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
			}
		}
		else if (this.OnFindPlayersFailed != null)
		{
			this.OnFindPlayersFailed(standardFailure);
		}
	}

	private static Failure GetStandardFailure(OperationResponse response)
	{
		Failure failure = null;
		if (response.ReturnCode != 0)
		{
			try
			{
				failure = new Failure(response);
				Debug.LogError(failure.FullErrorInfo);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Error logging {0} operation error: {1}", new object[]
				{
					Enum.GetName(typeof(OperationCode), response.OperationCode),
					ex.Message
				});
				Debug.LogErrorFormat("Original error: {0}", new object[] { response.DebugMessage });
			}
		}
		return failure;
	}

	private void HandleGetChatPlayersCount(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			int num = (int)response.Parameters[229];
			if (this.OnGotChatPlayersCount != null)
			{
				this.OnGotChatPlayersCount(num);
			}
		}
		else
		{
			Failure failure = new Failure(response);
			Debug.LogErrorFormat(failure.FullErrorInfo, new object[0]);
			if (this.OnGettingChatPlayersCountFailed != null)
			{
				this.OnGettingChatPlayersCountFailed(failure);
			}
		}
	}

	private void HandleSendChatCommandResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			ChatCommands chatCommands = (ChatCommands)response.Parameters[244];
			switch (chatCommands)
			{
			case 2:
			{
				string userId = (string)response.Parameters[225];
				if (this.Profile.Friends == null)
				{
					this.Profile.Friends = new List<Player>();
				}
				Player player = this.Profile.Friends.FirstOrDefault((Player p) => p.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase));
				if (player != null)
				{
					player.Status = FriendStatus.Ignore;
				}
				else
				{
					this.InternalAddFriend(new Player
					{
						UserId = userId,
						UserName = (string)response.Parameters[211],
						Status = FriendStatus.Ignore
					});
				}
				break;
			}
			case 3:
			{
				string text = (string)response.Parameters[211];
				this.InternalRemoveFriendByName(text);
				break;
			}
			case 4:
				this.Profile.IsIncognito = !this.Profile.IsIncognito;
				if (this.OnIncognitoChanged != null)
				{
					this.OnIncognitoChanged(this.Profile.IsIncognito);
				}
				break;
			}
			if (chatCommands != 4 && this.OnChatCommandSucceeded != null)
			{
				this.OnChatCommandSucceeded(new ChatCommandInfo
				{
					Command = chatCommands,
					Username = (string)response.Parameters[211]
				});
			}
		}
		else
		{
			Failure failure = new Failure(response);
			Debug.LogError(failure.FullErrorInfo);
			if (this.OnChatCommandFailed != null)
			{
				this.OnChatCommandFailed(failure);
			}
		}
	}

	private void HandleGetProtocolVersion(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			int num = (int)response.Parameters[188];
			if (num != this.ProtocolVersion)
			{
				Debug.LogErrorFormat("Server {0} client {1}", new object[] { num, this.ProtocolVersion });
			}
			if (this.OnGotProtocolVersion != null)
			{
				this.OnGotProtocolVersion(num == this.ProtocolVersion);
			}
		}
		else
		{
			Debug.LogErrorFormat("GetProtocolVersion failed. Code: {0} Message: {1}", new object[]
			{
				Enum.GetName(typeof(ErrorCode), response.ReturnCode),
				response.DebugMessage
			});
			if (this.OnOperationFailed != null)
			{
				this.OnOperationFailed(new Failure(response));
			}
		}
	}

	private void HandleGetTopsResponse(OperationResponse response)
	{
		if (response.ReturnCode == 0)
		{
			TopLeadersResult result = new TopLeadersResult
			{
				Kind = (byte)response.Parameters[173]
			};
			AsynchProcessor.ProcessByteArrayWorkItem(new InputByteArrayWorkItem
			{
				Data = (byte[])response.Parameters[192],
				ProcessAction = delegate(byte[] p)
				{
					TopKind kind = result.Kind;
					if (kind == 1)
					{
						byte[] array = (byte[])response.Parameters[192];
						string text = CompressHelper.DecompressString(array);
						if (!string.IsNullOrEmpty(text))
						{
							result.Players = JsonConvert.DeserializeObject<List<TopPlayers>>(text, SerializationHelper.JsonSerializerSettings);
						}
						return result;
					}
					if (kind == 2)
					{
						byte[] array2 = (byte[])response.Parameters[192];
						string text2 = CompressHelper.DecompressString(array2);
						if (!string.IsNullOrEmpty(text2))
						{
							result.Fish = JsonConvert.DeserializeObject<List<TopFish>>(text2, SerializationHelper.JsonSerializerSettings);
						}
						return result;
					}
					if (kind != 3)
					{
						return result;
					}
					byte[] array3 = (byte[])response.Parameters[192];
					string text3 = CompressHelper.DecompressString(array3);
					if (!string.IsNullOrEmpty(text3))
					{
						result.Tournaments = JsonConvert.DeserializeObject<List<TopTournamentPlayers>>(text3, SerializationHelper.JsonSerializerSettings);
					}
					result.TournamentKind = new TopTournamentKind?((TopTournamentKind)response.Parameters[171]);
					return result;
				},
				ResultAction = delegate(object r)
				{
					TopLeadersResult topLeadersResult = (TopLeadersResult)r;
					if (this.OnGotLeaderboards != null)
					{
						this.OnGotLeaderboards(topLeadersResult);
					}
				}
			});
		}
		else if (this.OnGettingLeaderboardsFailed != null)
		{
			this.OnGettingLeaderboardsFailed(new Failure(response));
		}
	}

	private void HandleTournamentOperationResponse(OperationResponse response)
	{
		Failure failure = null;
		if (response.ReturnCode != 0)
		{
			failure = new TournamentFailure(response);
			Debug.LogError(failure.FullErrorInfo);
		}
		if (!response.Parameters.ContainsKey(1))
		{
			Debug.LogError("Tournament operation response has no subcode");
			return;
		}
		TournamentSubOperationCode tournamentSubOperationCode = (TournamentSubOperationCode)response[1];
		OpTimer.End(168, tournamentSubOperationCode);
		switch (tournamentSubOperationCode)
		{
		case 1:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotOpenTournaments != null)
				{
					this.DeserializeListAsync<Tournament>(response, delegate(IEnumerable<Tournament> res)
					{
						if (this.OnGotOpenTournaments != null)
						{
							this.OnGotOpenTournaments(res.ToList<Tournament>());
						}
					}, true);
				}
			}
			else if (this.OnGettingOpenTournamentsFailed != null)
			{
				this.OnGettingOpenTournamentsFailed(failure);
			}
			break;
		case 2:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotMyTournaments != null)
				{
					byte[] array = (byte[])response.Parameters[10];
					string json = CompressHelper.DecompressString(array);
					List<Tournament> list = JsonConvert.DeserializeObject<List<Tournament>>(json, SerializationHelper.JsonSerializerSettings);
					this.OnGotMyTournaments(list);
				}
			}
			else if (this.OnGettingMyTournamentsFailed != null)
			{
				this.OnGettingMyTournamentsFailed(failure);
			}
			break;
		case 3:
			if (response.ReturnCode == 0)
			{
				if (this.OnRegisteredForTournament != null)
				{
					this.OnRegisteredForTournament();
				}
			}
			else if (this.OnRegisterForTournamentFailed != null)
			{
				this.OnRegisterForTournamentFailed(failure);
			}
			break;
		case 4:
			if (response.ReturnCode == 0)
			{
				if (this.OnTournamentStartPreview != null)
				{
					this.OnTournamentStartPreview();
				}
			}
			else if (this.OnPreviewTournamentStartFailed != null)
			{
				this.OnPreviewTournamentStartFailed(failure);
			}
			break;
		case 6:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotTournamentParticipationHistory != null)
				{
					byte[] array = (byte[])response.Parameters[10];
					string json = CompressHelper.DecompressString(array);
					List<TournamentIndividualResults> list2 = JsonConvert.DeserializeObject<List<TournamentIndividualResults>>(json, SerializationHelper.JsonSerializerSettings);
					this.OnGotTournamentParticipationHistory(list2, 0);
				}
			}
			else if (this.OnGettingTournamentParticipationHistoryFailed != null)
			{
				this.OnGettingTournamentParticipationHistoryFailed(failure);
			}
			break;
		case 7:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotTournamentSeries != null)
				{
					this.DeserializeListAsync<TournamentSerie>(response, delegate(IEnumerable<TournamentSerie> series)
					{
						if (this.OnGotTournamentSeries != null)
						{
							this.OnGotTournamentSeries(series.ToList<TournamentSerie>());
						}
					}, true);
				}
			}
			else if (this.OnGettingTournamentSeriesFailed != null)
			{
				this.OnGettingTournamentSeriesFailed(failure);
			}
			break;
		case 8:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotTournamentGrid != null)
				{
					byte[] array = (byte[])response.Parameters[10];
					string json = CompressHelper.DecompressString(array);
					List<TournamentGridItem> list3 = JsonConvert.DeserializeObject<List<TournamentGridItem>>(json, SerializationHelper.JsonSerializerSettings);
					this.OnGotTournamentGrid(list3);
				}
			}
			else if (this.OnGettingTournamentGridFailed != null)
			{
				this.OnGettingTournamentGridFailed(failure);
			}
			break;
		case 9:
			if (response.ReturnCode == 0)
			{
				int? tournamentKind = null;
				if (response.Parameters.ContainsKey(27))
				{
					tournamentKind = new int?((int)response[27]);
				}
				if (this.OnGotTournaments != null)
				{
					string json;
					AsynchProcessor.ProcessByteArrayWorkItem(new InputByteArrayWorkItem
					{
						Data = (byte[])response.Parameters[10],
						ProcessAction = delegate(byte[] p)
						{
							json = CompressHelper.DecompressString(p);
							return JsonConvert.DeserializeObject<List<Tournament>>(json, SerializationHelper.JsonSerializerSettings);
						},
						ResultAction = delegate(object r)
						{
							List<Tournament> list7 = (List<Tournament>)r;
							if (this.OnGotTournaments != null)
							{
								this.OnGotTournaments(tournamentKind, list7);
							}
						}
					});
				}
			}
			else if (this.OnGettingTournamentsFailed != null)
			{
				this.OnGettingTournamentsFailed(failure);
			}
			break;
		case 10:
			if (response.ReturnCode == 0 && this.OnGotTournamentIntermediateResult != null)
			{
				string json;
				AsynchProcessor.ProcessByteArrayWorkItem(new InputByteArrayWorkItem
				{
					Data = (byte[])response.Parameters[10],
					ProcessAction = delegate(byte[] p)
					{
						json = CompressHelper.DecompressString(p);
						return JsonConvert.DeserializeObject<List<TournamentIndividualResults>>(json, SerializationHelper.JsonSerializerSettings);
					},
					ResultAction = delegate(object r)
					{
						List<TournamentIndividualResults> list8 = (List<TournamentIndividualResults>)r;
						int num = 0;
						if (response.Parameters.ContainsKey(29))
						{
							num = (int)response.Parameters[29];
						}
						if (this.OnGotTournamentIntermediateResult != null)
						{
							this.OnGotTournamentIntermediateResult(list8, num);
						}
					}
				});
			}
			break;
		case 11:
			if (response.ReturnCode == 0 && this.OnGotTournamentFinalResult != null)
			{
				InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
				inputByteArrayWorkItem.Data = (byte[])response.Parameters[10];
				inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
				{
					string text2 = CompressHelper.DecompressString(p);
					return JsonConvert.DeserializeObject<List<TournamentIndividualResults>>(text2, SerializationHelper.JsonSerializerSettings);
				};
				inputByteArrayWorkItem.ResultAction = delegate(object r)
				{
					List<TournamentIndividualResults> list9 = (List<TournamentIndividualResults>)r;
					int num2 = 0;
					if (response.Parameters.ContainsKey(29))
					{
						num2 = (int)response.Parameters[29];
					}
					if (this.OnGotTournamentFinalResult != null)
					{
						this.OnGotTournamentFinalResult(list9, num2);
					}
				};
				AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
			}
			break;
		case 12:
			if (response.ReturnCode == 0)
			{
				if (this.OnTournamentEnded != null)
				{
					this.OnTournamentEnded();
				}
			}
			else if (this.OnTournamentEndFailed != null)
			{
				this.OnTournamentEndFailed(failure);
			}
			break;
		case 13:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotTournament != null)
				{
					byte[] array = (byte[])response.Parameters[10];
					string json = CompressHelper.DecompressString(array);
					Tournament tournament = JsonConvert.DeserializeObject<Tournament>(json, SerializationHelper.JsonSerializerSettings);
					this.OnGotTournament(tournament);
				}
			}
			else if (this.OnGettingTournamentFailed != null)
			{
				this.OnGettingTournamentFailed(failure);
			}
			break;
		case 14:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotTournamentRewards != null)
				{
					InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
					inputByteArrayWorkItem.Data = (byte[])response.Parameters[10];
					inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
					{
						string text3 = CompressHelper.DecompressString(p);
						return JsonConvert.DeserializeObject<List<Reward>>(text3, SerializationHelper.JsonSerializerSettings);
					};
					inputByteArrayWorkItem.ResultAction = delegate(object r)
					{
						List<Reward> list10 = (List<Reward>)r;
						if (this.OnGotTournamentRewards != null)
						{
							this.OnGotTournamentRewards(list10);
						}
					};
					AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
				}
			}
			else if (this.OnGettingTournamentRewardsFailed != null)
			{
				this.OnGettingTournamentRewardsFailed(failure);
			}
			break;
		case 15:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotTournamentWeather != null)
				{
					byte[] array = (byte[])response.Parameters[10];
					string json = CompressHelper.DecompressString(array);
					WeatherDesc[] array2 = JsonConvert.DeserializeObject<WeatherDesc[]>(json, SerializationHelper.JsonSerializerSettings);
					this.OnGotTournamentWeather(array2);
				}
			}
			else if (this.OnGettingTournamentWeatherFailed != null)
			{
				this.OnGettingTournamentWeatherFailed(failure);
			}
			break;
		case 16:
			if (response.ReturnCode == 0 && this.OnGotTournamentSecondaryResult != null)
			{
				byte[] array = (byte[])response.Parameters[10];
				string json = CompressHelper.DecompressString(array);
				List<TournamentSecondaryResult> list4 = JsonConvert.DeserializeObject<List<TournamentSecondaryResult>>(json, SerializationHelper.JsonSerializerSettings);
				this.OnGotTournamentSecondaryResult(list4);
			}
			break;
		case 17:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotTournamentSerieInstances != null)
				{
					byte[] array = (byte[])response.Parameters[10];
					string json = CompressHelper.DecompressString(array);
					List<TournamentSerieInstance> list5 = JsonConvert.DeserializeObject<List<TournamentSerieInstance>>(json, SerializationHelper.JsonSerializerSettings);
					this.OnGotTournamentSerieInstances(list5);
				}
			}
			else if (this.OnGettingTournamentSerieInstancesFailed != null)
			{
				this.OnGettingTournamentSerieInstancesFailed(failure);
			}
			break;
		case 18:
			if (response.ReturnCode == 0)
			{
				if (this.CheckProfile() && response.Parameters.ContainsKey(36))
				{
					string text = (string)response[36];
					LevelLockRemoval levelLockRemoval = JsonConvert.DeserializeObject<LevelLockRemoval>(text, SerializationHelper.JsonSerializerSettings);
					if (this.Profile.LevelLockRemovals == null)
					{
						this.Profile.LevelLockRemovals = new List<LevelLockRemoval>();
					}
					this.Profile.LevelLockRemovals.Add(levelLockRemoval);
				}
				if (this.OnPondLevelInvalidated != null)
				{
					this.OnPondLevelInvalidated();
				}
				if (this.OnRegisteredForTournamentSerie != null)
				{
					this.OnRegisteredForTournamentSerie();
				}
			}
			else if (this.OnRegisterForTournamentSerieFailed != null)
			{
				this.OnRegisterForTournamentSerieFailed(failure);
			}
			break;
		case 19:
			if (response.ReturnCode == 0 && this.OnGotIntermediateTournamentTeamResult != null)
			{
				string json;
				AsynchProcessor.ProcessByteArrayWorkItem(new InputByteArrayWorkItem
				{
					Data = (byte[])response.Parameters[10],
					ProcessAction = delegate(byte[] p)
					{
						json = CompressHelper.DecompressString(p);
						return JsonConvert.DeserializeObject<List<TournamentTeamResult>>(json, SerializationHelper.JsonSerializerSettings);
					},
					ResultAction = delegate(object r)
					{
						List<TournamentTeamResult> list11 = (List<TournamentTeamResult>)r;
						int num3 = 0;
						if (response.Parameters.ContainsKey(29))
						{
							num3 = (int)response.Parameters[29];
						}
						if (this.OnGotIntermediateTournamentTeamResult != null)
						{
							this.OnGotIntermediateTournamentTeamResult(list11, num3);
						}
					}
				});
			}
			break;
		case 20:
			if (response.ReturnCode == 0 && this.OnGotFinalTournamentTeamResult != null)
			{
				InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
				inputByteArrayWorkItem.Data = (byte[])response.Parameters[10];
				inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
				{
					string text4 = CompressHelper.DecompressString(p);
					return JsonConvert.DeserializeObject<List<TournamentTeamResult>>(text4, SerializationHelper.JsonSerializerSettings);
				};
				inputByteArrayWorkItem.ResultAction = delegate(object r)
				{
					List<TournamentTeamResult> list12 = (List<TournamentTeamResult>)r;
					int num4 = 0;
					if (response.Parameters.ContainsKey(29))
					{
						num4 = (int)response.Parameters[29];
					}
					if (this.OnGotFinalTournamentTeamResult != null)
					{
						this.OnGotFinalTournamentTeamResult(list12, num4);
					}
				};
				AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
			}
			break;
		case 21:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotTournamentSerieStats != null)
				{
					byte[] array = (byte[])response.Parameters[10];
					string json = CompressHelper.DecompressString(array);
					List<TournamentSerieStat> list6 = JsonConvert.DeserializeObject<List<TournamentSerieStat>>(json, SerializationHelper.JsonSerializerSettings);
					this.OnGotTournamentSerieStats(list6);
				}
			}
			else if (this.OnGettingTournamentSerieStatsFailed != null)
			{
				this.OnGettingTournamentSerieStatsFailed(failure);
			}
			break;
		case 22:
			if (response.ReturnCode == 0)
			{
				if (this.OnUnregisteredFromTournament != null)
				{
					this.OnUnregisteredFromTournament();
				}
			}
			else if (this.OnUnregisterFromTournamentFailed != null)
			{
				this.OnUnregisterFromTournamentFailed(failure);
			}
			break;
		case 23:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotPlayerTournaments != null)
				{
					byte[] array = (byte[])response.Parameters[10];
					string json = CompressHelper.DecompressString(array);
					List<Tournament> list = JsonConvert.DeserializeObject<List<Tournament>>(json, SerializationHelper.JsonSerializerSettings);
					this.OnGotPlayerTournaments(list);
				}
			}
			else if (this.OnGettingPlayerTournamentsFailed != null)
			{
				this.OnGettingPlayerTournamentsFailed(failure);
			}
			break;
		}
	}

	private void HandleSysOperationResponse(OperationResponse response)
	{
		Failure failure = null;
		if (response.ReturnCode != 0)
		{
			try
			{
				failure = new SysOpFailure(response);
				Debug.LogError(failure.FullErrorInfo);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Error logging SysOperation error: {0}", new object[] { ex.Message });
				Debug.LogErrorFormat("Original error: {0}", new object[] { response.DebugMessage });
			}
		}
		SysSubOperationCode sysSubOperationCode = (SysSubOperationCode)response[1];
		OpTimer.End(167, sysSubOperationCode);
		switch (sysSubOperationCode)
		{
		case 1:
			if (response.ReturnCode == 0)
			{
				if (this.profile != null)
				{
					this.profile.LanguageId = this.languageIdToSet;
				}
				if (this.OnSetAddPropsComplete != null)
				{
					this.OnSetAddPropsComplete();
				}
				if (response.Parameters.ContainsKey(88))
				{
					response[1] = 250;
					response[250] = this.UserId;
					OpTimer.Start(200, 250);
					this.HandleProfileOperationResponse(response);
				}
			}
			else if (this.OnSetAddPropsFailed != null)
			{
				this.OnSetAddPropsFailed(failure);
			}
			break;
		case 2:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotWhatsNewList != null && response.Parameters.ContainsKey(192))
				{
					InputByteArrayWorkItem inputByteArrayWorkItem = new InputByteArrayWorkItem();
					inputByteArrayWorkItem.Data = (byte[])response.Parameters[192];
					inputByteArrayWorkItem.ProcessAction = delegate(byte[] p)
					{
						string text3 = CompressHelper.DecompressString(p);
						return JsonConvert.DeserializeObject<List<WhatsNewItem>>(text3, SerializationHelper.JsonSerializerSettings);
					};
					inputByteArrayWorkItem.ResultAction = delegate(object r)
					{
						List<WhatsNewItem> list = (List<WhatsNewItem>)r;
						if (this.OnGotWhatsNewList != null)
						{
							this.OnGotWhatsNewList(list);
						}
					};
					AsynchProcessor.ProcessByteArrayWorkItem(inputByteArrayWorkItem);
				}
			}
			else if (this.OnGettingWhatsNewListFailed != null)
			{
				this.OnGettingWhatsNewListFailed(failure);
			}
			break;
		case 4:
			if (response.ReturnCode == 0)
			{
				if (this.OnGotCurrentEvent != null)
				{
					if (response.Parameters.ContainsKey(192))
					{
						byte[] array = (byte[])response.Parameters[192];
						string text = CompressHelper.DecompressString(array);
						MarketingEvent marketingEvent = JsonConvert.DeserializeObject<MarketingEvent>(text, SerializationHelper.JsonSerializerSettings);
						this.OnGotCurrentEvent(marketingEvent);
					}
					else
					{
						this.OnGotCurrentEvent(null);
					}
				}
			}
			else if (this.OnGettingCurrentEventFailed != null)
			{
				this.OnGettingCurrentEventFailed(failure);
			}
			break;
		case 5:
			if (response.ReturnCode == 0)
			{
				byte[] array2 = (byte[])response.Parameters[10];
				string text2 = CompressHelper.DecompressString(array2);
				EulaInfo eulaInfo = JsonConvert.DeserializeObject<EulaInfo>(text2, SerializationHelper.JsonSerializerSettings);
				if (this.OnGotLatestEula != null)
				{
					this.OnGotLatestEula(eulaInfo);
				}
			}
			else if (this.OnGettingLatestEulaFailed != null)
			{
				this.OnGettingLatestEulaFailed(failure);
			}
			break;
		case 6:
			if (response.ReturnCode == 0)
			{
				if (this.CheckProfile())
				{
					this.Profile.IsLatestEulaSigned = true;
				}
				if (this.OnEulaSigned != null)
				{
					this.OnEulaSigned();
				}
			}
			else if (this.OnEulaSignFailed != null)
			{
				this.OnEulaSignFailed(failure);
			}
			break;
		case 7:
			if (response.ReturnCode == 0)
			{
				if (this.OnSupportTicketCreated != null)
				{
					this.OnSupportTicketCreated();
				}
			}
			else if (this.OnCreateSupportTicketFailed != null)
			{
				this.OnCreateSupportTicketFailed(failure);
			}
			break;
		case 8:
			if (response.ReturnCode == 0)
			{
				Hashtable hashtable = (Hashtable)response.Parameters[188];
				Dictionary<int, bool> dictionary = hashtable.ToDictionary((KeyValuePair<object, object> i) => (int)i.Key, (KeyValuePair<object, object> i) => (bool)i.Value);
				if (this.OnGotAbTestSelection != null)
				{
					this.OnGotAbTestSelection(dictionary);
				}
			}
			else if (this.OnGettingAbTestSelectionFailed != null)
			{
				this.OnGettingAbTestSelectionFailed(failure);
			}
			break;
		case 10:
			if (response.ReturnCode == 0)
			{
				if (this.OnWhatsNewShown != null)
				{
					this.OnWhatsNewShown();
				}
			}
			else if (this.OnWhatsNewShownFailed != null)
			{
				this.OnWhatsNewShownFailed(failure);
			}
			break;
		case 11:
			if (response.ReturnCode == 0)
			{
				if (this.OnWhatsNewClicked != null)
				{
					this.OnWhatsNewClicked();
				}
			}
			else if (this.OnWhatsNewClickedFailed != null)
			{
				this.OnWhatsNewClickedFailed(failure);
			}
			break;
		}
	}

	private void HandlePremiumShopOperationResponse(OperationResponse response)
	{
		PremiumShopSubOperationCode premiumShopSubOperationCode = (PremiumShopSubOperationCode)response[191];
		OpTimer.End(136, premiumShopSubOperationCode);
		if (premiumShopSubOperationCode != null)
		{
			if (premiumShopSubOperationCode == 1)
			{
				if (response.ReturnCode == 0)
				{
					int num = (int)response.Parameters[194];
					if (this.OnCompleteOffer != null)
					{
						this.OnCompleteOffer(num);
					}
				}
				else if (this.OnCompleteOfferFailed != null)
				{
					this.OnCompleteOfferFailed(new Failure(response));
				}
			}
		}
		else if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[192];
			string text = CompressHelper.DecompressString(array);
			List<OfferClient> list = JsonConvert.DeserializeObject<List<OfferClient>>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnGotOffers != null)
			{
				this.OnGotOffers(list);
			}
		}
		else if (this.OnGetOffersFailed != null)
		{
			this.OnGetOffersFailed(new Failure(response));
		}
	}

	public int PreviewCurrencyExchange(int amount)
	{
		return (int)Math.Round((double)((float)amount * Inventory.ExchangeRate()));
	}

	public void ExchangeCurrency(int amount)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(144);
		operationRequest.Parameters[183] = amount;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(144);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnCurrencyExchanged OnCurrencyExchanged;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnCurrencyExchangeFailed;

	private void HandleExchangeCurrencyResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			SysOpFailure sysOpFailure = new SysOpFailure(response);
			Debug.LogError(sysOpFailure.FullErrorInfo);
			if (this.OnCurrencyExchangeFailed != null)
			{
				this.OnCurrencyExchangeFailed(sysOpFailure);
			}
			return;
		}
		int num = (int)response.Parameters[183];
		int num2 = (int)response.Parameters[194];
		this.Profile.IncrementBalance("GC", (double)(-(double)num));
		this.Profile.IncrementBalance("SC", (double)num2);
		if (this.OnCurrencyExchanged != null)
		{
			this.OnCurrencyExchanged(num, num2);
		}
	}

	public void GetPondUnlocks()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(139);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(139);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotPondUnlocks OnGotPondUnlocks;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGettingPondUnlocksFailed;

	private void HandleGetPondUnlocksResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			SysOpFailure sysOpFailure = new SysOpFailure(response);
			Debug.LogError(sysOpFailure.FullErrorInfo);
			if (this.OnGettingPondUnlocksFailed != null)
			{
				this.OnGettingPondUnlocksFailed(sysOpFailure);
			}
			return;
		}
		if (this.OnGotPondUnlocks != null)
		{
			byte[] array = (byte[])response.Parameters[192];
			List<PondUnlockInfo> list = JsonConvert.DeserializeObject<List<PondUnlockInfo>>(CompressHelper.DecompressString(array));
			this.OnGotPondUnlocks(list);
		}
	}

	public void UnlockPond(int pondId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(138);
		operationRequest.Parameters[184] = pondId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(138);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnPondUnlocked OnPondUnlocked;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnPondUnlockFailed;

	private void HandleUnlockPondResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			SysOpFailure sysOpFailure = new SysOpFailure(response);
			Debug.LogError(sysOpFailure.FullErrorInfo);
			if (this.OnPondUnlockFailed != null)
			{
				this.OnPondUnlockFailed(sysOpFailure);
			}
			return;
		}
		int num = (int)response.Parameters[184];
		int num2 = (int)response.Parameters[169];
		int num3 = (int)response.Parameters[183];
		string text = (string)response.Parameters[192];
		LevelLockRemoval levelLockRemoval = JsonConvert.DeserializeObject<LevelLockRemoval>(text, SerializationHelper.JsonSerializerSettings);
		this.Profile.IncrementBalance("GC", (double)(-(double)num3));
		if (this.pondInfos.ContainsKey(num))
		{
			this.pondInfos[num].MinLevel = 1;
		}
		if (this.Profile.LevelLockRemovals == null)
		{
			this.Profile.LevelLockRemovals = new List<LevelLockRemoval>();
		}
		this.Profile.LevelLockRemovals.RemoveAll((LevelLockRemoval r) => r.EndDate == null && r.AccessibleLevel != null);
		this.Profile.LevelLockRemovals.Add(levelLockRemoval);
		if (this.OnPondUnlocked != null)
		{
			this.OnPondUnlocked(num, num2);
		}
	}

	public void GetReferralReward()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(135);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(135);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGotReferralReward OnGotReferralReward;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailure OnGetingReferralRewardFailed;

	private void HandleGetReferralRewardsResponse(OperationResponse response)
	{
		if (response.ReturnCode != 0)
		{
			SysOpFailure sysOpFailure = new SysOpFailure(response);
			Debug.LogError(sysOpFailure.FullErrorInfo);
			if (this.OnGetingReferralRewardFailed != null)
			{
				this.OnGetingReferralRewardFailed(sysOpFailure);
			}
			return;
		}
		if (this.OnGotReferralReward != null)
		{
			byte[] array = (byte[])response.Parameters[192];
			string text = CompressHelper.DecompressString(array);
			ReferralReward referralReward = JsonConvert.DeserializeObject<ReferralReward>(text);
			this.OnGotReferralReward(referralReward);
		}
	}

	private void HandleChatMessageEvent(EventData eventData)
	{
		string text = (string)eventData.Parameters[200];
		if (this.IsPlayerIgnored(text))
		{
			return;
		}
		object obj;
		eventData.Parameters.TryGetValue(160, out obj);
		object obj2;
		eventData.Parameters.TryGetValue(147, out obj2);
		object obj3;
		eventData.Parameters.TryGetValue(145, out obj3);
		DateTime dateTime = new DateTime((long)eventData.Parameters[181]);
		string text2 = (string)eventData.Parameters[199];
		string text3 = (string)eventData.Parameters[198];
		object obj4;
		eventData.Parameters.TryGetValue(196, out obj4);
		object obj5;
		eventData.Parameters.TryGetValue(162, out obj5);
		object obj6;
		eventData.Parameters.TryGetValue(197, out obj6);
		object obj7;
		eventData.Parameters.TryGetValue(195, out obj7);
		object obj8;
		eventData.Parameters.TryGetValue(175, out obj8);
		object obj9;
		eventData.Parameters.TryGetValue(144, out obj9);
		ChatMessage chatMessage = new ChatMessage();
		chatMessage.Timestamp = new DateTime?(dateTime);
		ChatMessage chatMessage2 = chatMessage;
		Player player = new Player();
		player.UserId = text;
		player.UserName = text2;
		Player player2 = player;
		int? num = (int?)obj;
		player2.Level = ((num == null) ? 0 : num.Value);
		Player player3 = player;
		int? num2 = (int?)obj2;
		player3.Rank = ((num2 == null) ? 0 : num2.Value);
		player.ExternalId = (string)obj3;
		chatMessage2.Sender = player;
		chatMessage.Recepient = new Player
		{
			UserId = text3
		};
		chatMessage.Message = (string)obj4;
		chatMessage.Channel = ((obj5 != null) ? ((string)obj5) : "Private");
		chatMessage.Group = (string)obj6;
		chatMessage.Data = (string)obj7;
		chatMessage.OneTimeData = (string)obj9;
		chatMessage.Id = (string)obj8;
		ChatMessage chatMessage3 = chatMessage;
		if (chatMessage3.Id != null && string.IsNullOrEmpty((string)obj5))
		{
			this.SendChatMessageConfirmation(chatMessage3.Id);
		}
		if (chatMessage3.IsRequest)
		{
			if (chatMessage3.Data == "AddToFriendsConfirmed")
			{
				Player player4 = this.InternalAddFriend(chatMessage3.Sender);
				if (this.OnFriendshipAdded != null)
				{
					this.OnFriendshipAdded(player4);
				}
			}
			else if (chatMessage3.Data == "AddToFriendsDenied")
			{
				Player player5 = this.InternalRemoveFriend(chatMessage3.Sender.UserId, true);
				if (this.OnFriendshipRemoved != null)
				{
					this.OnFriendshipRemoved(player5);
				}
			}
			else
			{
				char requestType = chatMessage3.RequestType;
				switch (requestType)
				{
				case 'E':
					break;
				case 'F':
					chatMessage3.Sender.Status = FriendStatus.FriendshipRequest;
					SerializationHelper.DeserializeFriend(chatMessage3.Sender, chatMessage3.Data);
					this.InternalAddFriend(chatMessage3.Sender);
					if (this.OnFriendshipRequest != null)
					{
						this.OnFriendshipRequest(chatMessage3.Sender);
					}
					goto IL_70A;
				case 'G':
				{
					string text4;
					string text5;
					string text6;
					if (SerializationHelper.DeserializeGift(chatMessage3.Data, out text4, out text5, out text6))
					{
						Inventory inventory = JsonConvert.DeserializeObject<Inventory>(text6, SerializationHelper.JsonSerializerSettings);
						if (this.CheckProfile())
						{
							for (int i = 0; i < inventory.Count; i++)
							{
								InventoryItem inventoryItem = inventory[i];
								this.Profile.Inventory.JoinItem(inventoryItem, null);
							}
						}
						if (this.OnItemGifted != null)
						{
							Action<Player, IEnumerable<InventoryItem>> onItemGifted = this.OnItemGifted;
							player = new Player();
							player.UserId = text4;
							player.UserName = text5;
							Player player6 = player;
							int? num3 = (int?)obj;
							player6.Level = ((num3 == null) ? 0 : num3.Value);
							onItemGifted(player, inventory);
						}
						this.RaiseInventoryUpdated(false);
					}
					goto IL_70A;
				}
				default:
					switch (requestType)
					{
					case 'b':
						if (this.Profile != null)
						{
							if (chatMessage3.Data.Length > 1 && chatMessage3.Data[1] == 'Y')
							{
								this.Profile.ChatBanEndDate = new DateTime?(DateSerializer.ToDateTime(chatMessage3.Data.Substring(2)));
							}
							else
							{
								this.Profile.ChatBanEndDate = null;
							}
						}
						goto IL_70A;
					default:
					{
						if (requestType == 'R')
						{
							TournamentFinalResult tournamentFinalResult;
							TournamentSerializationHelper.DeserializeTournamentResult(chatMessage3.Data, out tournamentFinalResult);
							List<Reward> list = new List<Reward>();
							list.Add(tournamentFinalResult.CurrentPlayerResult.Reward);
							if (tournamentFinalResult.SecondaryWinners != null)
							{
								list.AddRange(from sw in tournamentFinalResult.SecondaryWinners
									where sw.Id == this.UserId
									select sw.SecondaryReward);
							}
							if (this.profile != null)
							{
								this.profile.ApplyTournamentResult(tournamentFinalResult.CurrentPlayerResult.TitleGiven, tournamentFinalResult.CurrentPlayerResult.TitleProlonged, list, tournamentFinalResult.KindId, tournamentFinalResult.CurrentPlayerResult.Rating);
							}
							if (this.OnTournamentResult != null)
							{
								this.OnTournamentResult(tournamentFinalResult);
							}
							goto IL_70A;
						}
						if (requestType != 'S')
						{
							goto IL_70A;
						}
						string[] array = chatMessage3.Data.Split(new char[] { ',' });
						if (array.Length != 6)
						{
							throw new InvalidOperationException("Unrecognized data in TournamentStarted message: " + chatMessage3.Data);
						}
						TournamentStartInfo tournamentStartInfo = new TournamentStartInfo
						{
							TournamentId = int.Parse(array[1]),
							ImageBID = int.Parse(array[2]),
							Name = array[3],
							PondId = int.Parse(array[4]),
							EndDate = DateSerializer.ToDateTime(array[5])
						};
						if (this.OnTournamentStarted != null)
						{
							this.OnTournamentStarted(tournamentStartInfo);
						}
						goto IL_70A;
					}
					case 'e':
						break;
					case 'f':
						if (this.Profile != null)
						{
							int? num4 = SerializationHelper.DeserializeProfileFlag(chatMessage3.Data);
							this.Profile.Flags = ((num4 != null) ? ProfileFlags.AddFlag(this.Profile.Flags, num4.Value) : 0);
							Debug.Log("Profile Flags changed to " + this.Profile.Flags);
						}
						goto IL_70A;
					}
					break;
				}
				this.InternalRemoveFriend(chatMessage3.Sender.UserId, true);
				if (this.OnFriendshipRemoved != null)
				{
					this.OnFriendshipRemoved(chatMessage3.Sender);
				}
			}
			IL_70A:;
		}
		else if (chatMessage3.IsAnswer)
		{
			char answerType = chatMessage3.AnswerType;
			if (answerType == 'F')
			{
				if (this.profile != null)
				{
					this.InternalAddFriend(chatMessage3.Sender);
				}
				if (this.OnFriendshipAdded != null)
				{
					this.OnFriendshipAdded(chatMessage3.Sender);
				}
			}
		}
		else
		{
			this.InternalHandleChatMessage(chatMessage3);
		}
	}

	private bool IsPlayerIgnored(string userId)
	{
		return this.profile != null && this.profile.Friends != null && this.profile.Friends.Any((Player p) => p.UserId.Equals(userId, StringComparison.InvariantCultureIgnoreCase) && p.Status == FriendStatus.Ignore);
	}

	private Player InternalAddFriend(Player player)
	{
		if (this.profile == null)
		{
			return null;
		}
		if (this.profile.Friends == null)
		{
			this.profile.Friends = new List<Player>();
		}
		Player player2 = this.Room.FindPlayer(player.UserId);
		if (player2 != null)
		{
			player2.RequestTime = null;
			player2.Status = FriendStatus.Friend;
		}
		Player player3 = this.profile.Friends.FirstOrDefault((Player f) => f.UserId.Equals(player.UserId, StringComparison.InvariantCultureIgnoreCase));
		if (player3 == null)
		{
			this.profile.Friends.Add(player);
			return player;
		}
		if (player3.Status == FriendStatus.Friend && (player.Status == FriendStatus.FriendshipRequest || player.Status == FriendStatus.FriendshipRequested || player.Status == FriendStatus.Friend))
		{
			return player3;
		}
		if (player3.Status == FriendStatus.Ignore && player.Status == FriendStatus.FriendshipRequest)
		{
			return player3;
		}
		player3.Status = player.Status;
		return player3;
	}

	private Player InternalRemoveFriend(string userId, bool removeIfNotIgnored = false)
	{
		if (this.profile == null || this.profile.Friends == null)
		{
			return null;
		}
		Player player = this.Room.FindPlayer(userId);
		if (player != null)
		{
			player.RequestTime = null;
			player.Status = FriendStatus.None;
		}
		Player player2 = this.profile.Friends.FirstOrDefault((Player f) => f.UserId == userId);
		if (player2 == null)
		{
			return null;
		}
		if (removeIfNotIgnored && player2.Status == FriendStatus.Ignore)
		{
			return null;
		}
		this.profile.Friends.Remove(player2);
		return player2;
	}

	private void InternalRemoveFriendByName(string username)
	{
		if (this.profile == null || this.profile.Friends == null)
		{
			return;
		}
		Player player = this.profile.Friends.FirstOrDefault((Player f) => f.UserName == username);
		if (player != null)
		{
			this.profile.Friends.Remove(player);
		}
	}

	private void HandleLocalChatMessage(string sender, string senderName, string externalId, int level, DateTime timestamp, Hashtable eventData, int rank)
	{
		string text = null;
		if (this.profile != null)
		{
			text = this.profile.UserId.ToString();
		}
		string text2 = (string)eventData["m"];
		ChatMessage chatMessage = new ChatMessage
		{
			Sender = new Player
			{
				UserId = sender,
				UserName = senderName,
				Rank = rank,
				Level = level,
				ExternalId = externalId
			},
			Recepient = new Player
			{
				UserId = text
			},
			Timestamp = new DateTime?(timestamp),
			Message = text2,
			Channel = "Local"
		};
		this.InternalHandleChatMessage(chatMessage);
	}

	private void HandleJoinEvent(EventData eventData)
	{
		string text = (string)eventData.Parameters[225];
		object obj;
		eventData.Parameters.TryGetValue(145, out obj);
		if (this.IsPlayerIgnored(text))
		{
			return;
		}
		if (this.UserId == text || this.CurrentPondId == null || !this.IsInRoom)
		{
			return;
		}
		string text2 = (string)eventData.Parameters[211];
		Player player = new Player
		{
			UserId = text,
			UserName = text2,
			IsOnline = true,
			ExternalId = (string)obj
		};
		this.ParceSingleActorProperties(eventData, player);
		this.Room.Add(player);
		if (this.OnCharacterCreated != null)
		{
			this.OnCharacterCreated(player);
		}
		if (this.OnLocalEvent != null)
		{
			this.OnLocalEvent(new LocalEvent
			{
				EventType = LocalEventType.Join,
				Player = player
			});
		}
	}

	private void HandleLeaveEvent(EventData eventData)
	{
		string text = (string)eventData.Parameters[225];
		object obj;
		eventData.Parameters.TryGetValue(145, out obj);
		if (this.IsPlayerIgnored(text))
		{
			return;
		}
		string text2 = (string)eventData.Parameters[211];
		if (this.UserId == text || this.CurrentPondId == null)
		{
			return;
		}
		Player player = new Player
		{
			UserId = text,
			UserName = text2,
			ExternalId = (string)obj
		};
		this.ParceSingleActorProperties(eventData, player);
		this.Room.Remove(player);
		if (this.OnCharacterDestroyed != null)
		{
			this.OnCharacterDestroyed(player);
		}
		if (this.OnLocalEvent != null)
		{
			this.OnLocalEvent(new LocalEvent
			{
				EventType = LocalEventType.Leave,
				Player = player
			});
		}
	}

	private void ParceSingleActorProperties(EventData eventData, Player player)
	{
		if (eventData.Parameters.ContainsKey(249))
		{
			Hashtable hashtable = (Hashtable)eventData.Parameters[249];
			this.ParceActorProps(player, hashtable);
		}
	}

	private void ParceMultipleActorsProperties(OperationResponse response)
	{
		if (response.Parameters.ContainsKey(249))
		{
			Hashtable hashtable = (Hashtable)response.Parameters[249];
			foreach (object obj in hashtable.Keys)
			{
				Hashtable hashtable2 = (Hashtable)hashtable[obj];
				Player player = new Player();
				this.ParceActorProps(player, hashtable2);
				this.Room.Add(player);
			}
		}
	}

	private void ParceActorProps(Player player, Hashtable actorProperties)
	{
		if (actorProperties.ContainsKey(225))
		{
			player.UserId = (string)actorProperties[225];
		}
		if (actorProperties.ContainsKey(211))
		{
			player.UserName = (string)actorProperties[211];
		}
		if (actorProperties.ContainsKey(250))
		{
			player.AvatarBID = new int?((int)actorProperties[250]);
		}
		if (actorProperties.ContainsKey(240))
		{
			player.AvatarUrl = (string)actorProperties[240];
		}
		if (actorProperties.ContainsKey(249))
		{
			player.Level = (int)actorProperties[249];
		}
		if (actorProperties.ContainsKey(242))
		{
			player.Rank = (int)actorProperties[242];
		}
		if (actorProperties.ContainsKey(248))
		{
			object obj = actorProperties[248];
			if (obj != null)
			{
				player.PondId = new int?((int)obj);
			}
		}
		if (actorProperties.ContainsKey(247))
		{
			player.HasPremium = (bool)actorProperties[247];
		}
		if (actorProperties.ContainsKey(246))
		{
			player.Gender = (string)actorProperties[246];
		}
		if (actorProperties.ContainsKey(245))
		{
			player.Source = (string)actorProperties[245];
		}
		if (actorProperties.ContainsKey(244))
		{
			player.ExternalId = (string)actorProperties[244];
		}
		if (actorProperties.ContainsKey(243))
		{
			player.RoomId = (string)actorProperties[243];
		}
		if (this.CheckProfile() && this.profile.Friends != null)
		{
			Player player2 = this.Profile.Friends.FirstOrDefault((Player f) => f.UserId == player.UserId);
			if (player2 != null)
			{
				player.Status = player2.Status;
				player.RequestTime = player2.RequestTime;
			}
			else
			{
				player.Status = FriendStatus.None;
			}
		}
		else
		{
			player.Status = FriendStatus.None;
		}
		Hashtable hashtable = ((!actorProperties.ContainsKey(241)) ? new Hashtable() : ((Hashtable)actorProperties[241]));
		string text = ((!actorProperties.ContainsKey(102)) ? null : ((string)actorProperties[102]));
		string text2 = ((!actorProperties.ContainsKey(103)) ? null : ((string)actorProperties[103]));
		string text3 = ((!actorProperties.ContainsKey(117)) ? null : ((string)actorProperties[117]));
		player.TpmCharacterModel = new TPMCharacterModel((!string.IsNullOrEmpty(text)) ? int.Parse(text) : 0, (!string.IsNullOrEmpty(text2)) ? int.Parse(text2) : 0, (!string.IsNullOrEmpty(text3)) ? int.Parse(text3) : 0, hashtable, actorProperties);
	}

	private void HandleLocalEvent(EventData eventData)
	{
		try
		{
			string text = (string)eventData.Parameters[225];
			if (!this.IsPlayerIgnored(text))
			{
				string text2 = (string)eventData.Parameters[211];
				int num = (int)eventData.Parameters[169];
				int num2 = (int)eventData.Parameters[147];
				object obj;
				eventData.Parameters.TryGetValue(145, out obj);
				DateTime dateTime = new DateTime((long)eventData.Parameters[181]);
				LocalEventType localEventType = (LocalEventType)((byte)eventData.Parameters[194]);
				LocalEvent localEvent = new LocalEvent
				{
					EventType = localEventType,
					Player = new Player
					{
						UserId = text,
						UserName = text2,
						Level = num,
						ExternalId = (string)obj,
						Rank = num2
					}
				};
				Hashtable hashtable = (Hashtable)eventData.Parameters[245];
				switch (localEventType)
				{
				case LocalEventType.FishCaught:
				{
					string text3 = (string)hashtable["fish"];
					localEvent.Fish = JsonConvert.DeserializeObject<global::ObjectModel.Fish>(text3, SerializationHelper.JsonSerializerSettings);
					break;
				}
				case LocalEventType.TackleBroken:
					localEvent.TackleType = (BrokenTackleType)((byte)hashtable["t"]);
					break;
				case LocalEventType.Level:
					if (hashtable.ContainsKey("l"))
					{
						localEvent.Level = (int)hashtable["l"];
						this.UpdateRoomPlayerLevel(text, localEvent.Level);
					}
					if (hashtable.ContainsKey("r"))
					{
						localEvent.Rank = (int)hashtable["r"];
					}
					break;
				case LocalEventType.Achivement:
					localEvent.Achivement = (string)hashtable["a"];
					localEvent.AchivementStage = (string)hashtable["s"];
					break;
				case LocalEventType.Chat:
					if (!string.Equals(text, this.UserId, StringComparison.CurrentCultureIgnoreCase))
					{
						this.HandleLocalChatMessage(text, text2, (string)obj, num, dateTime, hashtable, num2);
					}
					break;
				case LocalEventType.FireworkLaunched:
					localEvent.ItemId = (int)hashtable["i"];
					localEvent.Position = new Vector3((float)hashtable["x"], (float)hashtable["y"], (float)hashtable["z"]);
					break;
				case LocalEventType.BuoyShared:
				{
					string text4 = (string)hashtable["buoy"];
					localEvent.Buoy = JsonConvert.DeserializeObject<BuoySetting>(text4, SerializationHelper.JsonSerializerSettings);
					localEvent.Receiver = (string)hashtable["receiver"];
					break;
				}
				case LocalEventType.ItemCaught:
				{
					localEvent.ItemId = (int)hashtable["i"];
					string text5 = (string)hashtable["item"];
					localEvent.CaughtItem = JsonConvert.DeserializeObject<InventoryItem>(text5, SerializationHelper.JsonSerializerSettings);
					break;
				}
				case LocalEventType.MissionCompleted:
				{
					string text6 = (string)hashtable["mission"];
					MissionOnClient missionOnClient = JsonConvert.DeserializeObject<MissionOnClient>(text6, SerializationHelper.JsonSerializerSettings);
					localEvent.Quest = missionOnClient.Name;
					localEvent.QuestStage = ((missionOnClient.Group == null) ? "Misc." : missionOnClient.Group.Name);
					break;
				}
				}
				if (this.OnLocalEvent != null)
				{
					this.OnLocalEvent(localEvent);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Error processing local event");
			Debug.LogException(ex);
		}
	}

	private void UpdateRoomPlayerLevel(string userId, int level)
	{
		foreach (Player player in this.Room.Players)
		{
			if (player.UserId == userId)
			{
				player.Level = level;
			}
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnLocalEvent OnLocalEvent;

	public string SteamAuthTicket { get; set; }

	public void BackupSteamAuthTicket()
	{
		this.steamAuthTicketBackup = this.SteamAuthTicket;
		this.SteamAuthTicket = null;
	}

	private void RestoreSteamAuthTicket()
	{
		if (!string.IsNullOrEmpty(this.steamAuthTicketBackup))
		{
			this.SteamAuthTicket = this.steamAuthTicketBackup;
		}
		this.steamAuthTicketBackup = null;
	}

	private void SaveSteamSecondaryAuth(Dictionary<byte, object> responseParameters)
	{
		if (responseParameters.ContainsKey(242))
		{
			string text = (string)responseParameters[242];
			if (text == "Steam")
			{
				string text2 = (string)responseParameters[248];
				string text3 = (string)responseParameters[247];
				string text4 = StaticUserData.SteamId.ToString();
				ObscuredPrefs.SetString("Email" + text4, text2);
				ObscuredPrefs.SetString("Password" + text4, text3);
			}
		}
	}

	private static OperationRequest CreateOperationRequest(byte operationCode)
	{
		return new OperationRequest
		{
			OperationCode = operationCode,
			Parameters = new Dictionary<byte, object>()
		};
	}

	private void DisconnectToReconnect(string serverAddress, string roomId, string gameAction)
	{
		DebugUtility.Connection.Trace("DisconnectToReconnect requested! Action: " + gameAction + " Room: " + roomId, new object[0]);
		this.cachedServerAddress = serverAddress;
		this.Room.RoomId = roomId;
		DebugUtility.RoomIdIssue.Trace("Set RoomId = {0}", new object[] { roomId });
		this.cachedGameAction = gameAction;
		this.IsAuthenticated = false;
		if (GameFactory.Player != null)
		{
			GameFactory.Player.StartMoveToNewLocation();
		}
		this.PreviewDisconnect();
	}

	private void PreviewDisconnect()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(166);
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(166);
	}

	private void DoOnPreviewDisconnectComplete()
	{
		string roomId = this.Room.RoomId;
		string text = this.cachedGameAction;
		this.isDisconnectToReconnect = true;
		if (this.Peer != null && this.Peer.PeerState != null && this.Peer.PeerState != 4)
		{
			this.Peer.Disconnect();
			this.Peer.SendOutgoingCommands();
		}
		this.Peer = new LoadbalancingPeer(this, this.Protocol);
		this.SetupPeer();
		OpTimer.Reset();
		this.Peer.Connect(this.cachedServerAddress, this.AppName);
		DebugUtility.Connection.Trace("DoOnPreviewDisconnectComplete - Connection to server (" + this.cachedServerAddress + ") requested", new object[0]);
		this.cachedServerAddress = null;
		this.Room.RoomId = roomId;
		DebugUtility.RoomIdIssue.Trace("Set RoomId = {0}", new object[] { roomId });
		this.cachedGameAction = text;
		DebugUtility.Connection.Trace("Preview disconnect complete. cachedGameAction=" + this.cachedGameAction, new object[0]);
		this.IsMessageQueueRunning = true;
	}

	private void PreviewQuit()
	{
		if (!this.IsAuthenticated)
		{
			if (this.OnPreviewQuitComplete != null)
			{
				this.OnPreviewQuitComplete();
			}
			return;
		}
		if (this.CheckProfile())
		{
			AsynchProcessor.ProcessByteWorkItem(new InputByteWorkItem
			{
				Data = 200,
				ProcessAction = delegate(byte p)
				{
					OperationRequest operationRequest2 = PhotonServerConnection.CreateOperationRequest(200);
					operationRequest2.Parameters = ProfileSerializationHelper.Serialize(this.Profile);
					operationRequest2.Parameters[1] = 233;
					return operationRequest2;
				},
				ResultAction = delegate(object r)
				{
					OperationRequest operationRequest3 = (OperationRequest)r;
					this.Peer.OpCustom(operationRequest3, true, 0, true);
					OpTimer.Start(200, 233);
					Debug.LogWarning("PreviewQuit sent!");
				}
			});
		}
		else
		{
			OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(200);
			operationRequest.Parameters[1] = 233;
			this.Peer.OpCustom(operationRequest, true, 0, true);
			OpTimer.Start(200, 233);
			Debug.LogWarning("PreviewQuit sent!");
		}
	}

	private Hashtable GetJoinRandomRoomOptions()
	{
		Hashtable hashtable = new Hashtable();
		int? num = this.tournamentToEnter;
		if (num == null)
		{
			hashtable["t"] = 0;
		}
		else
		{
			hashtable["t"] = 1;
			hashtable["ti"] = this.tournamentToEnter.Value;
		}
		if (this.isMovingToBase)
		{
			hashtable["pond"] = 0;
		}
		else
		{
			Hashtable hashtable2 = hashtable;
			object obj = "pond";
			int? num2 = this.pondToEnter;
			hashtable2[obj] = ((num2 == null) ? 0 : num2.Value);
		}
		if (this.profile != null)
		{
			hashtable["lng"] = PhotonServerConnection.JoinLanguagesIntoOneRoom(this.profile.LanguageId);
		}
		hashtable["p"] = 0;
		return hashtable;
	}

	private RoomOptions GetRoomOptions()
	{
		Hashtable hashtable = new Hashtable();
		int? num = this.tournamentToEnter;
		if (num == null)
		{
			hashtable["t"] = 0;
		}
		else
		{
			hashtable["t"] = 1;
			hashtable["ti"] = this.tournamentToEnter.Value;
		}
		if (this.isMovingToBase)
		{
			hashtable["pond"] = 0;
		}
		else
		{
			Hashtable hashtable2 = hashtable;
			object obj = "pond";
			int? num2 = this.pondToEnter;
			hashtable2[obj] = ((num2 == null) ? 0 : num2.Value);
		}
		hashtable["p"] = 0;
		string[] array;
		if (this.profile != null)
		{
			hashtable["lng"] = PhotonServerConnection.JoinLanguagesIntoOneRoom(this.profile.LanguageId);
			int? num3 = this.tournamentToEnter;
			if (num3 != null)
			{
				array = new string[] { "t", "ti", "pond", "lng", "p" };
			}
			else
			{
				array = new string[] { "t", "pond", "lng", "p" };
			}
		}
		else
		{
			int? num4 = this.tournamentToEnter;
			if (num4 != null)
			{
				array = new string[] { "t", "ti", "pond", "p" };
			}
			else
			{
				array = new string[] { "t", "pond", "p" };
			}
		}
		int? num5 = this.pondToEnter;
		int num6;
		if (num5 != null)
		{
			num6 = ((!this.isMovingToRoom) ? 20 : this.MaxRoomCapacity);
		}
		else
		{
			num6 = 20;
		}
		return new RoomOptions
		{
			customRoomProperties = hashtable,
			customRoomPropertiesForLobby = array,
			isOpen = true,
			isVisible = true,
			maxPlayers = num6
		};
	}

	private RoomOptions GetFriendsOnlyRoomOptions()
	{
		RoomOptions roomOptions = this.GetRoomOptions();
		roomOptions.customRoomProperties["p"] = 1;
		return roomOptions;
	}

	private RoomOptions GetPrivateRoomOptions()
	{
		Hashtable hashtable = new Hashtable();
		Hashtable hashtable2 = hashtable;
		object obj = "pond";
		int? num = this.pondToEnter;
		hashtable2[obj] = ((num == null) ? 0 : num.Value);
		return new RoomOptions
		{
			customRoomProperties = hashtable,
			isOpen = false,
			isVisible = false,
			maxPlayers = 1
		};
	}

	private static int JoinLanguagesIntoOneRoom(int languageId)
	{
		if (languageId == 3)
		{
			return 1;
		}
		return languageId;
	}

	private void ReAuthenticate()
	{
		if (this.isAuthenticating)
		{
			return;
		}
		this.isAuthenticating = true;
		this.Peer.OpAuthenticate(this.AppName, this.ProtocolVersion.ToString(CultureInfo.InvariantCulture), null, new AuthenticationValues
		{
			Secret = this.securityToken
		}, null);
		OpTimer.Start(230);
	}

	private void RefreshMainProfileValues(Dictionary<byte, object> parameters)
	{
		if (parameters == null)
		{
			return;
		}
		object obj;
		if (parameters.TryGetValue(186, out obj) && obj != null)
		{
			this.IsTempUser = new bool?((bool)obj);
		}
		object obj2;
		if (parameters.TryGetValue(248, out obj2) && obj2 != null)
		{
			this.Email = (string)obj2;
		}
		object obj3;
		if (parameters.TryGetValue(249, out obj3) && obj3 != null)
		{
			this.UserName = (string)obj3;
		}
	}

	private void ClearInternalState()
	{
		DebugUtility.Connection.Trace("ClearInternalState - State cleared", new object[0]);
		this.isDisconnectToReconnect = false;
		this.isAuthenticating = false;
		this.isAuthenticatingWithSteam = false;
		this.cachedEmail = null;
		this.cachedPassword = null;
		this.cachedGameAction = null;
		this.securityToken = null;
		this.IsMessageQueueRunning = false;
		this.ResetMovingState();
		this.IsAuthenticated = false;
		this.IsConnectedToGameServer = false;
		this.IsConnectedToMaster = false;
		this.IsOnBase = false;
		this.UserId = null;
		this.UserName = null;
		this.Room.RoomId = null;
		this.CurrentPondId = null;
		this.CurrentTournamentId = null;
		this.IsInRoom = false;
		this.Room.Clear();
		OpTimer.Reset();
	}

	private bool CheckIsAuthed()
	{
		if (this.IsAuthenticated)
		{
			return true;
		}
		Debug.LogError("Not Authenticated");
		return false;
	}

	private bool CheckGameConnectedAndAuthed()
	{
		if (!this.IsConnectedToGameServer || !this.IsAuthenticated)
		{
			Debug.LogError("Not connected to game server or not authenticated!");
			return false;
		}
		return true;
	}

	private bool CheckProfile()
	{
		if (this.profile == null)
		{
			Debug.LogError("Profile is not available!");
			return false;
		}
		return true;
	}

	private void InternalHandleChatMessage(ChatMessage message)
	{
		if (StaticChatChannels.IsStatic(message.Channel) && string.Equals(message.Sender.UserId, this.UserId, StringComparison.CurrentCultureIgnoreCase))
		{
			return;
		}
		message.Message = AbusiveWords.ReplaceAbusiveWords(message.Message);
		this.ProcessChatMessageInt(message);
	}

	private void ProcessChatMessageInt(ChatMessage message)
	{
		if (!StaticChatChannels.IsStatic(message.Channel))
		{
			if (this.OnIncomingChannelMessage != null)
			{
				this.OnIncomingChannelMessage(message);
			}
			return;
		}
		if (this.IsChatMessagingOn && this.internalChatMessageQueue.Count == 0)
		{
			if (this.OnIncomingChatMessage != null)
			{
				this.OnIncomingChatMessage(message);
			}
		}
		else
		{
			this.internalChatMessageQueue.Enqueue(message);
		}
	}

	public void DispatchChatMessages()
	{
		if (this.IsChatMessagingOn && this.internalChatMessageQueue.Count > 0)
		{
			if (this.OnIncomingChatMessage != null)
			{
				foreach (ChatMessage chatMessage in this.internalChatMessageQueue)
				{
					this.OnIncomingChatMessage(chatMessage);
				}
			}
			this.internalChatMessageQueue.Clear();
		}
	}

	private void DeserializeListAsync<T>(OperationResponse response, Action<IEnumerable<T>> callback, bool isTournament = false)
	{
		byte[] value = ((!isTournament) ? ((byte[])response.Parameters[192]) : ((byte[])response.Parameters[10]));
		AsynchProcessor.ProcessDictionaryWorkItem(new InputDictionaryWorkItem
		{
			Data = response.Parameters,
			ProcessAction = (Dictionary<byte, object> parameters) => JsonConvert.DeserializeObject<List<T>>(CompressHelper.DecompressString(value), SerializationHelper.JsonSerializerSettings),
			ResultAction = delegate(object r)
			{
				callback((IEnumerable<T>)r);
			}
		});
	}

	public void GetMetadataForCompetitionSearch()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 0;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 0);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGetMetadataForCompetitionSearch OnGetMetadataForCompetitionSearch;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureGetMetadataForCompetitionSearch;

	private void UGCHandleOperationResponse_GetMetadataForCompetitionSearch(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			MetadataForUserCompetitionSearch metadataForUserCompetitionSearch = JsonConvert.DeserializeObject<MetadataForUserCompetitionSearch>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnGetMetadataForCompetitionSearch != null)
			{
				this.OnGetMetadataForCompetitionSearch(metadataForUserCompetitionSearch);
			}
		}
		else if (this.OnFailureGetMetadataForCompetitionSearch != null)
		{
			this.OnFailureGetMetadataForCompetitionSearch(failure);
		}
	}

	public void GetMetadataForCompetition()
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 1;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 1);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGetMetadataForCompetition OnGetMetadataForCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureGetMetadataForCompetition;

	private void UGCHandleOperationResponse_GetMetadataForCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			MetadataForUserCompetition metadataForUserCompetition = JsonConvert.DeserializeObject<MetadataForUserCompetition>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnGetMetadataForCompetition != null)
			{
				this.OnGetMetadataForCompetition(metadataForUserCompetition);
			}
		}
		else if (this.OnFailureGetMetadataForCompetition != null)
		{
			this.OnFailureGetMetadataForCompetition(failure);
		}
	}

	public void GetMetadataForCompetitionPond(int pondId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 2;
		operationRequest.Parameters[22] = pondId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 2);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGetMetadataForCompetitionPond OnGetMetadataForCompetitionPond;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureGetMetadataForCompetitionPond;

	private void UGCHandleOperationResponse_GetMetadataForCompetitionPond(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			MetadataForUserCompetitionPond metadataForUserCompetitionPond = JsonConvert.DeserializeObject<MetadataForUserCompetitionPond>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnGetMetadataForCompetitionPond != null)
			{
				this.OnGetMetadataForCompetitionPond(metadataForUserCompetitionPond);
			}
		}
		else if (this.OnFailureGetMetadataForCompetitionPond != null)
		{
			this.OnFailureGetMetadataForCompetitionPond(failure);
		}
	}

	public void GetMetadataForCompetitionPondWeather(int pondId, int? templateId = null)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 3;
		operationRequest.Parameters[22] = pondId;
		if (templateId != null)
		{
			operationRequest.Parameters[35] = templateId;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 3);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGetMetadataForCompetitionPondWeather OnGetMetadataForCompetitionPondWeather;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureGetMetadataForCompetitionPondWeather;

	private void UGCHandleOperationResponse_GetMetadataForCompetitionPondWeather(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			MetadataForUserCompetitionPondWeather metadataForUserCompetitionPondWeather = JsonConvert.DeserializeObject<MetadataForUserCompetitionPondWeather>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnGetMetadataForCompetitionPondWeather != null)
			{
				this.OnGetMetadataForCompetitionPondWeather(metadataForUserCompetitionPondWeather);
			}
		}
		else if (this.OnFailureGetMetadataForCompetitionPondWeather != null)
		{
			this.OnFailureGetMetadataForCompetitionPondWeather(failure);
		}
	}

	public void GetMetadataForPredefinedCompetitionTemplate(int templateId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 4;
		operationRequest.Parameters[35] = templateId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 4);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGetMetadataForPredefinedCompetitionTemplate OnGetMetadataForPredefinedCompetitionTemplate;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureGetMetadataForPredefinedCompetitionTemplate;

	private void UGCHandleOperationResponse_GetMetadataForPredefinedCompetitionTemplate(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetition userCompetition = JsonConvert.DeserializeObject<UserCompetition>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnGetMetadataForPredefinedCompetitionTemplate != null)
			{
				this.OnGetMetadataForPredefinedCompetitionTemplate(userCompetition);
			}
		}
		else if (this.OnFailureGetMetadataForPredefinedCompetitionTemplate != null)
		{
			this.OnFailureGetMetadataForPredefinedCompetitionTemplate(failure);
		}
	}

	public void TryCreateCompetition(bool removeOtherRegistrations = false)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 28;
		operationRequest.Parameters[36] = removeOtherRegistrations;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 28);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnTryCreateCompetition OnTryCreateCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureTryCreateCompetition;

	private void UGCHandleOperationResponse_TryCreateCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnTryCreateCompetition != null)
			{
				this.OnTryCreateCompetition();
			}
		}
		else if (this.OnFailureTryCreateCompetition != null)
		{
			this.OnFailureTryCreateCompetition(failure);
		}
	}

	public void SaveCompetition(UserCompetition competition, bool removeOtherRegistrations = false)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 5;
		operationRequest.Parameters[10] = CompressHelper.CompressString(JsonConvert.SerializeObject(competition, SerializationHelper.JsonSerializerSettings));
		operationRequest.Parameters[36] = removeOtherRegistrations;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 5);
		this.UGC_savingCompetition = competition;
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnSaveCompetition OnSaveCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureSaveCompetition;

	private void UGCHandleOperationResponse_SaveCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			int num = (int)response.Parameters[21];
			this.UGC_savingCompetition.TournamentId = num;
			if (this.OnSaveCompetition != null)
			{
				this.OnSaveCompetition(this.UGC_savingCompetition);
			}
		}
		else if (this.OnFailureSaveCompetition != null)
		{
			this.OnFailureSaveCompetition(failure);
		}
	}

	public void UpdateCompetitionLastActivityTime(int tournamentId, bool isOnline)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 8;
		operationRequest.Parameters[21] = tournamentId;
		operationRequest.Parameters[44] = isOnline;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 8);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUpdateCompetitionLastActivityTime OnUpdateCompetitionLastActivityTime;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureUpdateCompetitionLastActivityTime;

	private void UGCHandleOperationResponse_UpdateCompetitionLastActivityTime(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnUpdateCompetitionLastActivityTime != null)
			{
				this.OnUpdateCompetitionLastActivityTime();
			}
		}
		else if (this.OnFailureUpdateCompetitionLastActivityTime != null)
		{
			this.OnFailureUpdateCompetitionLastActivityTime(failure);
		}
	}

	public void LoadCompetition(int competitionId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[1] = 6;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 6);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnLoadCompetition OnLoadCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureLoadCompetition;

	private void UGCHandleOperationResponse_LoadCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			UserCompetition userCompetition = null;
			if (response.Parameters.ContainsKey(10))
			{
				byte[] array = (byte[])response.Parameters[10];
				string text = CompressHelper.DecompressString(array);
				userCompetition = JsonConvert.DeserializeObject<UserCompetition>(text, SerializationHelper.JsonSerializerSettings);
			}
			if (this.OnLoadCompetition != null)
			{
				this.OnLoadCompetition(userCompetition);
			}
		}
		else if (this.OnFailureLoadCompetition != null)
		{
			this.OnFailureLoadCompetition(failure);
		}
	}

	public void RemoveCompetition(int competitionId, bool removePublished)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[1] = 7;
		operationRequest.Parameters[44] = removePublished;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 7);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnRemoveCompetition OnRemoveCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureRemoveCompetition;

	private void UGCHandleOperationResponse_RemoveCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			if (this.OnRemoveCompetition != null)
			{
				this.OnRemoveCompetition();
			}
		}
		else if (this.OnFailureRemoveCompetition != null)
		{
			this.OnFailureRemoveCompetition(failure);
		}
	}

	public void PublishCompetition(int competitionId, bool isPublished, bool removeOtherRegistrations)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[1] = 10;
		operationRequest.Parameters[44] = isPublished;
		operationRequest.Parameters[36] = removeOtherRegistrations;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 10);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnPublishCompetition OnPublishCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailurePublishCompetition;

	private void UGCHandleOperationResponse_PublishCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetition userCompetition = JsonConvert.DeserializeObject<UserCompetition>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnPublishCompetition != null)
			{
				this.OnPublishCompetition(userCompetition);
			}
		}
		else if (this.OnFailurePublishCompetition != null)
		{
			this.OnFailurePublishCompetition(failure);
		}
	}

	public void SendToReviewCompetition(int competitionId, string comment)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[1] = 9;
		operationRequest.Parameters[39] = comment;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 9);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnSendToReviewCompetition OnSendToReviewCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureSendToReviewCompetition;

	private void UGCHandleOperationResponse_SendToReviewCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetition userCompetition = JsonConvert.DeserializeObject<UserCompetition>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnSendToReviewCompetition != null)
			{
				this.OnSendToReviewCompetition(userCompetition);
			}
		}
		else if (this.OnFailureSendToReviewCompetition != null)
		{
			this.OnFailureSendToReviewCompetition(failure);
		}
	}

	public void GetUserCompetitions(FilterForUserCompetitions filter)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 11;
		operationRequest.Parameters[10] = CompressHelper.CompressString(JsonConvert.SerializeObject(filter, SerializationHelper.JsonSerializerSettings));
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 11);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGetUserCompetitions OnGetUserCompetitions;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureGetUserCompetitions;

	private void UGCHandleOperationResponse_GetUserCompetitions(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			Guid guid = ((!response.Parameters.ContainsKey(44)) ? Guid.Empty : new Guid((string)response.Parameters[44]));
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			List<UserCompetitionPublic> list = JsonConvert.DeserializeObject<List<UserCompetitionPublic>>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnGetUserCompetitions != null)
			{
				this.OnGetUserCompetitions(guid, list);
			}
		}
		else if (this.OnFailureGetUserCompetitions != null)
		{
			this.OnFailureGetUserCompetitions(failure);
		}
	}

	public void GetUserCompetition(int competitionId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 12;
		operationRequest.Parameters[21] = competitionId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 12);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGetUserCompetition OnGetUserCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureGetUserCompetition;

	private void UGCHandleOperationResponse_GetUserCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			UserCompetitionPublic userCompetitionPublic = null;
			if (response.Parameters.ContainsKey(10))
			{
				byte[] array = (byte[])response.Parameters[10];
				string text = CompressHelper.DecompressString(array);
				userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			}
			if (this.OnGetUserCompetition != null)
			{
				this.OnGetUserCompetition(userCompetitionPublic);
			}
		}
		else if (this.OnFailureGetUserCompetition != null)
		{
			this.OnFailureGetUserCompetition(failure);
		}
	}

	public void GetUserCompetitionReward(int competitionId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 27;
		operationRequest.Parameters[21] = competitionId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 27);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnGetUserCompetitionReward OnGetUserCompetitionReward;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureGetUserCompetitionReward;

	private void UGCHandleOperationResponse_GetReward(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			Reward reward = null;
			int num = (int)response.Parameters[21];
			if (response.Parameters.ContainsKey(10))
			{
				byte[] array = (byte[])response.Parameters[10];
				string text = CompressHelper.DecompressString(array);
				reward = JsonConvert.DeserializeObject<Reward>(text, SerializationHelper.JsonSerializerSettings);
			}
			if (this.OnGetUserCompetitionReward != null)
			{
				this.OnGetUserCompetitionReward(num, reward);
			}
		}
		else if (this.OnFailureGetUserCompetitionReward != null)
		{
			this.OnFailureGetUserCompetitionReward(failure);
		}
	}

	public void InvitePlayerToCompetition(int competitionId, Guid playerId, string team)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 13;
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[46] = playerId.ToString();
		if (team != null)
		{
			operationRequest.Parameters[38] = team;
		}
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 13);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnInvitePlayerToCompetition OnInvitePlayerToCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureInvitePlayerToCompetition;

	private void UGCHandleOperationResponse_InvitePlayerToCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnInvitePlayerToCompetition != null)
			{
				this.OnInvitePlayerToCompetition(userCompetitionPublic);
			}
		}
		else if (this.OnFailureInvitePlayerToCompetition != null)
		{
			this.OnFailureInvitePlayerToCompetition(failure);
		}
	}

	public void AcceptInvitationToCompetition(int competitionId, string team, bool removeOtherRegistrations = false)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 14;
		operationRequest.Parameters[21] = competitionId;
		if (team != null)
		{
			operationRequest.Parameters[38] = team;
		}
		operationRequest.Parameters[36] = removeOtherRegistrations;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 14);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnAcceptInvitationToCompetition OnAcceptInvitationToCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureAcceptInvitationToCompetition;

	private void UGCHandleOperationResponse_AcceptInvitationToCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnAcceptInvitationToCompetition != null)
			{
				this.OnAcceptInvitationToCompetition(userCompetitionPublic);
			}
		}
		else if (this.OnFailureAcceptInvitationToCompetition != null)
		{
			this.OnFailureAcceptInvitationToCompetition(failure);
		}
	}

	public void RegisterInCompetition(int competitionId, string team, string password, bool removeOtherRegistrations = false)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 15;
		operationRequest.Parameters[21] = competitionId;
		if (team != null)
		{
			operationRequest.Parameters[38] = team;
		}
		if (password != null)
		{
			operationRequest.Parameters[39] = password;
		}
		operationRequest.Parameters[36] = removeOtherRegistrations;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 15);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnRegisterInCompetition OnRegisterInCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureRegisterInCompetition;

	private void UGCHandleOperationResponse_RegisterInCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnRegisterInCompetition != null)
			{
				this.OnRegisterInCompetition(userCompetitionPublic);
			}
		}
		else if (this.OnFailureRegisterInCompetition != null)
		{
			this.OnFailureRegisterInCompetition(failure);
		}
	}

	public void UnregisterFromCompetition(int competitionId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 16;
		operationRequest.Parameters[21] = competitionId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 16);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUnregisterFromCompetition OnUnregisterFromCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureUnregisterFromCompetition;

	private void UGCHandleOperationResponse_UnregisterFromCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnUnregisterFromCompetition != null)
			{
				this.OnUnregisterFromCompetition(userCompetitionPublic);
			}
		}
		else if (this.OnFailureUnregisterFromCompetition != null)
		{
			this.OnFailureUnregisterFromCompetition(failure);
		}
	}

	public void ApproveParticipationInCompetition(int competitionId, bool isApproved)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 20;
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[44] = isApproved;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 20);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnApproveParticipationInCompetition OnApproveParticipationInCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureApproveParticipationInCompetition;

	private void UGCHandleOperationResponse_ApproveParticipationInCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnApproveParticipationInCompetition != null)
			{
				this.OnApproveParticipationInCompetition(userCompetitionPublic);
			}
		}
		else if (this.OnFailureApproveParticipationInCompetition != null)
		{
			this.OnFailureApproveParticipationInCompetition(failure);
		}
	}

	public void MovePlayerToCompetitionTeam(int competitionId, Guid playerId, string team)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 21;
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[46] = playerId.ToString();
		operationRequest.Parameters[38] = team;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 21);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMovePlayerToCompetitionTeam OnMovePlayerToCompetitionTeam;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureMovePlayerToCompetitionTeam;

	private void UGCHandleOperationResponse_MovePlayerToCompetitionTeam(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnMovePlayerToCompetitionTeam != null)
			{
				this.OnMovePlayerToCompetitionTeam(userCompetitionPublic);
			}
		}
		else if (this.OnFailureMovePlayerToCompetitionTeam != null)
		{
			this.OnFailureMovePlayerToCompetitionTeam(failure);
		}
	}

	public void LockPlayerInCompetitionTeam(int competitionId, Guid playerId, bool isLocked)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 22;
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[46] = playerId.ToString();
		operationRequest.Parameters[44] = isLocked;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 22);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnLockPlayerInCompetitionTeam OnLockPlayerInCompetitionTeam;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureLockPlayerInCompetitionTeam;

	private void UGCHandleOperationResponse_LockPlayerInCompetitionTeam(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnLockPlayerInCompetitionTeam != null)
			{
				this.OnLockPlayerInCompetitionTeam(userCompetitionPublic);
			}
		}
		else if (this.OnFailureLockPlayerInCompetitionTeam != null)
		{
			this.OnFailureLockPlayerInCompetitionTeam(failure);
		}
	}

	public void RemovePlayerFromCompetition(int competitionId, Guid playerId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 23;
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[46] = playerId.ToString();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 23);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnRemovePlayerFromCompetition OnRemovePlayerFromCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureRemovePlayerFromCompetition;

	private void UGCHandleOperationResponse_RemovePlayerFromCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnRemovePlayerFromCompetition != null)
			{
				this.OnRemovePlayerFromCompetition(userCompetitionPublic);
			}
		}
		else if (this.OnFailureRemovePlayerFromCompetition != null)
		{
			this.OnFailureRemovePlayerFromCompetition(failure);
		}
	}

	public void AutoArrangePlayersAndTeams(int competitionId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 26;
		operationRequest.Parameters[21] = competitionId;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 26);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnAutoArrangePlayersAndTeams OnAutoArrangePlayersAndTeams;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureAutoArrangePlayersAndTeams;

	private void UGCHandleOperationResponse_AutoArrangePlayersAndTeams(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnAutoArrangePlayersAndTeams != null)
			{
				this.OnAutoArrangePlayersAndTeams(userCompetitionPublic);
			}
		}
		else if (this.OnFailureAutoArrangePlayersAndTeams != null)
		{
			this.OnFailureAutoArrangePlayersAndTeams(failure);
		}
	}

	public void MoveToCompetitionTeam(int competitionId, string team)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 17;
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[38] = team;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 17);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnMoveToCompetitionTeam OnMoveToCompetitionTeam;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureMoveToCompetitionTeam;

	private void UGCHandleOperationResponse_MoveToCompetitionTeam(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnMoveToCompetitionTeam != null)
			{
				this.OnMoveToCompetitionTeam(userCompetitionPublic);
			}
		}
		else if (this.OnFailureMoveToCompetitionTeam != null)
		{
			this.OnFailureMoveToCompetitionTeam(failure);
		}
	}

	public void ProposeCompetitionTeamExchange(int competitionId, Guid playerId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 18;
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[46] = playerId.ToString();
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 18);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnProposeCompetitionTeamExchange OnProposeCompetitionTeamExchange;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureProposeCompetitionTeamExchange;

	private void UGCHandleOperationResponse_ProposeCompetitionTeamExchange(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnProposeCompetitionTeamExchange != null)
			{
				this.OnProposeCompetitionTeamExchange(userCompetitionPublic);
			}
		}
		else if (this.OnFailureProposeCompetitionTeamExchange != null)
		{
			this.OnFailureProposeCompetitionTeamExchange(failure);
		}
	}

	public void AcceptCompetitionTeamExchange(int competitionId, string team)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[1] = 19;
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[38] = team;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 19);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnAcceptCompetitionTeamExchange OnAcceptCompetitionTeamExchange;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureAcceptCompetitionTeamExchange;

	private void UGCHandleOperationResponse_AcceptCompetitionTeamExchange(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetitionPublic userCompetitionPublic = JsonConvert.DeserializeObject<UserCompetitionPublic>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnAcceptCompetitionTeamExchange != null)
			{
				this.OnAcceptCompetitionTeamExchange(userCompetitionPublic);
			}
		}
		else if (this.OnFailureAcceptCompetitionTeamExchange != null)
		{
			this.OnFailureAcceptCompetitionTeamExchange(failure);
		}
	}

	public void RequestStartCompetition(int competitionId, bool isStartRequested)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[1] = 24;
		operationRequest.Parameters[44] = isStartRequested;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 24);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnRequestStartCompetition OnRequestStartCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureRequestStartCompetition;

	private void UGCHandleOperationResponse_RequestStartCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetition userCompetition = JsonConvert.DeserializeObject<UserCompetition>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnRequestStartCompetition != null)
			{
				this.OnRequestStartCompetition(userCompetition);
			}
		}
		else if (this.OnFailureRequestStartCompetition != null)
		{
			this.OnFailureRequestStartCompetition(failure);
		}
	}

	public void StartCompetition(int competitionId)
	{
		OperationRequest operationRequest = PhotonServerConnection.CreateOperationRequest(176);
		operationRequest.Parameters[21] = competitionId;
		operationRequest.Parameters[1] = 25;
		this.Peer.OpCustom(operationRequest, true, 0, true);
		OpTimer.Start(176, 25);
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnStartCompetition OnStartCompetition;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnFailureUserCompetition OnFailureStartCompetition;

	private void UGCHandleOperationResponse_StartCompetition(OperationResponse response, UserCompetitionFailure failure)
	{
		if (response.ReturnCode == 0)
		{
			byte[] array = (byte[])response.Parameters[10];
			string text = CompressHelper.DecompressString(array);
			UserCompetition userCompetition = JsonConvert.DeserializeObject<UserCompetition>(text, SerializationHelper.JsonSerializerSettings);
			if (this.OnStartCompetition != null)
			{
				this.OnStartCompetition(userCompetition);
			}
		}
		else if (this.OnFailureStartCompetition != null)
		{
			this.OnFailureStartCompetition(failure);
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionReviewed OnUserCompetitionApprovedOnReview;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionReviewed OnUserCompetitionDeclinedOnReview;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionReviewed OnUserCompetitionRemovedOnReview;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionCancelled OnUserCompetitionCancelled;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionReverted OnUserCompetitionReverted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionPromotion OnUserCompetitionPromotion;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionInvitation OnUserCompetitionInvitation;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionInvitationResponse OnUserCompetitionInvitationResponse;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionStarted OnUserCompetitionStarted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionTeamExchange OnUserCompetitionTeamExchange;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event OnUserCompetitionTeamExchangeResponse OnUserCompetitionTeamExchangeResponse;

	private void UGCHandleOperationResponse(OperationResponse response)
	{
		UserCompetitionFailure userCompetitionFailure = null;
		if (response.ReturnCode != 0)
		{
			userCompetitionFailure = new UserCompetitionFailure(response);
			Debug.LogError(userCompetitionFailure.FullErrorInfo);
		}
		if (!response.Parameters.ContainsKey(1))
		{
			Debug.LogError("UserCompetition operation response has no subcode");
			return;
		}
		UserCompetitionSubOperationCode userCompetitionSubOperationCode = (UserCompetitionSubOperationCode)response[1];
		OpTimer.End(176, userCompetitionSubOperationCode);
		switch (userCompetitionSubOperationCode)
		{
		case 0:
			this.UGCHandleOperationResponse_GetMetadataForCompetitionSearch(response, userCompetitionFailure);
			break;
		case 1:
			this.UGCHandleOperationResponse_GetMetadataForCompetition(response, userCompetitionFailure);
			break;
		case 2:
			this.UGCHandleOperationResponse_GetMetadataForCompetitionPond(response, userCompetitionFailure);
			break;
		case 3:
			this.UGCHandleOperationResponse_GetMetadataForCompetitionPondWeather(response, userCompetitionFailure);
			break;
		case 4:
			this.UGCHandleOperationResponse_GetMetadataForPredefinedCompetitionTemplate(response, userCompetitionFailure);
			break;
		case 5:
			this.UGCHandleOperationResponse_SaveCompetition(response, userCompetitionFailure);
			break;
		case 6:
			this.UGCHandleOperationResponse_LoadCompetition(response, userCompetitionFailure);
			break;
		case 7:
			this.UGCHandleOperationResponse_RemoveCompetition(response, userCompetitionFailure);
			break;
		case 8:
			this.UGCHandleOperationResponse_UpdateCompetitionLastActivityTime(response, userCompetitionFailure);
			break;
		case 9:
			this.UGCHandleOperationResponse_SendToReviewCompetition(response, userCompetitionFailure);
			break;
		case 10:
			this.UGCHandleOperationResponse_PublishCompetition(response, userCompetitionFailure);
			break;
		case 11:
			this.UGCHandleOperationResponse_GetUserCompetitions(response, userCompetitionFailure);
			break;
		case 12:
			this.UGCHandleOperationResponse_GetUserCompetition(response, userCompetitionFailure);
			break;
		case 13:
			this.UGCHandleOperationResponse_InvitePlayerToCompetition(response, userCompetitionFailure);
			break;
		case 14:
			this.UGCHandleOperationResponse_AcceptInvitationToCompetition(response, userCompetitionFailure);
			break;
		case 15:
			this.UGCHandleOperationResponse_RegisterInCompetition(response, userCompetitionFailure);
			break;
		case 16:
			this.UGCHandleOperationResponse_UnregisterFromCompetition(response, userCompetitionFailure);
			break;
		case 17:
			this.UGCHandleOperationResponse_MoveToCompetitionTeam(response, userCompetitionFailure);
			break;
		case 18:
			this.UGCHandleOperationResponse_ProposeCompetitionTeamExchange(response, userCompetitionFailure);
			break;
		case 19:
			this.UGCHandleOperationResponse_AcceptCompetitionTeamExchange(response, userCompetitionFailure);
			break;
		case 20:
			this.UGCHandleOperationResponse_ApproveParticipationInCompetition(response, userCompetitionFailure);
			break;
		case 21:
			this.UGCHandleOperationResponse_MovePlayerToCompetitionTeam(response, userCompetitionFailure);
			break;
		case 22:
			this.UGCHandleOperationResponse_LockPlayerInCompetitionTeam(response, userCompetitionFailure);
			break;
		case 23:
			this.UGCHandleOperationResponse_RemovePlayerFromCompetition(response, userCompetitionFailure);
			break;
		case 24:
			this.UGCHandleOperationResponse_RequestStartCompetition(response, userCompetitionFailure);
			break;
		case 25:
			this.UGCHandleOperationResponse_StartCompetition(response, userCompetitionFailure);
			break;
		case 26:
			this.UGCHandleOperationResponse_AutoArrangePlayersAndTeams(response, userCompetitionFailure);
			break;
		case 27:
			this.UGCHandleOperationResponse_GetReward(response, userCompetitionFailure);
			break;
		case 28:
			this.UGCHandleOperationResponse_TryCreateCompetition(response, userCompetitionFailure);
			break;
		}
	}

	private bool isAuthenticating;

	private bool isMovingToBase;

	private bool isMovingToRoom;

	private string roomToEnter;

	private int? pondToEnter;

	private int? tournamentToEnter;

	private int? daysToStay;

	private int? daysToAdd;

	private bool? isMovingByCar;

	private bool isDisconnectToReconnect;

	private string securityToken;

	private string cachedServerAddress;

	private string cachedGameAction;

	private string cachedEmail;

	private string cachedPassword;

	private bool raiseJoinErrorEvents;

	private string priorRoomName;

	private bool connectWithoutAuth;

	private Profile profile;

	private int languageIdToSet;

	private ServerTimeCache serverTimeCache;

	private readonly Queue<Action> cachedActionQueue = new Queue<Action>();

	private Thread backgroundServiceThread;

	private DateTime lastRequestProfileTime;

	private DateTime lastInventoryRequestTime;

	private string lastInventoryStackTrace;

	private int inventoryRequestNumber;

	private int inventoryResponseNumber;

	private int inventoryRequestNumberRequestProfileOnError;

	private DateTime? lastRefreshLogMissedInventoryItem;

	private bool isRoomCreatedOnMaster;

	private bool isConnectionLocked;

	private float processingTime;

	private const float ProcessingTimeout = 30f;

	private InteractiveObject lastInteractedObject;

	private readonly IDictionary<int, Pond> pondInfos = new Dictionary<int, Pond>();

	private bool ignoreAsUnfriend;

	private Game[] gameSlots = new Game[8];

	private Hashtable globalVariables;

	private List<OperationRequest> errorsCache = new List<OperationRequest>();

	private List<OperationRequest> telemetryCache = new List<OperationRequest>();

	private readonly Dictionary<int, MissionClientConfiguration> missionClientConfigurationCahce = new Dictionary<int, MissionClientConfiguration>();

	private bool IsGameResumed;

	private bool receiveProfileInProgress;

	private bool isAuthenticatingWithSteam;

	private string steamAuthTicketBackup;

	private readonly Queue<ChatMessage> internalChatMessageQueue = new Queue<ChatMessage>();

	private UserCompetition UGC_savingCompetition;
}
