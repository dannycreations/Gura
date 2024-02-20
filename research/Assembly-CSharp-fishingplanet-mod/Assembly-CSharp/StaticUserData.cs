using System;
using System.Collections.Generic;
using System.Text;
using ExitGames.Client.Photon;
using ObjectModel;
using Steamworks;
using UnityEngine;

public static class StaticUserData
{
	public static bool IS_TPM_ENABLED
	{
		get
		{
			return true;
		}
	}

	public static bool IS_TPM_VISIBLE
	{
		get
		{
			return StaticUserData.IS_TPM_ENABLED && SettingsManager.ShowCharacters;
		}
	}

	public static bool DISABLE_MISSIONS
	{
		get
		{
			return false;
		}
	}

	public static bool HIDE_WATER
	{
		get
		{
			return false;
		}
	}

	public static bool HIDE_VEGETATION
	{
		get
		{
			return false;
		}
	}

	public static bool IS_IN_TUTORIAL
	{
		get
		{
			return StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == 2;
		}
	}

	public static bool ResetStaticUserDate(bool afterDisconnect = true)
	{
		StaticUserData.StartConnection = false;
		StaticUserData.AllCategories = new List<InventoryCategory>();
		StaticUserData.AllStates = new List<State>();
		StaticUserData.PremiumSalesAvailable = null;
		StaticUserData.RodInHand = new AssembledRod();
		StaticUserData.FireworkItems = null;
		StaticUserData.CurrentLocation = null;
		StaticUserData.CurrentPond = null;
		bool flag = false;
		CacheLibrary.ResetAllCaches(flag);
		return flag;
	}

	public static bool IsSteam
	{
		get
		{
			return StaticUserData.ClientType == ClientTypes.SteamWindows || StaticUserData.ClientType == ClientTypes.SteamLinux || StaticUserData.ClientType == ClientTypes.SteamOsX;
		}
	}

	public static bool IsMailRu
	{
		get
		{
			return StaticUserData.ClientType == ClientTypes.MailRuWindows || StaticUserData.ClientType == ClientTypes.MailRuLinux;
		}
	}

	public static bool IsTencent
	{
		get
		{
			return StaticUserData.ClientType == ClientTypes.TencentWindows;
		}
	}

	public static ClientTypes ClientType
	{
		get
		{
			ClientTypes clientTypes = ClientTypes.unknown;
			RuntimePlatform platform = Application.platform;
			if (platform != 25)
			{
				if (platform != 27)
				{
					if (platform == 19)
					{
						clientTypes = ClientTypes.WindowsUWP;
					}
				}
				else
				{
					clientTypes = ClientTypes.XBoxOne;
				}
			}
			else
			{
				clientTypes = ClientTypes.PS4;
			}
			if (clientTypes != ClientTypes.PS4 && clientTypes != ClientTypes.XBoxOne && clientTypes != ClientTypes.WindowsUWP)
			{
				clientTypes = ((Application.platform != 2) ? ClientTypes.StandaloneOsX : ClientTypes.StandaloneWindows);
				if (SteamManager.Instance != null && SteamManager.Initialized)
				{
					AppId_t appID = SteamUtils.GetAppID();
					if (StaticUserData.SteamAppId != appID.m_AppId)
					{
						return clientTypes;
					}
					return (Application.platform != 2) ? ClientTypes.SteamOsX : ClientTypes.SteamWindows;
				}
			}
			return clientTypes;
		}
	}

	public static ConnectionProtocol ServerConnectionProtocol
	{
		get
		{
			return 1;
		}
	}

	public static string ServerConnectionPort
	{
		get
		{
			return ":4530";
		}
	}

	public static string GetDefaultServerConnectionString
	{
		get
		{
			return StaticUserData._connectionStringDefault;
		}
	}

	public static CSteamID SteamId
	{
		get
		{
			return StaticUserData._steamId;
		}
	}

