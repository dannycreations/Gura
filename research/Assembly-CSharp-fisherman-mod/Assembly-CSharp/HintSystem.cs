using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using ObjectModel.Common;
using UnityEngine;

public class HintSystem : MonoBehaviour
{
	private void Start()
	{
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		this._missionWidget = Object.Instantiate<MissionWidgetManager>(this._missionWidgetPrefab, Vector3.zero, Quaternion.identity, base.transform);
		PhotonConnectionFactory.Instance.MissionProgressReceived += this.Instance_MissionProgressReceived;
		ScreenManager.Instance.OnTransfer += this.Instance_OnTransfer;
	}

	private void OnDestroy()
	{
		base.StopAllCoroutines();
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		this.UnsubscribeGetMission();
		PhotonConnectionFactory.Instance.MissionProgressReceived -= this.Instance_MissionProgressReceived;
		if (InfoMessageController.Instance != null)
		{
			InfoMessageController.Instance.OnActivate -= this.Instance_OnActivate;
		}
		ScreenManager.Instance.OnTransfer -= this.Instance_OnTransfer;
		if (this._missionWidget != null)
		{
			Object.Destroy(this._missionWidget.gameObject);
		}
	}

	public static void RegisterItemId(Transform tr, InventoryItem item)
	{
		if (!HintSystem.ItemIds.ContainsKey(item.ItemId))
		{
			HintSystem.ItemIds.Add(item.ItemId, new List<HintSystem.TransformItemPair>());
		}
		HintSystem.TransformItemPair transformItemPair = HintSystem.ItemIds[item.ItemId].FirstOrDefault((HintSystem.TransformItemPair x) => x.transform == tr);
		if (transformItemPair != null)
		{
			transformItemPair.item = item;
		}
		else
		{
			HintSystem.TransformItemPair transformItemPair2 = new HintSystem.TransformItemPair
			{
				transform = tr,
				item = item
			};
			HintSystem.ItemIds[item.ItemId].Add(transformItemPair2);
		}
	}

	public static void UnregisterItemId(int itemId, Transform tr)
	{
		if (!HintSystem.ItemIds.ContainsKey(itemId))
		{
			return;
		}
		HintSystem.TransformItemPair transformItemPair = HintSystem.ItemIds[itemId].FirstOrDefault((HintSystem.TransformItemPair x) => x.transform == tr);
		if (transformItemPair != null)
		{
			HintSystem.ItemIds[itemId].Remove(transformItemPair);
		}
	}

	public static void RegisterCategories(List<string> names, Transform transform, InventoryItem item = null)
	{
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		foreach (string text in names)
		{
			if (!string.IsNullOrEmpty(text))
			{
				if (!HintSystem.Categories.ContainsKey(text))
				{
					HintSystem.Categories.Add(text, new List<HintSystem.TransformItemPair>());
				}
				HintSystem.TransformItemPair transformItemPair = HintSystem.Categories[text].FirstOrDefault((HintSystem.TransformItemPair x) => x.transform == transform);
				if (transformItemPair == null)
				{
					HintSystem.TransformItemPair transformItemPair2 = new HintSystem.TransformItemPair();
					transformItemPair2.transform = transform;
					transformItemPair2.item = item;
					HintSystem.Categories[text].Add(transformItemPair2);
				}
				else
				{
					transformItemPair.item = item;
				}
			}
		}
	}

	public static void UnregisterCategories(List<string> names, Transform transform)
	{
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		foreach (string text in names)
		{
			if (!string.IsNullOrEmpty(text))
			{
				if (HintSystem.Categories.ContainsKey(text))
				{
					HintSystem.TransformItemPair transformItemPair = HintSystem.Categories[text].FirstOrDefault((HintSystem.TransformItemPair x) => x.transform == transform);
					if (transformItemPair != null)
					{
						HintSystem.Categories[text].Remove(transformItemPair);
					}
					if (HintSystem.Categories[text].Count == 0)
					{
						HintSystem.Categories.Remove(text);
					}
				}
			}
		}
	}

	private void Awake()
	{
		HintSystem.Instance = this;
		this._mission = null;
		PhotonConnectionFactory.Instance.GameScreenChanged += this.SetCurrentGameScreen;
		PhotonConnectionFactory.Instance.ActiveMissionGet += this.OnGotActiveMission;
		PhotonConnectionFactory.Instance.ActiveMissionGetFailed += this.OnGotActiveMissionFailed;
		PhotonConnectionFactory.Instance.ActiveMissionChanged += this.ActiveMissionChanged;
		PhotonConnectionFactory.Instance.OnTutorialFinished += this.RefreshActiveMission;
		if (StaticUserData.DISABLE_MISSIONS)
		{
			base.enabled = false;
			return;
		}
		this.TextHintParent = Object.Instantiate<TextHintParent>(this.TextHintsParentPrefab, base.transform);
		base.StartCoroutine(this.WaitForLoadingAndGetActiveMission());
	}

