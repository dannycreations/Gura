using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Steamwork.NET;
using Assets.Scripts.UI._2D.PlayerProfile;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoldFishTimeIsUpMessage : MonoBehaviour
{
	public AlphaFade AlphaFade
	{
		get
		{
			return this._af;
		}
	}

	private void Awake()
	{
		this._totalCatchTitle.text = ScriptLocalization.Get("UGC_TotalCatchResult");
		this._totalWeightTitle.text = ScriptLocalization.Get("UGC_TotalWeight");
		this._totalMoneyTitle.text = ScriptLocalization.Get("[TotalTravelCaption]");
		this._catchTitle.text = ScriptLocalization.Get("PhotoModeGlanceCatch");
		this._weightTitle.text = ScriptLocalization.Get("WeightCaption");
		this._lengthTitle.text = ScriptLocalization.Get("UGC_LengthResult");
		this._moneyTitle.text = ScriptLocalization.Get("MoneySortCaption");
		this._primaryTitle.text = ScriptLocalization.Get("UGC_PrimaryResult");
		this._secondaryTitle.text = ScriptLocalization.Get("UGC_SecondaryResult");
		this._currentScoreTitle.text = ScriptLocalization.Get("UGC_CurrentScoreResult");
	}

	private void OnDestroy()
	{
		CacheLibrary.MapCache.OnFishesBriefsLoaded -= this.MapCache_OnFishesBriefsLoaded;
	}

	private void OnEnable()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile != null && this._userAvatar.gameObject.activeSelf && this._userAvatar.gameObject.activeInHierarchy)
		{
			PlayerProfileHelper.SetAvatarIco(profile, this._userAvatar, SteamAvatarLoader.AvatarTypes.Large);
		}
	}

	public void Init(EndTournamentTimeResult r)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		ProfileTournament tournament = profile.Tournament;
		if (tournament == null)
		{
			LogHelper.Error("SoldFishTimeIsUpMessage:Init - Profile.Tournament is Null", new object[0]);
			return;
		}
		this.Bg.enabled = !ScreenManager.Instance.IsIn3D;
		if (CacheLibrary.MapCache.FishesLight != null)
		{
			this.SetFishes(r, CacheLibrary.MapCache.FishesLight);
		}
		else
		{
			this._result = r;
			CacheLibrary.MapCache.OnFishesBriefsLoaded += this.MapCache_OnFishesBriefsLoaded;
		}
		this._fishHint.text = ScriptLocalization.Get((r.SellFishInfo == null || !r.SellFishInfo.FishWasSold) ? "UGC_TournamentAllReleasedFish" : "TournamentAllCaughtFish");
		string text = "-";
		string text2 = string.Empty;
		if (r.SellFishInfo != null && r.SellFishInfo.FishWasSold)
		{
			text = string.Format("{0}", r.SellFishInfo.FishSilver);
			text2 = MeasuringSystemManager.GetCurrencyIcon("SC");
		}
		this._totalMoneyValue.text = string.Format("<b>{0}</b> {1}", text, text2);
		if (tournament.IsUgc())
		{
			UserCompetitionHelper.GetDefaultName(this._tournamentName, tournament.UserCompetition);
		}
		else
		{
			this._tournamentName.text = tournament.Name;
		}
		if (this._userAvatar.gameObject.activeSelf && this._userAvatar.gameObject.activeInHierarchy)
		{
			PlayerProfileHelper.SetAvatarIco(profile, this._userAvatar, SteamAvatarLoader.AvatarTypes.Large);
		}
		this._userName.text = profile.Name;
		TournamentIndividualResults tournamentResult = r.TournamentResult;
		if (tournamentResult.TeamResult != null)
		{
			this._userTeam.color = UgcConsts.GetTeamColor(tournamentResult.TeamResult.Team);
		}
		this._primaryValue.text = MeasuringSystemManager.GetTournamentScoreValueToString(tournament.PrimaryScoring.ScoringType, tournamentResult.Score, tournament.PrimaryScoring.TotalScoreKind, "3");
		this._secondaryValue.text = MeasuringSystemManager.GetTournamentScoreValueToString(tournament.SecondaryScoring.ScoringType, tournamentResult.SecondaryScore, tournament.SecondaryScoring.TotalScoreKind, "3");
	}

	private void SetFishes(EndTournamentTimeResult r, List<FishBrief> fishAssets)
	{
		Profile owner = PhotonConnectionFactory.Instance.Profile;
		float totalWeight = 0f;
		if (r.CatchStats != null)
		{
			int fishCount = ((!r.SellFishInfo.FishWasSold) ? (-1) : r.SellFishInfo.FishInCageCount);
			r.CatchStats.ForEach(delegate(FishRef f)
			{
				FishBrief fishBrief = fishAssets.FirstOrDefault((FishBrief p) => p.FishId == f.FishId);
				float? num = null;
				float? num2 = null;
				if (r.SellFishInfo.FishWasSold && owner.FishCage != null)
				{
					CaughtFish caughtFish = owner.FishCage.Fish.FirstOrDefault((CaughtFish p) => p.Fish.InstanceId == f.InstanceId);
					if (caughtFish != null && caughtFish.Fish != null)
					{
						num = caughtFish.Fish.GoldCost;
						num2 = caughtFish.Fish.SilverCost;
					}
					else
					{
						LogHelper.Log("___kocha SoldFishTimeIsUpMessage: fish skipped for sale FishId:{0} Weight:{1} Length:{2}", new object[] { f.FishId, f.Weight, f.Length });
					}
				}
				totalWeight += f.Weight;
				GUITools.AddChild(this._fishListContent.gameObject, this._fishListPrefab).GetComponent<ConcreteFishInMissionResult>().InitItem(new ConcreteFishInMissionResult.ConcreteFishResultData
				{
					FishCount = fishCount,
					Length = f.Length,
					Name = fishBrief.Name,
					ThumbnailBID = fishBrief.ThumbnailBID,
					Weight = f.Weight,
					Score = f.Score,
					ChangeColorForScoringFish = true,
					GoldCost = num,
					SilverCost = num2,
					WeightAlignmentMiddleRight = false
				});
			});
		}
		this._totalCatchValue.text = string.Format("{0}", this._fishListContent.transform.childCount);
		this._totalWeightValue.text = string.Format("{0} {1}", MeasuringSystemManager.FishWeight(totalWeight).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
		if (this._fishListContent.transform.childCount > 0)
		{
			this.CorrectFishListContentHeight();
		}
	}

	private void MapCache_OnFishesBriefsLoaded()
	{
		CacheLibrary.MapCache.OnFishesBriefsLoaded -= this.MapCache_OnFishesBriefsLoaded;
		this.SetFishes(this._result, CacheLibrary.MapCache.FishesLight);
	}

	private void CorrectFishListContentHeight()
	{
		float spacing = this._fishListContent.GetComponent<VerticalLayoutGroup>().spacing;
		float preferredHeight = this._fishListContent.transform.GetChild(0).GetComponent<LayoutElement>().preferredHeight;
		float height = this._fishListContent.rect.height;
		float num = (float)this._fishListContent.transform.childCount * (preferredHeight + spacing);
		if (num > height)
		{
			this._fishListContent.sizeDelta = new Vector2(this._fishListContent.rect.width, num);
			this._fishListScrollrect.verticalNormalizedPosition = 0f;
		}
	}

	[SerializeField]
	protected Image Bg;

	[SerializeField]
	private TextMeshProUGUI _totalCatchTitle;

	[SerializeField]
	private TextMeshProUGUI _totalMoneyTitle;

	[SerializeField]
	private TextMeshProUGUI _totalWeightTitle;

	[SerializeField]
	private TextMeshProUGUI _currentScoreTitle;

	[SerializeField]
	private TextMeshProUGUI _primaryTitle;

	[SerializeField]
	private TextMeshProUGUI _secondaryTitle;

	[SerializeField]
	private TextMeshProUGUI _catchTitle;

	[SerializeField]
	private TextMeshProUGUI _weightTitle;

	[SerializeField]
	private TextMeshProUGUI _lengthTitle;

	[SerializeField]
	private TextMeshProUGUI _moneyTitle;

	[Space(5f)]
	[SerializeField]
	private GameObject _fishListPrefab;

	[SerializeField]
	private RectTransform _fishListContent;

	[SerializeField]
	private ScrollRect _fishListScrollrect;

	[SerializeField]
	private TextMeshProUGUI _primaryValue;

	[SerializeField]
	private TextMeshProUGUI _secondaryValue;

	[SerializeField]
	private TextMeshProUGUI _totalCatchValue;

	[SerializeField]
	private TextMeshProUGUI _totalWeightValue;

	[SerializeField]
	private TextMeshProUGUI _totalMoneyValue;

	[SerializeField]
	private TextMeshProUGUI _tournamentName;

	[Space(5f)]
	[SerializeField]
	private Image _userTeam;

	[SerializeField]
	private TextMeshProUGUI _userName;

	[SerializeField]
	private Image _userAvatar;

	[SerializeField]
	private TextMeshProUGUI _fishHint;

	[SerializeField]
	private AlphaFade _af;

	private EndTournamentTimeResult _result;
}