	public static string ServerConnectionString
	{
		get
		{
			return (StaticUserData._connectionString == null) ? (StaticUserData._connectionStringDefault + StaticUserData.ServerConnectionPort) : (StaticUserData._connectionString + StaticUserData.ServerConnectionPort);
		}
		set
		{
			StaticUserData.IPHostResolved = true;
			StaticUserData._connectionString = value;
		}
	}

	public static Pond CurrentPond { get; set; }

	public static Location CurrentLocation { get; set; }

	public static void ConnectToMasterWrapper()
	{
		StaticUserData.StartConnection = true;
		if (SteamManager.Instance != null && SteamManager.Initialized)
		{
			StaticUserData.SteamInit();
		}
		else if (SteamManager.Instance != null && SteamManager.Initialized)
		{
			StaticUserData.SteamInit();
		}
		else
		{
			PhotonConnectionFactory.Instance.ConnectToMaster();
		}
	}

	private static void SteamInit()
	{
		StaticUserData._steamId = SteamUser.GetSteamID();
		AppId_t appID = SteamUtils.GetAppID();
		if (StaticUserData.SteamAppId != appID.m_AppId)
		{
			Debug.LogErrorFormat("[Steamworks.NET] AppID is {0} while it should be {1}", new object[]
			{
				appID.m_AppId,
				StaticUserData.SteamAppId
			});
			return;
		}
		StaticUserData.RequestAuthTicket();
	}

	public static void RequestAuthTicket()
	{
		StaticUserData.sessionTicketCallback = Callback<GetAuthSessionTicketResponse_t>.Create(new Callback<GetAuthSessionTicketResponse_t>.DispatchDelegate(StaticUserData.AuthCallback));
		byte[] array = new byte[1024];
		uint num;
		if (SteamUser.GetAuthSessionTicket(array, array.Length, ref num) == HAuthTicket.Invalid)
		{
			Debug.LogError("[Steamworks.NET] Invalid auth ticket.");
			PhotonConnectionFactory.Instance.ConnectToMaster();
			return;
		}
		StaticUserData.authTicket = StaticUserData.ByteArrayToString(array, 0, (int)num);
		DebugUtility.Steam.Trace("AuthTicket requested", new object[0]);
	}

	public static void AuthCallback(GetAuthSessionTicketResponse_t result)
	{
		DebugUtility.Steam.Trace("AuthCallback responced", new object[0]);
		if (result.m_eResult == 1)
		{
			PhotonConnectionFactory.Instance.SteamAuthTicket = StaticUserData.authTicket;
		}
		StaticUserData.sessionTicketCallback.Unregister();
		StaticUserData.IsSignInToServer = true;
		PhotonConnectionFactory.Instance.ConnectToMaster();
	}

	public static string ByteArrayToString(byte[] buffer, int offset, int length)
	{
		StringBuilder stringBuilder = new StringBuilder(length * 2);
		for (int i = offset; i < offset + length; i++)
		{
			byte b = buffer[i];
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		return stringBuilder.ToString();
	}

	internal const MeasuringSystem DefaultMeasuringSystem = MeasuringSystem.Imperial;

	public static int CountryId = 1;

	public static int AllCountriesId = -1;

	public static bool StartConnection = false;

	public static List<InventoryCategory> AllCategories;

	public static List<State> AllStates;

	public static List<SaleFlags> PremiumSalesAvailable = null;

	public static AssembledRod RodInHand = new AssembledRod();

	public static List<InventoryItem> FireworkItems = null;

	public static bool HasServer = true;

	public static ChatController ChatController = new ChatController();

	internal static bool IsShowDashboard = false;

	internal static ActivityState CurrentForm;

	public static bool IPHostResolved = false;

	private static string _connectionStringDefault = "192.40.222.2";

	private static string _connectionString = null;

	private static CSteamID _steamId = CSteamID.Nil;

	public static bool IsSignInToServer;

	public static string GameObjectCommonDataName = "GameObjectCommonData";

	public static uint SteamAppId = 380600U;

	private static Callback<GetAuthSessionTicketResponse_t> sessionTicketCallback;

	private const int TicketBufferLength = 1024;

	private static string authTicket;
}