	private IEnumerator WaitForLoadingAndGetActiveMission()
	{
		while (!CacheLibrary.AllChachesInited)
		{
			yield return null;
		}
		if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.IsAuthenticated && (PhotonConnectionFactory.Instance.IsConnectedToMaster || PhotonConnectionFactory.Instance.IsConnectedToGameServer))
		{
			PhotonConnectionFactory.Instance.GetActiveMission();
		}
		yield break;
	}

	public void OnDisable()
	{
		PhotonConnectionFactory.Instance.GameScreenChanged -= this.SetCurrentGameScreen;
		PhotonConnectionFactory.Instance.ActiveMissionGet -= this.OnGotActiveMission;
		PhotonConnectionFactory.Instance.ActiveMissionGetFailed -= this.OnGotActiveMissionFailed;
		PhotonConnectionFactory.Instance.ActiveMissionChanged -= this.ActiveMissionChanged;
		PhotonConnectionFactory.Instance.OnTutorialFinished -= this.RefreshActiveMission;
	}

	public bool IsNewMissionWidgetActive
	{
		get
		{
			return this._missionWidget.MissionId != 0 && ClientMissionsManager.CurrentWidgetTaskId == null;
		}
	}

	private void ActiveMissionChanged(int missionId)
	{
		if (missionId > 0)
		{
			base.StartCoroutine(this.WaitForLoadingAndGetNewMission(missionId));
		}
		base.StartCoroutine(this.WaitForLoadingAndGetActiveMission());
	}

	private IEnumerator WaitForLoadingAndGetNewMission(int missionId)
	{
		while (!CacheLibrary.AllChachesInited)
		{
			yield return null;
		}
		MissionWidgetManager.SetPlayerPrefs(missionId);
		if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.IsAuthenticated && (PhotonConnectionFactory.Instance.IsConnectedToMaster || PhotonConnectionFactory.Instance.IsConnectedToGameServer))
		{
			PhotonConnectionFactory.Instance.MissionGet += this.Instance_MissionGet;
			PhotonConnectionFactory.Instance.MissionGetFailed += this.Instance_MissionGetFailed;
			PhotonConnectionFactory.Instance.GetMission(missionId);
		}
		yield break;
	}

	private void Instance_MissionGet(MissionOnClient mission)
	{
		this.UnsubscribeGetMission();
		this.ShowInfoMsgNewMission(mission);
	}

	private void Instance_MissionGetFailed(Failure failure)
	{
		Debug.LogErrorFormat("HintSystem:GetMission ErrorCode:{0} ErrorMessage:{1}", new object[] { failure.ErrorCode, failure.ErrorMessage });
		this.UnsubscribeGetMission();
	}

	private void UnsubscribeGetMission()
	{
		PhotonConnectionFactory.Instance.MissionGet -= this.Instance_MissionGet;
		PhotonConnectionFactory.Instance.MissionGetFailed -= this.Instance_MissionGetFailed;
	}

	private void RefreshActiveMission()
	{
		base.StartCoroutine(this.WaitForLoadingAndGetActiveMission());
	}

	private void OnGotActiveMission(MissionOnClient mission)
	{
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		ClientMissionsManager.Instance.CurrentTrackedMission = mission;
		if (this._mission == null || mission == null || this._mission.MissionId != mission.MissionId)
		{
			this.MissionWidgetPushMission();
		}
		this._mission = mission;
	}

	private void ShowInfoMsgNewMission(MissionOnClient mission)
	{
		if (mission == null)
		{
			return;
		}
		if (ScreenManager.Instance != null && ScreenManager.Instance.GameScreen == GameScreenType.Missions)
		{
			LogHelper.Log("ShowInfoMsgNewMission - skip window for Missions game screen");
			return;
		}
		GameObject gameObject = GUITools.AddChild(base.gameObject, this._missionNewPrefab);
		gameObject.transform.localScale = Vector3.zero;
		gameObject.SetActive(false);
		gameObject.GetComponent<AlphaFade>().FastHidePanel();
		RectTransform component = gameObject.GetComponent<RectTransform>();
		RectTransform rectTransform = component;
		Vector2 zero = Vector2.zero;
		component.sizeDelta = zero;
		rectTransform.anchoredPosition = zero;
		Reward reward = mission.Reward;
		if (reward == null)
		{
			reward = new Reward();
		}
		gameObject.GetComponent<MissionAccomplishedInit>().Init(mission.Name, mission.Description, mission.ThumbnailBID, new AchivementInfo
		{
			ItemRewards = reward.GetItemRewards(),
			LicenseReward = reward.GetLicenseRewards(),
			ProductReward = reward.GetProductRewards(),
			Experience = reward.Experience,
			Amount1 = new Amount
			{
				Currency = reward.Currency1,
				Value = ((reward.Money1 == null) ? 0 : ((int)reward.Money1.Value))
			},
			Amount2 = new Amount
			{
				Currency = reward.Currency2,
				Value = ((reward.Money2 == null) ? 0 : ((int)reward.Money2.Value))
			}
		}, InfoMessageTypes.MissionNew, null, null);
		InfoMessage component2 = gameObject.GetComponent<InfoMessage>();
		component2.MessageType = InfoMessageTypes.MissionNew;
		MessageFactory.InfoMessagesQueue.Enqueue(component2);
	}

	private void OnGotActiveMissionFailed(Failure failure)
	{
		DebugUtility.Missions.Important(failure.FullErrorInfo, new object[0]);
		PhotonConnectionFactory.Instance.GetActiveMission();
	}

	public void PushWidgetTimer(MissionWidgetManager.TimerWidget tw)
	{
		this._missionWidget.PushTimer(tw);
	}

	public void ClearWidgetTimer()
	{
		this._missionWidget.ClearTimer();
	}

	private void SetCurrentGameScreen(GameScreenType type, GameScreenTabType tab)
	{
		this.ScreenType = type;
		this.ScreenTab = tab;
		this._missionWidget.SetCurrentGameScreen(this.ScreenType);
	}

	private void Instance_MissionProgressReceived(List<MissionTaskOnClient> messagesProgress)
	{
		if (ClientMissionsManager.Instance.CurrentTrackedMission != null && messagesProgress[0].MissionId == ClientMissionsManager.Instance.CurrentTrackedMission.MissionId)
		{
			this.MissionWidgetPushTask();
		}
	}

	private void MissionWidgetPushMission()
	{
		if (ClientMissionsManager.Instance.CurrentTrackedMission == null)
		{
			HintSystem.hasNoMissions = true;
			this._missionWidget.ClearAll(false);
			return;
		}
		if (!(InfoMessageController.Instance.currentMessage != null) || InfoMessageController.Instance.currentMessage.MessageType != InfoMessageTypes.MissionAccomplished)
		{
			if (!MessageFactory.InfoMessagesQueue.Any((InfoMessage p) => p.MessageType == InfoMessageTypes.MissionAccomplished))
			{
				if (HintSystem.hasNoMissions)
				{
					this._missionWidget_PushMisssion();
					HintSystem.hasNoMissions = false;
					return;
				}
				if (this.ScreenType != GameScreenType.Undefined)
				{
					if (ClientMissionsManager.Instance.CurrentTrackedMission.MissionId != this._missionWidget.MissionId)
					{
						this._missionWidget.OnHide += this._missionWidget_PushMisssion;
						this._missionWidget.ClearAll(false);
					}
				}
				else
				{
					this._missionWidget.ClearAll(false);
				}
				return;
			}
		}
		InfoMessageController.Instance.OnActivate += this.Instance_OnActivate;
	}

	private void _missionWidget_PushMisssion()
	{
		this._missionWidget.OnHide -= this._missionWidget_PushMisssion;
		this.MissionWidgetPushTask();
	}

	private void MissionWidgetPushTask()
	{
		List<MissionTaskOnClient> list = ClientMissionsManager.Instance.CurrentTrackedMission.Tasks.FindAll((MissionTaskOnClient p) => !p.IsCompleted && !p.IsHidden);
		for (int i = 0; i < list.Count; i++)
		{
			this._missionWidget.Push(ClientMissionsManager.Instance.CurrentTrackedMission, new int?(list[i].TaskId), false);
		}
	}

	private void Instance_OnActivate(InfoMessageTypes messageType, bool isActive)
	{
		if (messageType == InfoMessageTypes.MissionAccomplished && !isActive)
		{
			InfoMessageController.Instance.OnActivate -= this.Instance_OnActivate;
			this._missionWidget_PushMisssion();
		}
	}

	private void Instance_OnTransfer(bool flag)
	{
		if (flag)
		{
			this._missionWidget.Pause(true);
		}
	}

	public void SpawnActiveMissionMarks()
	{
		foreach (ManagedHint managedHint in this.activeHints)
		{
			HintMessage message = managedHint.Message;
			if (message.ScenePosition != null)
			{
				Vector3 vector;
				vector..ctor(message.ScenePosition.X, message.ScenePosition.Y, message.ScenePosition.Z);
				Quaternion quaternion = Quaternion.identity;
				if (message.Rotation != null)
				{
					quaternion = Quaternion.Euler(message.Rotation.X, message.Rotation.Y, message.Rotation.Z);
				}
				if (InGameMap.Instance != null && message.ScreenType == GameScreenType.Map && message.MarkerState != GameMarkerState.Undefined)
				{
					InGameMap.Instance.AddMissionObject(this.hintId, vector, quaternion.eulerAngles, message.MarkerState, message.TaskName);
				}
				if (CompassManager.Instance != null && new HintArrowType3D[]
				{
					HintArrowType3D.Undefined,
					HintArrowType3D.Pointer,
					HintArrowType3D.Fish
				}.ToList<HintArrowType3D>().Contains(message.ArrowType3D))
				{
					CompassManager.Instance.RemoveObject(this.hintId);
					CompassManager.Instance.AddObject(this.hintId, vector, quaternion.eulerAngles, message.ArrowType3D);
				}
			}
		}
	}

	public void AddHint(HintMessage hint)
	{
		if (hint.ScenePosition != null)
		{
			Vector3 vector;
			vector..ctor(hint.ScenePosition.X, hint.ScenePosition.Y, hint.ScenePosition.Z);
			Quaternion quaternion = Quaternion.identity;
			if (hint.Rotation != null)
			{
				quaternion = Quaternion.Euler(hint.Rotation.X, hint.Rotation.Y, hint.Rotation.Z);
			}
			if (InGameMap.Instance != null && hint.ScreenType == GameScreenType.Map && hint.MarkerState != GameMarkerState.Undefined)
			{
				InGameMap.Instance.AddMissionObject(this.hintId, vector, quaternion.eulerAngles, hint.MarkerState, hint.TaskName);
			}
			if (CompassManager.Instance != null && new HintArrowType3D[]
			{
				HintArrowType3D.Undefined,
				HintArrowType3D.Pointer,
				HintArrowType3D.Fish
			}.ToList<HintArrowType3D>().Contains(hint.ArrowType3D))
			{
				CompassManager.Instance.AddObject(this.hintId, vector, quaternion.eulerAngles, hint.ArrowType3D);
			}
		}
		this.activeHints.Add(new ManagedHint(hint, this.hintId++, this));
	}

	public bool IsHintActive(int id)
	{
		ManagedHint managedHint = this.activeHints.FirstOrDefault((ManagedHint p) => p.parentId == id);
		return managedHint != null && managedHint.CanShow(-1);
	}

	public void RemoveHint(HintMessage hint)
	{
		ManagedHint managedHint = this.activeHints.FirstOrDefault((ManagedHint x) => x.Message.MessageId == hint.MessageId);
		if (managedHint == null)
		{
			return;
		}
		if (InGameMap.Instance != null)
		{
			InGameMap.Instance.RemoveMissionObject(managedHint.parentId);
		}
		if (CompassManager.Instance != null)
		{
			CompassManager.Instance.RemoveObject(managedHint.parentId);
		}
		managedHint.Deinit();
		this.activeHints.Remove(managedHint);
	}

	public static string GenerateHighlightSlotIdFromItemType(ItemSubTypes type)
	{
		switch (type)
		{
		case ItemSubTypes.Rod:
		case ItemSubTypes.TelescopicRod:
		case ItemSubTypes.MatchRod:
		case ItemSubTypes.SpinningRod:
		case ItemSubTypes.CastingRod:
		case ItemSubTypes.FeederRod:
		case ItemSubTypes.BottomRod:
		case ItemSubTypes.FlyRod:
		case ItemSubTypes.SpodRod:
			return "PD_Rod";
		case ItemSubTypes.Reel:
		case ItemSubTypes.SpinReel:
		case ItemSubTypes.LineRunningReel:
		case ItemSubTypes.CastReel:
		case ItemSubTypes.FlyReel:
			return "PD_Reel";
		case ItemSubTypes.Line:
		case ItemSubTypes.MonoLine:
		case ItemSubTypes.BraidLine:
		case ItemSubTypes.FlurLine:
			return "PD_Line";
		case ItemSubTypes.Hook:
		case ItemSubTypes.Bait:
		case ItemSubTypes.Lure:
		case ItemSubTypes.JigBait:
		case ItemSubTypes.FreshBait:
		case ItemSubTypes.CommonBait:
		case ItemSubTypes.BoilBait:
		case ItemSubTypes.InsectsWormBait:
		case ItemSubTypes.Worm:
		case ItemSubTypes.Grub:
		case ItemSubTypes.Shad:
		case ItemSubTypes.Tube:
		case ItemSubTypes.Craw:
			return "PD_LureBait";
		case ItemSubTypes.Bobber:
			return "PD_TackleBobber";
		case ItemSubTypes.Sinker:
			if (InitRods.Instance != null && InitRods.Instance.ActiveRod.PVASinker.gameObject.activeInHierarchy)
			{
				return "PD_Sinker";
			}
			return "PD_Feeder";
		case ItemSubTypes.RodCase:
			return "PD_RodCase";
		case ItemSubTypes.LuresBox:
			return "PD_LureBox";
		case ItemSubTypes.Waistcoat:
			return "PD_WaistCoat";
		case ItemSubTypes.Hat:
			return "PD_Hat";
		case ItemSubTypes.JigHead:
		case ItemSubTypes.SimpleHook:
		case ItemSubTypes.LongHook:
		case ItemSubTypes.BarblessHook:
		case ItemSubTypes.Spoon:
		case ItemSubTypes.Spinner:
		case ItemSubTypes.Spinnerbait:
		case ItemSubTypes.Cranckbait:
		case ItemSubTypes.Popper:
		case ItemSubTypes.Swimbait:
		case ItemSubTypes.Jerkbait:
		case ItemSubTypes.BassJig:
		case ItemSubTypes.Frog:
		case ItemSubTypes.BarblessJigHeads:
		case ItemSubTypes.CommonJigHeads:
		case ItemSubTypes.BarblessSpoons:
		case ItemSubTypes.BarblessSpinners:
		case ItemSubTypes.Walker:
			return "PD_Hooks";
		case ItemSubTypes.Boat:
		case ItemSubTypes.Kayak:
		case ItemSubTypes.Zodiak:
		case ItemSubTypes.MotorBoat:
			return "PD_Boat";
		case ItemSubTypes.Bell:
		case ItemSubTypes.CommonBell:
		case ItemSubTypes.ElectronicBell:
			return "PD_Bell";
		case ItemSubTypes.Leader:
		case ItemSubTypes.MonoLeader:
		case ItemSubTypes.FlurLeader:
		case ItemSubTypes.BraidLeader:
		case ItemSubTypes.SteelLeader:
		case ItemSubTypes.CarpLeader:
			return "PD_Leader";
		case ItemSubTypes.Keepnet:
		case ItemSubTypes.Stringer:
			return "PD_Keepnet";
		case ItemSubTypes.RodStand:
			return "PD_RodStand";
		case ItemSubTypes.Chum:
		case ItemSubTypes.ChumBase:
		case ItemSubTypes.ChumGroundbaits:
		case ItemSubTypes.ChumCarpbaits:
		case ItemSubTypes.ChumMethodMix:
			if (InitRods.Instance != null && InitRods.Instance.ActiveRod.SpodChumAdditional.gameObject.activeInHierarchy && InitRods.Instance.ActiveRod.Chum.InventoryItem != null)
			{
				return "PD_ChumAdditional";
			}
			return "PD_Chum";
		case ItemSubTypes.CageFeeder:
		case ItemSubTypes.FlatFeeder:
		case ItemSubTypes.PvaFeeder:
			return "PD_Feeder";
		case ItemSubTypes.SpodFeeder:
			return "PD_SpodFeeder";
		}
		Debug.LogWarning("Cant find paperdoll slot for type: " + type.ToString());
		return null;
	}

	public static List<string> GenerateShopPathByType(ItemSubTypes type)
	{
		List<string> list = new List<string>();
		switch (type)
		{
		case ItemSubTypes.Outfit:
		case ItemSubTypes.Boots:
		case ItemSubTypes.RescueVest:
		case ItemSubTypes.Hat:
		case ItemSubTypes.Clothing:
		case ItemSubTypes.Belt:
		case ItemSubTypes.Gloves:
		case ItemSubTypes.Talisman:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			list.Add("Sh_Tools_Outfits");
			break;
		case ItemSubTypes.Rod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			break;
		case ItemSubTypes.Reel:
			list.Add("Sh_Reels");
			list.Add("Sh_All_Reels");
			break;
		case ItemSubTypes.TerminalTackle:
			list.Add("Sh_T_Tackles");
			break;
		case ItemSubTypes.Line:
			list.Add("Sh_Lines");
			list.Add("Sh_All_Lines");
			break;
		case ItemSubTypes.Hook:
		case ItemSubTypes.LongHook:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_T_Tackles_Hooks");
			break;
		case ItemSubTypes.Bobber:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_T_Tackles_Bobbers");
			break;
		case ItemSubTypes.Sinker:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Sinkers");
			list.Add("Sh_T_Tackles_Sinkers_Simple");
			break;
		case ItemSubTypes.Bait:
		case ItemSubTypes.Lure:
		case ItemSubTypes.Jerkbait:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			break;
		case ItemSubTypes.JigBait:
			list.Add("Sh_Lures");
			list.Add("Sh_Lures_Jig_Baits");
			break;
		case ItemSubTypes.Tool:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			break;
		case ItemSubTypes.RodCase:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			list.Add("Sh_Tools_Rod_Cases");
			break;
		case ItemSubTypes.LuresBox:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			list.Add("Sh_Tools_Tackle_Boxes");
			break;
		case ItemSubTypes.Waistcoat:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			list.Add("Sh_Tools_Outfits");
			list.Add("Sh_Tools_Waistcoats");
			break;
		case ItemSubTypes.TelescopicRod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			list.Add("Sh_Rods_Telescopic");
			break;
		case ItemSubTypes.MatchRod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			list.Add("Sh_Rods_Matching");
			break;
		case ItemSubTypes.SpinningRod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			list.Add("Sh_Rods_Spinning");
			break;
		case ItemSubTypes.CastingRod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			list.Add("Sh_Rods_Casting");
			break;
		case ItemSubTypes.FeederRod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			list.Add("Sh_Rods_Feeder");
			break;
		case ItemSubTypes.BottomRod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			list.Add("Sh_Rods_Bottom");
			break;
		case ItemSubTypes.FlyRod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			list.Add("Sh_Rods_Fly");
			break;
		case ItemSubTypes.SpinReel:
			list.Add("Sh_Reels");
			list.Add("Sh_All_Reels");
			list.Add("Sh_Reels_Spin");
			break;
		case ItemSubTypes.LineRunningReel:
			list.Add("Sh_Reels");
			list.Add("Sh_All_Reels");
			break;
		case ItemSubTypes.CastReel:
			list.Add("Sh_Reels");
			list.Add("Sh_All_Reels");
			list.Add("Sh_Reels_Cast");
			break;
		case ItemSubTypes.FlyReel:
			list.Add("Sh_Reels");
			list.Add("Sh_All_Reels");
			break;
		case ItemSubTypes.FreshBait:
			list.Add("Sh_Baits");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_Baits_Living");
			break;
		case ItemSubTypes.JigHead:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Jigheads");
			break;
		case ItemSubTypes.MonoLine:
			list.Add("Sh_Lines");
			list.Add("Sh_All_Lines");
			list.Add("Sh_Lines_Mono");
			break;
		case ItemSubTypes.BraidLine:
			list.Add("Sh_Lines");
			list.Add("Sh_All_Lines");
			list.Add("Sh_Lines_Braid");
			break;
		case ItemSubTypes.FlurLine:
			list.Add("Sh_Lines");
			list.Add("Sh_All_Lines");
			list.Add("Sh_Lines_Fluoro");
			break;
		case ItemSubTypes.SimpleHook:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_T_Tackles_Hooks");
			list.Add("Sh_T_Tackles_Simple_Hooks");
			break;
		case ItemSubTypes.BarblessHook:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_T_Tackles_Hooks");
			list.Add("Sh_T_Tackles_Barbless_Hooks");
			break;
		case ItemSubTypes.CommonBait:
			list.Add("Sh_Baits");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_Baits_Common");
			break;
		case ItemSubTypes.BoilBait:
			list.Add("Sh_Baits");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_Baits_Boilies");
			break;
		case ItemSubTypes.InsectsWormBait:
			list.Add("Sh_Baits");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_Baits_Insect_Worm");
			break;
		case ItemSubTypes.Spoon:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Spoons");
			break;
		case ItemSubTypes.Spinner:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Spinners");
			break;
		case ItemSubTypes.Spinnerbait:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Spinnerbaits");
			break;
		case ItemSubTypes.Cranckbait:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Crankbaits");
			break;
		case ItemSubTypes.Popper:
		case ItemSubTypes.Frog:
		case ItemSubTypes.Walker:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Topwater");
			break;
		case ItemSubTypes.Swimbait:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Hard_Baits");
			list.Add("Sh_Lures_Swimbaits");
			break;
		case ItemSubTypes.BassJig:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Bass_Jigs");
			break;
		case ItemSubTypes.Worm:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Worm");
			break;
		case ItemSubTypes.Grub:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Grub");
			break;
		case ItemSubTypes.Shad:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Shad");
			break;
		case ItemSubTypes.Tube:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Tube");
			break;
		case ItemSubTypes.Craw:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Craw_Creatures");
			break;
		case ItemSubTypes.Boat:
			list.Add("Sh_Boats");
			break;
		case ItemSubTypes.Glasses:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			list.Add("Sh_Tools_Outfits");
			list.Add("Sh_Tools_Glasses");
			break;
		case ItemSubTypes.Bell:
		case ItemSubTypes.CommonBell:
		case ItemSubTypes.ElectronicBell:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Bells");
			break;
		case ItemSubTypes.Leader:
			list.Add("Sh_Lines");
			list.Add("Sh_Lines_Leaders");
			break;
		case ItemSubTypes.MonoLeader:
			list.Add("Sh_Lines");
			list.Add("Sh_Lines_Leaders");
			list.Add("Sh_Lines_Leaders_Mono");
			break;
		case ItemSubTypes.FlurLeader:
			list.Add("Sh_Lines");
			list.Add("Sh_Lines_Leaders");
			list.Add("Sh_Lines_Leaders_Fluoro");
			break;
		case ItemSubTypes.Keepnet:
			list.Add("Sh_Tools");
			list.Add("Sh_Tools_Keepnet");
			break;
		case ItemSubTypes.FishNet:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			list.Add("Sh_Tools_Fish_Holders");
			break;
		case ItemSubTypes.RodStand:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			list.Add("Sh_Tools_Outfits");
			list.Add("Sh_Tools_RodStand");
			break;
		case ItemSubTypes.Firework:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			list.Add("Sh_Tools_Rod_Cases");
			break;
		case ItemSubTypes.BarblessJigHeads:
			list.Add("Sh_Lures");
			list.Add("Sh_Lures_Barbless_Jig");
			break;
		case ItemSubTypes.CommonJigHeads:
			list.Add("Sh_Lures");
			list.Add("Sh_Lures_Common_Jig");
			break;
		case ItemSubTypes.BarblessSpoons:
			list.Add("Sh_Lures");
			list.Add("Sh_Lures_Barbless_Spoons");
			break;
		case ItemSubTypes.BarblessSpinners:
			list.Add("Sh_Lures");
			list.Add("Sh_Lures_Barbless_Spinners");
			break;
		case ItemSubTypes.Stringer:
			list.Add("Sh_Tools");
			list.Add("Sh_All_Tools");
			list.Add("Sh_Tools_Stringer");
			break;
		case ItemSubTypes.Kayak:
			list.Add("Sh_Boats");
			list.Add("Sh_Boats_Kayaks");
			break;
		case ItemSubTypes.Zodiak:
		case ItemSubTypes.MotorBoat:
			list.Add("Sh_Boats");
			list.Add("Sh_Boats_Motor");
			break;
		case ItemSubTypes.BassBoat:
			list.Add("Sh_Boats");
			list.Add("Sh_Boats_Bass");
			break;
		case ItemSubTypes.ChumBase:
			list.Add("Sh_Chums");
			list.Add("Sh_Chum_Groundbaits");
			break;
		case ItemSubTypes.ChumParticle:
			list.Add("Sh_Boats");
			list.Add("Sh_Chum_Particles");
			break;
		case ItemSubTypes.ChumAroma:
			list.Add("Sh_Chums");
			list.Add("Sh_Chum_Aromas");
			break;
		case ItemSubTypes.CarpRod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			list.Add("Sh_Rods_Carp");
			break;
		case ItemSubTypes.SpodRod:
			list.Add("Sh_Rods");
			list.Add("Sh_All_Rods");
			list.Add("Sh_Rods_Spod");
			break;
		case ItemSubTypes.CarpHook:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_T_Tackles_Hooks");
			list.Add("Sh_T_Tackles_Carp_Hooks");
			break;
		case ItemSubTypes.CarpLeader:
			list.Add("Sh_Lines");
			list.Add("Sh_Lines_Leaders");
			list.Add("Sh_Lines_Leaders_Carp");
			break;
		case ItemSubTypes.CageFeeder:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Feeders_All");
			list.Add("Sh_T_Tackles_Feeders");
			break;
		case ItemSubTypes.FlatFeeder:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Feeders_All");
			list.Add("Sh_T_Tackles_FlatFeeders");
			break;
		case ItemSubTypes.PvaFeeder:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Feeders_All");
			list.Add("Sh_T_Tackles_PVA");
			break;
		case ItemSubTypes.SpodFeeder:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Feeders_All");
			list.Add("Sh_T_Tackles_Spod");
			break;
		case ItemSubTypes.ChumGroundbaits:
			list.Add("Sh_Chums");
			list.Add("Sh_Chum_Feeder");
			break;
		case ItemSubTypes.ChumCarpbaits:
			list.Add("Sh_Chums");
			list.Add("Sh_Chum_Carpbait");
			break;
		case ItemSubTypes.CarolinaRig:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Rigs");
			list.Add("Sh_T_Tackles_Rigs_Carolina");
			break;
		case ItemSubTypes.ChumMethodMix:
			list.Add("Sh_Chums");
			list.Add("Sh_Chum_Method");
			break;
		case ItemSubTypes.TexasRig:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Rigs");
			list.Add("Sh_T_Tackles_Rigs_Texas");
			break;
		case ItemSubTypes.ThreewayRig:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Rigs");
			list.Add("Sh_T_Tackles_Rigs_Threeway");
			break;
		case ItemSubTypes.SpinningSinker:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Sinkers");
			list.Add("Sh_T_Tackles_BulletSinkers");
			break;
		case ItemSubTypes.OffsetHook:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_All_Float_Tackles");
			list.Add("Sh_T_Tackles_Hooks");
			list.Add("Sh_T_Tackles_Offset_Hooks");
			break;
		case ItemSubTypes.Tail:
			list.Add("Sh_Lures");
			list.Add("Sh_All_Lures");
			list.Add("Sh_Lures_Tail");
			break;
		case ItemSubTypes.DropSinker:
			list.Add("Sh_T_Tackles");
			list.Add("Sh_T_Tackles_Sinkers");
			list.Add("Sh_T_Tackles_DropSinkers");
			break;
		case ItemSubTypes.TitaniumLeader:
			list.Add("Sh_Lines");
			list.Add("Sh_Lines_Leaders");
			list.Add("Sh_Lines_Leaders_Titanium");
			break;
		}
		if (list == null || list.Count == 0)
		{
			Debug.LogWarning("Cant find shop path: " + type.ToString());
		}
		return list;
	}

	public void Update()
	{
		for (int i = 0; i < this.activeHints.Count; i++)
		{
			this.activeHints[i].Update();
		}
	}

	[SerializeField]
	private MissionWidgetManager _missionWidgetPrefab;

	private MissionWidgetManager _missionWidget;

	[SerializeField]
	private GameObject _missionNewPrefab;

	public GameScreenType ScreenType;

	public GameScreenTabType ScreenTab;

	public static Color BaseHintsColor = Color.cyan;

	public TextHintParent TextHintsParentPrefab;

	public PrefabContainer TextHint;

	public PrefabContainer TooltipHint;

	public PrefabContainer PinOutlineHint;

	public PrefabContainer LocationOutlineHint;

	public PrefabContainer CircleOutlineHint;

	public PrefabContainer RectOutlineHint;

	public PrefabContainer RectUnderlineHint;

	public PrefabContainer RectUnderlineTextHighlightHint;

	public PrefabContainer ArrowHint;

	public PrefabContainer DollHighlightHint;

	public PrefabContainer Bobber;

	public PrefabContainer Hook;

	public PrefabContainer RMB;

	public PrefabContainer LMB;

	public PrefabContainer ArrowHint3D;

	public PrefabContainer CrossHint3D;

	public PrefabContainer RingHint3DBold;

	public PrefabContainer RingHint3DNormal;

	public PrefabContainer RingHint3DSlim;

	public PrefabContainer HintColorImageParentPrefab;

	public PrefabContainer HintColorTextChildrenPrefab;

	public PrefabContainer HintColorImageChildrenPrefab;

	public PrefabContainer HintHUDBobberIndicatorBottomPrefab;

	public PrefabContainer HintHUDBobberIndicatorTopPrefab;

	public PrefabContainer HintHUDBobberIndicatorTimerPrefab;

	public PrefabContainer HUDLineRodReelIndicatorOnePrefab;

	public PrefabContainer HUDLineRodReelIndicatorThreePrefab;

	public PrefabContainer HUDFrictionSpeedPrefab;

	public PrefabContainer HUDFrictionPrefab;

	public PrefabContainer HUDCastSimplePrefab;

	public PrefabContainer HUDCastTargetPrefab;

	public PrefabContainer HUDBobberPrefab;

	public PrefabContainer AchivementsPrefab;

	public PrefabContainer PondLicensesTogglePrefab;

	public PrefabContainer FeederFishingIndicatorPrefab;

	public PrefabContainer BottomFishingIndicatorPrefab;

	public HintSystem.KeyPrefabPair[] buttonsPrefabs;

	public static Dictionary<string, HintElementId> ElementIds = new Dictionary<string, HintElementId>();

	public static Dictionary<string, HintSide> ElementPreferredSides = new Dictionary<string, HintSide>();

	public static Dictionary<int, List<HintSystem.TransformItemPair>> ItemIds = new Dictionary<int, List<HintSystem.TransformItemPair>>();

	public static Dictionary<string, List<HintSystem.TransformItemPair>> Categories = new Dictionary<string, List<HintSystem.TransformItemPair>>();

	private int hintId = 1111;

	public List<ManagedHint> activeHints = new List<ManagedHint>();

	[HideInInspector]
	public TextHintParent TextHintParent;

	private MissionOnClient _mission;

	public static HintSystem Instance;

	private static bool hasNoMissions = true;

	[Serializable]
	public class KeyPrefabPair
	{
		public HintKeyType Key;

		public PrefabContainer Prefab;
	}

	public class TransformItemPair
	{
		public Transform transform;

		public InventoryItem item;
	}

	public class TransformIdPair
	{
		public Transform transform;

		public int hintId;
	}
}
