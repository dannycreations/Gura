using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using ObjectModel.Tournaments;
using UnityEngine;
using UnityEngine.Events;

public static class TournamentHelper
{
	public static bool TOURNAMENT_ENDED_STILL_IN_ROOM
	{
		get
		{
			return PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.IsTutorialFinished && PhotonConnectionFactory.Instance.CurrentTournamentId != null && PhotonConnectionFactory.Instance.Profile.Tournament == null;
		}
	}

	public static bool IS_IN_TOURNAMENT
	{
		get
		{
			return PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.IsTutorialFinished && (PhotonConnectionFactory.Instance.Profile.Tournament != null || PhotonConnectionFactory.Instance.CurrentTournamentId != null);
		}
	}

	public static string CurrentChannelName
	{
		get
		{
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			if (profile.Tournament != null && !profile.Tournament.IsEnded && profile.Tournament.KindId == 4)
			{
				return UserCompetitionLogic.ChatChannelName(profile.Tournament.TournamentId);
			}
			return null;
		}
	}

	public static List<StoreProduct> GetProductsFromReward(Reward r)
	{
		ProductReward[] productRewards = r.GetProductRewards();
		if (productRewards != null)
		{
			IEnumerable<int> productsIds = productRewards.Select((ProductReward p) => p.ProductId);
			return CacheLibrary.ProductCache.Products.Where((StoreProduct p) => productsIds.Contains(p.ProductId)).ToList<StoreProduct>();
		}
		return new List<StoreProduct>();
	}

	public static List<ShopLicense> GetLicensesFromReward(Reward r)
	{
		LicenseRef[] licenseRewards = r.GetLicenseRewards();
		if (licenseRewards != null)
		{
			IEnumerable<int> licIds = licenseRewards.Select((LicenseRef p) => p.LicenseId);
			return CacheLibrary.MapCache.AllLicenses.Where((ShopLicense p) => licIds.Contains(p.LicenseId)).ToList<ShopLicense>();
		}
		return new List<ShopLicense>();
	}

	public static void CheckDesubscribe()
	{
		if (TournamentHelper.seriesSubscribed)
		{
			PhotonConnectionFactory.Instance.OnGotTournamentSerieInstances -= TournamentHelper.InstanceOnGotTournamentSerieInstances;
			PhotonConnectionFactory.Instance.OnGettingTournamentSerieInstancesFailed -= TournamentHelper.InstanceOnGettingTournamentSerieInstancesFailed;
			TournamentHelper.seriesSubscribed = false;
		}
	}

	public static void UpdateSeries()
	{
		if (!TournamentHelper.seriesSubscribed)
		{
			TournamentHelper.seriesSubscribed = true;
			PhotonConnectionFactory.Instance.OnGotTournamentSerieInstances += TournamentHelper.InstanceOnGotTournamentSerieInstances;
			PhotonConnectionFactory.Instance.OnGettingTournamentSerieInstancesFailed += TournamentHelper.InstanceOnGettingTournamentSerieInstancesFailed;
		}
		PhotonConnectionFactory.Instance.GetTournamentSerieInstances();
	}

	public static bool HasSeries
	{
		get
		{
			return TournamentHelper.SeriesInstances != null && TournamentHelper.SeriesInstances.Count > 0;
		}
	}

