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
		if (Input.GetKeyDown(289))
		{
			this.isModOn = !this.isModOn;
		}
		if (Input.GetKeyDown(288))
		{
			PhotonDispatcher.Globals.ESP = !PhotonDispatcher.Globals.ESP;
		}
		if (Input.GetKeyDown(287))
		{
			PhotonDispatcher.Globals.AlarmIndicator = !PhotonDispatcher.Globals.AlarmIndicator;
		}
		if (Input.GetKeyDown(286))
		{
			PhotonDispatcher.Globals.StandardUnit = !PhotonDispatcher.Globals.StandardUnit;
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
		bool esp = PhotonDispatcher.Globals.ESP;
		if (GameFactory.FishSpawner != null && PhotonDispatcher.Globals.ESP)
		{
			foreach (IFishController fishController in GameFactory.FishSpawner.Fish.Values)
			{
				PhotonDispatcher.Globals.iFish = fishController;
				Vector3 vector = Camera.main.WorldToScreenPoint(fishController.Owner.transform.position);
				Vector2 vector2;
				vector2..ctor(vector.x, vector.y);
				string text = fishController.FishTemplate.Weight.ToString();
				if (PhotonDispatcher.Globals.StandardUnit)
				{
					text = (fishController.FishTemplate.Weight * 2.20462f).ToString();
				}
				string text2 = (fishController.Owner.Length * 100f).ToString();
				if (PhotonDispatcher.Globals.StandardUnit)
				{
					text2 = (fishController.Owner.Length * 39.3701f).ToString();
				}
				float weight = fishController.FishTemplate.Weight;
				string fullName = fishController.State.FullName;
				string text3 = fullName.Substring(4);
				text.LastIndexOf(".");
				text2.LastIndexOf(".");
				int num = text.LastIndexOf(".");
				text2.LastIndexOf(".");
				string text4 = " kg";
				string text5 = " cm";
				if (PhotonDispatcher.Globals.StandardUnit)
				{
					text4 = " lb";
					text5 = " in";
				}
				string text6 = "<color=#F0E68C>";
				if (weight > 8.9f)
				{
					text6 = "<color=#FFA500>";
				}
				if (weight > 13.5f)
				{
					text6 = "<color=#FF8C00>";
				}
				if (weight > 17.5f)
				{
					text6 = "<color=#FF6347>";
				}
				if (weight > 22.9f)
				{
					text6 = "<color=#FF4500>";
				}
				if (weight > 28f)
				{
					text6 = "<color=#FF0000>";
				}
				string codeName = fishController.FishTemplate.CodeName;
				string text7 = codeName.Substring(codeName.Length - 1);
				bool flag = codeName.IndexOf("trophy", StringComparison.OrdinalIgnoreCase) >= 0;
				bool flag2 = codeName.IndexOf("young", StringComparison.OrdinalIgnoreCase) >= 0;
				bool flag3 = codeName.IndexOf("unique", StringComparison.OrdinalIgnoreCase) >= 0;
				bool flag4 = text7.Contains("T");
				bool flag5 = text7.Contains("Y");
				bool flag6 = text7.Contains("U");
				if (flag || flag4)
				{
					PhotonDispatcher.Globals.fishclass = "<color=#00FF00>Trophy </color>";
					PhotonDispatcher.Globals.isCommon = false;
				}
				else if (flag2 || flag5)
				{
					PhotonDispatcher.Globals.fishclass = "Young ";
					PhotonDispatcher.Globals.isCommon = false;
				}
				else if (flag3 || flag6)
				{
					PhotonDispatcher.Globals.fishclass = "<color=#FFA500>Unique </color>";
					PhotonDispatcher.Globals.isCommon = false;
				}
				else
				{
					PhotonDispatcher.Globals.fishclass = " ";
					PhotonDispatcher.Globals.isCommon = true;
				}
				if (PhotonDispatcher.Globals.IgnoreYoung && (flag2 || flag5))
				{
					PhotonDispatcher.Globals.iFish.Escape();
					PhotonDispatcher.Globals.iFish.Behavior = FishBehavior.Go;
				}
				if (PhotonDispatcher.Globals.IgnoreCommon && PhotonDispatcher.Globals.isCommon)
				{
					PhotonDispatcher.Globals.iFish.Escape();
					PhotonDispatcher.Globals.iFish.Behavior = FishBehavior.Go;
				}
				if (PhotonDispatcher.Globals.IgnoreTrophy && (flag || flag4))
				{
					PhotonDispatcher.Globals.iFish.Escape();
					PhotonDispatcher.Globals.iFish.Behavior = FishBehavior.Go;
				}
				if (PhotonDispatcher.Globals.IgnoreUnique && (flag3 || flag6))
				{
					PhotonDispatcher.Globals.iFish.Escape();
					PhotonDispatcher.Globals.iFish.Behavior = FishBehavior.Go;
				}
				GUI.contentColor = Color.white;
				GUIStyle guistyle = new GUIStyle(GUI.skin.box);
				guistyle.normal.background = this.MakeTex(2, 2, new Color(0f, 0f, 0f, 0.3f));
				guistyle.fontSize = 12;
				GUI.Box(new Rect(vector2.x - 75f, (float)Screen.height - vector2.y, 180f, 80f), string.Concat(new object[]
				{
					"<b>" + PhotonDispatcher.Globals.fishclass + fishController.FishTemplate.Name + "</b>",
					"\n\n <b>[State]</b> " + text3,
					"\n <b>[Weight]</b> ",
					text6 + text.Substring(0, num + 4) + text4 + "</color>",
					"\n <b>[Length]</b> ",
					"<color=#008FFF>" + text2.Substring(0, num + 4) + text5 + "</color>"
				}), guistyle);
				if (fullName == "FishSwimAway" && PhotonDispatcher.Globals.AlarmIndicator)
				{
					RandomSounds.PlaySoundAtPoint("Sounds/incoming_bite2", GameFactory.Player.Position, 0.02f, false);
				}
			}
		}
		if (!this.isModOn)
		{
			return;
		}
		string text8 = "<color=#FF0000>Disable</color>";
		string text9 = "<color=#FF0000>Disable</color>";
		string text10 = "Metric";
		if (PhotonDispatcher.Globals.ESP)
		{
			text8 = "<color=#00FF00>Enable</color>";
		}
		if (PhotonDispatcher.Globals.AlarmIndicator)
		{
			text9 = "<color=#00FF00>Enable</color>";
		}
		if (PhotonDispatcher.Globals.StandardUnit)
		{
			text10 = "Imprerial";
		}
		GUIStyle guistyle2 = new GUIStyle(GUI.skin.box);
		guistyle2.normal.background = this.MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f));
		guistyle2.normal.textColor = Color.white;
		guistyle2.fontSize = 14;
		GUILayout.BeginArea(this.screenRectMod);
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		GUILayout.Box(string.Concat(new object[]
		{
			"\n<b><color=#FFD700>[Fishing Planet HACK]</color></b> <color=#FFA500>v0.5.8</color>",
			"\nComplete Edition",
			"\n<color=#5e5e5e>────────────────────</color>",
			"\n[F7]<b>ESP:</b> " + text8,
			"\n[F6]<b>AlarmIndicator:</b> " + text9,
			"\n[F5]<b>Units: </b><color=#FFFFFF>" + text10 + "</color>",
			"\n<color=#5e5e5e>────────────────────</color>",
			"\n<b>FPS:</b> " + (int)FPSController.GetFps(),
			"\n<color=#5e5e5e>────────────────────</color>",
			"\n<color=#00FFFF>https://discord.gg/8eT7apU</color>",
			"\n"
		}), guistyle2, new GUILayoutOption[0]);
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
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

	private Texture2D MakeTex(int width, int height, Color col)
	{
		Color[] array = new Color[width * height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = col;
		}
		Texture2D texture2D = new Texture2D(width, height);
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	private DateTime? tokenLastUpdated;

	private const int TokenUpdateTimeout = 10;

	public static PhotonDispatcher Instance;

	private bool isConnectionDebugOn;

	private readonly Rect screenRect = new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);

	private bool isModOn;

	private readonly Rect screenRectMod = new Rect((float)((double)(Screen.width / 100) * 3.5), (float)(Screen.height / 100 * 30), 250f, 400f);

	public static class Globals
	{
		public static bool ESP = true;

		public static float MinWeight = 0f;

		public static string[] TargetFish;

		public static int TargetFishIndex = 0;

		public static int MiscIndex = 0;

		public static float DragQuality = 0f;

		public static IFishController iFish;

		public static string FishType;

		public static bool AlarmIndicator = true;

		public static bool StandardUnit = false;

		public static string fishclass;

		public static bool isCommon;

		public static bool IgnoreYoung = false;

		public static bool IgnoreCommon = false;

		public static bool IgnoreTrophy = false;

		public static bool IgnoreUnique = false;
	}
}
