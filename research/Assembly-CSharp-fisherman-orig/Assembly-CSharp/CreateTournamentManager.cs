using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces.Tournaments;
using UnityEngine;
using UnityEngine.UI;

public class CreateTournamentManager : MainPageItem
{
	protected virtual void Awake()
	{
		List<UserCompetitionDuration> list = new List<UserCompetitionDuration> { UserCompetitionDuration.None };
		if (!PhotonConnectionFactory.Instance.IsTest)
		{
			list.Add(UserCompetitionDuration.Min2);
			list.Add(UserCompetitionDuration.Min5);
		}
		this.AllUserCompetitionDurations = UserCompetitionHelper.EnumToList<UserCompetitionDuration>(list);
		this.Init();
	}

	protected virtual UserCompetitionType CompetitionType()
	{
		return UserCompetitionType.Predefined;
	}

	protected virtual bool SponsoredEnabled()
	{
		return false;
	}

	protected virtual List<UgcConsts.Components> BlockedComponents()
	{
		return new List<UgcConsts.Components>
		{
			UgcConsts.Components.Name,
			UgcConsts.Components.Location,
			UgcConsts.Components.Fish,
			UgcConsts.Components.Scoring,
			UgcConsts.Components.Setups,
			UgcConsts.Components.Equipment,
			UgcConsts.Components.Rulesets,
			UgcConsts.Components.TimeAdnWeather
		};
	}

	protected virtual List<UgcConsts.Components> Last()
	{
		return new List<UgcConsts.Components>
		{
			UgcConsts.Components.InitialPrizePool,
			UgcConsts.Components.TimeAdnWeather,
			UgcConsts.Components.Equipment
		};
	}

	public virtual void LoadCompetition(UserCompetitionPublic c)
	{
	}

	protected override void SetHelp()
	{
		base.SetHelp();
		UIHelper.Waiting(false, null);
		this.GetUserCompetitions();
		PhotonConnectionFactory.Instance.OnUserCompetitionApprovedOnReview += this.Instance_OnUserCompetitionApprovedOnReview;
		PhotonConnectionFactory.Instance.OnUserCompetitionDeclinedOnReview += this.Instance_OnUserCompetitionDeclinedOnReview;
		PhotonConnectionFactory.Instance.OnUserCompetitionRemovedOnReview += this.Instance_OnUserCompetitionRemovedOnReview;
	}

	protected override void HideHelp()
	{
		this.UnsubscribeLoadCompetition();
		this.UnsubscribeGetUserCompetitions();
		this.UnsubscribeSendToReviewCompetition();
		this.UnsubscribeSaveCompetition();
		this.UnsubscribePublishCompetition();
		this.UnsubscribeRegisterInCompetitio();
		this.UnsubscribeGetMetadataForPredefinedCompetitionTemplate();
		this.UnsubscribeGetMetadataForCompetitionPondWeather();
		PhotonConnectionFactory.Instance.OnUserCompetitionApprovedOnReview -= this.Instance_OnUserCompetitionApprovedOnReview;
		PhotonConnectionFactory.Instance.OnUserCompetitionDeclinedOnReview -= this.Instance_OnUserCompetitionDeclinedOnReview;
		PhotonConnectionFactory.Instance.OnUserCompetitionRemovedOnReview -= this.Instance_OnUserCompetitionRemovedOnReview;
	}

	protected void Init()
	{
		this.SponsoredTglColorTransitionChanges = this.SponsoredTgl.GetComponent<ToggleColorTransitionChanges>();
		this.SponsoredTgl.GetComponent<CanvasGroup>().alpha = ((!this.SponsoredEnabled()) ? 0f : 1f);
		this.UpdateSponsoredInteractable();
		this.AddSponsoredTglListener();
		List<UgcConsts.Components> list = this.Last();
		List<UgcConsts.Components> list2 = this.BlockedComponents();
		for (int i = 0; i < this.ComponentsData.Length; i++)
		{
			for (int j = 0; j < this.ComponentsData[i].Types.Length; j++)
			{
				GameObject gameObject = this.SimpleItemPrefab;
				UgcConsts.Components t = this.ComponentsData[i].Types[j];
				bool isBlocked = list2.Contains(t);
				if (t == UgcConsts.Components.FormatTypes)
				{
					gameObject = this.ThreeButtonsItemPrefab;
				}
				else if (t == UgcConsts.Components.MaxPlayers)
				{
					gameObject = this.UpDownItemPrefab;
				}
				else if (t == UgcConsts.Components.TimeAdnWeather)
				{
					gameObject = this.SimpleItemWeatherPrefab;
				}
				GameObject gameObject2 = GUITools.AddChild(this.ComponentsData[i].ItemsRoot, gameObject);
				CompetitionViewItem component = gameObject2.GetComponent<CompetitionViewItem>();
				component.SetBlocked(isBlocked);
				if (t == UgcConsts.Components.FormatTypes)
				{
					gameObject2.GetComponent<CompetitionViewItemThreeBtns>().OnSelectFormat += delegate(UserCompetitionFormat f)
					{
						if (!isBlocked)
						{
							this.SetFormatTypes(f);
							if (this.SponsoredEnabled())
							{
								if (this.Competition.Format == UserCompetitionFormat.Duel && this.Competition.IsSponsored)
								{
									this.Competition.IsSponsored = false;
								}
								this.UpdateSponsored();
								this.UpdateSponsoredInteractable();
								this.UpdateBlocked();
							}
						}
					};
				}
				else if (t == UgcConsts.Components.MaxPlayers)
				{
					gameObject2.GetComponent<CompetitionViewItemUpDown>().OnValueChanged += delegate(int v)
					{
						if (!isBlocked && this.Competition != null)
						{
							this.IncPlayers(v);
						}
					};
				}
				component.OnSelect += delegate
				{
					if (!isBlocked && this.Competition != null)
					{
						this.OpenComponentWindow(t);
					}
				};
				WindowList.WindowListElem componentLoc = UgcConsts.GetComponentLoc(t);
				string desc = componentLoc.Desc;
				string text = string.Format("{0}:", componentLoc.Name);
				component.Init(text, desc, null, this.ToggleGroup, !list.Contains(t));
				if (t == UgcConsts.Components.Level)
				{
					component.SetStarIcoActive(true);
				}
				component.gameObject.SetActive(true);
				this.ComponentsDataCache[t] = component;
			}
		}
	}

	protected virtual void AddSponsoredTglListener()
	{
	}

	protected void SetOnSponsoredTgl(bool isOn)
	{
		this.SponsoredTgl.onValueChanged.RemoveAllListeners();
		PlayButtonEffect.SetToogleOn(isOn, this.SponsoredTgl);
		this.AddSponsoredTglListener();
	}

	protected void GetUserCompetitions()
	{
		LogHelper.Log("___kocha GetUserCompetitions name:{0}", new object[] { base.gameObject.name });
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		this.OwnerUgcList = null;
		PhotonConnectionFactory.Instance.OnGetUserCompetitions += this.Instance_OnGetUserCompetitions;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetitions += new OnFailureUserCompetition(this.Instance_OnFailureGetUserCompetitions);
		PhotonConnectionFactory.Instance.GetUserCompetitions(new FilterForUserCompetitions
		{
			HostUserId = new Guid?(profile.UserId),
			RequestId = this.RequestIdGetUserCompetitions
		});
	}