	private static void InstanceOnGettingTournamentSerieInstancesFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
		PhotonConnectionFactory.Instance.GetTournamentSerieInstances();
	}

	private static void InstanceOnGotTournamentSerieInstances(List<TournamentSerieInstance> series)
	{
		TournamentHelper.SeriesInstances = series;
		if (TournamentHelper.OnSeriesUpdated != null)
		{
			TournamentHelper.OnSeriesUpdated.Invoke();
		}
	}

	public static void Instance_OnTournamentTimeEnded(EndTournamentTimeResult endTournamentResult)
	{
		LogHelper.Log("___kocha OnTournamentTimeEnded GameScreen:{0} AllMapChachesInited:{1}", new object[]
		{
			ScreenManager.Instance.GameScreen,
			CacheLibrary.MapCache.AllMapChachesInited
		});
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile != null && profile.Tournament != null && profile.Tournament.TournamentId == endTournamentResult.TournamentResult.TournamentId && (profile.Tournament.EndDate - PhotonConnectionFactory.Instance.ServerUtcNow).TotalSeconds < 3.0)
		{
			UIAudioSourceListener.Instance.SportHorn();
		}
		TournamentIndividualResults result = endTournamentResult.TournamentResult;
		if (!endTournamentResult.Prematurely && ShowHudElements.Instance != null)
		{
			ShowHudElements.Instance.TournamentTimeEnded(result);
		}
		if (profile.Tournament.KindId != 4)
		{
			TournamentHelper.TournamentTimeEnded(result);
			return;
		}
		TournamentHelper.PrepareEndTournamentTime(MessageBoxList.Instance.SoldFishTimeIsUpMessagePrefab, ref TournamentHelper._messageEndTournamentTime);
		InfoMessage component = TournamentHelper._messageEndTournamentTime.GetComponent<InfoMessage>();
		component.MessageType = InfoMessageTypes.SoldFishTimeIsUp;
		if (!endTournamentResult.Prematurely)
		{
			InfoMessage infoMessage = component;
			infoMessage.ExecuteAfterShow = (Action)Delegate.Combine(infoMessage.ExecuteAfterShow, new Action(delegate
			{
				BlackScreenHandler.Hide();
			}));
			InfoMessageController.Instance.OnActivate += TournamentHelper.OnActivateInfoMessage;
			component.ShowDelayTime = 0.5f;
		}
		SoldFishTimeIsUpMessage component2 = TournamentHelper._messageEndTournamentTime.GetComponent<SoldFishTimeIsUpMessage>();
		bool isShow = false;
		component2.AlphaFade.ShowFinished += delegate(object sender, EventArgs args)
		{
			if (isShow)
			{
				return;
			}
			isShow = true;
			LogHelper.Log("___kocha SoldFishTimeIsUpMessage:ShowFinished Prematurely:{0}", new object[] { endTournamentResult.Prematurely });
			if (TransferToLocation.Instance != null)
			{
				TransferToLocation.Instance.TournamentTimeEnded(result);
				TransferToLocation.Instance.SetActivePondUi(false);
			}
			else if (MenuHelpers.Instance != null)
			{
				MenuHelpers.Instance.HideMenu(false, false, false);
			}
			StaticUserData.ChatController.LeaveChatChannel();
			if (ToggleChatController.Instance != null)
			{
				ToggleChatController.Instance.RemoveChannelChat();
			}
		};
		bool isHide = false;
		component2.AlphaFade.HideFinished += delegate(object sender, EventArgsAlphaFade fade)
		{
			if (isHide)
			{
				return;
			}
			isHide = true;
			if (TransferToLocation.Instance != null)
			{
				TransferToLocation.Instance.SetActivePondUi(true);
			}
			else if (MenuHelpers.Instance != null)
			{
				MenuHelpers.Instance.ShowMenu(false, false);
			}
			if (!ScreenManager.Instance.IsIn3D)
			{
				CursorManager.ResetCursor();
				CursorManager.ShowCursor();
			}
			if (ScreenManager.Instance.IsIn3D)
			{
				ShowHudElements.Instance.StartTimer(10f, delegate
				{
					if (!(InfoMessageController.Instance.currentMessage != null) || InfoMessageController.Instance.currentMessage.MessageType != InfoMessageTypes.CompetitionDetails)
					{
						if (MessageFactory.InfoMessagesQueue.All((InfoMessage w) => w.MessageType != InfoMessageTypes.CompetitionDetails))
						{
							LogHelper.Log("___kocha CloseTournamentDetailsWindow AUTO !!!");
							TournamentHelper.CloseTournamentDetailsWindow(true);
						}
					}
				});
			}
		};
		component2.Init(endTournamentResult);
		MessageFactory.InfoMessagesQueue.Enqueue(component);
	}

	private static void OnActivateInfoMessage(InfoMessageTypes t, bool f)
	{
		if (f && t == InfoMessageTypes.SoldFishTimeIsUp)
		{
			InfoMessageController.Instance.OnActivate -= TournamentHelper.OnActivateInfoMessage;
			if (GameFactory.Player != null && ScreenManager.Instance.IsIn3D)
			{
				BlackScreenHandler.Show(true, new float?(0.5f));
				GameFactory.Player.CameraRoutingTurnOn();
				ShowHudElements.Instance.HideHud();
			}
		}
	}

	private static void PrepareEndTournamentTime(GameObject prefab, ref GameObject container)
	{
		container = GUITools.AddChild(MessageBoxList.Instance.gameObject, prefab);
		RectTransform component = container.GetComponent<RectTransform>();
		component.anchoredPosition = Vector3.zero;
		component.sizeDelta = Vector2.zero;
		AlphaFade component2 = container.GetComponent<AlphaFade>();
		container.SetActive(false);
	}

	private static void EndFishSold()
	{
		if (ShowLocationInfo.Instance != null)
		{
			ShowLocationInfo.Instance.SelectRandomRoom();
		}
		if (HudTournamentHandler.IsInHUD)
		{
			GameFactory.Player.StartMoveToNewLocation();
			KeysHandlerAction.EscapeHandler(false);
		}
		if (TournamentHelper.FishSold != null)
		{
			TournamentHelper.FishSold();
		}
	}

	public static void TournamentTimeEnded(TournamentResult result)
	{
		if ((PondControllers.Instance == null || !PondControllers.Instance.IsInMenu) && GameFactory.Message != null)
		{
			GameFactory.Message.ShowEntryTimeIsOver();
		}
		TournamentHelper.PrepareEndTournamentTime(MessageBoxList.Instance.tournamentTimeEndedPrefab, ref TournamentHelper._messageEndTournamentTime);
		TournamentHelper._messageEndTournamentTime.GetComponent<AlphaFade>().FastHidePanel();
		TournamentTimeEndHandler component = TournamentHelper._messageEndTournamentTime.GetComponent<TournamentTimeEndHandler>();
		component.Init(result);
		MessageFactory.MessageBoxQueue.Enqueue(component);
	}

	public static void OnFishSoldHandler(int fishCount, int goldEarned, int silverEarned)
	{
		TournamentHelper.PrepareEndTournamentTime(MessageBoxList.Instance.fishSoldPrefab, ref TournamentHelper._messageFishSold);
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		FishCageContents fishCage = profile.FishCage;
		List<Fish> list;
		if (fishCage != null && fishCage.Fish != null)
		{
			list = fishCage.Fish.Select((CaughtFish x) => x.Fish).ToList<Fish>();
		}
		else
		{
			list = new List<Fish>();
		}
		List<Fish> list2 = list;
		AlphaFade component = TournamentHelper._messageFishSold.GetComponent<AlphaFade>();
		component.FastHidePanel();
		component.HideFinished += TournamentHelper.FishSold_HideFinished;
		TournamentHelper._messageFishSold.GetComponent<FishSoldInit>().Init(list2, new int?(silverEarned), new int?(goldEarned), new float?((fishCage == null) ? 0f : fishCage.SumFishExperience()), fishCount);
		MessageFactory.InfoMessagesQueue.Enqueue(TournamentHelper._messageFishSold.GetComponent<InfoMessage>());
	}

	private static void FishSold_HideFinished(object sender, EventArgsAlphaFade e)
	{
		TournamentHelper._messageFishSold.GetComponent<AlphaFade>().HideFinished -= TournamentHelper.FishSold_HideFinished;
		Object.Destroy(TournamentHelper._messageFishSold);
		TournamentHelper._messageFishSold = null;
		TournamentHelper.EndFishSold();
	}

	public static TournamentHelper.TournamentWeatherDesc GetTournamentWeather(Tournament t, WeatherDesc[] weathers)
	{
		if (weathers == null || weathers.Length == 0 || t == null)
		{
			return null;
		}
		TimeOfDay tournamentTimeOfDay;
		if (t.InGameStartHour != null)
		{
			tournamentTimeOfDay = new TimeSpan(t.InGameStartHour.Value, 0, 0).ToTimeOfDay();
		}
		else
		{
			tournamentTimeOfDay = new TimeSpan(0, t.InGameStartMinute, 0).ToTimeOfDay();
		}
		WeatherDesc weatherDesc = weathers.First((WeatherDesc x) => x.TimeOfDay == tournamentTimeOfDay.ToString());
		return TournamentHelper.GetTournamentWeather(weatherDesc);
	}

	public static TournamentHelper.TournamentWeatherDesc GetTournamentWeather(WeatherDesc weather)
	{
		string text = string.Empty;
		string text2 = weather.Pressure.ToUpper();
		if (text2 != null)
		{
			if (!(text2 == "HIGH"))
			{
				if (!(text2 == "MEDIUM"))
				{
					if (text2 == "LOW")
					{
						text = "\ue616";
					}
				}
				else
				{
					text = "\ue615";
				}
			}
			else
			{
				text = "\ue614";
			}
		}
		return new TournamentHelper.TournamentWeatherDesc
		{
			WeatherWindDirection = ((weather.WindDirection == null) ? null : ScriptLocalization.Get(weather.WindDirection)),
			WeatherWindSuffix = MeasuringSystemManager.WindSpeedSufix(),
			WeatherWindPower = MeasuringSystemManager.WindSpeed(weather.WindSpeed).ToString("F1"),
			PressureIcon = text,
			WeatherIcon = WeatherHelper.GetWeatherIcon(weather.Icon),
			WeatherTemperature = string.Format("{0}{1}", ((int)MeasuringSystemManager.Temperature((float)weather.AirTemperature)).ToString(CultureInfo.InvariantCulture), MeasuringSystemManager.TemperatureSufix()),
			WaterTemperature = string.Format("{0}{1}", ((int)MeasuringSystemManager.Temperature((float)weather.WaterTemperature)).ToString(CultureInfo.InvariantCulture), MeasuringSystemManager.TemperatureSufix())
		};
	}

	public static TournamentStatus GetCompetitionStatus(Tournament competition)
	{
		TournamentStatus tournamentStatus = TournamentStatus.Planned;
		if (competition.IsEnded || competition.EndDate < PhotonConnectionFactory.Instance.ServerUtcNow)
		{
			return TournamentStatus.Finished;
		}
		if (competition.IsDone)
		{
			return TournamentStatus.ScoreSet;
		}
		if (competition.IsActive)
		{
			if (!competition.IsRegistered || !competition.IsApproved)
			{
				return TournamentStatus.NotRegAndStarting;
			}
			if (competition.IsRegistered)
			{
				return TournamentStatus.RegAndStarting;
			}
		}
		if (competition.RegistrationStart < PhotonConnectionFactory.Instance.ServerUtcNow && competition.IsRegistered)
		{
			return TournamentStatus.Signed;
		}
		if (competition.RegistrationStart < PhotonConnectionFactory.Instance.ServerUtcNow && !competition.IsRegistered)
		{
			return TournamentStatus.Registration;
		}
		return tournamentStatus;
	}

	public static GameObject ShowWindowListCompetitions(WindowListCompetitions.WindowListContainerCompetitions o)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.WindowListCompetitionsPrefab);
		WindowListCompetitions component = gameObject.GetComponent<WindowListCompetitions>();
		component.Init(o);
		component.Open();
		return gameObject;
	}

	public static bool IsCompetition
	{
		get
		{
			return TournamentHelper.ProfileTournament.IsCompetition();
		}
	}

	public static ProfileTournament ProfileTournament
	{
		get
		{
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			return (profile == null) ? null : profile.Tournament;
		}
	}

	public static string GetCompetitionStatusName(TournamentStatus status)
	{
		string text = string.Empty;
		switch (status)
		{
		case TournamentStatus.Planned:
			text = ScriptLocalization.Get("PlannedStatusText");
			break;
		case TournamentStatus.Registration:
			text = ScriptLocalization.Get("RegOpenCaption");
			break;
		case TournamentStatus.Signed:
			text = ScriptLocalization.Get("TournamentSignedCaption");
			break;
		case TournamentStatus.RegAndStarting:
			text = ScriptLocalization.Get("RunningStatusText");
			break;
		case TournamentStatus.NotRegAndStarting:
			text = ScriptLocalization.Get("RunningStatusText");
			break;
		case TournamentStatus.ScoreSet:
			text = ScriptLocalization.Get("TournamentScoreSet");
			break;
		case TournamentStatus.Finished:
			text = ScriptLocalization.Get("FinishedStatusText");
			break;
		case TournamentStatus.Unavailable:
			text = ScriptLocalization.Get("UnavailableStatusText");
			break;
		}
		return text;
	}

	public static GameObject ShowingTournamentDetails(TournamentFinalResult finalResult)
	{
		return TournamentHelper.ShowingTournamentDetails(finalResult.UserCompetition, false, false, finalResult, false);
	}

	public static GameObject ShowingTournamentDetails(UserCompetitionPublic tournament, bool viewDetailsOnly, bool needRefresh, TournamentFinalResult finalResult = null, bool fromTournamentId = false)
	{
		bool isEnd = finalResult != null;
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.TournamentResultWindowPrefab);
		gameObject.SetActive(false);
		AlphaFade component = gameObject.GetComponent<AlphaFade>();
		bool isShow = false;
		component.ShowFinished += delegate(object sender, EventArgs args)
		{
			if (!isShow)
			{
				isShow = true;
				TournamentHelper.HideMenu();
			}
		};
		TournamentDetailsMessageNew component2 = gameObject.GetComponent<TournamentDetailsMessageNew>();
		if (isEnd)
		{
			component2.Init(finalResult);
		}
		else if (fromTournamentId)
		{
			component2.Init(tournament.TournamentId);
		}
		else
		{
			component2.Init(tournament, viewDetailsOnly, needRefresh);
		}
		component2.CancelActionCalled += delegate
		{
			TournamentHelper.CloseTournamentDetailsWindow(isEnd);
		};
		component2.Ok += delegate
		{
			TournamentHelper.ShowMenu();
		};
		InfoMessage component3 = component2.GetComponent<InfoMessage>();
		component3.MessageType = InfoMessageTypes.CompetitionDetails;
		MessageFactory.InfoMessagesQueue.Enqueue(component3);
		return gameObject;
	}

	private static void CloseTournamentDetailsWindow(bool isEnd)
	{
		if (ShowHudElements.Instance != null)
		{
			ShowHudElements.Instance.StopTimer();
		}
		if (GameFactory.Player != null && GameFactory.Player.State == typeof(PlayerCameraRouting))
		{
			GameFactory.Player.CameraRoutingTurnOff();
		}
		if (isEnd)
		{
			TournamentHelper.EndFishSold();
		}
		TournamentHelper.ShowMenu();
	}

	private static void HideMenu()
	{
		if (TransferToLocation.Instance != null)
		{
			TransferToLocation.Instance.SetActivePondUi(false);
		}
		else if (MenuHelpers.Instance != null)
		{
			MenuHelpers.Instance.HideMenu(false, false, false);
		}
	}

	private static void ShowMenu()
	{
		if (TransferToLocation.Instance != null)
		{
			TransferToLocation.Instance.SetActivePondUi(true);
		}
		else if (MenuHelpers.Instance != null)
		{
			MenuHelpers.Instance.ShowMenu(false, false);
		}
	}

	public static GameObject ShowingTournamentDetails(Tournament tournament, bool isEnd = false)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.tournamentDetailMessagePrefab);
		TournamentDetailsMessage component = gameObject.GetComponent<TournamentDetailsMessage>();
		if (isEnd)
		{
			component.CloseOnClick += delegate(object sender, MessageBoxEventArgs args)
			{
				ProfileTournament tournament2 = PhotonConnectionFactory.Instance.Profile.Tournament;
				if (tournament2 == null || tournament2.TournamentId == tournament.TournamentId)
				{
					TournamentHelper.EndFishSold();
				}
			};
		}
		component.Init(tournament);
		return gameObject;
	}

	public static GameObject ShowingInGameTournamentDetails(UserCompetitionPublic tournament, bool viewDetailsOnly, bool needRefresh)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.TournamentResultWindowPrefab);
		TournamentDetailsMessageNew component = gameObject.GetComponent<TournamentDetailsMessageNew>();
		component.Init(tournament, viewDetailsOnly, needRefresh);
		component.Open();
		return gameObject;
	}

	public static GameObject ShowingTournamentDetails(TournamentTemplate template, Tournament tournament)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.tournamentDetailMessagePrefab);
		TournamentDetailsMessage tdm = gameObject.GetComponent<TournamentDetailsMessage>();
		tdm.CloseOnClick += delegate(object sender, MessageBoxEventArgs args)
		{
			tdm.Close();
		};
		tdm.Init(template, tournament);
		return gameObject;
	}

	public static GameObject ShowingSerieDetails(TournamentSerieInstance serie)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.tournamentSerieDetailMessagePrefab);
		gameObject.GetComponent<TournamentSerieDetailsMessage>().Init(serie);
		return gameObject;
	}

	public static GameObject ShowWindowList(WindowList.WindowListContainer c)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.WindowListPrefab);
		WindowList component = gameObject.GetComponent<WindowList>();
		component.Init(c);
		component.Open();
		return gameObject;
	}

	public static GameObject WindowLevelMinMax(Range currentRange, Range minRange, Range maxRange)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.WindowLevelMinMaxPrefab);
		WindowLevelMinMax component = gameObject.GetComponent<WindowLevelMinMax>();
		component.Init(currentRange, minRange, maxRange);
		component.Open();
		return gameObject;
	}

	public static GameObject WindowEntryFee(WindowEntryFee.EntryFeeData d)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.WindowEntryFeePrefab);
		WindowEntryFee component = gameObject.GetComponent<WindowEntryFee>();
		component.Init(d);
		component.Open();
		return gameObject;
	}

	public static GameObject WindowTimer(int duration, string title, bool isButtonCancelActive, bool isAutoHide = true)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.ChumMixingProgressPrefab);
		ChumMixProgress component = gameObject.GetComponent<ChumMixProgress>();
		component.Init(duration, title, isButtonCancelActive, isAutoHide);
		component.Open();
		return gameObject;
	}

	public static GameObject WindowInitialPrizePool(List<WindowList.WindowListElem> data, string title, string dataTitle, string descTitle, WindowEntryFee.EntryFeeData d, int index = 0)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.WindowInitialPrizePoolPrefab);
		WindowInitialPrizePool component = gameObject.GetComponent<WindowInitialPrizePool>();
		component.Init(data, title, dataTitle, descTitle, d, index);
		component.Open();
		return gameObject;
	}

	public static GameObject WindowTextField(string v, string title, string titleLocalized, bool allowedEmpty = false, int? chumNameMaxCharacters = null, List<char> chars = null, bool onlyNumbers = false)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.RenameChumPrefab);
		ChumRename component = gameObject.GetComponent<ChumRename>();
		component.Init(v, title, titleLocalized, allowedEmpty, chumNameMaxCharacters, chars, onlyNumbers);
		component.Open();
		return gameObject;
	}

	public static InputAreaWnd WindowTextArea(string title, bool allowedEmpty = true, bool isScreenKeyboard = true)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.InputAreaWndPrefab);
		InputAreaWnd component = gameObject.GetComponent<InputAreaWnd>();
		component.Init(title, allowedEmpty, isScreenKeyboard);
		component.Open();
		return component;
	}

	public static GameObject WindowScheduleAndDuration(List<WindowList.WindowListElem> durationData, int durationIndex, List<WindowList.WindowListElem> stData, int stIndex, DateTime? dt, int overlayIndex)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.WindowScheduleAndDurationPrefab);
		WindowScheduleAndDuration component = gameObject.GetComponent<WindowScheduleAndDuration>();
		component.Init(durationData, durationIndex, stData, stIndex, dt, overlayIndex);
		component.Open();
		return gameObject;
	}

	public static GameObject WindowTimeAndWeather(int pondId, MetadataForUserCompetitionPondWeather.DayWeather[] dayWeathers, MetadataForUserCompetitionPondWeather.DayWeather selectedDayWeather)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.WindowTimeAndWeatherPrefab);
		WindowTimeAndWeather component = gameObject.GetComponent<WindowTimeAndWeather>();
		component.Init(pondId, dayWeathers, selectedDayWeather);
		component.Open();
		return gameObject;
	}

	public static GameObject WindowSearch(FilterForUserCompetitions f)
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.WindowSearchPrefab);
		WindowSearchOptions component = gameObject.GetComponent<WindowSearchOptions>();
		component.Init(f);
		component.Open();
		return gameObject;
	}

	public static GameObject ShowingEulaMessage()
	{
		GameObject gameObject = TournamentHelper.Prepare(MessageBoxList.Instance.eulaMessagePrefab);
		EulaMessageBox component = gameObject.GetComponent<EulaMessageBox>();
		component.OnPriority = true;
		component.TopText.text = ScriptLocalization.Get("EULATopText");
		component.BottomText.text = ScriptLocalization.Get("EULABottomText");
		component.LinkText.TextData = ScriptLocalization.Get("EULALinkText");
		component.LinkText.Localize();
		return gameObject;
	}

	public static GameObject Prepare(GameObject prefab)
	{
		GameObject gameObject = GUITools.AddChild(MessageBoxList.Instance.gameObject, prefab);
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.anchoredPosition = Vector3.zero;
		component.sizeDelta = Vector2.zero;
		return gameObject;
	}

	private static bool seriesSubscribed = false;

	public static UnityEvent OnSeriesUpdated = new UnityEvent();

	private const float TournamentDetailsWindowAutoHideTime = 10f;

	public static List<TournamentSerieInstance> SeriesInstances;

	private static GameObject _messageEndTournamentTime;

	private static GameObject _messageFishSold;

	public static Action FishSold;

	public class TournamentWeatherDesc
	{
		public string PressureIcon { get; set; }

		public string WaterTemperature { get; set; }

		public string WeatherIcon { get; set; }

		public string WeatherTemperature { get; set; }

		public string WeatherWindDirection { get; set; }

		public string WeatherWindPower { get; set; }

		public string WeatherWindSuffix { get; set; }
	}
}
