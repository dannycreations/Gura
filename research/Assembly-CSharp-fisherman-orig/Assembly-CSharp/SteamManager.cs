using System;
using System.Collections.Generic;
using System.Text;
using ObjectModel;
using Steamworks;
using UnityEngine;

internal class SteamManager : MonoBehaviour
{
	public static SteamManager Instance
	{
		get
		{
			return SteamManager._instance ?? new GameObject("SteamManager").AddComponent<SteamManager>();
		}
	}

	public static bool Initialized
	{
		get
		{
			return SteamManager._instance != null && SteamManager._instance._initialized;
		}
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	public void Awake()
	{
		if (SteamManager._instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		SteamManager._instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary((AppId_t)StaticUserData.SteamAppId))
			{
				return;
			}
		}
		catch (DllNotFoundException ex)
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex, this);
			Application.Quit();
			return;
		}
		try
		{
			this._initialized = SteamAPI.Init();
		}
		catch (Exception)
		{
		}
		if (!this._initialized)
		{
			Debug.LogWarning("[Steamworks.NET] SteamAPI_Init() failed. You may not have Steam running.", this);
		}
	}

	public void OnEnable()
	{
		if (SteamManager._instance == null)
		{
			SteamManager._instance = this;
		}
		if (!this._initialized)
		{
			return;
		}
		if (this.m_SteamAPIWarningMessageHook == null)
		{
			this.m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamManager.SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(this.m_SteamAPIWarningMessageHook);
		}
	}

	public void OnDestroy()
	{
		if (SteamManager._instance != this)
		{
			return;
		}
		SteamManager._instance = null;
		if (!this._initialized)
		{
			return;
		}
		SteamAPI.Shutdown();
	}

	public void Update()
	{
		if (!this._initialized)
		{
			return;
		}
		SteamAPI.RunCallbacks();
		this.UpdateSteamFriends();
	}

	private void UpdateSteamFriends()
	{
		if (!this._initialized)
		{
			return;
		}
		IPhotonServerConnection instance = PhotonConnectionFactory.Instance;
		if (!instance.IsConnectedToGameServer || !instance.IsAuthenticated)
		{
			return;
		}
		if (instance.Profile == null)
		{
			return;
		}
		if (this.m_bFriendsUpdatedFromSteam)
		{
			return;
		}
		this.m_bFriendsUpdatedFromSteam = true;
		if (instance.FriendsCount >= 100)
		{
			return;
		}
		int friendCount = SteamFriends.GetFriendCount(4);
		List<string> list = null;
		for (int i = 0; i < friendCount; i++)
		{
			string text = SteamFriends.GetFriendByIndex(i, 4).m_SteamID.ToString();
			if (!this.HasFriend(text))
			{
				if (list == null)
				{
					list = new List<string>();
				}
				if (PhotonConnectionFactory.Instance.FriendsCount + list.Count >= 100)
				{
					break;
				}
				list.Add(text);
			}
		}
		if (list != null && list.Count > 0)
		{
			instance.AddFriendsByExternalIds("Steam", list);
		}
	}

	private bool HasFriend(string steamId)
	{
		List<Player> friends = PhotonConnectionFactory.Instance.Profile.Friends;
		if (friends == null || friends.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < friends.Count; i++)
		{
			Player player = friends[i];
			if (player.Source == "Steam" && player.ExternalId == steamId)
			{
				return true;
			}
		}
		return false;
	}

	private static SteamManager _instance;

	private bool _initialized;

	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

	private bool m_bFriendsUpdatedFromSteam;
}