	protected void Instance_OnGetUserCompetitions(Guid requestId, List<UserCompetitionPublic> list)
	{
		if (this.RequestIdGetUserCompetitions != requestId)
		{
			return;
		}
		this.UnsubscribeGetUserCompetitions();
		this.OwnerUgcList = ((list == null) ? new List<UserCompetitionPublic>() : list);
		if (this.Competition == null)
		{
			this.SetOnSponsoredTgl(false);
			this.Instance_OnGetMetadataForCompetition(CacheLibrary.MapCache.UgcMetadata);
			this.UpdateBlocked();
			this.UpdateSponsoredInteractable();
			return;
		}
		if (list.Count == 0 && this.Competition.TournamentId != 0)
		{
			this.Instance_OnGetMetadataForCompetition(CacheLibrary.MapCache.UgcMetadata);
			this.RefreshCompetition(this.Competition);
		}
		else if (this.Metadata != null && (this.MetadataTemplate != null || this.CompetitionType() != UserCompetitionType.Predefined))
		{
			if (this.Competition != null)
			{
				UserCompetitionPublic userCompetitionPublic = this.OwnerUgcList.FirstOrDefault((UserCompetitionPublic p) => p.TournamentId == this.Competition.TournamentId);
				if (userCompetitionPublic != null)
				{
					this.Competition.ApproveStatus = userCompetitionPublic.ApproveStatus;
				}
			}
			this.RefreshCompetition(this.Competition);
		}
		else if (list.Count > 0 && this.Competition.TournamentId == 0)
		{
			this.UpdateBlocked();
			this.UpdateSponsoredInteractable();
		}
	}

	protected void RefreshCompetition(UserCompetition competition)
	{
		this.Competition = competition;
		this.UpdateName();
		this.UpdateFormat(this.Competition.Format);
		this.UpdateLocation();
		this.UpdateEntryFee();
		this.UpdateInitialPrizePool();
		this.UpdateLevel();
		this.UpdateDuration();
		this.UpdatePlayersCount(this.Competition.Format);
		this.UpdateTimeAndWeather();
		this.UpdateFish();
		this.UpdateScoring();
		this.UpdateSetups();
		this.UpdateEquipment();
		this.UpdateRulesets();
		this.UpdatePrivacy(UgcConsts.PrivacyDataLoc(this.Competition.IsPrivate), this.Competition.IsPrivate, this.Competition.Password);
		this.UpdateSponsored();
		this.UpdateBlocked();
		this.UpdateSponsoredInteractable();
		LogHelper.Log("___kocha RefreshCompetition TournamentId:{0} IsSponsored:{1} ApproveStatus:{2} SponsoredTgl.interactable:{3} IsBlockedSponsored:{4} gameObject.name:{5}", new object[]
		{
			this.Competition.TournamentId,
			this.Competition.IsSponsored,
			this.Competition.ApproveStatus,
			this.SponsoredTgl.interactable,
			this.IsBlockedSponsored,
			base.gameObject.name
		});
		this.Validate();
	}

	protected void Instance_OnLoadCompetition(UserCompetition competition)
	{
		this.UnsubscribeLoadCompetition();
		if (this.Metadata == null)
		{
			this.Instance_OnGetMetadataForCompetition(CacheLibrary.MapCache.UgcMetadata);
		}
		this.RefreshCompetition(competition);
		LogHelper.Log("___kocha LoadCompetition TournamentId:{0} curr:{1}", new object[]
		{
			competition.TournamentId,
			(this.Competition == null) ? (-1) : this.Competition.TournamentId
		});
		UIHelper.Waiting(false, null);
	}

	protected virtual void Instance_OnUserCompetitionRemovedOnReview(UserCompetitionReviewMessage competition)
	{
	}

	protected virtual void Instance_OnUserCompetitionDeclinedOnReview(UserCompetitionReviewMessage competition)
	{
	}

	protected virtual void Instance_OnUserCompetitionApprovedOnReview(UserCompetitionReviewMessage competition)
	{
	}

	public virtual void SetSponsored()
	{
	}

	public virtual void SelectSponsored()
	{
	}

	public virtual void Details()
	{
		if (this.Competition != null)
		{
			TournamentHelper.ShowingTournamentDetails(this.Competition, true, this.Competition.IsSponsored && this.Competition.ApproveStatus == UserCompetitionApproveStatus.Approved, null, false);
		}
	}

	public virtual void Create()
	{
		if (this.Competition != null)
		{
			if (this.Competition.SelectedDayWeather == null)
			{
				LogHelper.Log("___kocha Create competition: SelectedDayWeather == null");
				return;
			}
			if (this.Competition.IsSponsored && this.Competition.ApproveStatus == UserCompetitionApproveStatus.Approved)
			{
				PhotonConnectionFactory.Instance.OnPublishCompetition += this.Instance_OnPublishCompetition;
				PhotonConnectionFactory.Instance.OnFailurePublishCompetition += this.Instance_OnFailurePublishCompetition;
				PhotonConnectionFactory.Instance.PublishCompetition(this.Competition.TournamentId, true, false);
				return;
			}
			if (this.Competition.IsSponsored && this.Competition.TournamentId > 0 && this.Competition.ApproveStatus != UserCompetitionApproveStatus.Declined)
			{
				this.Instance_OnSaveCompetition(this.Competition);
				return;
			}
			UIHelper.ShowYesNo(ScriptLocalization.Get("UGC_CreateCompQuestionCaption"), delegate
			{
				UIHelper.Waiting(true, null);
				PhotonConnectionFactory.Instance.OnSaveCompetition += this.Instance_OnSaveCompetition;
				PhotonConnectionFactory.Instance.OnFailureSaveCompetition += this.Instance_OnFailureSaveCompetition;
				PhotonConnectionFactory.Instance.SaveCompetition(this.Competition, false);
			}, null, "YesCaption", null, "NoCaption", null, null, null);
		}
	}

	public static string GetWeatherNameLoc(string wName)
	{
		wName = Regex.Replace(wName.Replace("_", string.Empty), "[\\d-]", string.Empty);
		return ScriptLocalization.Get(string.Format("UGC_Weather{0}", wName));
	}

