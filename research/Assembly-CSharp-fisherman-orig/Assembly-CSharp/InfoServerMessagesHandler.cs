using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using ObjectModel.Common;
using UnityEngine;

public class InfoServerMessagesHandler : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		if (this.AchivPrefab != null)
		{
			this.AchivPrefab.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		}
		PhotonConnectionFactory.Instance.OnItemGained += this.Instance_OnItemGained;
		PhotonConnectionFactory.Instance.OnItemGifted += this.OnItemGifted;
		PhotonConnectionFactory.Instance.OnInteractedWithObject += this.PhotonServer_OnInteractedWithObject;
		PhotonConnectionFactory.Instance.OnTournamentResult += this.PhotonServer_OnTournamentResult;
		PhotonConnectionFactory.Instance.OnTournamentCancelled += this.PhotonServer_OnTournamentCancelled;
		PhotonConnectionFactory.Instance.OnTournamentStarted += this.OnTournamentStarted;
		PhotonConnectionFactory.Instance.OnAdsTriggered += this.Instance_OnAdsTriggered;
		PhotonConnectionFactory.Instance.OnProductDelivered += this.OnProductDelivered;
		PhotonConnectionFactory.Instance.OnEndOfDayResult += this.OnEndOfDayResult;
		PhotonConnectionFactory.Instance.OnEndOfMissionResult += this.OnEndOfMissionResult;
		PhotonConnectionFactory.Instance.OnPrimarySteamAuthenticationFailed += this.Instance_OnPrimarySteamAuthenticationFailed;
		PhotonConnectionFactory.Instance.OnReferralReward += this.Instance_OnReferralReward;
		PhotonConnectionFactory.Instance.OnReceiveNewRodSetup += this.OnRodSetupReceived;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed += this.OnAuthenticationFailed;
		if (!StaticUserData.DISABLE_MISSIONS)
		{
			PhotonConnectionFactory.Instance.MissionHintsReceived += this.MissionHintsReceived;
			PhotonConnectionFactory.Instance.OnMissionReward += this.OnMissionAccomplished;
		}
		PhotonConnectionFactory.Instance.OnAchivementGained += this.OnAchivementGained;
		PhotonConnectionFactory.Instance.OnBonusGained += this.OnBonusGained;
		PhotonConnectionFactory.Instance.OnLevelGained += this.OnLevelGained;
		PhotonConnectionFactory.Instance.OnXboxGoldAccountNeeded += this.OnXboxGoldAccountNeeded;
		PhotonConnectionFactory.Instance.OnTournamentTimeEnded += this.Instance_OnTournamentTimeEnded;
		PhotonConnectionFactory.Instance.OnUserCompetitionStarted += this.Instance_OnUserCompetitionStarted;
		PhotonConnectionFactory.Instance.OnUserCompetitionCancelled += this.Instance_OnUserCompetitionCancelled;
		PhotonConnectionFactory.Instance.OnUserCompetitionApprovedOnReview += this.Instance_OnUserCompetitionApprovedOnReview;
		PhotonConnectionFactory.Instance.OnUserCompetitionDeclinedOnReview += this.Instance_OnUserCompetitionDeclinedOnReview;
		PhotonConnectionFactory.Instance.OnUserCompetitionRemovedOnReview += this.Instance_OnUserCompetitionRemovedOnReview;
		PhotonConnectionFactory.Instance.OnMissionTimedOut += this.Instance_OnMissionTimedOut;
		PhotonConnectionFactory.Instance.OnMissionTrackStarted += this.Instance_OnMissionTrackStarted;
		PhotonConnectionFactory.Instance.OnMissionTrackStopped += this.Instance_OnMissionTrackStopped;
		PhotonConnectionFactory.Instance.OnFishSold += this.Instance_OnFishSold;
		PhotonConnectionFactory.Instance.ServerCachesRefreshed += this.Instance_ServerCachesRefreshed;
		PhotonConnectionFactory.Instance.OnWearedItemLostOnDeequip += this.Instance_OnWearedItemLostOnDeequip;
		PhotonConnectionFactory.Instance.OnUserCompetitionReverted += this.Instance_OnUserCompetitionReverted;
		PhotonConnectionFactory.Instance.OnGotProfile += this.OnGotProfile;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnTournamentTimeEnded -= this.Instance_OnTournamentTimeEnded;
		PhotonConnectionFactory.Instance.OnEndOfDayResult -= this.OnEndOfDayResult;
		PhotonConnectionFactory.Instance.OnEndOfMissionResult -= this.OnEndOfMissionResult;
		PhotonConnectionFactory.Instance.OnReferralReward -= this.Instance_OnReferralReward;
		PhotonConnectionFactory.Instance.OnAuthenticationFailed -= this.OnAuthenticationFailed;
		PhotonConnectionFactory.Instance.OnAchivementGained -= this.OnAchivementGained;
		PhotonConnectionFactory.Instance.OnBonusGained -= this.OnBonusGained;
		PhotonConnectionFactory.Instance.OnLevelGained -= this.OnLevelGained;
		PhotonConnectionFactory.Instance.OnXboxGoldAccountNeeded -= this.OnXboxGoldAccountNeeded;
		if (!StaticUserData.DISABLE_MISSIONS)
		{
			PhotonConnectionFactory.Instance.MissionHintsReceived -= this.MissionHintsReceived;
			PhotonConnectionFactory.Instance.OnMissionReward -= this.OnMissionAccomplished;
		}
		PhotonConnectionFactory.Instance.OnAdsTriggered -= this.Instance_OnAdsTriggered;
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.OnProductDelivered;
		PhotonConnectionFactory.Instance.OnItemGained -= this.Instance_OnItemGained;
		PhotonConnectionFactory.Instance.OnItemGifted -= this.OnItemGifted;
		PhotonConnectionFactory.Instance.OnInteractedWithObject -= this.PhotonServer_OnInteractedWithObject;
		PhotonConnectionFactory.Instance.OnTournamentResult -= this.PhotonServer_OnTournamentResult;
		PhotonConnectionFactory.Instance.OnTournamentCancelled -= this.PhotonServer_OnTournamentCancelled;
		PhotonConnectionFactory.Instance.OnTournamentStarted -= this.OnTournamentStarted;
		PhotonConnectionFactory.Instance.OnUserCompetitionStarted -= this.Instance_OnUserCompetitionStarted;
		PhotonConnectionFactory.Instance.OnUserCompetitionCancelled -= this.Instance_OnUserCompetitionCancelled;
		PhotonConnectionFactory.Instance.OnUserCompetitionApprovedOnReview -= this.Instance_OnUserCompetitionApprovedOnReview;
		PhotonConnectionFactory.Instance.OnUserCompetitionDeclinedOnReview -= this.Instance_OnUserCompetitionDeclinedOnReview;
		PhotonConnectionFactory.Instance.OnUserCompetitionRemovedOnReview -= this.Instance_OnUserCompetitionRemovedOnReview;
		PhotonConnectionFactory.Instance.OnMissionTimedOut -= this.Instance_OnMissionTimedOut;
		PhotonConnectionFactory.Instance.OnMissionTrackStarted -= this.Instance_OnMissionTrackStarted;
		PhotonConnectionFactory.Instance.OnMissionTrackStopped -= this.Instance_OnMissionTrackStopped;
		PhotonConnectionFactory.Instance.OnFishSold -= this.Instance_OnFishSold;
		PhotonConnectionFactory.Instance.ServerCachesRefreshed -= this.Instance_ServerCachesRefreshed;
		PhotonConnectionFactory.Instance.OnWearedItemLostOnDeequip -= this.Instance_OnWearedItemLostOnDeequip;
		PhotonConnectionFactory.Instance.OnUserCompetitionReverted -= this.Instance_OnUserCompetitionReverted;
		PhotonConnectionFactory.Instance.OnGotProfile -= this.OnGotProfile;
	}

	private void Instance_OnReferralReward(bool isInitial, bool isInvited, AchivementInfo info)
	{
		if (info.ItemRewards != null && info.ItemRewards.Length != 0)
		{
			this._isInitial = isInitial;
			this._isInvited = isInvited;
			this._amount1 = info.Amount1;
			this._amount2 = info.Amount2;
			this._products = info.ProductReward;
			PhotonConnectionFactory.Instance.OnGotItems += this.OnGotItems;
			PhotonConnectionFactory.Instance.GetItemsByIds(info.ItemRewards.Select((ItemReward item) => item.ItemId).ToArray<int>(), this.ItemSubscriberId, true);
			return;
		}
		this.ShowReferralReward(isInitial, isInvited, info.Amount1, info.Amount2, null, info.ProductReward);
	}

	private void OnGotItems(List<InventoryItem> items, int sunscriberId)
	{
		if (sunscriberId != this.ItemSubscriberId)
		{
			return;
		}
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
		this.ShowReferralReward(this._isInitial, this._isInvited, this._amount1, this._amount2, items, this._products);
	}

	private void ShowReferralReward(bool isInitial, bool isInvited, Amount amount1, Amount amount2, List<InventoryItem> items, ProductReward[] products)
	{
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetReferralMessage(isInitial, isInvited, amount1, amount2, items, products));
	}

	private void OnAuthenticationFailed(Failure failure)
	{
		if (failure.ErrorCode == 32594)
		{
			MessageFactory.InfoMessagesQueue.Enqueue(this.GetFatalProblem(ScriptLocalization.Get("SteamAuthError")));
			base.GetComponent<InfoMessageController>().CanShow = true;
			base.GetComponent<InfoMessageController>().ShowMandatory = true;
		}
	}

	private void Instance_OnPrimarySteamAuthenticationFailed()
	{
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetProblem(ScriptLocalization.Get("SteamAuthProblem")));
	}

	private void Instance_OnItemGained(IEnumerable<InventoryItem> items, bool announce)
	{
		if (!announce)
		{
			return;
		}
		IList<InventoryItem> list = (items as IList<InventoryItem>) ?? items.ToList<InventoryItem>();
		List<InventoryItem> storageExceededInventory = PhotonConnectionFactory.Instance.Profile.Inventory.StorageExceededInventory;
		for (int i = 0; i < list.Count; i++)
		{
			InventoryItem inventoryItem = list[i];
			if (inventoryItem.Storage != StoragePlaces.ParentItem)
			{
				bool flag = inventoryItem.ItemId == 0 && inventoryItem.Desc == "#GiveNoItemInteraction#";
				MessageFactory.InfoMessagesQueue.Enqueue(this.GetInteractedObjectResult((!flag) ? inventoryItem : null, null, null, storageExceededInventory.Contains(inventoryItem)));
			}
		}
	}

	private void OnAchivementGained(AchivementInfo achivement)
	{
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetAchivInfo(achivement));
	}

	private void OnBonusGained(BonusInfo bonus)
	{
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetDailyBonus(bonus));
	}

	private void OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		if (announce)
		{
			MessageFactory.InfoMessagesQueue.Enqueue(this.GetProductDelivery(product, false, false));
		}
	}

	private void OnXboxGoldAccountNeeded(XboxGoldAccountNeedReason reason)
	{
		string text = string.Empty;
		if (reason != XboxGoldAccountNeedReason.Buoy)
		{
			if (reason != XboxGoldAccountNeedReason.Gift)
			{
				if (reason == XboxGoldAccountNeedReason.Preset)
				{
					text = ScriptLocalization.Get("XboxGoldAccountPreset");
				}
			}
			else
			{
				text = ScriptLocalization.Get("XboxGoldAccountGift");
			}
		}
		else
		{
			text = ScriptLocalization.Get("XboxGoldAccountBuoy");
		}
		MessageFactory.InfoMessagesQueue.Enqueue(this.XboxGoldAccountNeeded(text));
	}

	internal void OnEndOfDayResult(PeriodStats result)
	{
		Action continuation = delegate
		{
			if (result != null && result.IsLastDay)
			{
				BackToLobbyClick.GoToLobby();
			}
		};
		if (result.ShouldDisplayDialog)
		{
			InfoMessage endDay = this.GetEndDay(result);
			AlphaFade component = endDay.GetComponent<AlphaFade>();
			component.HideFinished += delegate(object s, EventArgsAlphaFade e)
			{
				continuation();
			};
			MessageFactory.InfoMessagesQueue.Enqueue(endDay);
		}
		else
		{
			continuation();
		}
	}

	internal void OnEndOfMissionResult(PeriodStats result)
	{
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetEndMission(result));
	}

	private void PhotonServer_OnInteractedWithObject(IEnumerable<InventoryItem> itemsGained, int? amount, string currency)
	{
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetInteractedObjectResult((itemsGained != null && itemsGained.Count<InventoryItem>() != 0) ? itemsGained.ToList<InventoryItem>()[0] : null, amount, currency, false));
	}

	internal void OnItemGifted(Player player, IEnumerable<InventoryItem> items)
	{
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetItemGifted(items.ToList<InventoryItem>()[0], player));
	}

	internal void OnRodSetupReceived(RodSetup setup, string friendName)
	{
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetRodSetupSent(setup, friendName));
	}

	internal void OnMissionAccomplished(int missionId, string missionName, string congratsText, int? missionImage, AchivementInfo info)
	{
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetMissionAccomplished(missionName, congratsText, missionImage, info));
	}

	private void PhotonServer_OnTournamentResult(TournamentFinalResult info)
	{
		LogHelper.Log("___kocha OnTournamentResult KindId:{0} TournamentId:{1}", new object[] { info.KindId, info.TournamentId });
		if (info.KindId == 4)
		{
			if (TournamentManager.Instance != null)
			{
				TournamentManager.Instance.Refresh();
			}
			if (info.UserCompetition != null)
			{
				base.StartCoroutine(this.ShowTournamentResult(info));
			}
			else
			{
				Debug.LogError("UGC:OnGetUserCompetitionRefresh returned null value!!!!! ");
			}
		}
		else
		{
			if (TournamentManager.Instance != null)
			{
				TournamentManager.Instance.RefreshMyTournaments();
			}
			if (info.Tournament == null)
			{
				Debug.LogError("PhotonServerOnGotTournament returned null value!!!!! ");
			}
			else
			{
				base.StartCoroutine(this.ShowTournamentResult(info.Tournament));
			}
		}
	}

	private void PhotonServer_OnTournamentCancelled(TournamentCancelInfo info)
	{
		LogHelper.Log("___kocha OnTournamentCancelled TournamentId:{0}", new object[] { info.TournamentId });
		if (TournamentManager.Instance != null)
		{
			TournamentManager.Instance.RefreshMyTournaments();
		}
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetTournamentCanceledInfo(info));
	}

	private void Instance_OnTournamentTimeEnded(EndTournamentTimeResult result)
	{
		LogHelper.Log("___kocha OnTournamentTimeEnded TournamentId:{0}", new object[] { result.TournamentResult.TournamentId });
		if (TournamentManager.Instance != null)
		{
			TournamentManager.Instance.FullRefresh();
		}
		TournamentHelper.Instance_OnTournamentTimeEnded(result);
	}

	private void OnTournamentStarted(TournamentStartInfo info)
	{
		base.StartCoroutine(this.ShowTournamentStarted(info));
	}

	private IEnumerator ShowTournamentStarted(TournamentStartInfo info)
	{
		while (!CacheLibrary.MapCache.AllMapChachesInited)
		{
			yield return null;
		}
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetTournamentStartedInfo(info));
		yield break;
	}

	private void Instance_OnAdsTriggered(TargetedAdSlide[] ads)
	{
		base.StartCoroutine(this.ShowTargetedAds(ads));
	}

	private IEnumerator ShowTargetedAds(TargetedAdSlide[] ads)
	{
		while (!CacheLibrary.ProductCache.AreProductsAvailable)
		{
			yield return null;
		}
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetTargetedAds(ads));
		yield break;
	}

	private InfoMessage GetReferralMessage(bool isInitial, bool isInvited, Amount amount1, Amount amount2, List<InventoryItem> items, ProductReward[] products)
	{
		GameObject gameObject = this.PreparePanel(this.ReferralMessage);
		gameObject.GetComponent<RefferalDeliveryInit>().Init(isInitial, isInvited, amount1, amount2, items, products);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetTournamentCanceledInfo(TournamentCancelInfo info)
	{
		GameObject gameObject = this.PreparePanel(this.TournamentCanceledPrefab);
		gameObject.GetComponent<TournamentCanceledInit>().Init(info);
		return gameObject.GetComponent<InfoMessage>();
	}

	private IEnumerator ShowTournamentResult(Tournament tournament)
	{
		bool isCameraRouting = GameFactory.Player != null && GameFactory.Player.State == typeof(PlayerCameraRouting);
		InfoMessage curMsg = InfoMessageController.Instance.currentMessage;
		bool ugcWndActive = curMsg != null && (curMsg.MessageType == InfoMessageTypes.CompetitionDetails || curMsg.MessageType == InfoMessageTypes.SoldFishTimeIsUp);
		bool ugcWndInQueue = MessageFactory.InfoMessagesQueue.Any((InfoMessage p) => p.MessageType == InfoMessageTypes.CompetitionDetails || p.MessageType == InfoMessageTypes.SoldFishTimeIsUp);
		while (CacheLibrary.MapCache.CachedPonds == null || isCameraRouting || ugcWndActive || ugcWndInQueue)
		{
			yield return null;
		}
		TournamentHelper.ShowingTournamentDetails(tournament, true);
		yield break;
	}

	private IEnumerator ShowTournamentResult(TournamentFinalResult finalResult)
	{
		while (!CacheLibrary.MapCache.AllMapChachesInited)
		{
			yield return null;
		}
		TournamentHelper.ShowingTournamentDetails(finalResult);
		yield break;
	}

	private void MissionHintsReceived(List<HintMessage> messages)
	{
		foreach (HintMessage hintMessage in messages)
		{
			MissionEventType eventType = hintMessage.EventType;
			if (eventType != MissionEventType.MissionTaskHintMessageAdded)
			{
				if (eventType == MissionEventType.MissionTaskHintMessageRemoved)
				{
					this.HintSystem.RemoveHint(hintMessage);
				}
			}
			else
			{
				this.HintSystem.AddHint(hintMessage);
			}
		}
	}

	private InfoMessage GetItemGifted(InventoryItem item, Player player)
	{
		GameObject gameObject = this.PreparePanel(this.InteractResult);
		gameObject.GetComponent<InteractResultInit>().InitGift(player, item);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetRodSetupSent(RodSetup setup, string friendName)
	{
		GameObject gameObject = this.PreparePanel(this.RodSetupDelivered);
		gameObject.GetComponent<RodSetupDeliveredInit>().Init(setup, friendName);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetMissionAccomplished(string missionName, string congratsText, int? missionImage, AchivementInfo info)
	{
		GameObject gameObject;
		if ((info.ItemRewards != null && info.ItemRewards.Length > 0) || (info.LicenseReward != null && info.LicenseReward.Length > 0) || (info.ProductReward != null && info.ProductReward.Length > 0))
		{
			gameObject = this.PreparePanel(this.MissionAccomplished);
		}
		else
		{
			gameObject = this.PreparePanel(this.MissionAccomplishedWithoutProducts);
		}
		gameObject.GetComponent<MissionAccomplishedInit>().Init(missionName, congratsText, missionImage, info, InfoMessageTypes.MissionAccomplished, null, null);
		InfoMessage component = gameObject.GetComponent<InfoMessage>();
		component.MessageType = InfoMessageTypes.MissionAccomplished;
		return component;
	}

	private InfoMessage GetMissionFailed(string missionName, int? missionImage)
	{
		GameObject gameObject = this.PreparePanel(this.MissionFailed);
		gameObject.GetComponent<MissionFailed>().Init(missionName, missionImage, null);
		InfoMessage component = gameObject.GetComponent<InfoMessage>();
		component.MessageType = InfoMessageTypes.MissionFailed;
		return component;
	}

	private InfoMessage GetAchivInfo(AchivementInfo achivement)
	{
		GameObject gameObject;
		if ((achivement.ItemRewards != null && achivement.ItemRewards.Length > 0) || (achivement.LicenseReward != null && achivement.LicenseReward.Length > 0) || (achivement.ProductReward != null && achivement.ProductReward.Length > 0))
		{
			gameObject = this.PreparePanel(this.AchivPrefab);
		}
		else
		{
			gameObject = this.PreparePanel(this.AchivWithouProductsPrefab);
		}
		gameObject.GetComponent<AchivRewardsInit>().Init(achivement);
		string externalAchievementId = achivement.ExternalAchievementId;
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetDailyBonus(BonusInfo bonus)
	{
		GameObject gameObject = this.PreparePanel(this.DailyBonusPrefab);
		gameObject.GetComponent<DailyBonusInit>().Init(bonus);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetProductDelivery(ProfileProduct product, bool isInExceed = false, bool nothingDelivered = false)
	{
		GameObject gameObject = this.PreparePanel(this.InteractResult);
		if (nothingDelivered)
		{
			gameObject.GetComponent<InteractResultInit>().InitEmpty();
		}
		else
		{
			gameObject.GetComponent<InteractResultInit>().InitProductDelivered(product, isInExceed);
		}
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage XboxGoldAccountNeeded(string messageText)
	{
		GameObject gameObject = this.PreparePanel(this.ProblemMessage);
		gameObject.GetComponent<ProblemMessageInit>().Init(messageText);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetProblem(string messageText)
	{
		GameObject gameObject = this.PreparePanel(this.ProblemMessage);
		gameObject.GetComponent<ProblemMessageInit>().Init(messageText);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetFatalProblem(string messageText)
	{
		GameObject gameObject = this.PreparePanel(this.FatalProblemMessage);
		gameObject.GetComponent<FatalProblemMessageInit>().Init(messageText);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetInteractedObjectResult(InventoryItem itemGained, int? amount, string currency, bool excess = false)
	{
		GameObject gameObject = this.PreparePanel(this.InteractResult);
		gameObject.GetComponent<InteractResultInit>().InitItemGained(itemGained, amount, currency, excess);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetTournamentStartedInfo(TournamentStartInfo info)
	{
		GameObject gameObject = this.PreparePanel(this.InteractResult);
		gameObject.GetComponent<InteractResultInit>().InitCompetitionStarted(info);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetTargetedAds(TargetedAdSlide[] ads)
	{
		ClientDebugHelper.Log(ProfileFlag.GameLogic, string.Format("GetTargetedAds - {0}", string.Join(";", ads.Select((TargetedAdSlide p) => string.Format("ItemId:{0} ProductId:{1} End:{2}", p.ItemId, p.ProductId, p.End)).ToArray<string>())));
		GameObject gameObject = this.PreparePanel(this.TargetedAdsdPrefab);
		gameObject.GetComponent<TargetedAdsController>().Init(ads.ToList<TargetedAdSlide>());
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetEndDay(PeriodStats result)
	{
		GameObject gameObject = this.PreparePanel(this.DayResultPrefab);
		gameObject.GetComponent<DayResultInit>().Init(result);
		return gameObject.GetComponent<InfoMessage>();
	}

	private InfoMessage GetEndMission(PeriodStats result)
	{
		GameObject gameObject = this.PreparePanel(this.MissionResultPrefab);
		gameObject.GetComponent<MissionResultInit>().Init(result);
		return gameObject.GetComponent<InfoMessage>();
	}

	private GameObject PreparePanel(InfoMessage im)
	{
		GameObject gameObject = GUITools.AddChild(base.gameObject, im.gameObject);
		gameObject.transform.localScale = new Vector3(0f, 0f, 0f);
		gameObject.SetActive(false);
		gameObject.GetComponent<AlphaFade>().FastHidePanel();
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		return gameObject;
	}

	private void OnLevelGained(LevelInfo o)
	{
		CacheLibrary.MapCache.RefreshMetadataForUserCompetition();
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetLevelInfo(o));
	}

	private InfoMessage GetLevelInfo(LevelInfo levelInfo)
	{
		Pond pond2 = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond pond) => pond.OriginalMinLevel == PhotonConnectionFactory.Instance.Profile.Level && pond.IsVisible);
		GameObject gameObject = this.PreparePanel((pond2 == null) ? this.NewLevel : this.NewLevelWithPond);
		gameObject.GetComponent<MissionAccomplishedInit>().Init(string.Format(ScriptLocalization.Get((!levelInfo.IsLevel) ? "LockedRank" : "NewLevel").ToUpper(), levelInfo.Level), string.Empty, null, new AchivementInfo
		{
			Experience = 0,
			Amount1 = levelInfo.Amount1,
			Amount2 = levelInfo.Amount2
		}, InfoMessageTypes.LevelUp, levelInfo.GlobalItemsForLevel, pond2);
		InfoMessage component = gameObject.GetComponent<InfoMessage>();
		component.MessageType = InfoMessageTypes.LevelUp;
		return component;
	}

	public void ShowCanceledMsg(string title, string message, TournamentCanceledInit.MessageTypes type, DateTime endDate, Action okFunc = null, bool forceShow = false)
	{
		GameObject gameObject = this.PreparePanel(this.TournamentCanceledPrefab);
		if (endDate == DateTime.MinValue)
		{
			gameObject.GetComponent<TournamentCanceledInit>().Init(title, message, type);
		}
		else
		{
			gameObject.GetComponent<TournamentCanceledInit>().Init(title, message, type, endDate);
		}
		gameObject.GetComponent<AlphaFade>().HideFinished += delegate(object s, EventArgsAlphaFade e)
		{
			if (okFunc != null)
			{
				okFunc();
			}
		};
		if (forceShow)
		{
			InfoMessageController.Instance.ForceShow(gameObject.GetComponent<InfoMessage>());
		}
		else
		{
			MessageFactory.InfoMessagesQueue.Enqueue(gameObject.GetComponent<InfoMessage>());
		}
	}

	private void Instance_OnUserCompetitionCancelled(UserCompetitionCancellationMessage o)
	{
		LogHelper.Log("___kocha OnUserCompetitionCancelled TournamentId:{0}", new object[] { o.TournamentId });
		if (TournamentManager.Instance != null)
		{
			TournamentManager.Instance.Refresh();
		}
		GameObject gameObject = this.PreparePanel(this.TournamentCanceledPrefab);
		gameObject.GetComponent<TournamentCanceledInit>().Init(o);
		MessageFactory.InfoMessagesQueue.Enqueue(gameObject.GetComponent<InfoMessage>());
	}

	private void Instance_OnUserCompetitionStarted(UserCompetitionStartMessage o)
	{
		base.StartCoroutine(this.ShowUserCompetitionStarted(o));
	}

	private IEnumerator ShowUserCompetitionStarted(UserCompetitionStartMessage o)
	{
		while (!CacheLibrary.MapCache.AllMapChachesInited)
		{
			yield return null;
		}
		GameObject currentPanel = this.PreparePanel(this.InteractResult);
		currentPanel.GetComponent<InteractResultInit>().InitCompetitionStarted(o);
		MessageFactory.InfoMessagesQueue.Enqueue(currentPanel.GetComponent<InfoMessage>());
		yield break;
	}

	private void Instance_OnUserCompetitionRemovedOnReview(UserCompetitionReviewMessage competition)
	{
		UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("UGC_UserCompetitionRemovedOnReview"), UgcConsts.GetYellowTan(competition.NameCustom), UgcConsts.GetYellowTan(competition.ReviewComment)), TournamentCanceledInit.MessageTypes.Error, delegate
		{
		}, false);
	}

	private void Instance_OnUserCompetitionDeclinedOnReview(UserCompetitionReviewMessage competition)
	{
		UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("UGC_UserCompetitionDeclinedOnReview"), UgcConsts.GetYellowTan(competition.NameCustom), UgcConsts.GetYellowTan(competition.ReviewComment)), TournamentCanceledInit.MessageTypes.Warning, null, false);
	}

	private void Instance_OnUserCompetitionApprovedOnReview(UserCompetitionReviewMessage competition)
	{
		string text = ((!string.IsNullOrEmpty(competition.ReviewComment)) ? string.Format("\n{0}\n", UgcConsts.GetYellowTan(competition.ReviewComment)) : "\n");
		string text2 = string.Format(ScriptLocalization.Get("UGC_UserCompetitionApprovedOnReview"), UgcConsts.GetYellowTan(competition.NameCustom), text);
		UIHelper.ShowCanceledMsg(ScriptLocalization.Get("UGC_SuccessfulAction"), text2, TournamentCanceledInit.MessageTypes.Ok, null, false);
	}

	private void Instance_OnMissionTimedOut(int missionId, string missionName, int? missionImage)
	{
		LogHelper.Log("___kocha OnMissionTimedOut missionId:{0} missionName:{1} missionImage:{2}", new object[] { missionId, missionName, missionImage });
		MessageFactory.InfoMessagesQueue.Enqueue(this.GetMissionFailed(missionName, missionImage));
	}

	private void Instance_OnMissionTrackStopped(int missionId)
	{
	}

	private void Instance_OnMissionTrackStarted(int missionId)
	{
	}

	private void Instance_OnFishSold(int fishCount, int goldEarned, int silverEarned)
	{
		LogHelper.Log("___kocha Instance_OnFishSold fishCount:{0} goldEarned:{1} silverEarned:{2}", new object[] { fishCount, goldEarned, silverEarned });
		TournamentHelper.OnFishSoldHandler(fishCount, goldEarned, silverEarned);
	}

	private void Instance_ServerCachesRefreshed(string[] obj)
	{
		LogHelper.Log("___kocha Instance_ServerCachesRefreshed Caches:{0}", new object[] { string.Join(",", obj) });
		if (obj.Contains("Tournaments") && TournamentManager.Instance != null)
		{
			TournamentManager.Instance.RefreshMyTournaments();
		}
	}

	private void Instance_OnWearedItemLostOnDeequip(InventoryItem item)
	{
		GameObject gameObject = this.PreparePanel(this.InteractResult);
		gameObject.GetComponent<InteractResultInit>().InitItemWearedOnDeequip(item);
		MessageFactory.InfoMessagesQueue.Enqueue(gameObject.GetComponent<InfoMessage>());
	}

	private void Instance_OnUserCompetitionReverted(UserCompetitionRevertMessage o)
	{
		GameObject gameObject = this.PreparePanel(this.TournamentCanceledPrefab);
		gameObject.GetComponent<TournamentCanceledInit>().Init(o);
		MessageFactory.InfoMessagesQueue.Enqueue(gameObject.GetComponent<InfoMessage>());
	}

	private void OnGotProfile(Profile profile)
	{
		MeasuringSystemManager.ChangeMeasuringSystem();
	}

	public HintSystem HintSystem;

	public InfoMessage AchivPrefab;

	public InfoMessage AchivWithouProductsPrefab;

	public InfoMessage DailyBonusPrefab;

	public InfoMessage ProductDelivery;

	public InfoMessage InteractResult;

	public InfoMessage TournamentResultPrefab;

	public InfoMessage TournamentCanceledPrefab;

	public InfoMessage TargetedAdsdPrefab;

	public InfoMessage DayResultPrefab;

	public InfoMessage MissionResultPrefab;

	public InfoMessage ProblemMessage;

	public InfoMessage FatalProblemMessage;

	public InfoMessage ReferralMessage;

	public InfoMessage RodSetupDelivered;

	public InfoMessage MissionAccomplished;

	public InfoMessage MissionAccomplishedWithoutProducts;

	public InfoMessage NewLevel;

	public InfoMessage NewLevelWithPond;

	public InfoMessage MissionFailed;

	private int ItemSubscriberId = 10;

	private Amount _amount1;

	private Amount _amount2;

	private bool _isInitial;

	private bool _isInvited;

	private ProductReward[] _products;
}
