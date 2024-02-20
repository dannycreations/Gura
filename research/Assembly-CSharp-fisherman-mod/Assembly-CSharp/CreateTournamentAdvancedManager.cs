using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using UnityEngine;

public class CreateTournamentAdvancedManager : CreateTournamentManager
{
	protected override List<UgcConsts.Components> BlockedComponents()
	{
		return new List<UgcConsts.Components> { UgcConsts.Components.Template };
	}

	protected override UserCompetitionType CompetitionType()
	{
		return UserCompetitionType.Custom;
	}

	protected override bool SponsoredEnabled()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		return profile != null && profile.CanCreateUserCompetitionSponsored();
	}

	private List<Pond> PondsList
	{
		get
		{
			return (from p in CacheLibrary.MapCache.CachedPonds
				where !p.PondLocked() && p.PondId != 2
				orderby p.OriginalMinLevel
				select p).ToList<Pond>();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	public override void LoadCompetition(UserCompetitionPublic c)
	{
		UIHelper.Waiting(true, null);
		PhotonConnectionFactory.Instance.OnLoadCompetition += base.Instance_OnLoadCompetition;
		PhotonConnectionFactory.Instance.OnFailureLoadCompetition += base.Instance_OnFailureLoadCompetition;
		PhotonConnectionFactory.Instance.LoadCompetition(c.TournamentId);
	}

	protected override void Instance_OnUserCompetitionRemovedOnReview(UserCompetitionReviewMessage competition)
	{
		if (this.Competition != null && this.Competition.TournamentId == competition.TournamentId)
		{
			this.Competition = null;
			base.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Create, false, false);
			base.MenuMgr.SetActiveFormSport(UgcMenuStateManager.UgcStates.Sport, true, false);
		}
	}

	protected override void Instance_OnUserCompetitionDeclinedOnReview(UserCompetitionReviewMessage competition)
	{
		if (this.Competition != null && this.Competition.TournamentId == competition.TournamentId)
		{
			this.Competition.ApproveStatus = UserCompetitionApproveStatus.Declined;
			base.RefreshCompetition(this.Competition);
		}
	}

	protected override void Instance_OnUserCompetitionApprovedOnReview(UserCompetitionReviewMessage competition)
	{
		if (this.Competition != null && this.Competition.TournamentId == competition.TournamentId)
		{
			this.Competition.ApproveStatus = UserCompetitionApproveStatus.Approved;
			base.UpdateSponsored();
			this.Validate();
		}
	}

	protected override void Instance_OnSendToReviewCompetition(UserCompetition competition)
	{
		base.Instance_OnSendToReviewCompetition(competition);
		this.Competition = competition;
		this.UpdateBlocked();
		this.Validate();
		UIHelper.ShowCanceledMsg(ScriptLocalization.Get("UGC_SuccessfulAction"), string.Format("{0}\n\n{1}", ScriptLocalization.Get("UGC_SendToReviewCompetitionOk"), ScriptLocalization.Get("UGC_SendToReviewCompetitionInfo")), TournamentCanceledInit.MessageTypes.Ok, null, false);
	}

	protected override void UpdateCompetition()
	{
		base.UpdateCompetition();
		List<Pond> pondsList = this.PondsList;
		this.ChangePond((StaticUserData.CurrentPond == null) ? pondsList[0] : StaticUserData.CurrentPond);
		this.ComponentsDataCache[UgcConsts.Components.Name].SetBlocked(!this.Competition.IsSponsored);
		this.ComponentsDataCache[UgcConsts.Components.Template].UpdateData("Custom", null);
		this.Competition.FishScore = UserCompetitionHelper.GetFishScore(pondsList.First((Pond p) => p.PondId == this.Competition.PondId).FishIds);
		this.UpdateFish();
		this.Competition.DollEquipment = new List<UserCompetitionEquipmentAllowed>(this.EquipmentAllowed).ToArray();
		this.UpdateEquipment();
		this.Competition.RodEquipment = this.RodEquipmentAllowed.ToArray();
		this.UpdateSetups();
		this.UpdateLocation();
		this.UpdateName();
		this.UpdateSetups();
		this.UpdateRulesets();
		this.UpdateScoring();
		base.RefreshWeather(base.PondId);
		base.UpdateSponsored();
	}

	protected override void SelectLocation()
	{
		List<Pond> data = this.PondsList;
		List<Pond> data2 = data;
		WindowList.WindowListDataGetter<Pond> windowListDataGetter = new WindowList.WindowListDataGetter<Pond>();
		windowListDataGetter.LocName = (Pond pond) => pond.Name;
		windowListDataGetter.LocDesc = (Pond pond) => pond.Desc;
		windowListDataGetter.GetImgPath = (Pond pond) => string.Format("Textures/Inventory/{0}", pond.PhotoBID);
		UserCompetitionHelper.OpenWindowList<Pond>(data2, windowListDataGetter, data.Find((Pond p) => p.PondId == this.PondId), delegate(Pond value)
		{
			this.ChangePond(value);
			this.UpdateName();
			this.UpdateLocation();
			this.Competition.FishScore = UserCompetitionHelper.GetFishScore(data.First((Pond p) => p.PondId == this.Competition.PondId).FishIds);
			this.UpdateFish();
			this.ClearWeather();
			this.RefreshWeather(this.PondId);
		}, new WindowList.Titles
		{
			Title = ScriptLocalization.Get("LocationCaption"),
			DataTitle = ScriptLocalization.Get("SortNameMenu"),
			DescTitle = ScriptLocalization.Get("DescriptionHelpLineText")
		}, null);
	}

	protected override void UpdateFish()
	{
		string[] array = (from p in UserCompetitionHelper.GetFishBriefs(this.Competition.FishScore)
			select p.Name).ToArray<string>();
		if (array.Length == 0)
		{
			this.ComponentsDataCache[UgcConsts.Components.Fish].UpdateData(ScriptLocalization.Get("NoRestriction"), null);
		}
		else
		{
			this.UpdateMultiString<string>(array, null, UgcConsts.Components.Fish, base.GetHint<string>(array, UgcConsts.Components.Fish));
		}
		this.Validate();
	}

	protected override void SelectFish()
	{
		List<FishBrief> fishesLight = CacheLibrary.MapCache.FishesLight;
		int[] fishIds = this.PondsList.First((Pond p) => p.PondId == this.PondId).FishIds;
		List<FishBrief> list = fishesLight.Where((FishBrief p) => fishIds.Contains(p.FishId)).ToList<FishBrief>();
		FishBrief[] fishBriefs = UserCompetitionHelper.GetFishBriefs(this.Competition.FishScore);
		List<FishBrief> list2 = list;
		WindowList.WindowListDataGetter<FishBrief> windowListDataGetter = new WindowList.WindowListDataGetter<FishBrief>();
		windowListDataGetter.LocName = (FishBrief fish) => fish.Name;
		windowListDataGetter.LocDesc = (FishBrief fish) => fish.Desc.Replace("<br>", "\n");
		windowListDataGetter.GetImgPath = (FishBrief fish) => string.Format("Textures/Inventory/{0}", fish.ThumbnailBID);
		UserCompetitionHelper.OpenWindowListMultiselect<FishBrief>(list2, windowListDataGetter, fishBriefs, delegate(List<FishBrief> ids)
		{
			this.Competition.FishScore = ids.Select((FishBrief p) => p.CodeName).ToArray<string>();
			this.UpdateFish();
		}, new WindowList.Titles
		{
			Title = ScriptLocalization.Get("FishCaption"),
			DataTitle = ScriptLocalization.Get("SortNameMenu"),
			DescTitle = ScriptLocalization.Get("DescriptionHelpLineText")
		}, ScriptLocalization.Get("UGC_AllFishSelectedDesc"));
	}

	protected override void SelectScoring()
	{
		TournamentFishSource tournamentFishSource = TournamentFishSource.Cage;
		WindowList.WindowListDataGetter<TournamentFishSource> windowListDataGetter = new WindowList.WindowListDataGetter<TournamentFishSource>();
		windowListDataGetter.LocName = (TournamentFishSource v) => UgcConsts.GetCompetitionScoringLoc(v).Name;
		windowListDataGetter.LocDesc = (TournamentFishSource v) => UgcConsts.GetCompetitionScoringLoc(v).Desc;
		windowListDataGetter.Interactable = (TournamentFishSource v) => v != TournamentFishSource.Cage || this.IsKeepEnabled();
		UserCompetitionHelper.OpenWindowList<TournamentFishSource>(tournamentFishSource, windowListDataGetter, this.Competition.FishSource, delegate(TournamentFishSource value)
		{
			this.Competition.FishSource = value;
			this.UpdateScoring();
			this.Validate();
		}, new WindowList.Titles
		{
			Title = ScriptLocalization.Get("UGC_Scoring"),
			DataTitle = ScriptLocalization.Get("SortNameMenu"),
			DescTitle = ScriptLocalization.Get("DescriptionHelpLineText")
		}, false);
	}

	protected override void SelectSetups()
	{
		List<UserCompetitionRodEquipmentAllowed> list = UserCompetitionHelper.EnumToList<UserCompetitionRodEquipmentAllowed>(new List<UserCompetitionRodEquipmentAllowed>
		{
			UserCompetitionRodEquipmentAllowed.None,
			UserCompetitionRodEquipmentAllowed.Spinnerbaits,
			UserCompetitionRodEquipmentAllowed.SpinnersSpinnerbaitsAndTails
		});
		WindowList.WindowListDataGetter<UserCompetitionRodEquipmentAllowed> windowListDataGetter = new WindowList.WindowListDataGetter<UserCompetitionRodEquipmentAllowed>();
		windowListDataGetter.LocName = (UserCompetitionRodEquipmentAllowed v) => UgcConsts.GetUserCompetitionRodEquipmentAllowedLoc(v).Name;
		windowListDataGetter.LocDesc = (UserCompetitionRodEquipmentAllowed v) => UgcConsts.GetUserCompetitionRodEquipmentAllowedLoc(v).Desc;
		UserCompetitionHelper.OpenWindowListMultiselect<UserCompetitionRodEquipmentAllowed>(list, windowListDataGetter, this.Competition.RodEquipment, delegate(List<UserCompetitionRodEquipmentAllowed> value)
		{
			this.Competition.RodEquipment = value.ToArray();
			this.UpdateSetups();
			this.Validate();
		}, new WindowList.Titles
		{
			Title = ScriptLocalization.Get("UGC_Setups"),
			DataTitle = ScriptLocalization.Get("SortNameMenu"),
			DescTitle = ScriptLocalization.Get("DescriptionHelpLineText")
		}, null);
	}

	protected override void SelectEquipment()
	{
		List<UserCompetitionEquipmentAllowed> list = this.EquipmentAllowed.ToList<UserCompetitionEquipmentAllowed>();
		WindowList.WindowListDataGetter<UserCompetitionEquipmentAllowed> windowListDataGetter = new WindowList.WindowListDataGetter<UserCompetitionEquipmentAllowed>();
		windowListDataGetter.LocName = (UserCompetitionEquipmentAllowed v) => UgcConsts.GetEquipmentAllowedLoc(v).Name;
		windowListDataGetter.LocDesc = (UserCompetitionEquipmentAllowed v) => UgcConsts.GetEquipmentAllowedLoc(v).Desc;
		windowListDataGetter.RadioId = (UserCompetitionEquipmentAllowed v) => (v != UserCompetitionEquipmentAllowed.Keepnet && v != UserCompetitionEquipmentAllowed.Stringer) ? 0 : 1;
		UserCompetitionHelper.OpenWindowListMultiselect<UserCompetitionEquipmentAllowed>(list, windowListDataGetter, this.Competition.DollEquipment, delegate(List<UserCompetitionEquipmentAllowed> ids)
		{
			this.Competition.DollEquipment = ids.ToArray();
			this.UpdateEquipment();
			if (!this.IsKeepEnabled())
			{
				this.Competition.FishSource = TournamentFishSource.Catch;
				this.UpdateScoring();
			}
			this.Validate();
		}, new WindowList.Titles
		{
			Title = ScriptLocalization.Get("UGC_Equipment"),
			DataTitle = ScriptLocalization.Get("SortNameMenu"),
			DescTitle = ScriptLocalization.Get("DescriptionHelpLineText")
		}, null);
	}

	protected override void SelectRulesets()
	{
		List<TournamentScoreType> list = ((this.Competition.Format != UserCompetitionFormat.Team) ? this._scoreTypes : this._teamScoreTypes);
		WindowList.WindowListDataGetter<TournamentScoreType> windowListDataGetter = new WindowList.WindowListDataGetter<TournamentScoreType>();
		windowListDataGetter.LocName = (TournamentScoreType v) => UgcConsts.GetScoreTypeLoc(v).Name;
		windowListDataGetter.LocDesc = (TournamentScoreType v) => UgcConsts.GetScoreTypeLoc(v).Desc;
		UserCompetitionHelper.OpenWindowList<TournamentScoreType>(list, windowListDataGetter, this.Competition.ScoringType, delegate(TournamentScoreType value)
		{
			if (value == TournamentScoreType.BestWeightMatch)
			{
				string text = string.Format("{0} ({1})", ScriptLocalization.Get("UGC_BestWeightMatch"), MeasuringSystemManager.FishWeightSufix());
				GameObject gameObject = TournamentHelper.WindowTextField(string.Empty, null, text, false, null, null, true);
				gameObject.GetComponent<ChumRename>().OnRenamed += delegate(string s)
				{
					if (!string.IsNullOrEmpty(s))
					{
						try
						{
							float num = float.Parse(s);
							if (MeasuringSystemManager.CurrentMeasuringSystem == MeasuringSystem.Imperial)
							{
								num *= 0.45359236f;
							}
							this.Competition.ReferenceWeight = new float?(num);
							this.FillRulesets(value);
						}
						catch (Exception ex)
						{
						}
					}
				};
			}
			else if (value == TournamentScoreType.TotalScore)
			{
				TournamentTotalScoreKind tournamentTotalScoreKind = TournamentTotalScoreKind.ScorePerFish;
				WindowList.WindowListDataGetter<TournamentTotalScoreKind> windowListDataGetter2 = new WindowList.WindowListDataGetter<TournamentTotalScoreKind>();
				windowListDataGetter2.LocName = new Func<TournamentTotalScoreKind, string>(UserCompetitionHelper.GetLocNameTotalScoreKind);
				windowListDataGetter2.LocDesc = (TournamentTotalScoreKind v) => UgcConsts.GetTotalScoreKindLoc(v).Desc;
				UserCompetitionHelper.OpenWindowList<TournamentTotalScoreKind>(tournamentTotalScoreKind, windowListDataGetter2, this.Competition.TotalScoreKind, delegate(TournamentTotalScoreKind totalScoreKind)
				{
					this.Competition.TotalScoreKind = totalScoreKind;
					this.FillRulesets(value);
				}, new WindowList.Titles
				{
					Title = ScriptLocalization.Get("UGC_TotalScore"),
					DataTitle = ScriptLocalization.Get("SortNameMenu"),
					DescTitle = ScriptLocalization.Get("DescriptionHelpLineText")
				}, false);
			}
			else
			{
				this.FillRulesets(value);
			}
		}, new WindowList.Titles
		{
			Title = ScriptLocalization.Get("UGC_Rulesets"),
			DataTitle = ScriptLocalization.Get("SortNameMenu"),
			DescTitle = ScriptLocalization.Get("DescriptionHelpLineText")
		}, null);
	}

	protected override void SelectName()
	{
		GameObject gameObject = TournamentHelper.WindowTextField(string.Empty, "SortNameMenu", null, false, null, null, false);
		gameObject.GetComponent<ChumRename>().OnRenamed += delegate(string s)
		{
			this.Competition.NameCustom = s;
			this.UpdateName();
		};
	}

	protected override void UpdateName()
	{
		string text = ((string.IsNullOrEmpty(this.Competition.NameCustom) || !this.Competition.IsSponsored) ? UserCompetitionHelper.GetDefaultName(this.Competition) : this.Competition.NameCustom);
		this.ComponentsDataCache[UgcConsts.Components.Name].UpdateData(text, null);
	}

	protected override void UpdateEquipment()
	{
		if (this.Competition.DollEquipment.Length == this.EquipmentAllowed.Length)
		{
			this.ComponentsDataCache[UgcConsts.Components.Equipment].UpdateData(ScriptLocalization.Get("AllCaption"), null);
		}
		else
		{
			this.UpdateMultiString<UserCompetitionEquipmentAllowed>(this.Competition.DollEquipment, (UserCompetitionEquipmentAllowed v) => UgcConsts.GetEquipmentAllowedLoc(v).Name, UgcConsts.Components.Equipment, base.GetHint<UserCompetitionEquipmentAllowed>(this.Competition.DollEquipment, UgcConsts.Components.Equipment));
		}
	}

	protected override void UpdateSetups()
	{
		if (this.Competition.RodEquipment.Length == this.RodEquipmentAllowed.Count)
		{
			this.ComponentsDataCache[UgcConsts.Components.Setups].UpdateData(ScriptLocalization.Get("AllCaption"), null);
		}
		else
		{
			this.UpdateMultiString<UserCompetitionRodEquipmentAllowed>(this.Competition.RodEquipment, (UserCompetitionRodEquipmentAllowed v) => UgcConsts.GetUserCompetitionRodEquipmentAllowedLoc(v).Name, UgcConsts.Components.Setups, null);
		}
	}

	protected override void SetFormatTypes(UserCompetitionFormat v)
	{
		base.SetFormatTypes(v);
		if (this.Competition.Format == UserCompetitionFormat.Team && !this._teamScoreTypes.Contains(this.Competition.ScoringType))
		{
			this.FillRulesets(TournamentScoreType.TotalWeight);
		}
	}

	public override void SelectSponsored()
	{
		if (this.Competition == null)
		{
			return;
		}
		if (this.Competition.Format == UserCompetitionFormat.Duel)
		{
			return;
		}
		if (SettingsManager.InputType == InputModuleManager.InputType.Mouse && !base.IsBlockedSponsored)
		{
			this.Competition.IsSponsored = this.SponsoredTgl.isOn;
			base.UpdateSponsored();
			this.UpdateBlocked();
		}
	}

	public override void SetSponsored()
	{
		if (this.Competition == null)
		{
			return;
		}
		if (this.Competition.Format == UserCompetitionFormat.Duel)
		{
			return;
		}
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			if (!(UINavigation.CurrentSelectedGo != null) || !(UINavigation.CurrentSelectedGo.name == this.SponsoredTgl.name))
			{
				return;
			}
			if (!base.IsBlockedSponsored)
			{
				this.Competition.IsSponsored = !this.Competition.IsSponsored;
				base.UpdateSponsored();
				this.UpdateBlocked();
			}
		}
	}

	protected override void AddSponsoredTglListener()
	{
		this.SponsoredTgl.onValueChanged.AddListener(delegate(bool isOn)
		{
			this.SelectSponsored();
		});
	}

	private void OnInputTypeChanged(InputModuleManager.InputType iType)
	{
		if (iType == InputModuleManager.InputType.Mouse && this.Competition != null)
		{
			base.SetOnSponsoredTgl(this.Competition.IsSponsored);
		}
	}

	private bool IsKeepEnabled()
	{
		if (this.Competition == null || this.Competition.DollEquipment == null)
		{
			return true;
		}
		List<UserCompetitionEquipmentAllowed> list = this.Competition.DollEquipment.ToList<UserCompetitionEquipmentAllowed>();
		return list.Contains(UserCompetitionEquipmentAllowed.Keepnet) || list.Contains(UserCompetitionEquipmentAllowed.Stringer);
	}

	private void FillRulesets(TournamentScoreType value)
	{
		if (value == TournamentScoreType.BestWeightMatch && this.Competition.ReferenceWeight == null)
		{
			return;
		}
		this.Competition.ScoringType = value;
		this.UpdateRulesets();
		this.Validate();
	}

	private void ChangePond(Pond p)
	{
		this.Competition.PondId = p.PondId;
		this.Competition.PondName = p.Name;
		base.ResetFee();
	}

	private readonly List<TournamentScoreType> _teamScoreTypes = new List<TournamentScoreType>
	{
		TournamentScoreType.TotalWeight,
		TournamentScoreType.TotalFishCount,
		TournamentScoreType.TotalLength
	};

	private readonly List<TournamentScoreType> _scoreTypes = new List<TournamentScoreType>
	{
		TournamentScoreType.TotalWeight,
		TournamentScoreType.TotalFishCount,
		TournamentScoreType.TotalWeightByLineMaxLoad,
		TournamentScoreType.TotalFishTypeCount,
		TournamentScoreType.BestWeightMatch,
		TournamentScoreType.BiggestFish,
		TournamentScoreType.SmallestFish,
		TournamentScoreType.BiggestSizeDiff,
		TournamentScoreType.LongestFish,
		TournamentScoreType.TotalLength
	};
}