	protected virtual void Validate()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		bool flag = this.OwnerUgcList == null || this.OwnerUgcList.Any((UserCompetitionPublic p) => p.TournamentId != this.Competition.TournamentId && profile.UserId == p.HostUserId);
		LogHelper.Log("___kocha Validate isUgcHost:{0} IsBlockedSponsored:{1} Competition.ApproveStatus:{2}", new object[]
		{
			flag,
			this.IsBlockedSponsored,
			this.Competition.ApproveStatus
		});
		this.CreateBtn.interactable = (!this.IsBlockedSponsored || this.Competition.ApproveStatus == UserCompetitionApproveStatus.Approved) && !flag;
	}

	protected bool IsBlockedSponsored
	{
		get
		{
			return this.Competition != null && this.Competition.IsSponsored && (this.Competition.ApproveStatus != UserCompetitionApproveStatus.Declined && this.Competition.ApproveStatus != UserCompetitionApproveStatus.InDevelopment) && this.Competition.ApproveStatus != UserCompetitionApproveStatus.NoReview;
		}
	}

	protected virtual void UpdateBlocked()
	{
		if (this.Competition == null)
		{
			return;
		}
		List<UgcConsts.Components> list = this.BlockedComponents();
		bool isBlockedSponsored = this.IsBlockedSponsored;
		bool flag = this.OwnerUgcList != null && this.OwnerUgcList.Any((UserCompetitionPublic p) => p.TournamentId != this.Competition.TournamentId);
		LogHelper.Log("___kocha UpdateBlocked isBlocked:{0} isBlockedOtherUgc:{1}", new object[] { isBlockedSponsored, flag });
		foreach (KeyValuePair<UgcConsts.Components, CompetitionViewItem> keyValuePair in this.ComponentsDataCache)
		{
			keyValuePair.Value.SetBlocked(isBlockedSponsored || flag || list.Contains(keyValuePair.Key) || (keyValuePair.Key == UgcConsts.Components.Name && !this.Competition.IsSponsored));
		}
	}

	protected virtual void UpdateCompetition()
	{
		this.CreateBtnLbl.text = ScriptLocalization.Get("UGC_CreateCompetition");
		TournamentTemplateBrief tournamentTemplateBrief = null;
		if (StaticUserData.CurrentPond != null)
		{
			tournamentTemplateBrief = this.Metadata.Templates.FirstOrDefault((TournamentTemplateBrief p) => p.PondId == StaticUserData.CurrentPond.PondId);
		}
		if (tournamentTemplateBrief == null)
		{
			tournamentTemplateBrief = this.Metadata.Templates[0];
		}
		this.Competition.Format = UserCompetitionFormat.Individual;
		this.UpdateFormat(this.Competition.Format);
		this.ChangeTournamentTemplate(tournamentTemplateBrief);
		this.UpdateTemplates();
		this.UpdateName();
		this.UpdateEntryFee();
		this.UpdatePrivacy(UgcConsts.PrivacyDataLoc(false), false, null);
		this.UpdateLevel();
		this.UpdateDuration();
		if (this.CompetitionType() == UserCompetitionType.Predefined)
		{
			this.GetMetadataForPredefinedCompetitionTemplate();
		}
		else
		{
			UIHelper.Waiting(false, null);
		}
	}

	protected virtual UserCompetition CreateCompetition
	{
		get
		{
			Range defaultMinMaxLevel = this.GetDefaultMinMaxLevel();
			return new UserCompetition
			{
				EntranceFee = new double?(0.0),
				HostEntranceFee = new double?(0.0),
				SortType = UserCompetitionSortType.Manual,
				Duration = this.AllUserCompetitionDurations.First<UserCompetitionDuration>(),
				IsPrivate = false,
				Currency = "SC",
				Type = this.CompetitionType(),
				FishSource = TournamentFishSource.Catch,
				HostName = PhotonConnectionFactory.Instance.Profile.Name,
				MinLevel = new int?(defaultMinMaxLevel.Min),
				MaxLevel = new int?(defaultMinMaxLevel.Max)
			};
		}
	}

	protected virtual void Instance_OnSaveCompetition(UserCompetition competition)
	{
		this.UnsubscribeSaveCompetition();
		if (!competition.IsSponsored)
		{
			this.Instance_OnPublishCompetition(competition);
		}
		else
		{
			UIHelper.Waiting(false, null);
			InputAreaWnd inputAreaWnd = TournamentHelper.WindowTextArea(ScriptLocalization.Get("UGC_AddCoomentForReview"), false, true);
			inputAreaWnd.OnOk += delegate(string comment)
			{
				LogHelper.Log("___kocha OnOk :{0}", new object[] { comment });
				PhotonConnectionFactory.Instance.OnFailureSendToReviewCompetition += this.Instance_OnFailureSendToReviewCompetition;
				PhotonConnectionFactory.Instance.OnSendToReviewCompetition += this.Instance_OnSendToReviewCompetition;
				PhotonConnectionFactory.Instance.SendToReviewCompetition(competition.TournamentId, comment);
			};
		}
	}

	protected virtual void Instance_OnSendToReviewCompetition(UserCompetition competition)
	{
		this.UnsubscribeSendToReviewCompetition();
	}

	protected virtual void Back(UserCompetitionPublic competition)
	{
		UIHelper.Waiting(false, null);
		UIAudioSourceListener.Instance.Successfull();
		this.Competition = null;
		this.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Create, false, false);
		this.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Room, true, false);
		this.MenuMgr.RoomUserCompetitionCtrl.Init(competition);
	}

	protected virtual void Instance_OnPublishCompetition(UserCompetition competition)
	{
		this.UnsubscribePublishCompetition();
		PhotonConnectionFactory.Instance.OnRegisterInCompetition += this.Instance_OnRegisterInCompetition;
		PhotonConnectionFactory.Instance.OnFailureRegisterInCompetition += this.Instance_OnFailureRegisterInCompetition;
		string text = ((competition.Format != UserCompetitionFormat.Team) ? null : "Red");
		PhotonConnectionFactory.Instance.RegisterInCompetition(competition.TournamentId, text, this.Competition.Password, false);
	}

	protected virtual void Instance_OnRegisterInCompetition(UserCompetitionPublic competition)
	{
		if (TournamentManager.Instance != null)
		{
			TournamentManager.Instance.Refresh();
		}
		this.UnsubscribeRegisterInCompetitio();
		this.Back(competition);
	}

	protected UgcMenuStateManager MenuMgr
	{
		get
		{
			return MenuHelpers.Instance.MenuPrefabsList.UgcMenuManager;
		}
	}

	protected virtual void Instance_OnGetMetadataForCompetition(MetadataForUserCompetition metadata)
	{
		this.Metadata = metadata;
		this.Competition = this.CreateCompetition;
		if (!this.Competition.IsSponsored || !this.IsBlockedSponsored)
		{
			this.UpdateCompetition();
		}
	}

	protected void GetMetadataForPredefinedCompetitionTemplate()
	{
		if (this.Competition.TournamentTemplateId != null)
		{
			PhotonConnectionFactory.Instance.OnGetMetadataForPredefinedCompetitionTemplate += this.Instance_OnGetMetadataForPredefinedCompetitionTemplate;
			PhotonConnectionFactory.Instance.OnFailureGetMetadataForPredefinedCompetitionTemplate += this.Instance_OnFailureGetMetadataForPredefinedCompetitionTemplate;
			PhotonConnectionFactory.Instance.GetMetadataForPredefinedCompetitionTemplate(this.Competition.TournamentTemplateId.Value);
		}
	}

	protected virtual void Instance_OnGetMetadataForPredefinedCompetitionTemplate(UserCompetition metadataTemplate)
	{
		this.UnsubscribeGetMetadataForPredefinedCompetitionTemplate();
		this.MetadataTemplate = metadataTemplate;
		this.Competition.SelectedDayWeather = metadataTemplate.SelectedDayWeather;
		this.Competition.WeatherName = this.Competition.SelectedDayWeather.Name;
		this.Competition.InGameStartHour = metadataTemplate.InGameStartHour;
		this.UpdateTimeAndWeather();
		this.Competition.FishScore = this.MetadataTemplate.FishScore;
		this.Competition.DollEquipment = this.MetadataTemplate.DollEquipment;
		this.Competition.TournamentEquipment = this.MetadataTemplate.TournamentEquipment;
		this.Competition.ScoringType = this.MetadataTemplate.ScoringType;
		this.Competition.ReferenceWeight = this.MetadataTemplate.ReferenceWeight;
		this.Competition.TotalScoreKind = this.MetadataTemplate.TotalScoreKind;
		this.Competition.Rules = this.MetadataTemplate.Rules;
		bool flag = this.Competition.ScoringType == TournamentScoreType.TotalScore || this.Competition.ScoringType == TournamentScoreType.TotalWeight;
		if (!flag && this.Competition.Format == UserCompetitionFormat.Team)
		{
			this.Competition.Format = UserCompetitionFormat.Individual;
			this.UpdateFormat(this.Competition.Format);
		}
		CompetitionViewItemThreeBtns competitionViewItemThreeBtns = (CompetitionViewItemThreeBtns)this.ComponentsDataCache[UgcConsts.Components.FormatTypes];
		competitionViewItemThreeBtns.SetBlocked(UserCompetitionFormat.Team, !flag);
		this.UpdateScoring();
		this.UpdateRulesets();
		this.UpdateSetups();
		this.UpdateEquipment();
		this.UpdateLocation();
		this.UpdateFish();
		UIHelper.Waiting(false, null);
	}

	protected virtual Range GetRangeParticipants(MetadataForUserCompetition md, UserCompetitionFormat format)
	{
		if (format == UserCompetitionFormat.Individual)
		{
			return new Range(md.MinParticipants, md.MaxParticipants);
		}
		if (format != UserCompetitionFormat.Team)
		{
			return new Range(md.MinTeams, md.MaxTeams);
		}
		return new Range(md.MinTeamParticipants, md.MaxTeamParticipants);
	}

	protected virtual void IncPlayers(int value)
	{
		this.Competition.MaxParticipants = new int?(value);
	}

	protected virtual void OpenComponentWindow(UgcConsts.Components c)
	{
		switch (c)
		{
		case UgcConsts.Components.Template:
			this.SelectTemplate();
			break;
		case UgcConsts.Components.Name:
			this.SelectName();
			break;
		case UgcConsts.Components.Location:
			this.SelectLocation();
			break;
		case UgcConsts.Components.EntryFee:
			this.SelectEntryFee();
			break;
		case UgcConsts.Components.InitialPrizePool:
			this.SelectInitialPrizePool();
			break;
		case UgcConsts.Components.Level:
			this.SelectLevel();
			break;
		case UgcConsts.Components.Duration:
			this.SelectDuration();
			break;
		case UgcConsts.Components.TimeAdnWeather:
			this.SelectTimeAdnWeather();
			break;
		case UgcConsts.Components.Fish:
			this.SelectFish();
			break;
		case UgcConsts.Components.Scoring:
			this.SelectScoring();
			break;
		case UgcConsts.Components.Setups:
			this.SelectSetups();
			break;
		case UgcConsts.Components.Equipment:
			this.SelectEquipment();
			break;
		case UgcConsts.Components.Rulesets:
			this.SelectRulesets();
			break;
		case UgcConsts.Components.Privacy:
			this.SelectPrivacy();
			break;
		}
	}

	protected virtual void SelectTemplate()
	{
		List<TournamentTemplateBrief> list = this.Metadata.Templates.ToList<TournamentTemplateBrief>();
		List<TournamentTemplateBrief> list2 = new List<TournamentTemplateBrief>();
		List<Pond> list3 = CacheLibrary.MapCache.CachedPonds.OrderBy((Pond p) => p2.OriginalMinLevel).ToList<Pond>();
		Dictionary<int, Pond> pondsDict = new Dictionary<int, Pond>();
		for (int i = 0; i < list3.Count; i++)
		{
			Pond p2 = list3[i];
			List<TournamentTemplateBrief> list4 = list.Where((TournamentTemplateBrief el) => el.PondId == p2.PondId).ToList<TournamentTemplateBrief>();
			if (list4.Count > 0)
			{
				pondsDict[p2.PondId] = p2;
				list2.AddRange(list4.OrderBy((TournamentTemplateBrief el) => el.Name));
			}
		}
		List<TournamentTemplateBrief> list5 = list2;
		WindowList.WindowListDataGetter<TournamentTemplateBrief> windowListDataGetter = new WindowList.WindowListDataGetter<TournamentTemplateBrief>();
		windowListDataGetter.LocName = (TournamentTemplateBrief t) => string.Format("{0} - {1}", pondsDict[t.PondId].Name, t.Name);
		windowListDataGetter.LocDesc = (TournamentTemplateBrief t) => t.Desc;
		UserCompetitionHelper.OpenWindowList<TournamentTemplateBrief>(list5, windowListDataGetter, list.Find((TournamentTemplateBrief p) => p.TemplateId == this.Competition.TournamentTemplateId), delegate(TournamentTemplateBrief value)
		{
			if (this.Competition.TournamentTemplateId != value.TemplateId)
			{
				this.ChangeTournamentTemplate(value);
				this.UpdateTemplates();
				this.ClearWeather();
				UIHelper.Waiting(true, null);
				this.GetMetadataForPredefinedCompetitionTemplate();
			}
		}, new WindowList.Titles
		{
			Title = ScriptLocalization.Get("RodPresetsMenuTitle"),
			DataTitle = ScriptLocalization.Get("SortNameMenu"),
			DescTitle = ScriptLocalization.Get("DescriptionHelpLineText")
		}, null);
	}

	protected virtual void SelectLevel()
	{
		int num = this.Metadata.MaxLevel;
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile != null)
		{
			num = Math.Min(num, profile.Level);
		}
		int? minLevel = this.Competition.MinLevel;
		int num2 = ((minLevel == null) ? this.Metadata.MinLevel : minLevel.Value);
		int? maxLevel = this.Competition.MaxLevel;
		int num3 = ((maxLevel == null) ? num : maxLevel.Value);
		Range range = new Range(num2, num3);
		Range range2 = new Range(this.Metadata.MinLevel, num);
		Range range3 = new Range(num, PhotonConnectionFactory.Instance.LevelCap);
		GameObject gameObject = TournamentHelper.WindowLevelMinMax(range, range2, range3);
		gameObject.GetComponent<WindowLevelMinMax>().OnSelected += delegate(Range r)
		{
			this.Competition.MinLevel = new int?(r.Min);
			this.Competition.MaxLevel = new int?(r.Max);
			this.UpdateLevel();
			this.Validate();
		};
	}

	protected virtual Range GetDefaultMinMaxLevel()
	{
		int num = this.Metadata.MaxLevel;
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile != null)
		{
			num = Math.Min(num, profile.Level);
		}
		return new Range(this.Metadata.MinLevel, num);
	}

	protected virtual void SelectEntryFee()
	{
		WindowEntryFee.CurrencyData currencyData = new WindowEntryFee.CurrencyData
		{
			Currency = this.Competition.Currency,
			EntranceFee = this.Competition.EntranceFee
		};
		MetadataForUserCompetitionPond pondMetadata = this.PondMetadata;
		GameObject gameObject = TournamentHelper.WindowEntryFee(new WindowEntryFee.EntryFeeData
		{
			Current = currencyData,
			EntryFeeRange = new Range(pondMetadata.MinEntranceFee, pondMetadata.MaxEntranceFee),
			EntryFeeRangeGold = new Range(pondMetadata.MinEntranceFeeGold, pondMetadata.MaxEntranceFeeGold),
			MaxAdditional = ((!(currencyData.Currency == "SC") || this.Competition.HostEntranceFee == null) ? 0 : ((int)this.Competition.HostEntranceFee.Value)),
			MaxAdditionalGold = ((!(currencyData.Currency != "SC") || this.Competition.HostEntranceFee == null) ? 0 : ((int)this.Competition.HostEntranceFee.Value))
		});
		gameObject.GetComponent<WindowEntryFee>().OnSelectedCurrency += delegate(WindowEntryFee.CurrencyData r)
		{
			this.Competition.Currency = r.Currency;
			this.Competition.EntranceFee = r.EntranceFee;
			this.UpdateEntryFee();
			this.CorrectionInitialPrizePool(pondMetadata);
			this.UpdateInitialPrizePool();
			this.Validate();
		};
	}

	protected void CorrectionInitialPrizePool(MetadataForUserCompetitionPond pondMetadata)
	{
		if (!this.Competition.IsSponsored)
		{
			int num = (int)(pondMetadata.InitPrizePoolToEntranceFeeKoef * (float)((int)this.Competition.EntranceFee.Value));
			double? hostEntranceFee = this.Competition.HostEntranceFee;
			if (((hostEntranceFee == null) ? 0.0 : hostEntranceFee.Value) > (double)num)
			{
				this.Competition.HostEntranceFee = new double?((double)num);
			}
		}
	}

	protected virtual void SelectPrivacy()
	{
		List<bool> list = new List<bool> { true, false };
		WindowList.WindowListDataGetter<bool> windowListDataGetter = new WindowList.WindowListDataGetter<bool>();
		windowListDataGetter.LocName = new Func<bool, string>(UgcConsts.PrivacyDataLoc);
		windowListDataGetter.LocDesc = new Func<bool, string>(UgcConsts.PrivacyDataDescLoc);
		UserCompetitionHelper.OpenWindowList<bool>(list, windowListDataGetter, this.Competition.IsPrivate, delegate(bool value)
		{
			this.Competition.IsPrivate = value;
			if (value)
			{
				GameObject gameObject = TournamentHelper.WindowTextField(string.Empty, "UGC_EnterPasswordCaption", null, false, null, null, false);
				gameObject.GetComponent<ChumRename>().OnRenamed += delegate(string s)
				{
					this.UpdatePrivacy(UgcConsts.PrivacyDataLoc(this.Competition.IsPrivate), true, s);
				};
			}
			else
			{
				this.UpdatePrivacy(UgcConsts.PrivacyDataLoc(this.Competition.IsPrivate), false, null);
			}
		}, new WindowList.Titles
		{
			Title = ScriptLocalization.Get("UGC_Privacy"),
			DataTitle = ScriptLocalization.Get("SortNameMenu"),
			DescTitle = ScriptLocalization.Get("DescriptionHelpLineText")
		}, null);
	}

	protected virtual void SelectInitialPrizePool()
	{
		WindowEntryFee.CurrencyData currencyData = new WindowEntryFee.CurrencyData
		{
			Currency = this.Competition.Currency,
			EntranceFee = this.Competition.HostEntranceFee
		};
		List<UserCompetitionRewardScheme> fs = UgcConsts.GetUserCompetitionRewardScheme(this.Competition.Format).ToList<UserCompetitionRewardScheme>();
		WindowList.WindowListDataGetter<UserCompetitionRewardScheme> windowListDataGetter = new WindowList.WindowListDataGetter<UserCompetitionRewardScheme>();
		windowListDataGetter.LocName = (UserCompetitionRewardScheme v) => UgcConsts.GetInitialPrizePoolLoc(v).Name;
		windowListDataGetter.LocDesc = (UserCompetitionRewardScheme v) => UgcConsts.GetInitialPrizePoolLoc(v).Desc;
		int num;
		List<WindowList.WindowListElem> list = UserCompetitionHelper.CreateWindowList<UserCompetitionRewardScheme>(out num, windowListDataGetter, fs, this.Competition.RewardScheme);
		MetadataForUserCompetitionPond pondMetadata = this.PondMetadata;
		int num2 = ((!this.Competition.IsSponsored) ? Math.Min(pondMetadata.MaxHostEntranceFee, (int)(pondMetadata.InitPrizePoolToEntranceFeeKoef * (float)((int)this.Competition.EntranceFee.Value))) : pondMetadata.MaxHostEntranceFee);
		int num3 = ((!this.Competition.IsSponsored) ? Math.Min(pondMetadata.MaxHostEntranceFeeGold, (int)(pondMetadata.InitPrizePoolToEntranceFeeKoef * (float)((int)this.Competition.EntranceFee.Value))) : pondMetadata.MaxHostEntranceFeeGold);
		GameObject gameObject = TournamentHelper.WindowInitialPrizePool(list, "title", "dataTitle", "descTitle", new WindowEntryFee.EntryFeeData
		{
			Current = currencyData,
			EntryFeeRange = new Range(pondMetadata.MinHostEntranceFee, num2),
			EntryFeeRangeGold = new Range(pondMetadata.MinHostEntranceFeeGold, num3),
			MaxAdditional = ((!(currencyData.Currency == "SC") || this.Competition.EntranceFee == null) ? 0 : ((int)this.Competition.EntranceFee.Value)),
			MaxAdditionalGold = ((!(currencyData.Currency != "SC") || this.Competition.EntranceFee == null) ? 0 : ((int)this.Competition.EntranceFee.Value))
		}, num);
		gameObject.GetComponent<WindowInitialPrizePool>().OnSelectedPrizePool += delegate(WindowEntryFee.CurrencyData currency, int i)
		{
			this.Competition.Currency = currency.Currency;
			this.Competition.HostEntranceFee = currency.EntranceFee;
			this.Competition.RewardScheme = fs[i];
			this.UpdateEntryFee();
			this.UpdateInitialPrizePool();
			this.Validate();
		};
	}

	protected virtual void SelectDuration()
	{
		List<UserCompetitionSortType> scheduleStartTypes = new List<UserCompetitionSortType>
		{
			UserCompetitionSortType.Manual,
			UserCompetitionSortType.Automatic
		};
		WindowList.WindowListDataGetter<UserCompetitionDuration> windowListDataGetter = new WindowList.WindowListDataGetter<UserCompetitionDuration>();
		windowListDataGetter.LocName = (UserCompetitionDuration v) => UgcConsts.GetDurationLoc(v).Name;
		windowListDataGetter.LocDesc = (UserCompetitionDuration v) => UgcConsts.GetDurationLoc(v).Desc;
		int num;
		List<WindowList.WindowListElem> list = UserCompetitionHelper.CreateWindowList<UserCompetitionDuration>(out num, windowListDataGetter, this.AllUserCompetitionDurations, this.Competition.Duration);
		WindowList.WindowListDataGetter<UserCompetitionSortType> windowListDataGetter2 = new WindowList.WindowListDataGetter<UserCompetitionSortType>();
		windowListDataGetter2.LocName = (UserCompetitionSortType v) => UgcConsts.GetScheduleLoc(v).Name;
		windowListDataGetter2.LocDesc = (UserCompetitionSortType v) => UgcConsts.GetScheduleLoc(v).Desc;
		windowListDataGetter2.Interactable = (UserCompetitionSortType v) => v == UserCompetitionSortType.Automatic || !this.Competition.IsSponsored;
		int num2;
		List<WindowList.WindowListElem> list2 = UserCompetitionHelper.CreateWindowList<UserCompetitionSortType>(out num2, windowListDataGetter2, scheduleStartTypes, this.Competition.SortType);
		GameObject gameObject = TournamentHelper.WindowScheduleAndDuration(list, num, list2, num2, this.Competition.FixedStartDate, scheduleStartTypes.FindIndex((UserCompetitionSortType p) => p == UserCompetitionSortType.Manual));
		gameObject.GetComponent<WindowScheduleAndDuration>().OnSelectedData += delegate(int i, int j, DateTime? dt)
		{
			this.Competition.SortType = scheduleStartTypes[j];
			this.Competition.Duration = this.AllUserCompetitionDurations[i];
			this.Competition.FixedStartDate = ((dt == null) ? null : new DateTime?(dt.Value.ToUniversalTime()));
			this.UpdateDuration();
			this.Validate();
		};
	}

	protected virtual void SelectTimeAdnWeather()
	{
		GameObject gameObject = TournamentHelper.WindowTimeAndWeather(this.PondId, this.Competition.DayWeathers, this.Competition.SelectedDayWeather);
		WindowTimeAndWeather component = gameObject.GetComponent<WindowTimeAndWeather>();
		component.OnSelected += delegate(MetadataForUserCompetitionPondWeather.DayWeather[] weathers, MetadataForUserCompetitionPondWeather.DayWeather weather)
		{
			this.Competition.WeatherName = weather.Name;
			this.Competition.InGameStartHour = new int?(weather.Hour);
			this.Competition.DayWeathers = weathers;
			this.Competition.SelectedDayWeather = weather;
			this.UpdateTimeAndWeather();
			this.Validate();
		};
	}

	protected virtual void SelectLocation()
	{
	}

	protected virtual void SelectFish()
	{
	}

	protected virtual void SelectScoring()
	{
	}

	protected virtual void SelectSetups()
	{
	}

	protected virtual void SelectEquipment()
	{
	}

	protected virtual void SelectRulesets()
	{
	}

	protected virtual void SelectName()
	{
	}

	protected virtual void UpdateTemplates()
	{
		string name = this.Metadata.Templates.First((TournamentTemplateBrief p) => p.TemplateId == this.Competition.TournamentTemplateId).Name;
		this.ComponentsDataCache[UgcConsts.Components.Template].UpdateData(name, null);
	}

	protected virtual void UpdateName()
	{
		string defaultName = UserCompetitionHelper.GetDefaultName(this.Competition);
		this.ComponentsDataCache[UgcConsts.Components.Name].UpdateData(defaultName, null);
	}

	protected virtual void UpdateLevel()
	{
		string level = UserCompetitionHelper.GetLevel(this.Competition);
		this.ComponentsDataCache[UgcConsts.Components.Level].UpdateData(level, this.GetHint(level, UgcConsts.Components.Level));
	}

	protected virtual void UpdateTimeAndWeather()
	{
		if (this.Competition.InGameStartHour != null && this.Competition.SelectedDayWeather != null)
		{
			CompetitionViewItemWeather component = this.ComponentsDataCache[UgcConsts.Components.TimeAdnWeather].GetComponent<CompetitionViewItemWeather>();
			component.UpdateData(this.Competition.SelectedDayWeather, this.Competition.InGameStartHour.Value);
		}
		else
		{
			this.ComponentsDataCache[UgcConsts.Components.TimeAdnWeather].UpdateData(null, this.GetHint(null, UgcConsts.Components.TimeAdnWeather));
		}
	}

	protected virtual void UpdateDuration()
	{
		string text = string.Format("{0}\n{1}", UgcConsts.GetDurationLoc(this.Competition.Duration).Name, (this.Competition.SortType != UserCompetitionSortType.Automatic || this.Competition.FixedStartDate == null) ? ScriptLocalization.Get("UGC_ManualStart") : MeasuringSystemManager.DateTimeString(this.Competition.FixedStartDate.Value.ToLocalTime()));
		this.ComponentsDataCache[UgcConsts.Components.Duration].UpdateData(text, this.GetHint(text, UgcConsts.Components.Duration));
	}

	protected virtual void UpdateInitialPrizePool()
	{
		double? hostEntranceFee = this.Competition.HostEntranceFee;
		double num = ((hostEntranceFee == null) ? 0.0 : hostEntranceFee.Value);
		string text = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon(this.Competition.Currency), num);
		string text2 = ((num <= 0.0) ? text : string.Format("{0}\n{1}", text, UgcConsts.GetInitialPrizePoolLoc(this.Competition.RewardScheme).Name));
		this.ComponentsDataCache[UgcConsts.Components.InitialPrizePool].UpdateData(text2, this.GetHint(text2, UgcConsts.Components.InitialPrizePool));
	}

	protected virtual void UpdateFish()
	{
		if (this.MetadataTemplate.FishScore != null)
		{
			string[] array = (from p in UserCompetitionHelper.GetFishBriefs(this.MetadataTemplate.FishScore)
				select p.Name).ToArray<string>();
			this.UpdateMultiString<string>(array, null, UgcConsts.Components.Fish, this.GetHint<string>(array, UgcConsts.Components.Fish));
		}
		else
		{
			this.ComponentsDataCache[UgcConsts.Components.Fish].UpdateData(ScriptLocalization.Get("NoRestriction"), null);
		}
		this.Validate();
	}

	protected virtual void UpdateRulesets()
	{
		string scoringTypeLoc = UserCompetitionHelper.GetScoringTypeLoc(this.Competition);
		this.ComponentsDataCache[UgcConsts.Components.Rulesets].UpdateData(scoringTypeLoc, this.GetHint(scoringTypeLoc, UgcConsts.Components.Rulesets));
	}

	protected virtual void UpdateScoring()
	{
		string name = UgcConsts.GetCompetitionScoringLoc((this.CompetitionType() != UserCompetitionType.Predefined) ? this.Competition.FishSource : this.MetadataTemplate.FishSource).Name;
		this.ComponentsDataCache[UgcConsts.Components.Scoring].UpdateData(name, this.GetHint(name, UgcConsts.Components.Scoring));
	}

	protected virtual void UpdateEquipment()
	{
		this.ComponentsDataCache[UgcConsts.Components.Equipment].UpdateData(ScriptLocalization.Get("UGC_WatchDetailsHint"), null);
	}

	protected virtual void UpdateSetups()
	{
		this.ComponentsDataCache[UgcConsts.Components.Setups].UpdateData(ScriptLocalization.Get("UGC_WatchDetailsHint"), null);
	}

	protected virtual void UpdateLocation()
	{
		if (this.Pond != null)
		{
			this.ComponentsDataCache[UgcConsts.Components.Location].UpdateData(this.Pond.Name, null);
		}
	}

	protected virtual void UpdateEntryFee()
	{
		string text = string.Format("{0} {1}", MeasuringSystemManager.GetCurrencyIcon(this.Competition.Currency), this.Competition.EntranceFee);
		this.ComponentsDataCache[UgcConsts.Components.EntryFee].UpdateData(text, this.GetHint(text, UgcConsts.Components.EntryFee));
	}

	protected virtual void UpdatePrivacy(string v, bool isPrivate, string password)
	{
		this.Competition.IsPrivate = isPrivate;
		this.Competition.Password = password;
		this.ComponentsDataCache[UgcConsts.Components.Privacy].UpdateData(v, null);
	}

	protected virtual void UpdateFormat(UserCompetitionFormat f)
	{
		this.ComponentsDataCache[UgcConsts.Components.FormatTypes].GetComponent<CompetitionViewItemThreeBtns>().SetActive(f);
		this.UpdatePlayersCount(f);
		List<UserCompetitionRewardScheme> list = UgcConsts.GetUserCompetitionRewardScheme(f).ToList<UserCompetitionRewardScheme>();
		if (!list.Contains(this.Competition.RewardScheme))
		{
			this.Competition.RewardScheme = list[0];
			this.UpdateInitialPrizePool();
		}
	}

	protected virtual void UpdatePlayersCount(UserCompetitionFormat f)
	{
		Range rangeParticipants = this.GetRangeParticipants(this.Metadata, f);
		CompetitionViewItem competitionViewItem = this.ComponentsDataCache[UgcConsts.Components.MaxPlayers];
		this.Competition.MaxParticipants = new int?((f != UserCompetitionFormat.Duel) ? this.Metadata.MaxParticipantsDefault : rangeParticipants.Max);
		competitionViewItem.GetComponent<CompetitionViewItemUpDown>().SetRange(new Range(rangeParticipants.Min, rangeParticipants.Max), this.Competition.MaxParticipants);
		competitionViewItem.UpdateData(string.Format("{0}", this.Competition.MaxParticipants), string.Format(ScriptLocalization.Get("UGC_MaxFee"), rangeParticipants.Max));
		competitionViewItem.SetBlocked(f == UserCompetitionFormat.Duel);
	}

	protected void UpdateSponsoredInteractable()
	{
		this.SponsoredTgl.interactable = this.SponsoredEnabled() && !this.IsBlockedSponsored && (this.Competition == null || this.Competition.Format != UserCompetitionFormat.Duel);
		if (this.SponsoredTgl.interactable)
		{
			this.SponsoredTglColorTransitionChanges.Enable();
		}
		else
		{
			this.SponsoredTglColorTransitionChanges.Disable();
		}
	}

	protected void UpdateSponsored()
	{
		if (this.SponsoredTgl.isOn != this.Competition.IsSponsored)
		{
			this.SetOnSponsoredTgl(this.Competition.IsSponsored);
		}
		LogHelper.Log("___kocha UpdateSponsored SponsoredTgl.isOn:{0} Competition.IsSponsored:{1}", new object[]
		{
			this.SponsoredTgl.isOn,
			this.Competition.IsSponsored
		});
		this.IsSponsored.SetActive(this.Competition.IsSponsored);
		if (!this.Competition.IsSponsored || string.IsNullOrEmpty(this.Competition.NameCustom))
		{
			this.UpdateName();
		}
		string text = ((!this.Competition.IsSponsored) ? "UGC_CreateCompetition" : ((this.Competition.ApproveStatus == UserCompetitionApproveStatus.Approved) ? "UGC_PublishCompetition" : "UGC_SendToReviewCompetition"));
		this.CreateBtnLbl.text = ScriptLocalization.Get(text);
		MetadataForUserCompetitionPond pondMetadata = this.PondMetadata;
		if (pondMetadata != null)
		{
			this.CorrectionInitialPrizePool(pondMetadata);
			this.UpdateInitialPrizePool();
		}
		if (this.Competition.IsSponsored && this.Competition.SortType == UserCompetitionSortType.Manual)
		{
			this.Competition.SortType = UserCompetitionSortType.Automatic;
			this.Competition.FixedStartDate = new DateTime?(WindowScheduleAndDuration.InitDt(null).ToUniversalTime());
			this.UpdateDuration();
		}
		((CompetitionViewItemThreeBtns)this.ComponentsDataCache[UgcConsts.Components.FormatTypes]).SetSponsored(this.Competition.IsSponsored);
	}

	protected virtual void SetFormatTypes(UserCompetitionFormat v)
	{
		if (this.Competition == null)
		{
			return;
		}
		this.Competition.Format = v;
		this.UpdateFormat(this.Competition.Format);
		this.UpdateName();
	}

	protected string GetHint<T>(T[] value, UgcConsts.Components c)
	{
		return UgcConsts.GetComponentLoc(c).Desc;
	}

	protected string GetHint(string value, UgcConsts.Components c)
	{
		return (!string.IsNullOrEmpty(value)) ? null : UgcConsts.GetComponentLoc(c).Desc;
	}

	protected Pond Pond
	{
		get
		{
			int pondId = this.PondId;
			if (this.CurrentPond == null || this.CurrentPond.PondId != pondId)
			{
				this.CurrentPond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.PondId == pondId);
			}
			return this.CurrentPond;
		}
	}

	protected MetadataForUserCompetitionPond PondMetadata
	{
		get
		{
			return this.Metadata.Ponds.ToList<MetadataForUserCompetitionPond>().FirstOrDefault((MetadataForUserCompetitionPond p) => p.PondId == this.PondId);
		}
	}

	protected int PondId
	{
		get
		{
			return this.Competition.PondId;
		}
	}

	protected virtual void UpdateMultiString<T>(T[] data, Func<T, string> funcLoc, UgcConsts.Components c, string hint = null)
	{
		string text = UserCompetitionHelper.JoinArrayAndLocalize<T>(data, funcLoc, false);
		this.ComponentsDataCache[c].UpdateData(text, hint);
	}

	protected void ClearWeather()
	{
		if (this.Competition != null)
		{
			this.Competition.SelectedDayWeather = null;
			this.Competition.DayWeathers = null;
			this.Competition.WeatherName = string.Empty;
			this.Competition.InGameStartHour = null;
		}
	}

	protected void RefreshWeather(int pondId)
	{
		PhotonConnectionFactory.Instance.OnGetMetadataForCompetitionPondWeather += this.GetMetadataForCompetitionPondWeather;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForCompetitionPondWeather += new OnFailureUserCompetition(this.FailureGetMetadataForCompetitionPondWeather);
		int? num = null;
		if (this.CompetitionType() == UserCompetitionType.Predefined)
		{
			num = this.Competition.TournamentTemplateId;
		}
		PhotonConnectionFactory.Instance.GetMetadataForCompetitionPondWeather(pondId, num);
	}

	protected void GetMetadataForCompetitionPondWeather(MetadataForUserCompetitionPondWeather metadataPondWeather)
	{
		this.UnsubscribeGetMetadataForCompetitionPondWeather();
		if (this.Competition != null && !this.IsBlockedSponsored)
		{
			int diff0 = int.MaxValue;
			metadataPondWeather.DayWeathers.ToList<MetadataForUserCompetitionPondWeather.DayWeather>().ForEach(delegate(MetadataForUserCompetitionPondWeather.DayWeather p)
			{
				int num = Math.Abs(p.Hour - 5);
				if (num < diff0)
				{
					diff0 = num;
					this.Competition.SelectedDayWeather = p;
				}
			});
			this.Competition.WeatherName = this.Competition.SelectedDayWeather.Name;
			this.Competition.InGameStartHour = new int?(this.Competition.SelectedDayWeather.Hour);
			this.Competition.DayWeathers = metadataPondWeather.DayWeathers;
			this.UpdateTimeAndWeather();
			this.Validate();
		}
	}

	protected void ResetFee()
	{
		MetadataForUserCompetitionPond pondMetadata = this.PondMetadata;
		if (pondMetadata == null)
		{
			return;
		}
		this.Competition.EntranceFee = new double?((double)((!(this.Competition.Currency == "SC")) ? pondMetadata.MinEntranceFeeGold : pondMetadata.MinEntranceFee));
		this.Competition.HostEntranceFee = new double?((double)((!(this.Competition.Currency == "SC")) ? pondMetadata.MinHostEntranceFeeGold : pondMetadata.MinHostEntranceFee));
		this.UpdateEntryFee();
		this.UpdateInitialPrizePool();
	}

	protected void ChangeTournamentTemplate(TournamentTemplateBrief template)
	{
		this.Competition.TournamentTemplateId = new int?(template.TemplateId);
		this.Competition.PondId = template.PondId;
		this.Competition.TemplateName = template.Name;
		this.UpdateName();
		this.ResetFee();
	}

	protected void Instance_OnFailureGetUserCompetitions(Failure failure)
	{
		this.UnsubscribeGetUserCompetitions();
		Debug.LogErrorFormat("FailureGetUserCompetitions FullErrorInfo:{0}", new object[] { failure.FullErrorInfo });
	}

	private void FailureGetMetadataForCompetitionPondWeather(Failure failure)
	{
		this.UnsubscribeGetMetadataForCompetitionPondWeather();
		LogHelper.Error("FailureGetMetadataForCompetitionPondWeather ", new object[] { failure.FullErrorInfo });
	}

	protected virtual void Instance_OnFailureSaveCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeSaveCompetition();
		UserCompetitionFailureHandler.Fire(failure, null, true);
	}

	protected virtual void Instance_OnFailurePublishCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribePublishCompetition();
		UIHelper.Waiting(false, null);
		UserCompetitionErrorCode userCompetitionErrorCode = failure.UserCompetitionErrorCode;
		switch (userCompetitionErrorCode)
		{
		case 61:
		case 64:
			UserCompetitionFailureHandler.ShowMsgSportEventWait(failure);
			return;
		case 62:
			break;
		default:
			if (userCompetitionErrorCode != 1 && userCompetitionErrorCode != 42)
			{
				UserCompetitionFailureHandler.Fire(failure, null, true);
				return;
			}
			break;
		}
		if (failure.UserCompetition.IsStarted)
		{
			UserCompetitionFailureHandler.ShowMsgSportEventWait(failure);
		}
		else
		{
			UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get("UGC_AlreadySignedToSportEventPublish"), UserCompetitionFailureHandler.GetSportEventName(failure)), delegate
			{
				PhotonConnectionFactory.Instance.OnPublishCompetition += this.Instance_OnPublishCompetition;
				PhotonConnectionFactory.Instance.OnFailurePublishCompetition += this.Instance_OnFailurePublishCompetition;
				PhotonConnectionFactory.Instance.PublishCompetition(this.Competition.TournamentId, true, true);
			}, null, "UGC_PublishAnywayCaption", null, "NoCaption", null, null, null);
		}
	}

	protected virtual void Instance_OnFailureRegisterInCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeRegisterInCompetitio();
		UserCompetitionFailureHandler.Fire(failure, null, true);
	}

	protected virtual void Instance_OnFailureGetMetadataForPredefinedCompetitionTemplate(UserCompetitionFailure failure)
	{
		this.UnsubscribeGetMetadataForPredefinedCompetitionTemplate();
		UserCompetitionFailureHandler.Fire(failure, null, true);
	}

	protected void Instance_OnFailureSendToReviewCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeSendToReviewCompetition();
		UserCompetitionFailureHandler.Fire(failure, null, true);
	}

	protected void Instance_OnFailureLoadCompetition(UserCompetitionFailure failure)
	{
		UserCompetitionFailureHandler.Fire(failure, null, true);
		this.UnsubscribeLoadCompetition();
	}

	protected void UnsubscribeSendToReviewCompetition()
	{
		PhotonConnectionFactory.Instance.OnFailureSendToReviewCompetition -= this.Instance_OnFailureSendToReviewCompetition;
		PhotonConnectionFactory.Instance.OnSendToReviewCompetition -= this.Instance_OnSendToReviewCompetition;
	}

	protected void UnsubscribeSaveCompetition()
	{
		PhotonConnectionFactory.Instance.OnSaveCompetition -= this.Instance_OnSaveCompetition;
		PhotonConnectionFactory.Instance.OnFailureSaveCompetition -= this.Instance_OnFailureSaveCompetition;
	}

	protected void UnsubscribeGetMetadataForCompetitionPondWeather()
	{
		PhotonConnectionFactory.Instance.OnGetMetadataForCompetitionPondWeather -= this.GetMetadataForCompetitionPondWeather;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForCompetitionPondWeather -= new OnFailureUserCompetition(this.FailureGetMetadataForCompetitionPondWeather);
	}

	protected void UnsubscribePublishCompetition()
	{
		PhotonConnectionFactory.Instance.OnPublishCompetition -= this.Instance_OnPublishCompetition;
		PhotonConnectionFactory.Instance.OnFailurePublishCompetition -= this.Instance_OnFailurePublishCompetition;
	}

	protected void UnsubscribeRegisterInCompetitio()
	{
		PhotonConnectionFactory.Instance.OnRegisterInCompetition -= this.Instance_OnRegisterInCompetition;
		PhotonConnectionFactory.Instance.OnFailureRegisterInCompetition -= this.Instance_OnFailureRegisterInCompetition;
	}

	protected void UnsubscribeGetMetadataForPredefinedCompetitionTemplate()
	{
		PhotonConnectionFactory.Instance.OnGetMetadataForPredefinedCompetitionTemplate -= this.Instance_OnGetMetadataForPredefinedCompetitionTemplate;
		PhotonConnectionFactory.Instance.OnFailureGetMetadataForPredefinedCompetitionTemplate -= this.Instance_OnFailureGetMetadataForPredefinedCompetitionTemplate;
	}

	protected void UnsubscribeGetUserCompetitions()
	{
		PhotonConnectionFactory.Instance.OnGetUserCompetitions -= this.Instance_OnGetUserCompetitions;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetitions -= new OnFailureUserCompetition(this.Instance_OnFailureGetUserCompetitions);
	}

	protected void UnsubscribeLoadCompetition()
	{
		PhotonConnectionFactory.Instance.OnLoadCompetition -= this.Instance_OnLoadCompetition;
		PhotonConnectionFactory.Instance.OnFailureLoadCompetition -= this.Instance_OnFailureLoadCompetition;
	}

	[SerializeField]
	protected BorderedButton CreateBtn;

	[SerializeField]
	protected Toggle SponsoredTgl;

	[SerializeField]
	protected GameObject IsSponsored;

	[SerializeField]
	protected Text CreateBtnLbl;

	[SerializeField]
	protected GameObject SimpleItemPrefab;

	[SerializeField]
	protected GameObject ThreeButtonsItemPrefab;

	[SerializeField]
	protected GameObject UpDownItemPrefab;

	[SerializeField]
	protected GameObject SimpleItemWeatherPrefab;

	[SerializeField]
	protected ToggleGroup ToggleGroup;

	[SerializeField]
	protected CreateTournamentManager.Component[] ComponentsData;

	protected readonly Dictionary<UgcConsts.Components, CompetitionViewItem> ComponentsDataCache = new Dictionary<UgcConsts.Components, CompetitionViewItem>();

	protected const int InGameStartHour = 5;

	protected readonly Guid RequestIdGetUserCompetitions = Guid.NewGuid();

	protected List<UserCompetitionPublic> OwnerUgcList;

	protected UserCompetition Competition;

	protected MetadataForUserCompetition Metadata;

	protected UserCompetition MetadataTemplate;

	protected Pond CurrentPond;

	protected readonly List<UserCompetitionRodEquipmentAllowed> RodEquipmentAllowed = UserCompetitionHelper.EnumToList<UserCompetitionRodEquipmentAllowed>(new List<UserCompetitionRodEquipmentAllowed>
	{
		UserCompetitionRodEquipmentAllowed.None,
		UserCompetitionRodEquipmentAllowed.Spinnerbaits,
		UserCompetitionRodEquipmentAllowed.SpinnersSpinnerbaitsAndTails
	});

	protected readonly UserCompetitionEquipmentAllowed[] EquipmentAllowed = new UserCompetitionEquipmentAllowed[]
	{
		UserCompetitionEquipmentAllowed.Keepnet,
		UserCompetitionEquipmentAllowed.Stringer,
		UserCompetitionEquipmentAllowed.RodStand,
		UserCompetitionEquipmentAllowed.Kayak,
		UserCompetitionEquipmentAllowed.MotorBoats_All
	};

	protected List<UserCompetitionDuration> AllUserCompetitionDurations;

	protected ToggleColorTransitionChanges SponsoredTglColorTransitionChanges;

	[Serializable]
	public class Component
	{
		[SerializeField]
		public UgcConsts.Components[] Types;

		[SerializeField]
		public GameObject ItemsRoot;
	}
}
