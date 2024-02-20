using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using Newtonsoft.Json;
using ObjectModel;
using ObjectModel.Tournaments;
using Photon.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseTournamentDetails : MessageBoxBase, ITournamentDetails
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ApplyOnClick;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnUnregister = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> HaventMoneyClickBuy;

	private void OnEnable()
	{
		if (this.SelectedTournamentLoading != null && this._currentTournament == null)
		{
			this.SelectedTournamentLoading.SetActive(true);
		}
	}

	private void Update()
	{
		if (this._currentTournament == null)
		{
			return;
		}
		if (this._currentTournament.IsStarted && !this._currentTournament.IsEnded)
		{
			this.StartTimeBellowLogo.text = string.Format(ScriptLocalization.Get("EndTournamentTimeText"), BaseTournamentDetails.GetTimerValue(this._currentTournament.EndDate));
		}
		if (this._currentTournament.StartDate > TimeHelper.UtcTime())
		{
			if (this._currentTournament.RegistrationStart > TimeHelper.UtcTime())
			{
				this.StartTimeBellowLogo.text = string.Format(ScriptLocalization.Get("RegTimeTournamentOpensIn"), BaseTournamentDetails.GetTimerValue(this._currentTournament.RegistrationStart));
			}
			else
			{
				this.StartTimeBellowLogo.text = string.Format(ScriptLocalization.Get("StartTournamentTimeText"), BaseTournamentDetails.GetTimerValue(this._currentTournament.StartDate));
			}
		}
		if (this._rewardItems1 != null && this._place_Counter == 1 && this._getRewardItemsFinished)
		{
			this.SetRewardItems(this._rewardItems1);
		}
		if (this._rewardItems2 != null && this._place_Counter == 2 && this._getRewardItemsFinished)
		{
			this.SetRewardItems(this._rewardItems2);
		}
		if (this._rewardItems3 != null && this._place_Counter == 3 && this._getRewardItemsFinished)
		{
			this.SetRewardItems(this._rewardItems3);
		}
	}

	public static string GetTimerValue(DateTime dateTime)
	{
		return dateTime.GetTimeFinishInValue(true);
	}

	private void SubscribeWeather()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentWeather += this._weatherProxy.OnRespond;
		PhotonConnectionFactory.Instance.OnGettingTournamentWeatherFailed += this.InstanceOnGettingTournamentWeatherFailed;
	}

	private void InstanceOnGettingTournamentWeatherFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	private void UnSubscribeWeather()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentWeather -= this._weatherProxy.OnRespond;
		PhotonConnectionFactory.Instance.OnGettingTournamentWeatherFailed -= this.InstanceOnGettingTournamentWeatherFailed;
	}

	private void SubscribeRewards()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentRewards += this._rewardsProxy.OnRespond;
		PhotonConnectionFactory.Instance.OnGettingTournamentRewardsFailed += this.InstanceOnGettingTournamentRewardsFailed;
	}

	private void InstanceOnGettingTournamentRewardsFailed(Failure failure)
	{
		Debug.LogError(failure.FullErrorInfo);
	}

	private void UnSubscribeRewards()
	{
		PhotonConnectionFactory.Instance.OnGotTournamentRewards -= this._rewardsProxy.OnRespond;
		PhotonConnectionFactory.Instance.OnGettingTournamentRewardsFailed -= this.InstanceOnGettingTournamentRewardsFailed;
	}

	public virtual void Init(Tournament tournament)
	{
		if (this._weatherProxy == null)
		{
			IPhotonServerConnection photon = PhotonConnectionFactory.Instance;
			this._weatherProxy = new ServerProxy<WeatherDesc[]>("ActiveTournamentWeatherDetails", new Action(this.SubscribeWeather), new Action(this.UnSubscribeWeather), delegate
			{
				photon.GetTournamentWeather(this._currentTournament.TournamentId);
			}, (WeatherDesc[] data) => true, new int[] { 0, 30 });
			this._weatherProxy.ERespond += this.PhotonServer_OnGetTournamentWeather;
			this._rewardsProxy = new ServerProxy<List<Reward>>("ActiveTournamentRewardsDetails", new Action(this.SubscribeRewards), new Action(this.UnSubscribeRewards), delegate
			{
				photon.GetTournamentRewards(this._currentTournament.TournamentId);
			}, (List<Reward> data) => true, new int[] { 0, 30 });
			this._rewardsProxy.ERespond += this.PhotonServer_OnGetTournamentRewards;
		}
		if (this.SelectedTournamentLoading != null)
		{
			this.SelectedTournamentLoading.SetActive(false);
		}
		this._place_Counter = 1;
		this._rewardItems1 = null;
		this._rewardItems2 = null;
		this._rewardItems3 = null;
		this.ClearRewardContent();
		if (this._currentTournament == null || this._currentTournament.TournamentId != tournament.TournamentId)
		{
			this._rewardsProxy.ClearCache();
			this._weatherProxy.ClearCache();
		}
		this._currentTournament = tournament;
		if (tournament.TournamentId != 0)
		{
			this._rewardsProxy.SendRequest();
			this._weatherProxy.SendRequest();
		}
		this.Title.text = this._currentTournament.Name;
		this.LogoImageLoadable.Image = this.LogoImage;
		if (this._currentTournament.ImageBID != null)
		{
			this.LogoImageLoadable.Load(string.Format("Textures/Inventory/{0}", this._currentTournament.ImageBID.Value));
		}
		this.Description.text = string.Format(this._currentTournament.Desc, "<b><color=yellow>", "</color></b>", "\n");
		this.RegisterTime.text = string.Format("{0} - {1}", MeasuringSystemManager.DateTimeString(this._currentTournament.RegistrationStart.ToLocalTime()), MeasuringSystemManager.DateTimeString(this._currentTournament.StartDate.ToLocalTime()));
		this.CompetitionTime.text = string.Format("{0} - {1}", MeasuringSystemManager.DateTimeString(this._currentTournament.StartDate.ToLocalTime()), MeasuringSystemManager.DateTimeString(this._currentTournament.EndDate.ToLocalTime()));
		this.RulesText.text = tournament.Rules;
		if (CacheLibrary.MapCache.CachedPonds != null)
		{
			Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.PondId == this._currentTournament.PondId);
			if (pond != null)
			{
				this.Location.text = pond.Name + ", " + ((pond.State == null) ? string.Empty : pond.State.Name);
			}
		}
		int num = (this._currentTournament.InGameDuration * 60 + this._currentTournament.InGameDurationMinute) / 4;
		this.EntryTime.text = num.ToString() + " min";
		List<TournamentPrecondition> list = PhotonConnectionFactory.Instance.Profile.CheckTournamentRegisterPreconditions(tournament);
		if (this._preconditionsObjects != null && this._preconditionsObjects.Count > 0)
		{
			for (int j = 0; j < this._preconditionsObjects.Count; j++)
			{
				Object.Destroy(this._preconditionsObjects[j]);
			}
		}
		this._preconditionsObjects = new List<GameObject>();
		if (this._currentTournament.Preconditions != null)
		{
			int i;
			for (i = 0; i < this._currentTournament.Preconditions.Count; i++)
			{
				GameObject gameObject = GUITools.AddChild(this.PreconditionsContentPanel, this.PreconditionPrefab);
				this._preconditionsObjects.Add(gameObject);
				gameObject.transform.Find("Text").GetComponent<Text>().text = this.PreconditionConstructor(this._currentTournament.Preconditions[i]);
				if (list != null && list.Any((TournamentPrecondition x) => x.PreconditionType == this._currentTournament.Preconditions[i].PreconditionType))
				{
					gameObject.transform.Find("InCorrect").gameObject.SetActive(true);
				}
				else
				{
					gameObject.transform.Find("Correct").gameObject.SetActive(true);
				}
			}
		}
		if (tournament.FreeForPremium && PhotonConnectionFactory.Instance.Profile.HasPremium)
		{
			this.OldFeeText.gameObject.SetActive(true);
			this.OldFeeText.text = tournament.EntranceFee.ToString();
			this.FeeCurrency.text = MeasuringSystemManager.GetCurrencyIcon(tournament.Currency);
			this.FeeText.text = ScriptLocalization.Get("TournamentFeeFreeCaption");
		}
		else
		{
			this.OldFeeText.gameObject.SetActive(false);
			if (tournament.EntranceFee > 0.0)
			{
				this.FeeText.text = tournament.EntranceFee.ToString();
				this.FeeCurrency.text = MeasuringSystemManager.GetCurrencyIcon(tournament.Currency);
			}
			else
			{
				this.FeeText.text = ScriptLocalization.Get("TournamentFeeFreeCaption");
				this.FeeCurrency.text = string.Empty;
			}
		}
		this.SetButtonView();
		this.RankingsPanel.GetComponent<TournamentRankingsInit>().Init(this._currentTournament);
		if (this._currentTournament.IsEnded)
		{
			this.ResultsPanelButton.SetActive(true);
			this.SummaryPanelButton.GetComponent<Toggle>().isOn = false;
			this.ResultsPanelButton.GetComponent<Toggle>().isOn = true;
			if (this.ResultsPanel != null)
			{
				this.ResultsPanel.SetActive(true);
				this.ResultsPanel.GetComponent<TournamentResultsInit>().Init(this._currentTournament);
			}
		}
		else
		{
			this.ResultsPanelButton.GetComponent<Toggle>().isOn = false;
			this.SummaryPanelButton.GetComponent<Toggle>().isOn = true;
		}
		this._getRewardItemsFinished = true;
	}

	private void ClearRewardContent()
	{
		IEnumerator enumerator = this.FirstPlaceRewardItemsContent.GetComponent<RectTransform>().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				Object.Destroy(transform.gameObject);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		IEnumerator enumerator2 = this.SecondPlaceRewardItemsContent.GetComponent<RectTransform>().GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				object obj2 = enumerator2.Current;
				Transform transform2 = (Transform)obj2;
				Object.Destroy(transform2.gameObject);
			}
		}
		finally
		{
			IDisposable disposable2;
			if ((disposable2 = enumerator2 as IDisposable) != null)
			{
				disposable2.Dispose();
			}
		}
		IEnumerator enumerator3 = this.ThirdPlaceRewardItemsContent.GetComponent<RectTransform>().GetEnumerator();
		try
		{
			while (enumerator3.MoveNext())
			{
				object obj3 = enumerator3.Current;
				Transform transform3 = (Transform)obj3;
				Object.Destroy(transform3.gameObject);
			}
		}
		finally
		{
			IDisposable disposable3;
			if ((disposable3 = enumerator3 as IDisposable) != null)
			{
				disposable3.Dispose();
			}
		}
		this.SetSizeForRewardContent();
	}

	private void PhotonServer_OnGetTournamentWeather(WeatherDesc[] weathers)
	{
		TournamentHelper.TournamentWeatherDesc tournamentWeather = TournamentHelper.GetTournamentWeather(this._currentTournament, weathers);
		if (tournamentWeather != null)
		{
			this.WeatherIcon.text = tournamentWeather.WeatherIcon;
			this.WeatherTemperature.text = tournamentWeather.WeatherTemperature;
			this.WeatherWindDirection.text = tournamentWeather.WeatherWindDirection;
			this.WeatherWindPower.text = tournamentWeather.WeatherWindPower;
			this.WeatherWindSuffix.text = tournamentWeather.WeatherWindSuffix;
			this.WeatherPressure.text = tournamentWeather.PressureIcon;
		}
	}

	private void PhotonServer_OnGetTournamentRewards(List<Reward> rewards)
	{
		double num = Reward.GetSumOfRewards(rewards, "SC");
		double num2 = Reward.GetSumOfRewards(rewards, "GC");
		ProductReward productReward = null;
		string text = ((num == 0.0) ? string.Empty : string.Format("\ue62b {0} ", (int)num));
		string text2 = ((num2 == 0.0) ? string.Empty : string.Format("\ue62c {0}", (int)num2));
		this.PrizePoolValue.text = text + text2;
		if (rewards.Count > 0)
		{
			this.FillReward(rewards[0], this.FirstPlaceValue);
			this._rewardItems1 = rewards[0].Items;
		}
		if (rewards.Count > 1)
		{
			this.FillReward(rewards[1], this.SecondPlaceValue);
			this._rewardItems2 = rewards[1].Items;
		}
		if (rewards.Count > 2)
		{
			this.FillReward(rewards[2], this.ThirdPlaceValue);
			this._rewardItems3 = rewards[2].Items;
		}
		this.OtherRewardsValue.text = string.Format(ScriptLocalization.Get("TournamentPrizeRest"), 10);
		if (this._currentTournament.SecondaryRewards != null)
		{
			SecondaryReward biggestFishReward = this._currentTournament.SecondaryRewards.FirstOrDefault((SecondaryReward x) => x.RewardType == SecondaryRewardType.BiggestFish);
			if (biggestFishReward != null)
			{
				Reward reward = rewards.FirstOrDefault((Reward x) => x.Name == biggestFishReward.RewardName);
				num = 0.0;
				num2 = 0.0;
				productReward = null;
				BaseTournamentDetails.GetMoneys(reward, ref num, ref num2, ref productReward);
				text = ((num == 0.0) ? string.Empty : string.Format("\ue62b {0} ", (int)num));
				text2 = ((num2 == 0.0) ? string.Empty : string.Format("\ue62c {0} ", (int)num2));
				this.BiggestFishRewardValue.text = text + text2;
				this.BiggestFishReward.SetActive(true);
			}
		}
	}

	private void FillReward(Reward reward, Text place)
	{
		double num = 0.0;
		double num2 = 0.0;
		ProductReward productReward = null;
		BaseTournamentDetails.GetMoneys(reward, ref num, ref num2, ref productReward);
		StoreProduct storeProduct = null;
		if (productReward != null)
		{
			storeProduct = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.ProductId == productReward.ProductId);
		}
		string text = ((num == 0.0) ? string.Empty : string.Format("\ue62b {0} ", (int)num));
		string text2 = ((num2 == 0.0) ? string.Empty : string.Format("\ue62c {0} ", (int)num2));
		if (storeProduct != null)
		{
			place.text = text + text2 + string.Format("\ue645 {0} {1}", storeProduct.Term, ScriptLocalization.Get("DaysCaptionLower"));
		}
		else
		{
			place.text = text + text2;
		}
	}

	private void SetRewardItems(string _items)
	{
		this._getRewardItemsFinished = false;
		if (!string.IsNullOrEmpty(_items))
		{
			ItemReward[] array = JsonConvert.DeserializeObject<ItemReward[]>(_items, SerializationHelper.JsonSerializerSettings);
			List<int> list = new List<int>();
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(array[i].ItemId);
			}
			PhotonConnectionFactory.Instance.OnGotItems += this.OnGotItems;
			PhotonConnectionFactory.Instance.GetItemsByIds(list.ToArray(), 1, true);
		}
	}

	private void OnGotItems(List<InventoryItem> items, int subscriberId)
	{
		if (subscriberId != 1)
		{
			return;
		}
		PhotonConnectionFactory.Instance.OnGotItems -= this.OnGotItems;
		int place_Counter = this._place_Counter;
		if (place_Counter != 1)
		{
			if (place_Counter != 2)
			{
				if (place_Counter == 3)
				{
					this._rewardContent = this.ThirdPlaceRewardItemsContent;
				}
			}
			else
			{
				this._rewardContent = this.SecondPlaceRewardItemsContent;
			}
		}
		else
		{
			this._rewardContent = this.FirstPlaceRewardItemsContent;
		}
		for (int i = 0; i < items.Count<InventoryItem>(); i++)
		{
			GameObject gameObject = GUITools.AddChild(this._rewardContent, this.TournamentRewardItemPrefab);
			gameObject.GetComponent<TournamentRewardItemInit>().Init(items[i]);
		}
		this.SetSizeForRewardContent();
		this._place_Counter++;
		this._getRewardItemsFinished = true;
	}

	private void SetSizeForRewardContent()
	{
		if (this.FirstPlaceContent == null || this.SecondPlaceContent == null || this.ThirdPlaceContent == null)
		{
			return;
		}
		int num = this.ChildCount(this.FirstPlaceRewardItemsContent);
		int num2 = this.ChildCount(this.SecondPlaceRewardItemsContent);
		int num3 = this.ChildCount(this.ThirdPlaceRewardItemsContent);
		float minHeight = this.TournamentRewardItemPrefab.GetComponent<LayoutElement>().minHeight;
		this.FirstPlaceContent.GetComponent<LayoutElement>().minHeight = minHeight * (float)num + 30f + this.Spacing(this.FirstPlaceRewardItemsContent) * (float)num;
		this.SecondPlaceContent.GetComponent<LayoutElement>().minHeight = minHeight * (float)num2 + 30f + this.Spacing(this.SecondPlaceRewardItemsContent) * (float)num;
		this.ThirdPlaceContent.GetComponent<LayoutElement>().minHeight = minHeight * (float)num3 + 30f + this.Spacing(this.ThirdPlaceRewardItemsContent) * (float)num;
	}

	private int ChildCount(GameObject go)
	{
		return (!(go != null)) ? 0 : go.GetComponent<RectTransform>().childCount;
	}

	private float Spacing(GameObject go)
	{
		return (!(go != null)) ? 0f : go.GetComponent<VerticalLayoutGroup>().spacing;
	}

	public static void GetMoneys(Reward reward, ref double silvers, ref double golds, ref ProductReward productReward)
	{
		ProductReward productReward2 = null;
		if (reward.Money1 != null)
		{
			if (reward.Currency1 == "SC")
			{
				silvers += reward.Money1.Value;
			}
			else
			{
				golds += reward.Money1.Value;
			}
		}
		if (reward.Money2 != null)
		{
			if (reward.Currency2 == "SC")
			{
				silvers += reward.Money2.Value;
			}
			else
			{
				golds += reward.Money2.Value;
			}
		}
		if (!string.IsNullOrEmpty(reward.Products))
		{
			ProductReward[] array = JsonConvert.DeserializeObject<ProductReward[]>(reward.Products, SerializationHelper.JsonSerializerSettings);
			productReward2 = array[0];
		}
		productReward = productReward2;
	}

	public void Init(TournamentTemplate template, Tournament tournament)
	{
		this._currentTournament = tournament;
		this._currentTournamentTemplate = template;
		if (this._currentTournament != null)
		{
			this.RegisterTime.text = string.Format("Register open: <b>{0}</b>", MeasuringSystemManager.DateTimeString(this._currentTournament.RegistrationStart.ToLocalTime()));
		}
		else
		{
			this.RegisterTime.text = string.Format("Register open: <b>{0}</b>", "unavailable");
		}
		if (this._currentTournament == null)
		{
			Selectable unregBtn = this.UnregBtn;
			bool flag = false;
			this.ApplyButton.interactable = flag;
			unregBtn.interactable = flag;
		}
		else
		{
			this.SetButtonView();
		}
	}

	public void ApplyClick()
	{
		if (this._currentTournament != null && !this._currentTournament.IsRegistered && !this._isRegistering)
		{
			if (this._currentTournament.EntranceFee > PhotonConnectionFactory.Instance.Profile.SilverCoins)
			{
				this._buyMoneyMessageBox = this._menuHelpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("TravelNotEnoughMoney"), true, false, false, null);
				UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.FailClip, SettingsManager.InterfaceVolume);
				this._buyMoneyMessageBox.GetComponent<EventAction>().ActionCalled += this.BuyMoneyCompleteMessage_ActionCalled;
			}
			else
			{
				this._isRegistering = true;
				PhotonConnectionFactory.Instance.OnRegisteredForTournament += this.PhotonServer_OnRegisteredForTournament;
				PhotonConnectionFactory.Instance.OnRegisterForTournamentFailed += this.PhotonServer_OnRegisterForTournamentFailed;
				PhotonConnectionFactory.Instance.RegisterForTournament(this._currentTournament.TournamentId, false);
			}
		}
	}

	public void Unregister()
	{
		if (this._currentTournament != null)
		{
			UIHelper.Waiting(true, null);
			PhotonConnectionFactory.Instance.OnUnregisteredFromTournament += this.Instance_OnUnregisteredFromTournament;
			PhotonConnectionFactory.Instance.OnUnregisterFromTournamentFailed += this.Instance_OnUnregisterFromTournamentFailed;
			PhotonConnectionFactory.Instance.UnregisterFromTournament(this._currentTournament.TournamentId);
		}
	}

	private void Instance_OnUnregisterFromTournamentFailed(Failure failure)
	{
		this.UnsubscribeUnregisteredFromTournament();
		UIHelper.Waiting(false, null);
		LogHelper.Error("UnregisterFromTournamentFailed - {0}", new object[] { failure.FullErrorInfo });
	}

	private void Instance_OnUnregisteredFromTournament()
	{
		this.UnsubscribeUnregisteredFromTournament();
		this._currentTournament.IsRegistered = false;
		if (TournamentManager.Instance != null)
		{
			TournamentManager.Instance.RefreshMyTournaments();
		}
		UIHelper.Waiting(false, null);
		this.SetButtonView();
		this.OnUnregister();
	}

	private void UnsubscribeUnregisteredFromTournament()
	{
		PhotonConnectionFactory.Instance.OnUnregisteredFromTournament -= this.Instance_OnUnregisteredFromTournament;
		PhotonConnectionFactory.Instance.OnUnregisterFromTournamentFailed -= this.Instance_OnUnregisterFromTournamentFailed;
	}

	protected void OnTournamentManagerRefreshed()
	{
		if (TournamentManager.Instance != null)
		{
			TournamentManager instance = TournamentManager.Instance;
			instance.OnRefreshed = (Action)Delegate.Remove(instance.OnRefreshed, new Action(this.OnTournamentManagerRefreshed));
		}
		this.ApplyClick();
	}

	private void Confirm_ActionCalled(object sender, EventArgs e)
	{
		this._messageBox.Close();
	}

	private void PhotonServer_OnRegisterForTournamentFailed(Failure failure)
	{
		ErrorCode errorCode = failure.ErrorCode;
		if (errorCode != 32552 && errorCode != 32554)
		{
			this.UnsubscribeRegisteredForTournament();
			this._messageBox = this._menuHelpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("TournamentRegistrationFailed"), true, false, false, null);
			this._messageBox.GetComponent<EventAction>().ActionCalled += this.Confirm_ActionCalled;
			LogHelper.Error("PhotonServer_OnRegisterForTournamentFailed FullErrorInfo:{0}", new object[] { failure.FullErrorInfo });
		}
		else
		{
			bool flag = failure.ErrorCode == 32552;
			UserCompetitionPublic userCompetition = ((TournamentFailure)failure).UserCompetition;
			string yellowTan = UgcConsts.GetYellowTan(UserCompetitionHelper.GetDefaultName(userCompetition));
			if (flag)
			{
				this.UnsubscribeRegisteredForTournament();
				if (this is TournamentDetailsMessage)
				{
					((TournamentDetailsMessage)this).CloseClick();
				}
				UserCompetitionFailureHandler.ShowMsgSportEventWait(yellowTan, userCompetition.EndDate);
			}
			else
			{
				UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get("UGC_AlreadySignedToSportEventJoinAnother"), yellowTan), delegate
				{
					PhotonConnectionFactory.Instance.RegisterForTournament(this._currentTournament.TournamentId, true);
				}, null, "UGC_ApplyAnywayButton", delegate
				{
					this.UnsubscribeRegisteredForTournament();
				}, "NoCaption", null, null, null);
			}
		}
	}

	private void PhotonServer_OnRegisteredForTournament()
	{
		if (TournamentManager.Instance != null)
		{
			TournamentManager.Instance.FullRefresh();
		}
		MenuHelpers.Instance.MenuPrefabsList.UgcMenuManager.SetState(UgcMenuStateManager.UgcStates.Sport);
		this.UnsubscribeRegisteredForTournament();
		this._messageBox = this._menuHelpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("YouWereRegisteredOnTournamentCaption"), true, false, false, null);
		this._messageBox.GetComponent<EventAction>().ActionCalled += this.Confirm_ActionCalled;
		this._currentTournament.IsRegistered = true;
		this.SetButtonView();
		if (this.ApplyOnClick != null)
		{
			this.ApplyOnClick(this, new EventArgs());
		}
		if (this._currentTournament.KindId == 1 && StaticUserData.CurrentPond == null)
		{
			GlobalMapInit component = this._menuHelpers.MenuPrefabsList.GetComponent<GlobalMapInit>();
			if (component != null)
			{
				component.RefreshPonds();
			}
		}
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.RegistrationComplete, SettingsManager.InterfaceVolume);
	}

	private void SetButtonView()
	{
		this.UnregBtn.interactable = false;
		if (this._currentTournament.SerieId != null)
		{
			this.UnregBtn.gameObject.SetActive(false);
			this.ApplyButton.gameObject.SetActive(false);
			return;
		}
		this.UnregBtn.gameObject.SetActive(true);
		this.ApplyButton.gameObject.SetActive(true);
		bool flag = PhotonConnectionFactory.Instance.Profile.CheckTournamentRegisterPreconditions(this._currentTournament) != null;
		if (this._currentTournament.IsRegistered || flag)
		{
			this.ApplyButton.interactable = false;
			this.UnregBtn.interactable = this._currentTournament.IsRegistered && !flag && this._currentTournament.StartDate > TimeHelper.UtcTime();
		}
		else
		{
			this.ApplyButton.interactable = this._currentTournament.RegistrationStart < TimeHelper.UtcTime() && this._currentTournament.StartDate > TimeHelper.UtcTime();
		}
	}

	private string PreconditionConstructor(TournamentPrecondition precondition)
	{
		string text = string.Empty;
		switch (precondition.PreconditionType)
		{
		case TournamentPreconditionType.Tournament:
			text = string.Format(ScriptLocalization.Get("TournamentPreconditionText"), precondition.TournamentTemplateId);
			break;
		case TournamentPreconditionType.Title:
			text = string.Format(ScriptLocalization.Get("TitlePreconditionText"), precondition.TournamentTemplateId);
			break;
		case TournamentPreconditionType.MinLevel:
			text = string.Format(ScriptLocalization.Get("MinLevelPreconditionText"), precondition.Level);
			break;
		case TournamentPreconditionType.MaxLevel:
			text = string.Format(ScriptLocalization.Get("MaxLevelPreconditionText"), precondition.Level);
			break;
		}
		return text;
	}

	private void BuyMoneyCompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		if (this._buyMoneyMessageBox != null)
		{
			this._buyMoneyMessageBox.Close();
		}
	}

	private void BuyClick_ThirdButtonActionCalled(object sender, EventArgs e)
	{
		if (this.HaventMoneyClickBuy != null)
		{
			this.HaventMoneyClickBuy(this, new EventArgs());
		}
		if (this._buyMoneyMessageBox != null)
		{
			this._buyMoneyMessageBox.Close();
		}
		ShopMainPageHandler.OpenPremiumShop();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.UnsubscribeUnregisteredFromTournament();
		this.UnsubscribeRegisteredForTournament();
		if (TournamentManager.Instance != null)
		{
			TournamentManager instance = TournamentManager.Instance;
			instance.OnRefreshed = (Action)Delegate.Remove(instance.OnRefreshed, new Action(this.OnTournamentManagerRefreshed));
		}
		if (this._weatherProxy != null)
		{
			this._weatherProxy.CancelRequest();
			this._weatherProxy.ERespond -= this.PhotonServer_OnGetTournamentWeather;
		}
		if (this._rewardsProxy != null)
		{
			this._rewardsProxy.CancelRequest();
			this._rewardsProxy.ERespond -= this.PhotonServer_OnGetTournamentRewards;
		}
	}

	private void UnsubscribeRegisteredForTournament()
	{
		this._isRegistering = false;
		PhotonConnectionFactory.Instance.OnRegisteredForTournament -= this.PhotonServer_OnRegisteredForTournament;
		PhotonConnectionFactory.Instance.OnRegisterForTournamentFailed -= this.PhotonServer_OnRegisterForTournamentFailed;
	}

	[SerializeField]
	protected BorderedButton UnregBtn;

	private const int ItemSubscriberId = 1;

	public Image LogoImage;

	private ResourcesHelpers.AsyncLoadableImage LogoImageLoadable = new ResourcesHelpers.AsyncLoadableImage();

	public Text Title;

	public Text CompetitionTime;

	public Text RegisterTime;

	public Text StartTimeBellowLogo;

	public Text Description;

	public GameObject PreconditionsContentPanel;

	public GameObject PreconditionPrefab;

	public Text RulesText;

	public Text WeatherIcon;

	public Text WeatherWindPower;

	public Text WeatherWindSuffix;

	public Text WeatherWindDirection;

	public Text WeatherTemperature;

	public Text WeatherPressure;

	public Text OldFeeText;

	public Text FeeText;

	public Text FeeCurrency;

	public Text PrizePoolValue;

	public GameObject SelectedTournamentLoading;

	private GameObject _rewardContent;

	public GameObject TournamentRewardItemPrefab;

	public GameObject FirstPlaceRewardItemsContent;

	public GameObject SecondPlaceRewardItemsContent;

	public GameObject ThirdPlaceRewardItemsContent;

	public Text FirstPlaceValue;

	public Text SecondPlaceValue;

	public Text ThirdPlaceValue;

	public GameObject FirstPlaceContent;

	public GameObject SecondPlaceContent;

	public GameObject ThirdPlaceContent;

	public Text OtherRewardsValue;

	public GameObject BiggestFishReward;

	public Text BiggestFishRewardValue;

	public Text Location;

	public Text EntryTime;

	public Button ApplyButton;

	public GameObject SummaryPanelButton;

	public GameObject RankingsPanel;

	public GameObject ResultsPanel;

	public GameObject ResultsPanelButton;

	private Tournament _currentTournament;

	private TournamentTemplate _currentTournamentTemplate;

	private MenuHelpers _menuHelpers = new MenuHelpers();

	private MessageBox _messageBox;

	private List<GameObject> _preconditionsObjects = new List<GameObject>();

	private MessageBox _buyMoneyMessageBox;

	private int _place_Counter;

	private string _rewardItems1;

	private string _rewardItems2;

	private string _rewardItems3;

	private bool _getRewardItemsFinished;

	private ServerProxy<WeatherDesc[]> _weatherProxy;

	private ServerProxy<List<Reward>> _rewardsProxy;

	private const float Offset = 30f;

	private bool _isRegistering;
}
