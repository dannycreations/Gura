using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using DG.Tweening;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentDetailsMessageNew : TournamentDetailsMessageBaseNew
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action Ok = delegate
	{
	};

	private UserCompetitionPublic Ugc { get; set; }

	private Camera Cam
	{
		get
		{
			return MenuHelpers.Instance.GUICamera;
		}
	}

	protected override void Update()
	{
		if (this._finalResult == null)
		{
			base.Update();
		}
		if (this.Ugc != null && this.DetailEquipmentContent.rect.height > 5f && !this._isEquipmentScrollChanged)
		{
			this._isEquipmentScrollChanged = true;
			base.StartCoroutine(this.CallEquipmentScrollChanged());
		}
	}

	public void Init(TournamentFinalResult finalResult)
	{
		this._finalResult = finalResult;
		this.Init(finalResult.UserCompetition, false, false);
	}

	public void Init(UserCompetitionPublic t, bool viewDetailsOnly, bool needRefresh)
	{
		this.Bg.enabled = !ScreenManager.Instance.IsIn3D;
		this._viewDetailsOnly = viewDetailsOnly;
		if (needRefresh)
		{
			this.FillData(t, false);
			this.Init(t.TournamentId);
		}
		else
		{
			this.FillData(t, true);
		}
	}

	public void Init(int tournamentId)
	{
		this.Bg.enabled = !ScreenManager.Instance.IsIn3D;
		PhotonConnectionFactory.Instance.OnGetUserCompetition += this.Instance_OnGetUserCompetition;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetition += base.Instance_OnFailureGetUserCompetition;
		PhotonConnectionFactory.Instance.GetUserCompetition(tournamentId);
	}

	protected override void AcceptActionCalled()
	{
		this.OkBtn.interactable = false;
		this.Ok();
	}

	protected override void Instance_OnGetMetadataForPredefinedCompetitionTemplate(UserCompetition metadataTemplate)
	{
		base.Instance_OnGetMetadataForPredefinedCompetitionTemplate(metadataTemplate);
		this.DetailRulesValue.text = metadataTemplate.Rules;
		LogHelper.Log("___kocha Instance_OnGetMetadataForPredefinedCompetitionTemplate Rules:{0}", new object[] { metadataTemplate.Rules });
		this.FillDetails(false);
	}

	protected override void Instance_OnGetUserCompetition(UserCompetitionPublic competition)
	{
		base.Instance_OnGetUserCompetition(competition);
		this.FillData(competition, true);
	}

	protected override void OnGotTournamentIntermediateResult(List<TournamentIndividualResults> results, int num)
	{
		base.OnGotTournamentIntermediateResult(results, num);
		this.FillScore(results, num);
	}

	protected override void OnGotTournamentFinalResult(List<TournamentIndividualResults> results, int num)
	{
		base.OnGotTournamentFinalResult(results, num);
		this.FillScore(results, num);
	}

	public void EquipmentScrollChanged()
	{
		if (this.Ugc != null && this.Ugc.Type == UserCompetitionType.Custom && base.gameObject.activeSelf && base.gameObject.activeInHierarchy)
		{
			base.StartCoroutine(this.CallEquipmentScrollChanged());
		}
	}

	public void ScoreListScrollChanged()
	{
		if (this.Ugc != null && this.OwnerInfo != null)
		{
			ShortcutExtensions.DOKill(this.OwnerInfo, false);
			ShortcutExtensions.DOFade(this.OwnerInfo, (!this.IsOwnerInRectMask(this.ScoreContentRt, this.ScoreContentScrollRect)) ? 1f : 0f, 0.2f);
		}
	}

	public void ScoreListTeamScrollChanged()
	{
		if (this.Ugc != null && this.OwnerTeamInfo != null)
		{
			float num = 1f;
			for (int i = 0; i < this.ScoreTeams.Length; i++)
			{
				if (this.IsOwnerInRectMask(this.ScoreTeams[i].ScoreTeamContent, this.ScoreTeamContentScrollRect))
				{
					num = 0f;
					break;
				}
			}
			ShortcutExtensions.DOKill(this.OwnerTeamInfo, false);
			ShortcutExtensions.DOFade(this.OwnerTeamInfo, num, 0.2f);
		}
	}

	private bool IsOwnerInRectMask(RectTransform rt, RectTransform rectScroll)
	{
		for (int i = 0; i < rt.transform.childCount; i++)
		{
			TournamentResultPlayer component = rt.transform.GetChild(i).GetComponent<TournamentResultPlayer>();
			if (component.Data.IsOwner && RectTransformUtility.RectangleContainsScreenPoint(rectScroll, this.Cam.WorldToScreenPoint(component.transform.position), this.Cam))
			{
				return true;
			}
		}
		return false;
	}

	protected override void WindowShow()
	{
		base.StartCoroutine(this.CorrectionDetailsScrollFishesContent());
		base.StartCoroutine(this.SetOwnerInfoVisibility());
		base.StartCoroutine(this.SetOwnerTeamInfoVisibility());
	}

	private void FillData(UserCompetitionPublic t, bool getAddInfo)
	{
		this.Ugc = t;
		this.TogglesUpdate(t);
		if (getAddInfo)
		{
			base.UpdateStatus(t);
		}
		bool flag = this.Ugc.Type == UserCompetitionType.Custom;
		bool flag2 = t.SortType == UserCompetitionSortType.Automatic;
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.PondId == t.PondId);
		DateTime dateTime = t.StartDate.ToLocalTime();
		DateTime dateTime2 = t.EndDate.ToLocalTime();
		if (!t.IsStarted && flag2)
		{
			dateTime2 = t.FixedStartDate.Value.ToLocalTime().AddMinutes((double)t.Duration);
		}
		string name = UgcConsts.GetDurationLoc(t.Duration).Name;
		string text = ((t.EntranceFee != null && !(t.EntranceFee <= 0.0)) ? t.EntranceFee.ToString() : ScriptLocalization.Get("TournamentFeeFreeCaption"));
		this.Logo.gameObject.SetActive(!this._viewDetailsOnly || t.IsRegistered || (t.IsSponsored && t.ApproveStatus != UserCompetitionApproveStatus.NoReview));
		this.OkBtn.gameObject.SetActive(!t.IsRegistered && !t.IsEnded && !this._viewDetailsOnly && !t.IsStarted && UserCompetitionHelper.IsUgcEnabled);
		UserCompetitionHelper.GetDefaultName(this.Title, t);
		this.ImgLoader.Load(t.ImageBID, this.Logo, "Textures/Inventory/{0}");
		this.TotalPlayersValue.text = t.RegistrationsCount.ToString(CultureInfo.InvariantCulture);
		this.ValueLocation.text = pond.Name;
		MetadataForUserCompetitionPondWeather.DayWeather dayWeather = t.SelectedDayWeather ?? ((t.InGameStartHour == null) ? null : t.DayWeathers.First((MetadataForUserCompetitionPondWeather.DayWeather x) => x.TimeOfDay == new TimeSpan(t.InGameStartHour.Value, 0, 0).ToTimeOfDay().ToString()));
		if (dayWeather != null)
		{
			base.SetWeather(TournamentHelper.GetTournamentWeather(dayWeather));
		}
		this.ValueDuration.text = name;
		this.ValueFree.text = text;
		this.ValueMin.text = TournamentDetailsPrecondition.PreconditionConstructor(new TournamentPrecondition
		{
			PreconditionType = TournamentPreconditionType.MinLevel,
			Level = t.MinLevel
		});
		TMP_Text preconditionIcoMin = this.PreconditionIcoMin;
		int? minLevel = t.MinLevel;
		preconditionIcoMin.text = ((!(profile.Level < minLevel)) ? "\ue783" : "\ue678");
		Graphic preconditionIcoMin2 = this.PreconditionIcoMin;
		int? minLevel2 = t.MinLevel;
		preconditionIcoMin2.color = ((!(profile.Level < minLevel2)) ? UgcConsts.WinnerColor : Color.red);
		this.ValueMax.text = TournamentDetailsPrecondition.PreconditionConstructor(new TournamentPrecondition
		{
			PreconditionType = TournamentPreconditionType.MaxLevel,
			Level = t.MaxLevel
		});
		TMP_Text preconditionIcoMax = this.PreconditionIcoMax;
		int? maxLevel = t.MaxLevel;
		preconditionIcoMax.text = ((!(profile.Level > maxLevel)) ? "\ue783" : "\ue678");
		Graphic preconditionIcoMax2 = this.PreconditionIcoMax;
		int? maxLevel2 = t.MaxLevel;
		preconditionIcoMax2.color = ((!(profile.Level > maxLevel2)) ? UgcConsts.WinnerColor : Color.red);
		this.ValueRuleset.text = UserCompetitionHelper.GetScoringTypeLoc(this.Ugc);
		this.ValueScoring.text = UgcConsts.GetCompetitionScoringLoc(this.Ugc.FishSource).Name;
		this.DetailEquipmentContent.gameObject.SetActive(flag);
		this.DetailsContentRules.gameObject.SetActive(!flag);
		this.DetailsContentRuleset.SetActive(flag);
		if (!getAddInfo)
		{
			return;
		}
		if (flag)
		{
			this.FillDetails(true);
			this.EquipmentScrollChanged();
		}
		else
		{
			this.TitleCurrentEquipment.text = ScriptLocalization.Get("RulesAndRestrictionsTournamentCaption").ToUpper();
			this.DetailRulesValue.text = this.Ugc.Rules;
			this.FillDetails(false);
		}
		bool flag3 = t.Rewards != null && t.Rewards.Length > 0;
		if (!flag3)
		{
			this.RewardSponsorsRewardsTitle.text = string.Empty;
		}
		if (t.Format == UserCompetitionFormat.Team)
		{
			Reward reward = null;
			if (this._finalResult != null)
			{
				reward = this._finalResult.CurrentPlayerResult.Reward;
				string text2;
				string text3;
				bool goldAndSilverTexts = UIHelper.GetGoldAndSilverTexts(reward, out text2, out text3);
				this.TeamRewardValue.text = text2;
				this.TeamRewardSponsoredValue.text = text3;
				this.TeamSponsoredReward.SetActive(goldAndSilverTexts);
				TMP_Text teamRewardDescription = this.TeamRewardDescription;
				string text4 = string.Empty;
				this.TeamRewardSponsoredDescription.text = text4;
				teamRewardDescription.text = text4;
			}
			else
			{
				this.TeamRewardValue.text = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon(t.Currency), UserCompetitionHelper.PrizePoolWithComission(t));
				if (flag3)
				{
					reward = t.Rewards[0];
					string text5;
					string text6;
					bool goldAndSilverTexts2 = UIHelper.GetGoldAndSilverTexts(reward, out text5, out text6);
					this.TeamRewardSponsoredValue.text = text6;
					this.TeamSponsoredReward.SetActive(goldAndSilverTexts2);
				}
				else
				{
					this.TeamSponsoredReward.SetActive(false);
				}
				this.TeamRewardDescription.text = UgcConsts.GetInitialPrizePoolLoc(t.RewardScheme).Name;
			}
			if (reward != null)
			{
				base.BuildRewards(reward, this.TeamRewardSponsoredItems);
				if (this.TeamRewardSponsoredItems.ToList<TournamentDetailsMessageBaseNew.RewardItem>().Any((TournamentDetailsMessageBaseNew.RewardItem p) => p.Item.activeSelf))
				{
					this.TeamRewardSponsoredContent.childAlignment = 1;
				}
			}
		}
		else if (this._finalResult != null)
		{
			Reward reward2 = this._finalResult.CurrentPlayerResult.Reward;
			string text7;
			string text8;
			bool goldAndSilverTexts3 = UIHelper.GetGoldAndSilverTexts(reward2, out text7, out text8);
			this.FinalRewardValue.text = text7;
			this.FinalRewardSponsoredValue.text = text8;
			this.FinalSponsoredReward.SetActive(goldAndSilverTexts3);
			base.BuildRewards(reward2, this.FinalRewardItems);
		}
		else
		{
			this.RewardDescription.text = UgcConsts.GetInitialPrizePoolLoc(t.RewardScheme).Name;
			this.RewardValue.text = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon(t.Currency), UserCompetitionHelper.PrizePoolWithComission(t));
			int num;
			if (t.Players != null)
			{
				num = t.Players.Count((UserCompetitionPlayer p) => p.IsApproved);
			}
			else
			{
				num = 0;
			}
			int num2 = num;
			int num3 = 1;
			if (this.Ugc.RewardScheme == UserCompetitionRewardScheme.Individual1_50_35_15 || this.Ugc.RewardScheme == UserCompetitionRewardScheme.Individual2_60_30_10 || this.Ugc.RewardScheme == UserCompetitionRewardScheme.Individual3_80_15_5)
			{
				num3 = 3;
			}
			num3 = Math.Min(num3, num2);
			if (flag3)
			{
				num3 = Math.Max(num3, t.Rewards.Length);
			}
			double num4 = t.PrizePoolWithoutComission(num2);
			base.ClearContent(this.RewardPlacesContent.gameObject);
			base.ClearContent(this.RewardCreditsGoldContent.gameObject);
			base.ClearContent(this.RewardProductsContent);
			for (int i = 0; i < num3; i++)
			{
				int num5 = i + 1;
				Reward reward3 = t.CalculateRewardForPlayer(num4, new int?(num5), null, null);
				string text9;
				string text10;
				UIHelper.GetGoldAndSilverTexts(reward3, out text9, out text10);
				GUITools.AddChild(this.RewardPlacesContent.gameObject, this.RewardPlacesItemPrefab).GetComponent<RewardPlaceItem>().Init(num5, text9);
			}
			if (num3 == 0)
			{
				TMP_Text rewardPlacesTitle = this.RewardPlacesTitle;
				string text4 = string.Empty;
				this.RewardCreditsTitle.text = text4;
				rewardPlacesTitle.text = text4;
			}
			if (flag3)
			{
				for (int j = 0; j < t.Rewards.Length; j++)
				{
					Reward reward4 = t.Rewards[j];
					string text11;
					string text12;
					bool goldAndSilverTexts4 = UIHelper.GetGoldAndSilverTexts(reward4, out text11, out text12);
					GUITools.AddChild(this.RewardCreditsGoldContent.gameObject, this.RewardCreditsGoldItemPrefab).GetComponent<RewardCreditsGoldItem>().Init((!goldAndSilverTexts4) ? string.Format("{0} 0", MeasuringSystemManager.GetCurrencyIcon("GC")) : text12);
					List<StoreProduct> productsFromReward = TournamentHelper.GetProductsFromReward(reward4);
					List<ShopLicense> licensesFromReward = TournamentHelper.GetLicensesFromReward(reward4);
					List<RewardInventoryItemBrief> list = ((reward4.ItemsBrief == null) ? new List<RewardInventoryItemBrief>() : reward4.ItemsBrief);
					if (productsFromReward.Count > 0 || licensesFromReward.Count > 0 || list.Count > 0)
					{
						GUITools.AddChild(this.RewardProductsContent, this.RewardProductItemPrefab).GetComponent<RewardSponsoredItem>().Init(productsFromReward, licensesFromReward, list);
					}
				}
			}
			float num6 = -741.1f;
			if (this.RewardProductsContent.transform.childCount > 0)
			{
				num6 = 118.45f;
				for (int k = 0; k < this.RewardProductsContent.transform.childCount; k++)
				{
					float height = this.RewardProductsContent.transform.GetChild(k).GetComponent<RewardSponsoredItem>().Height;
					if (this.RewardPlacesContent.transform.childCount > k)
					{
						this.RewardPlacesContent.transform.GetChild(k).GetComponent<LayoutElement>().preferredHeight = height;
					}
					this.RewardCreditsGoldContent.transform.GetChild(k).GetComponent<LayoutElement>().preferredHeight = height;
				}
			}
			else if (this.RewardCreditsGoldContent.transform.childCount > 0)
			{
				num6 = -521.8f;
			}
			if (!flag3 && this.RewardPlacesContent.transform.childCount > 0)
			{
				num6 = 118.45f;
			}
			this.RewardsScrollContent.offsetMax = new Vector2(num6, this.RewardsScrollContent.offsetMax.y);
		}
	}

	private void FillDetails(bool fillRules)
	{
		base.ClearContent(this.DetailFishesContent.gameObject);
		base.ClearContent(this.DetailEquipmentContent.gameObject);
		this._detailEquipments.Clear();
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		Dictionary<string, TournamentDetailFish.TournamentDetailFishData> dictionary = new Dictionary<string, TournamentDetailFish.TournamentDetailFishData>();
		List<FishBrief> list = UserCompetitionHelper.GetFishBriefs(this.Ugc.FishScore).ToList<FishBrief>();
		for (int i = 0; i < list.Count; i++)
		{
			FishBrief fishBrief = list[i];
			UIHelper.FishTypes fishType = UIHelper.GetFishType(fishBrief);
			string fishCodeName = UIHelper.GetFishCodeName(fishBrief, fishType);
			if (!dictionary.ContainsKey(fishCodeName))
			{
				dictionary[fishCodeName] = new TournamentDetailFish.TournamentDetailFishData(fishBrief);
			}
			dictionary[fishCodeName].AddFishTypeEnabled(fishType);
		}
		dictionary.Values.ToList<TournamentDetailFish.TournamentDetailFishData>().ForEach(delegate(TournamentDetailFish.TournamentDetailFishData f)
		{
			GUITools.AddChild(this.DetailFishesContent.gameObject, this.DetailFishPrefab).GetComponent<TournamentDetailFish>().Init(f);
		});
		if (fillRules)
		{
			this.AddRule<UserCompetitionEquipmentAllowed>(this.Ugc.DollEquipment, "UGC_Equipment", null, (UserCompetitionEquipmentAllowed eq) => UgcConsts.GetEquipmentAllowedLoc(eq).Name);
			if (this.Ugc.Type == UserCompetitionType.Custom)
			{
				this.AddRule<UserCompetitionRodEquipmentAllowed>(this.Ugc.RodEquipment, "UGC_Setups", (UserCompetitionRodEquipmentAllowed eq) => profile.HasCorrespondingRod(eq), (UserCompetitionRodEquipmentAllowed eq) => UgcConsts.GetUserCompetitionRodEquipmentAllowedLoc(eq).Name);
			}
		}
	}

	private void TogglesUpdate(UserCompetitionPublic t)
	{
		if (t.Format == UserCompetitionFormat.Team)
		{
			base.ChangePanelForToggle(this.Score, this.ScorePanelTeam);
			base.ChangePanelForToggle(this.Rewards, this.RewardsPanelTeam);
		}
		else if (this._finalResult != null)
		{
			base.ChangePanelForToggle(this.Rewards, this.RewardsPanelFinal);
		}
		this.Score.gameObject.SetActive(t.IsEnded);
		PlayButtonEffect.SetToogleOn(t.IsEnded, this.Score);
		PlayButtonEffect.SetToogleOn(!t.IsEnded, this.Details);
	}

	private void AddRule<T>(T[] array, string title, Func<T, bool> funcCheckEquiped, Func<T, string> funcLoc)
	{
		if (array != null)
		{
			TournamentDetailEquipment component = GUITools.AddChild(this.DetailEquipmentContent.gameObject, this.DetailEquipmentTitlePrefab).GetComponent<TournamentDetailEquipment>();
			component.Init(ScriptLocalization.Get(title).ToUpper(), typeof(T), true);
			this._detailEquipments.Add(component);
			foreach (T t in array)
			{
				bool flag = funcCheckEquiped == null || funcCheckEquiped(t);
				TournamentDetailEquipment component2 = GUITools.AddChild(this.DetailEquipmentContent.gameObject, this.DetailEquipmentPrefab).GetComponent<TournamentDetailEquipment>();
				component2.Init(funcLoc(t), (!flag) ? ScriptLocalization.Get("UGC_NotEquippedDetails") : string.Empty, typeof(T), false);
				this._detailEquipments.Add(component2);
			}
		}
	}

	private void FillScore(List<TournamentIndividualResults> results, int num)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		try
		{
			base.ClearContent(this.ScoreContentRt.gameObject);
			float num2 = 0f;
			int count = results.Count;
			TournamentIndividualResults tournamentIndividualResults = null;
			results = this.PrepareIndividualResults(results);
			for (int i = 0; i < results.Count; i++)
			{
				TournamentIndividualResults tournamentIndividualResults2 = results[i];
				bool flag = profile.UserId == tournamentIndividualResults2.UserId;
				GameObject gameObject = this.ScorePlayerPrefab;
				if (flag)
				{
					gameObject = this.ScoreOwnerPrefab;
					tournamentIndividualResults = tournamentIndividualResults2;
				}
				else if (tournamentIndividualResults2.Place != null && tournamentIndividualResults2.Place > 0 && tournamentIndividualResults2.Place < 4)
				{
					gameObject = this.ScorePlayerWinPrefab;
				}
				num2 += gameObject.GetComponent<LayoutElement>().preferredHeight;
				GameObject gameObject2 = this.ScoreContentRt.gameObject;
				GameObject gameObject3 = gameObject;
				TournamentIndividualResults tournamentIndividualResults3 = tournamentIndividualResults2;
				bool flag2 = flag;
				int? place = tournamentIndividualResults2.Place;
				base.AddTournamentResultPlayer(gameObject2, gameObject3, tournamentIndividualResults3, flag2, (place == null) ? 0 : place.Value, this.Ugc);
			}
			if (count > 9)
			{
				GameObject gameObject4 = GUITools.AddChild(this.ScoreOwnerContent.gameObject, this.ScoreEpmtyPrefab);
				gameObject4.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				GameObject gameObject5 = this.ScoreOwnerContent.gameObject;
				GameObject scoreOwnerPrefab = this.ScoreOwnerPrefab;
				TournamentIndividualResults tournamentIndividualResults4 = tournamentIndividualResults;
				bool flag3 = true;
				int? place2 = tournamentIndividualResults.Place;
				TournamentResultPlayer tournamentResultPlayer = base.AddTournamentResultPlayer(gameObject5, scoreOwnerPrefab, tournamentIndividualResults4, flag3, (place2 == null) ? 0 : place2.Value, this.Ugc);
				RectTransform component = tournamentResultPlayer.GetComponent<RectTransform>();
				LayoutElement component2 = tournamentResultPlayer.GetComponent<LayoutElement>();
				component.anchoredPosition = new Vector2(component2.preferredWidth / 2f, component2.preferredHeight / 2f);
				this.OwnerInfo = tournamentResultPlayer.GetComponent<CanvasGroup>();
				this.OwnerInfo.alpha = 0f;
				float spacing = this.ScoreContentRt.GetComponent<VerticalLayoutGroup>().spacing;
				this.CorrectScoreContentHeight(num2 + spacing * (float)count);
			}
		}
		catch (Exception ex)
		{
			string text = string.Format("TournamentDetailsMessageNew::FillScore e.Message:{0} results.Count:{1} profile != null:{2}", ex.Message, (results != null) ? results.Count : (-1), profile != null);
			PhotonConnectionFactory.Instance.PinError(text, ex.StackTrace);
		}
	}

	protected override void Instance_OnGotIntermediateTournamentTeamResult(List<TournamentTeamResult> results, int totalParticipants)
	{
		base.Instance_OnGotIntermediateTournamentTeamResult(results, totalParticipants);
		this.FillTeamsScore(results);
	}

	protected override void Instance_OnGotFinalTournamentTeamResult(List<TournamentTeamResult> results, int totalParticipants)
	{
		base.Instance_OnGotFinalTournamentTeamResult(results, totalParticipants);
		this.FillTeamsScore(results);
	}

	private void FillTeamsScore(List<TournamentTeamResult> results)
	{
		for (int i = 0; i < this.ScoreTeams.Length; i++)
		{
			base.ClearContent(this.ScoreTeams[i].ScoreTeamContent.gameObject);
		}
		if (results.Count == 0)
		{
			return;
		}
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		IEnumerable<TournamentTeamResult> enumerable = results.Where((TournamentTeamResult p) => p.Place == null);
		results = (from p in results
			where p.Place != null
			orderby p.Place
			select p).ToList<TournamentTeamResult>();
		results.AddRange(enumerable);
		for (int j = 0; j < results.Count; j++)
		{
			TournamentTeamResult tournamentTeamResult = results[j];
			TournamentDetailsMessageBaseNew.ScoreComponentTeam scoreComponentTeam = this.ScoreTeams[j];
			int num = tournamentTeamResult.IndividualResults.Length;
			scoreComponentTeam.ScoreTeamName.text = string.Format("{0} {1}", UgcConsts.GetTeamLoc(tournamentTeamResult.Team), UgcConsts.GetGrey(string.Format("({0})", num))).ToUpper();
			scoreComponentTeam.ScorePrimaryValue.text = UserCompetitionHelper.GetScoreString(this.Ugc.ScoringType, tournamentTeamResult.Score, this.Ugc.TotalScoreKind);
			scoreComponentTeam.ScoreSecondaryValue.text = UserCompetitionHelper.GetScoreString(this.Ugc.SecondaryScoringType, tournamentTeamResult.SecondaryScore, this.Ugc.TotalScoreKind);
			scoreComponentTeam.ScoreTeamImg.color = UgcConsts.GetTeamColor(tournamentTeamResult.Team);
			float num2 = 0f;
			TournamentIndividualResults tournamentIndividualResults = null;
			List<TournamentIndividualResults> list = this.PrepareIndividualResults(tournamentTeamResult.IndividualResults.ToList<TournamentIndividualResults>());
			for (int k = 0; k < num; k++)
			{
				TournamentIndividualResults tournamentIndividualResults2 = list[k];
				bool flag = profile.UserId == tournamentIndividualResults2.UserId;
				GameObject gameObject = ((!flag) ? this.ScoreTeamPlayerPrefab : this.ScoreTeamOwnerPrefab);
				num2 += gameObject.GetComponent<LayoutElement>().preferredHeight;
				if (flag)
				{
					tournamentIndividualResults = tournamentIndividualResults2;
				}
				base.AddTournamentResultPlayer(scoreComponentTeam.ScoreTeamContent.gameObject, gameObject, tournamentIndividualResults2, flag, k + 1, this.Ugc);
			}
			if (num > 6)
			{
				GameObject gameObject2 = GUITools.AddChild(scoreComponentTeam.ScoreOwnerTeamContent.gameObject, this.ScoreTeamEpmtyPrefab);
				gameObject2.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
				float spacing = scoreComponentTeam.ScoreTeamContent.GetComponent<VerticalLayoutGroup>().spacing;
				this.CorrectTeamScoreContentHeight(num2 + spacing * (float)num);
			}
			if (tournamentIndividualResults != null)
			{
				GameObject gameObject3 = scoreComponentTeam.ScoreOwnerTeamContent.gameObject;
				GameObject scoreTeamOwnerPrefab = this.ScoreTeamOwnerPrefab;
				TournamentIndividualResults tournamentIndividualResults3 = tournamentIndividualResults;
				bool flag2 = true;
				int? place = tournamentIndividualResults.Place;
				TournamentResultPlayer tournamentResultPlayer = base.AddTournamentResultPlayer(gameObject3, scoreTeamOwnerPrefab, tournamentIndividualResults3, flag2, (place == null) ? 0 : place.Value, this.Ugc);
				RectTransform component = tournamentResultPlayer.GetComponent<RectTransform>();
				LayoutElement component2 = tournamentResultPlayer.GetComponent<LayoutElement>();
				component.anchoredPosition = new Vector2(component2.preferredWidth / 2f, component2.preferredHeight / 2f);
				component.sizeDelta = new Vector2(component2.preferredWidth, component2.preferredHeight);
				this.OwnerTeamInfo = tournamentResultPlayer.GetComponent<CanvasGroup>();
				this.OwnerTeamInfo.alpha = 0f;
			}
		}
		int? place2 = results[0].Place;
		int valueOrDefault = place2.GetValueOrDefault();
		int? place3 = results[1].Place;
		bool flag3 = valueOrDefault == place3.GetValueOrDefault() && place2 != null == (place3 != null);
		if (flag3)
		{
			TournamentDetailsMessageBaseNew.ScoreComponentTeam scoreComponentTeam2 = this.ScoreTeams[0];
			TournamentDetailsMessageBaseNew.ScoreComponentTeam scoreComponentTeam3 = this.ScoreTeams[1];
			scoreComponentTeam2.ScoreTeamCupImg.gameObject.SetActive(false);
			TMP_Text scorePrimaryValue = scoreComponentTeam2.ScorePrimaryValue;
			TMP_FontAsset font = scoreComponentTeam3.ScorePrimaryValue.font;
			scoreComponentTeam2.ScoreSecondaryValue.font = font;
			scorePrimaryValue.font = font;
			Graphic scorePrimaryValue2 = scoreComponentTeam2.ScorePrimaryValue;
			Color color = scoreComponentTeam3.ScorePrimaryValue.color;
			scoreComponentTeam2.ScoreSecondaryValue.color = color;
			scorePrimaryValue2.color = color;
			RectTransform component3 = scoreComponentTeam2.ScoreTeamImg.GetComponent<RectTransform>();
			RectTransform component4 = scoreComponentTeam3.ScoreTeamImg.GetComponent<RectTransform>();
			float num3 = (component3.rect.height - component4.rect.height) / 2f;
			component4.sizeDelta = new Vector2(component4.rect.width, component3.rect.height);
			component4.anchoredPosition = new Vector2(component4.anchoredPosition.x, component4.anchoredPosition.y + num3);
		}
	}

	private List<TournamentIndividualResults> PrepareIndividualResults(List<TournamentIndividualResults> results)
	{
		List<TournamentIndividualResults> list = (from p in results
			where p.Place != null
			orderby p.Place
			select p).ToList<TournamentIndividualResults>();
		IEnumerable<TournamentIndividualResults> enumerable = results.Where((TournamentIndividualResults p) => p.Place == null);
		list.AddRange(enumerable);
		return list;
	}

	private void CorrectScoreContentHeight(float preferredHeight)
	{
		this.CorrectScoreContentHeight(preferredHeight, this.ScoreContentRt);
	}

	private void CorrectTeamScoreContentHeight(float preferredHeight)
	{
		this.CorrectScoreContentHeight(preferredHeight, this.ScoreTeamScrollContent);
	}

	private void CorrectScoreContentHeight(float preferredHeight, RectTransform scrollContent)
	{
		scrollContent.sizeDelta = new Vector2(scrollContent.rect.width, preferredHeight);
		ScrollRect component = scrollContent.parent.GetComponent<ScrollRect>();
		component.verticalNormalizedPosition = 1f;
	}

	private IEnumerator CorrectionDetailsScrollFishesContent()
	{
		while (this.Ugc == null)
		{
			yield return null;
		}
		if (this.Ugc.Type == UserCompetitionType.Custom)
		{
			while (this._detailEquipments.Count == 0)
			{
				yield return null;
			}
		}
		yield return new WaitForEndOfFrame();
		float height = ((this.Ugc.Type != UserCompetitionType.Custom) ? this.DetailsContentRules.rect.height : ((float)this._detailEquipments.Count * (this._detailEquipments[0].GetComponent<LayoutElement>().preferredHeight + this.DetailEquipmentContent.GetComponent<VerticalLayoutGroup>().spacing)));
		float h = Mathf.Max(this.DetailFishesContent.rect.height, height);
		this.DetailsScrollFishesContent.sizeDelta = new Vector2(this.DetailsScrollFishesContent.rect.width, h);
		yield break;
	}

	private IEnumerator SetOwnerInfoVisibility()
	{
		yield return new WaitForEndOfFrame();
		this.ScoreListScrollChanged();
		yield break;
	}

	private IEnumerator SetOwnerTeamInfoVisibility()
	{
		yield return new WaitForEndOfFrame();
		this.ScoreListTeamScrollChanged();
		yield break;
	}

	private IEnumerator CallEquipmentScrollChanged()
	{
		yield return new WaitForEndOfFrame();
		List<TournamentDetailEquipment> titles = this._detailEquipments.Where((TournamentDetailEquipment p) => p.IsTitle).ToList<TournamentDetailEquipment>();
		titles.ForEach(delegate(TournamentDetailEquipment p)
		{
			p.SetActive(true);
		});
		for (int i = 0; i < titles.Count; i++)
		{
			TournamentDetailEquipment eq = titles[i];
			bool flag = true;
			List<TournamentDetailEquipment> list = this._detailEquipments.Where((TournamentDetailEquipment p) => p.EquipmentType == eq.EquipmentType && !p.IsTitle).ToList<TournamentDetailEquipment>();
			for (int j = 0; j < list.Count; j++)
			{
				TournamentDetailEquipment tournamentDetailEquipment = list[j];
				bool flag2 = RectTransformUtility.RectangleContainsScreenPoint(this.DetailEquipmentScrollRect, this.Cam.WorldToScreenPoint(tournamentDetailEquipment.transform.position), this.Cam);
				if (flag2)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				eq.SetActive(false);
			}
		}
		for (int k = 0; k < titles.Count; k++)
		{
			TournamentDetailEquipment tournamentDetailEquipment2 = titles[k];
			if (tournamentDetailEquipment2.IsActive)
			{
				this.TitleCurrentEquipment.text = tournamentDetailEquipment2.Name;
				break;
			}
		}
		TournamentDetailEquipment current = titles.FirstOrDefault((TournamentDetailEquipment p) => p.Name == this.TitleCurrentEquipment.text);
		if (current != null)
		{
			current.SetActive(false);
		}
		yield break;
	}

	private bool _viewDetailsOnly;

	private List<TournamentDetailEquipment> _detailEquipments = new List<TournamentDetailEquipment>();

	private TournamentFinalResult _finalResult;

	private bool _isEquipmentScrollChanged;
}
