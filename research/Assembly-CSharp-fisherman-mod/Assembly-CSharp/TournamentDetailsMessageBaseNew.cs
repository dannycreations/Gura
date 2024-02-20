using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentDetailsMessageBaseNew : WindowBase
{
	protected override void Awake()
	{
		base.Awake();
		this.PlaceTitle.text = ScriptLocalization.Get("UGC_PlaceResult");
		this.PlayersTitle.text = ScriptLocalization.Get("UGC_AppliedPlayersCountCaption");
		this.PrimaryTitle.text = ScriptLocalization.Get("UGC_PrimaryResult");
		this.SecondaryTitle.text = ScriptLocalization.Get("UGC_SecondaryResult");
		this.TotalPlayersTitle.text = ScriptLocalization.Get("UGC_TotalPlayersResult");
		this.TitleLocation.text = ScriptLocalization.Get("UGC_Location").ToUpper();
		this.TitleDuration.text = ScriptLocalization.Get("UGC_Duration").ToUpper();
		this.TitleENTRYFEE.text = ScriptLocalization.Get("UGC_EntryFeeTournamentCaption").ToUpper();
		this.TitlePreconditions.text = ScriptLocalization.Get("PreconditionsTournamentCaption").ToUpper();
		this.TitleRuleset.text = ScriptLocalization.Get("UGC_Rulesets").ToUpper();
		this.TitleScoring.text = ScriptLocalization.Get("UGC_Scoring").ToUpper();
		this.TitleWeatherForecast.text = ScriptLocalization.Get("WeatherForecast").ToUpper();
		this.TitleScoringFish.text = ScriptLocalization.Get("UGC_Fish").ToUpper();
		this.TitleYoung.text = ScriptLocalization.Get("YoungType");
		this.TitleCommon.text = ScriptLocalization.Get("CommonType");
		this.TitleTrophy.text = ScriptLocalization.Get("TrophyType");
		this.TitleUnique.text = ScriptLocalization.Get("UniqueType");
		this.FishesHint.text = string.Empty;
		this.TitleCurrentEquipment.text = string.Empty;
		this.ScorePrimaryTitles.ToList<TextMeshProUGUI>().ForEach(delegate(TextMeshProUGUI p)
		{
			p.text = this.PrimaryTitle.text;
		});
		this.ScoreSecondaryTitles.ToList<TextMeshProUGUI>().ForEach(delegate(TextMeshProUGUI p)
		{
			p.text = this.SecondaryTitle.text;
		});
		this.ScoreTeams.ToList<TournamentDetailsMessageBaseNew.ScoreComponentTeam>().ForEach(delegate(TournamentDetailsMessageBaseNew.ScoreComponentTeam p)
		{
			p.ScorePlaceTitle.text = this.PlaceTitle.text;
			p.ScorePlayersTitle.text = this.PlayersTitle.text;
		});
		TMP_Text finalRewardTitle = this.FinalRewardTitle;
		string text = ScriptLocalization.Get("MissionRewards");
		this.TeamRewardTitle.text = text;
		finalRewardTitle.text = text;
		TMP_Text finalRewardSponsoredTitle = this.FinalRewardSponsoredTitle;
		text = ScriptLocalization.Get("UGC_PrizePoolSponsoredCaption");
		this.TeamRewardSponsoredTitle.text = text;
		finalRewardSponsoredTitle.text = text;
		this.TeamRewardSponsoredDescription.text = ScriptLocalization.Get("UGC_Team_SponsorRewardEach");
		this.RewardTitle.text = ScriptLocalization.Get("UGC_PrizePoolCaption");
		this.RewardPlacesTitle.text = ScriptLocalization.Get("UGC_PlaceResult");
		this.RewardCreditsTitle.text = ScriptLocalization.Get("MoneySortCaption");
		this.RewardSponsorsRewardsTitle.text = ScriptLocalization.Get("UGC_PrizePoolSponsoredCaption");
		this._alphaFade.ShowFinished += delegate(object s, EventArgs args)
		{
			LogHelper.Log("___kocha TournamentDetails:ShowFinished");
			this.WindowShow();
		};
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.StopAllCoroutines();
		this.UnsubscribeGetUserCompetition();
		this.UnsubscribeGetMetadataForPredefinedCompetitionTemplate();
		PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult -= this.OnGotTournamentIntermediateResult;
		PhotonConnectionFactory.Instance.OnGotTournamentFinalResult -= this.OnGotTournamentFinalResult;
		PhotonConnectionFactory.Instance.OnGotIntermediateTournamentTeamResult -= this.Instance_OnGotIntermediateTournamentTeamResult;
		PhotonConnectionFactory.Instance.OnGotFinalTournamentTeamResult -= this.Instance_OnGotFinalTournamentTeamResult;
	}

	public static void AddRewardAsyncList<T>(List<T> items, ref int rewardsIndex, ref List<ResourcesHelpers.AsyncLoadableData> images, Func<T, string> funcGetName, Func<T, int?> funcGetThumbnail, TournamentDetailsMessageBaseNew.RewardItem[] rewardViews)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (TournamentDetailsMessageBaseNew.AddRewardAsyncLoadableData(ref rewardsIndex, ref images, funcGetName(items[i]), funcGetThumbnail(items[i]), rewardViews))
			{
				break;
			}
		}
	}

	public static bool AddRewardAsyncLoadableData(ref int rewardsIndex, ref List<ResourcesHelpers.AsyncLoadableData> images, string name, int? thumbnailBID, TournamentDetailsMessageBaseNew.RewardItem[] rewardViews)
	{
		TournamentDetailsMessageBaseNew.RewardItem rewardItem = rewardViews[rewardsIndex++];
		rewardItem.ItemName.text = name;
		rewardItem.ItemImage.overrideSprite = null;
		rewardItem.Item.SetActive(true);
		images.Add(new ResourcesHelpers.AsyncLoadableData
		{
			ImagePath = thumbnailBID,
			Image = rewardItem.ItemImage
		});
		return rewardsIndex == rewardViews.Length - 1;
	}

	protected void BuildRewards(Reward r, TournamentDetailsMessageBaseNew.RewardItem[] rewardItems)
	{
		if (r != null)
		{
			List<StoreProduct> productsFromReward = TournamentHelper.GetProductsFromReward(r);
			List<ShopLicense> licensesFromReward = TournamentHelper.GetLicensesFromReward(r);
			List<RewardInventoryItemBrief> list = ((r.ItemsBrief == null) ? new List<RewardInventoryItemBrief>() : r.ItemsBrief);
			List<ResourcesHelpers.AsyncLoadableData> list2 = new List<ResourcesHelpers.AsyncLoadableData>();
			int num = 0;
			TournamentDetailsMessageBaseNew.AddRewardAsyncList<StoreProduct>(productsFromReward, ref num, ref list2, (StoreProduct brief) => brief.Name, (StoreProduct brief) => brief.ImageBID, rewardItems);
			if (num < rewardItems.Length)
			{
				TournamentDetailsMessageBaseNew.AddRewardAsyncList<ShopLicense>(licensesFromReward, ref num, ref list2, (ShopLicense brief) => brief.Name, (ShopLicense brief) => brief.LogoBID, rewardItems);
			}
			if (num < rewardItems.Length)
			{
				TournamentDetailsMessageBaseNew.AddRewardAsyncList<RewardInventoryItemBrief>(list, ref num, ref list2, (RewardInventoryItemBrief brief) => brief.Name, (RewardInventoryItemBrief brief) => brief.ThumbnailBID, rewardItems);
			}
			this.RewardItemLoader.Load(list2, "Textures/Inventory/{0}");
		}
	}

	protected virtual void WindowShow()
	{
	}

	protected void SetWeather(TournamentHelper.TournamentWeatherDesc tw)
	{
		this.ValueTemp.text = tw.WeatherTemperature;
		this.ValueTempIco.text = tw.WeatherIcon;
		this.ValuePreasureIco.text = tw.PressureIcon;
		this.ValueTempWater.text = tw.WaterTemperature;
		this.ValueWind.text = string.Format("{0} {1} {2}", tw.WeatherWindDirection, tw.WeatherWindPower, tw.WeatherWindSuffix);
	}

	protected void UpdateStatus(UserCompetitionPublic t)
	{
		bool flag = t.Format == UserCompetitionFormat.Team;
		bool flag2 = t.TournamentId != 0;
		if (t.IsSponsored)
		{
			flag2 = t.ApproveStatus == UserCompetitionApproveStatus.Approved;
		}
		if (t.IsEnded)
		{
			if (flag)
			{
				PhotonConnectionFactory.Instance.OnGotFinalTournamentTeamResult += this.Instance_OnGotFinalTournamentTeamResult;
				PhotonConnectionFactory.Instance.GetFinalTournamentTeamResult(t.TournamentId);
			}
			else
			{
				PhotonConnectionFactory.Instance.OnGotTournamentFinalResult += this.OnGotTournamentFinalResult;
				PhotonConnectionFactory.Instance.GetFinalTournamentResult(t.TournamentId);
			}
		}
		else if (flag2 && t.IsStarted)
		{
			if (flag)
			{
				PhotonConnectionFactory.Instance.OnGotIntermediateTournamentTeamResult += this.Instance_OnGotIntermediateTournamentTeamResult;
				PhotonConnectionFactory.Instance.GetIntermediateTournamentTeamResult(new int?(t.TournamentId));
			}
			else
			{
				PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult += this.OnGotTournamentIntermediateResult;
				PhotonConnectionFactory.Instance.GetIntermediateTournamentResult(new int?(t.TournamentId));
			}
		}
	}

	protected void ClearContent(GameObject o)
	{
		for (int i = 0; i < o.transform.childCount; i++)
		{
			Object.Destroy(o.transform.GetChild(i).gameObject);
		}
	}

	protected void ChangePanelForToggle(Toggle t, GameObject panel)
	{
		t.graphic = panel.GetComponent<Image>();
		ToggleSetAlphaForContent component = t.GetComponent<ToggleSetAlphaForContent>();
		component.SetContent(t.graphic);
		component.ResetView();
	}

	protected TournamentResultPlayer AddTournamentResultPlayer(GameObject root, GameObject prefab, TournamentIndividualResults p, bool isOwner, int place, UserCompetitionPublic ugc)
	{
		TournamentResultPlayer component = GUITools.AddChild(root, prefab).GetComponent<TournamentResultPlayer>();
		component.Init(new TournamentResultPlayer.TournamentResultPlayerData
		{
			UserId = p.UserId,
			UserName = p.Name,
			UserPlace = new int?(place),
			ScorePrimary = p.Score,
			ScoreSecondary = p.SecondaryScore,
			IsOwner = isOwner,
			InTop3 = (p.Place != null && p.Place > 0 && p.Place < 4),
			ScoringType = ugc.ScoringType,
			SecondaryScoringType = ugc.SecondaryScoringType,
			Team = p.Team,
			TotalScoreKind = ugc.TotalScoreKind
		});
		return component;
	}

	protected virtual void Instance_OnGetUserCompetition(UserCompetitionPublic competition)
	{
		this.UnsubscribeGetUserCompetition();
	}

	protected void UnsubscribeGetUserCompetition()
	{
		PhotonConnectionFactory.Instance.OnGetUserCompetition -= this.Instance_OnGetUserCompetition;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetition -= this.Instance_OnFailureGetUserCompetition;
	}

	protected void Instance_OnFailureGetUserCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeGetUserCompetition();
		LogHelper.Error("TournamentDetailsMessageNew:Instance_OnFailureGetUserCompetition {0}", new object[] { failure.FullErrorInfo });
	}

	protected void UnsubscribeGetMetadataForPredefinedCompetitionTemplate()
	{
		PhotonConnectionFactory.Instance.OnGetMetadataForPredefinedCompetitionTemplate -= this.Instance_OnGetMetadataForPredefinedCompetitionTemplate;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForPredefinedCompetitionTemplate -= this.Instance_OnFailureGetMetadataForPredefinedCompetitionTemplate;
	}

	protected virtual void Instance_OnGetMetadataForPredefinedCompetitionTemplate(UserCompetition metadataTemplate)
	{
		this.UnsubscribeGetMetadataForPredefinedCompetitionTemplate();
	}

	protected void Instance_OnFailureGetMetadataForPredefinedCompetitionTemplate(UserCompetitionFailure failure)
	{
		this.UnsubscribeGetMetadataForPredefinedCompetitionTemplate();
		LogHelper.Error("TournamentDetailsMessageNew:Instance_OnFailureGetMetadataForPredefinedCompetitionTemplate {0}", new object[] { failure.FullErrorInfo });
	}

	protected virtual void Instance_OnGotIntermediateTournamentTeamResult(List<TournamentTeamResult> results, int totalParticipants)
	{
		PhotonConnectionFactory.Instance.OnGotIntermediateTournamentTeamResult -= this.Instance_OnGotIntermediateTournamentTeamResult;
	}

	protected virtual void Instance_OnGotFinalTournamentTeamResult(List<TournamentTeamResult> results, int totalParticipants)
	{
		PhotonConnectionFactory.Instance.OnGotFinalTournamentTeamResult -= this.Instance_OnGotFinalTournamentTeamResult;
	}

	protected virtual void OnGotTournamentIntermediateResult(List<TournamentIndividualResults> results, int num)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentIntermediateResult -= this.OnGotTournamentIntermediateResult;
	}

	protected virtual void OnGotTournamentFinalResult(List<TournamentIndividualResults> results, int num)
	{
		PhotonConnectionFactory.Instance.OnGotTournamentFinalResult -= this.OnGotTournamentFinalResult;
	}

	[SerializeField]
	protected Image Bg;

	[SerializeField]
	protected Toggle Score;

	[SerializeField]
	protected Toggle Details;

	[SerializeField]
	protected Toggle Rewards;

	[SerializeField]
	protected GameObject ScorePanel;

	[SerializeField]
	protected GameObject ScorePanelTeam;

	[SerializeField]
	protected GameObject DetailsPanel;

	[SerializeField]
	protected GameObject RewardsPanel;

	[SerializeField]
	protected GameObject RewardsPanelFinal;

	[SerializeField]
	protected GameObject RewardsPanelTeam;

	[Space(10f)]
	[SerializeField]
	protected Image Logo;

	[SerializeField]
	protected TextMeshProUGUI Title;

	[SerializeField]
	protected TextMeshProUGUI TotalPlayersValue;

	[SerializeField]
	protected TextMeshProUGUI TotalPlayersTitle;

	[Space(10f)]
	[SerializeField]
	protected RectTransform ScoreContentScrollRect;

	[SerializeField]
	protected RectTransform ScoreContentRt;

	[SerializeField]
	protected RectTransform ScoreOwnerContent;

	[SerializeField]
	protected GameObject ScoreOwnerPrefab;

	[SerializeField]
	protected GameObject ScorePlayerPrefab;

	[SerializeField]
	protected GameObject ScorePlayerWinPrefab;

	[SerializeField]
	protected GameObject ScoreEpmtyPrefab;

	[SerializeField]
	protected TextMeshProUGUI PlaceTitle;

	[SerializeField]
	protected TextMeshProUGUI PlayersTitle;

	[SerializeField]
	protected TextMeshProUGUI PrimaryTitle;

	[SerializeField]
	protected TextMeshProUGUI SecondaryTitle;

	[Space(10f)]
	[SerializeField]
	protected TextMeshProUGUI TitleLocation;

	[SerializeField]
	protected TextMeshProUGUI ValueLocation;

	[SerializeField]
	protected TextMeshProUGUI TitleDuration;

	[SerializeField]
	protected TextMeshProUGUI TitleENTRYFEE;

	[SerializeField]
	protected TextMeshProUGUI ValueDuration;

	[SerializeField]
	protected TextMeshProUGUI ValueFree;

	[SerializeField]
	protected TextMeshProUGUI TitlePreconditions;

	[SerializeField]
	protected TextMeshProUGUI ValueMin;

	[SerializeField]
	protected TextMeshProUGUI ValueMax;

	[SerializeField]
	protected TextMeshProUGUI PreconditionIcoMin;

	[SerializeField]
	protected TextMeshProUGUI PreconditionIcoMax;

	[SerializeField]
	protected TextMeshProUGUI TitleRuleset;

	[SerializeField]
	protected TextMeshProUGUI ValueRuleset;

	[SerializeField]
	protected TextMeshProUGUI TitleScoring;

	[SerializeField]
	protected TextMeshProUGUI ValueScoring;

	[SerializeField]
	protected TextMeshProUGUI TitleWeatherForecast;

	[SerializeField]
	protected TextMeshProUGUI ValueTemp;

	[SerializeField]
	protected TextMeshProUGUI ValueTempIco;

	[SerializeField]
	protected TextMeshProUGUI ValuePreasureIco;

	[SerializeField]
	protected TextMeshProUGUI ValueTempWater;

	[SerializeField]
	protected TextMeshProUGUI ValueWind;

	[SerializeField]
	protected GameObject DetailFishPrefab;

	[SerializeField]
	protected RectTransform DetailFishesContent;

	[SerializeField]
	protected TextMeshProUGUI TitleScoringFish;

	[SerializeField]
	protected TextMeshProUGUI TitleYoung;

	[SerializeField]
	protected TextMeshProUGUI TitleCommon;

	[SerializeField]
	protected TextMeshProUGUI TitleTrophy;

	[SerializeField]
	protected TextMeshProUGUI TitleUnique;

	[SerializeField]
	protected TextMeshProUGUI TitleCurrentEquipment;

	[SerializeField]
	protected GameObject DetailEquipmentTitlePrefab;

	[SerializeField]
	protected GameObject DetailEquipmentPrefab;

	[SerializeField]
	protected RectTransform DetailEquipmentContent;

	[SerializeField]
	protected RectTransform DetailEquipmentScrollRect;

	[SerializeField]
	protected TextMeshProUGUI FishesHint;

	[SerializeField]
	protected TextMeshProUGUI DetailRulesValue;

	[SerializeField]
	protected RectTransform DetailsContentRules;

	[SerializeField]
	protected GameObject DetailsContentRuleset;

	[SerializeField]
	protected RectTransform DetailsScrollFishesContent;

	[Space(10f)]
	[SerializeField]
	protected RectTransform ScoreTeamContentScrollRect;

	[SerializeField]
	protected RectTransform ScoreTeamScrollContent;

	[SerializeField]
	protected TournamentDetailsMessageBaseNew.ScoreComponentTeam[] ScoreTeams;

	[SerializeField]
	protected TextMeshProUGUI[] ScorePrimaryTitles;

	[SerializeField]
	protected TextMeshProUGUI[] ScoreSecondaryTitles;

	[SerializeField]
	protected GameObject ScoreTeamOwnerPrefab;

	[SerializeField]
	protected GameObject ScoreTeamPlayerPrefab;

	[SerializeField]
	protected GameObject ScoreTeamEpmtyPrefab;

	[Space(10f)]
	[SerializeField]
	protected GameObject TeamSponsoredReward;

	[SerializeField]
	protected TextMeshProUGUI TeamRewardTitle;

	[SerializeField]
	protected TextMeshProUGUI TeamRewardValue;

	[SerializeField]
	protected TextMeshProUGUI TeamRewardDescription;

	[SerializeField]
	protected TextMeshProUGUI TeamRewardSponsoredTitle;

	[SerializeField]
	protected TextMeshProUGUI TeamRewardSponsoredValue;

	[SerializeField]
	protected TextMeshProUGUI TeamRewardSponsoredDescription;

	[SerializeField]
	protected TournamentDetailsMessageBaseNew.RewardItem[] TeamRewardSponsoredItems;

	[SerializeField]
	protected HorizontalLayoutGroup TeamRewardSponsoredContent;

	[Space(10f)]
	[SerializeField]
	protected TournamentDetailsMessageBaseNew.RewardItem[] FinalRewardItems;

	[SerializeField]
	protected GameObject FinalSponsoredReward;

	[SerializeField]
	protected TextMeshProUGUI FinalRewardTitle;

	[SerializeField]
	protected TextMeshProUGUI FinalRewardValue;

	[SerializeField]
	protected TextMeshProUGUI FinalRewardSponsoredTitle;

	[SerializeField]
	protected TextMeshProUGUI FinalRewardSponsoredValue;

	[Space(10f)]
	[SerializeField]
	protected RectTransform RewardsScrollContent;

	[SerializeField]
	protected TextMeshProUGUI RewardTitle;

	[SerializeField]
	protected TextMeshProUGUI RewardValue;

	[SerializeField]
	protected TextMeshProUGUI RewardDescription;

	[SerializeField]
	protected TextMeshProUGUI RewardPlacesTitle;

	[SerializeField]
	protected TextMeshProUGUI RewardCreditsTitle;

	[SerializeField]
	protected TextMeshProUGUI RewardSponsorsRewardsTitle;

	[SerializeField]
	protected RectTransform RewardPlacesContent;

	[SerializeField]
	protected GameObject RewardPlacesItemPrefab;

	[SerializeField]
	protected RectTransform RewardCreditsGoldContent;

	[SerializeField]
	protected GameObject RewardCreditsGoldItemPrefab;

	[SerializeField]
	protected GameObject RewardProductsContent;

	[SerializeField]
	protected GameObject RewardProductItemPrefab;

	[SerializeField]
	protected RectTransform RewardContent;

	protected const float RewardsScrollRightOne = -741.1f;

	protected const float RewardsScrollRightTwo = -521.8f;

	protected const float RewardsScrollRightThree = 118.45f;

	protected const int ScoreMaxPlayers = 9;

	protected const int ScoreMaxPlayersTeam = 6;

	protected const float OwnerInfoAnimTime = 0.2f;

	protected const float Offset = 30f;

	protected const int ItemSubscriberId = 1354512;

	protected ResourcesHelpers.AsyncLoadableImage ImgLoader = new ResourcesHelpers.AsyncLoadableImage();

	protected ResourcesHelpers.AsyncLoadableImage RewardItemLoader = new ResourcesHelpers.AsyncLoadableImage();

	protected CanvasGroup OwnerInfo;

	protected CanvasGroup OwnerTeamInfo;

	[Serializable]
	public class ScoreComponentTeam
	{
		[SerializeField]
		public Image ScoreTeamCupImg;

		[SerializeField]
		public Image ScoreTeamImg;

		[SerializeField]
		public TextMeshProUGUI ScoreTeamName;

		[SerializeField]
		public TextMeshProUGUI ScorePrimaryValue;

		[SerializeField]
		public TextMeshProUGUI ScoreSecondaryValue;

		[SerializeField]
		public RectTransform ScoreTeamContent;

		[SerializeField]
		public RectTransform ScoreOwnerTeamContent;

		[SerializeField]
		public TextMeshProUGUI ScorePlaceTitle;

		[SerializeField]
		public TextMeshProUGUI ScorePlayersTitle;
	}

	[Serializable]
	public class RewardItem
	{
		[SerializeField]
		public TextMeshProUGUI ItemName;

		[SerializeField]
		public Image ItemImage;

		[SerializeField]
		public GameObject Item;
	}
}
