using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;
using UnityEngine;

namespace Assets.Scripts.UI._2D.Tournaments.UGC
{
	public class UgcConsts
	{
		public static WindowList.WindowListElem GetRodTemplateLoc(RodTemplate c)
		{
			return UgcConsts.Loc(UgcConsts.CompetitionRodTemplateLoc[c]);
		}

		public static WindowList.WindowListElem GetComponentLoc(UgcConsts.Components c)
		{
			return UgcConsts.Loc(UgcConsts.ComponentsLoc[c]);
		}

		public static WindowList.WindowListElem GetDurationLoc(UserCompetitionDuration c)
		{
			return UgcConsts.Loc(UgcConsts.CompetitionDurationLoc[c]);
		}

		public static WindowList.WindowListElem GetScheduleLoc(UserCompetitionSortType c)
		{
			return UgcConsts.Loc(UgcConsts.CompetitionScheduleLoc[c]);
		}

		public static WindowList.WindowListElem GetEquipmentAllowedLoc(UserCompetitionEquipmentAllowed c)
		{
			return UgcConsts.Loc(UgcConsts.CompetitionEquipmentAllowedLoc[c]);
		}

		public static WindowList.WindowListElem GetScoreTypeLoc(TournamentScoreType c)
		{
			return UgcConsts.Loc(UgcConsts.CompetitionScoreTypeLoc[c]);
		}

		public static WindowList.WindowListElem GetUserCompetitionRodEquipmentAllowedLoc(UserCompetitionRodEquipmentAllowed c)
		{
			return UgcConsts.Loc(UgcConsts.UserCompetitionRodEquipmentAllowedLoc[c]);
		}

		public static WindowList.WindowListElem GetCompetitionScoringLoc(TournamentFishSource c)
		{
			return UgcConsts.Loc(UgcConsts.CompetitionScoringLoc[c]);
		}

		public static WindowList.WindowListElem GetCancellationReasonLoc(UserCompetitionCancellationReason c)
		{
			return UgcConsts.Loc(UgcConsts.CancellationReasonLoc[c]);
		}

		public static WindowList.WindowListElem GetScoringTypeLoc(TournamentFishOrigin c)
		{
			if (c == TournamentFishOrigin.Unspecified)
			{
				c = TournamentFishOrigin.Shore;
			}
			return UgcConsts.Loc(UgcConsts.ScoringTypeLoc[c]);
		}

		public static WindowList.WindowListElem GetInitialPrizePoolLoc(UserCompetitionRewardScheme c)
		{
			return UgcConsts.Loc(UgcConsts.InitialPrizePoolLoc[c]);
		}

		public static UserCompetitionRewardScheme[] GetUserCompetitionRewardScheme(UserCompetitionFormat c)
		{
			return UgcConsts.InitialPrizePool[c];
		}

		public static WindowList.WindowListElem GetCompetitionFormatLoc(UserCompetitionFormat c)
		{
			return UgcConsts.Loc(UgcConsts.CompetitionFormatLoc[c]);
		}

		public static WindowList.WindowListElem GetTotalScoreKindLoc(TournamentTotalScoreKind c)
		{
			return UgcConsts.Loc(UgcConsts.TotalScoreKindLoc[c]);
		}

		public static string PrivacyDataLoc(bool c)
		{
			return ScriptLocalization.Get(UgcConsts._privacyData[(!c) ? 0 : 1].Name);
		}

		public static string PrivacyDataDescLoc(bool c)
		{
			return ScriptLocalization.Get(UgcConsts._privacyData[(!c) ? 0 : 1].Desc);
		}

		public static Color GetTeamColor(string team)
		{
			return UgcConsts.TeamsColors[team.ToUpper()];
		}

		public static string GetYellowTan(string v)
		{
			return string.Format("<color=#FFDD77FF>{0}</color>", v);
		}

		public static string GetGrey(string v)
		{
			return string.Format("<color=#7C7C7CFF>{0}</color>", v);
		}

		public static string GetWinner(string v)
		{
			return string.Format("<color=#7ED321FF>{0}</color>", v);
		}

		public static string GetTeamLoc(string team)
		{
			return ScriptLocalization.Get(UgcConsts.TeamsLocalization[team.ToUpper()]);
		}

		public static string GetTeamScoreLoc(string team)
		{
			return ScriptLocalization.Get(UgcConsts.TeamsScoreLocalization[team.ToUpper()]);
		}

		private static WindowList.WindowListElem Loc(WindowList.WindowListElem c)
		{
			string text = ScriptLocalization.Get(c.Name);
			string text2 = ScriptLocalization.Get(c.Desc);
			return new WindowList.WindowListElem
			{
				Name = ((!string.IsNullOrEmpty(text)) ? text : c.Name),
				Desc = ((!string.IsNullOrEmpty(text2)) ? text2 : c.Desc),
				ImgPath = c.ImgPath
			};
		}

		public const float BlackScreenHideFadeTime = 0.5f;

		public static readonly Color WinnerColor = new Color(0.49411765f, 0.827451f, 0.12941177f);

		public static readonly Color YellowTan = new Color(1f, 0.827451f, 0.46666667f);

		public static readonly Color UpDownEnabled = new Color(0.19607843f, 0.26666668f, 0.29803923f);

		public static readonly Color UpDownDisabled = new Color(0.23529412f, 0.23529412f, 0.23529412f);

		public static readonly Color UpDownArrowDisabled = new Color(0.16470589f, 0.16470589f, 0.16470589f);

		private static readonly Dictionary<string, string> TeamsLocalization = new Dictionary<string, string>
		{
			{
				"Red".ToUpper(),
				"UGC_RedTeam"
			},
			{
				"Blue".ToUpper(),
				"UGC_BlueTeam"
			}
		};

		private static readonly Dictionary<string, string> TeamsScoreLocalization = new Dictionary<string, string>
		{
			{
				"Red".ToUpper(),
				"UGC_RedTeamScore"
			},
			{
				"Blue".ToUpper(),
				"UGC_BlueTeamScore"
			}
		};

		private static readonly Dictionary<string, Color> TeamsColors = new Dictionary<string, Color>
		{
			{
				"Red".ToUpper(),
				new Color(0.91764706f, 0.2784314f, 0.16470589f)
			},
			{
				"Blue".ToUpper(),
				new Color(0.15294118f, 0.42352942f, 0.79607844f)
			}
		};

		private static readonly Dictionary<UserCompetitionFormat, UserCompetitionRewardScheme[]> InitialPrizePool = new Dictionary<UserCompetitionFormat, UserCompetitionRewardScheme[]>
		{
			{
				UserCompetitionFormat.Duel,
				new UserCompetitionRewardScheme[] { UserCompetitionRewardScheme.Duel_AllForWinner }
			},
			{
				UserCompetitionFormat.Team,
				new UserCompetitionRewardScheme[] { UserCompetitionRewardScheme.Team1_EqualEachOther }
			},
			{
				UserCompetitionFormat.Individual,
				new UserCompetitionRewardScheme[]
				{
					UserCompetitionRewardScheme.Individual0_100_0_0,
					UserCompetitionRewardScheme.Individual1_50_35_15,
					UserCompetitionRewardScheme.Individual2_60_30_10,
					UserCompetitionRewardScheme.Individual3_80_15_5
				}
			}
		};

		private static readonly Dictionary<UserCompetitionCancellationReason, WindowList.WindowListElem> CancellationReasonLoc = new Dictionary<UserCompetitionCancellationReason, WindowList.WindowListElem>
		{
			{
				UserCompetitionCancellationReason.ReasonCompetitionRemovedBecauseInactivity,
				new WindowList.WindowListElem
				{
					Name = "UGC_ReasonCompetitionRemovedBecauseInactivity"
				}
			},
			{
				UserCompetitionCancellationReason.ReasonCompetitionRemovedByHost,
				new WindowList.WindowListElem
				{
					Name = "UGC_ReasonCompetitionRemovedByHost"
				}
			},
			{
				UserCompetitionCancellationReason.ReasonPlayerRemovedByHost,
				new WindowList.WindowListElem
				{
					Name = "UGC_ReasonPlayerRemovedByHost"
				}
			},
			{
				UserCompetitionCancellationReason.ReasonCompetitionUnpublishedByHost,
				new WindowList.WindowListElem
				{
					Name = "UGC_ReasonCompetitionUnpublishedByHost"
				}
			},
			{
				UserCompetitionCancellationReason.ReasonCompetitionCantStartOutdated,
				new WindowList.WindowListElem
				{
					Name = "UGC_ReasonCompetitionCantStartOutdated"
				}
			},
			{
				UserCompetitionCancellationReason.ReasonCompetitionCantStartParticipantsCount,
				new WindowList.WindowListElem
				{
					Name = "UGC_ReasonCompetitionCantStartParticipantsCount"
				}
			},
			{
				UserCompetitionCancellationReason.ReasonCompetitionCantStartUpcomingRelease,
				new WindowList.WindowListElem
				{
					Name = "UGC_ReasonCompetitionCantStartUpcomingRelease"
				}
			},
			{
				UserCompetitionCancellationReason.ReasonCompetitionCantStartGeneral,
				new WindowList.WindowListElem
				{
					Name = "UGC_ReasonCompetitionCantStartGeneral"
				}
			},
			{
				UserCompetitionCancellationReason.ReasonPlayerNotReady,
				new WindowList.WindowListElem
				{
					Name = "UGC_StartCompetitionPlayerNotReady"
				}
			},
			{
				UserCompetitionCancellationReason.ReasonCompetitionReverted,
				new WindowList.WindowListElem
				{
					Name = "UGC_IsCanceledFeeRevoked"
				}
			}
		};

		private static readonly Dictionary<UserCompetitionFormat, WindowList.WindowListElem> CompetitionFormatLoc = new Dictionary<UserCompetitionFormat, WindowList.WindowListElem>
		{
			{
				UserCompetitionFormat.Individual,
				new WindowList.WindowListElem
				{
					Name = "UGC_Individual"
				}
			},
			{
				UserCompetitionFormat.Duel,
				new WindowList.WindowListElem
				{
					Name = "UGC_Duel"
				}
			},
			{
				UserCompetitionFormat.Team,
				new WindowList.WindowListElem
				{
					Name = "UGC_Team"
				}
			}
		};

		private static readonly Dictionary<UserCompetitionRewardScheme, WindowList.WindowListElem> InitialPrizePoolLoc = new Dictionary<UserCompetitionRewardScheme, WindowList.WindowListElem>
		{
			{
				UserCompetitionRewardScheme.Duel_AllForWinner,
				new WindowList.WindowListElem
				{
					Name = "UGC_Duel_AllForWinner"
				}
			},
			{
				UserCompetitionRewardScheme.Team1_EqualEachOther,
				new WindowList.WindowListElem
				{
					Name = "UGC_Team1_EqualEachOther"
				}
			},
			{
				UserCompetitionRewardScheme.Individual0_100_0_0,
				new WindowList.WindowListElem
				{
					Name = "UGC_Duel_AllForWinner"
				}
			},
			{
				UserCompetitionRewardScheme.Individual1_50_35_15,
				new WindowList.WindowListElem
				{
					Name = "UGC_Individual1_50_35_15"
				}
			},
			{
				UserCompetitionRewardScheme.Individual2_60_30_10,
				new WindowList.WindowListElem
				{
					Name = "UGC_Individual2_60_30_10"
				}
			},
			{
				UserCompetitionRewardScheme.Individual3_80_15_5,
				new WindowList.WindowListElem
				{
					Name = "UGC_Individual3_80_15_5"
				}
			}
		};

		private static readonly List<WindowList.WindowListElem> _privacyData = new List<WindowList.WindowListElem>
		{
			new WindowList.WindowListElem
			{
				Name = "UGC_OpenCompCaption",
				Desc = "UGC_OpenCompCaptionHint"
			},
			new WindowList.WindowListElem
			{
				Name = "UGC_PrivateCompCaption",
				Desc = "UGC_PrivateCompCaptionHint"
			}
		};

		private static readonly Dictionary<TournamentTotalScoreKind, WindowList.WindowListElem> TotalScoreKindLoc = new Dictionary<TournamentTotalScoreKind, WindowList.WindowListElem>
		{
			{
				TournamentTotalScoreKind.ScorePerFish,
				new WindowList.WindowListElem
				{
					Name = "UGC_ScorePerFish"
				}
			},
			{
				TournamentTotalScoreKind.ScorePerKg,
				new WindowList.WindowListElem
				{
					Name = "UGC_ScorePerKg"
				}
			},
			{
				TournamentTotalScoreKind.ScorePerMeter,
				new WindowList.WindowListElem
				{
					Name = "UGC_ScorePerMeter"
				}
			}
		};

		private static readonly Dictionary<UgcConsts.Components, WindowList.WindowListElem> ComponentsLoc = new Dictionary<UgcConsts.Components, WindowList.WindowListElem>
		{
			{
				UgcConsts.Components.Template,
				new WindowList.WindowListElem
				{
					Name = "UGC_Template"
				}
			},
			{
				UgcConsts.Components.Name,
				new WindowList.WindowListElem
				{
					Name = "UGC_Name"
				}
			},
			{
				UgcConsts.Components.FormatTypes,
				new WindowList.WindowListElem
				{
					Name = "UGC_FormatTypes"
				}
			},
			{
				UgcConsts.Components.Location,
				new WindowList.WindowListElem
				{
					Name = "UGC_Location"
				}
			},
			{
				UgcConsts.Components.EntryFee,
				new WindowList.WindowListElem
				{
					Name = "UGC_EntryFee",
					Desc = "UGC_EntryFeeHint"
				}
			},
			{
				UgcConsts.Components.InitialPrizePool,
				new WindowList.WindowListElem
				{
					Name = "UGC_InitialPrizePool",
					Desc = "UGC_InitialPrizePoolHint"
				}
			},
			{
				UgcConsts.Components.Level,
				new WindowList.WindowListElem
				{
					Name = "UGC_Level",
					Desc = "UGC_LevelHint"
				}
			},
			{
				UgcConsts.Components.Duration,
				new WindowList.WindowListElem
				{
					Name = "UGC_Duration",
					Desc = "UGC_DurationHint"
				}
			},
			{
				UgcConsts.Components.MaxPlayers,
				new WindowList.WindowListElem
				{
					Name = "UGC_MaxPlayers",
					Desc = "UGC_MaxPlayersHint"
				}
			},
			{
				UgcConsts.Components.TimeAdnWeather,
				new WindowList.WindowListElem
				{
					Name = "UGC_TimeAdnWeatherHint",
					Desc = "UGC_TimeAdnWeatherHint"
				}
			},
			{
				UgcConsts.Components.Fish,
				new WindowList.WindowListElem
				{
					Name = "UGC_Fish",
					Desc = "UGC_FishHint"
				}
			},
			{
				UgcConsts.Components.Scoring,
				new WindowList.WindowListElem
				{
					Name = "UGC_Scoring",
					Desc = "UGC_ScoringHint"
				}
			},
			{
				UgcConsts.Components.Setups,
				new WindowList.WindowListElem
				{
					Name = "UGC_Setups",
					Desc = "UGC_SetupsHint"
				}
			},
			{
				UgcConsts.Components.Equipment,
				new WindowList.WindowListElem
				{
					Name = "UGC_Equipment",
					Desc = "UGC_EquipmentHint"
				}
			},
			{
				UgcConsts.Components.Rulesets,
				new WindowList.WindowListElem
				{
					Name = "UGC_Rulesets",
					Desc = "UGC_RulesetsHint"
				}
			},
			{
				UgcConsts.Components.Privacy,
				new WindowList.WindowListElem
				{
					Name = "UGC_Privacy"
				}
			}
		};

		private static readonly Dictionary<UserCompetitionDuration, WindowList.WindowListElem> CompetitionDurationLoc = new Dictionary<UserCompetitionDuration, WindowList.WindowListElem>
		{
			{
				UserCompetitionDuration.Min2,
				new WindowList.WindowListElem
				{
					Name = "UGC_Min2"
				}
			},
			{
				UserCompetitionDuration.Min5,
				new WindowList.WindowListElem
				{
					Name = "UGC_Min5"
				}
			},
			{
				UserCompetitionDuration.Min15,
				new WindowList.WindowListElem
				{
					Name = "UGC_Min15"
				}
			},
			{
				UserCompetitionDuration.Min30,
				new WindowList.WindowListElem
				{
					Name = "UGC_Min30"
				}
			},
			{
				UserCompetitionDuration.Hour1,
				new WindowList.WindowListElem
				{
					Name = "UGC_Hour1"
				}
			},
			{
				UserCompetitionDuration.Hour2,
				new WindowList.WindowListElem
				{
					Name = "UGC_Hour2"
				}
			},
			{
				UserCompetitionDuration.Hour3,
				new WindowList.WindowListElem
				{
					Name = "UGC_Hour3"
				}
			},
			{
				UserCompetitionDuration.Hour4,
				new WindowList.WindowListElem
				{
					Name = "UGC_Hour4"
				}
			}
		};

		private static readonly Dictionary<UserCompetitionSortType, WindowList.WindowListElem> CompetitionScheduleLoc = new Dictionary<UserCompetitionSortType, WindowList.WindowListElem>
		{
			{
				UserCompetitionSortType.Automatic,
				new WindowList.WindowListElem
				{
					Name = "UGC_ScheduledStart"
				}
			},
			{
				UserCompetitionSortType.Manual,
				new WindowList.WindowListElem
				{
					Name = "UGC_ManualStart"
				}
			}
		};

		private static readonly Dictionary<TournamentFishSource, WindowList.WindowListElem> CompetitionScoringLoc = new Dictionary<TournamentFishSource, WindowList.WindowListElem>
		{
			{
				TournamentFishSource.Catch,
				new WindowList.WindowListElem
				{
					Name = "UGC_Catch",
					Desc = "UGC_CatchHint"
				}
			},
			{
				TournamentFishSource.Cage,
				new WindowList.WindowListElem
				{
					Name = "UGC_Cage",
					Desc = "UGC_CageHint"
				}
			}
		};

		private static readonly Dictionary<TournamentFishOrigin, WindowList.WindowListElem> ScoringTypeLoc = new Dictionary<TournamentFishOrigin, WindowList.WindowListElem>
		{
			{
				TournamentFishOrigin.Shore,
				new WindowList.WindowListElem
				{
					Name = "UGC_Shore"
				}
			},
			{
				TournamentFishOrigin.Boat,
				new WindowList.WindowListElem
				{
					Name = "UGC_Boat"
				}
			}
		};

		private static readonly Dictionary<UserCompetitionEquipmentAllowed, WindowList.WindowListElem> CompetitionEquipmentAllowedLoc = new Dictionary<UserCompetitionEquipmentAllowed, WindowList.WindowListElem>
		{
			{
				UserCompetitionEquipmentAllowed.Stringer,
				new WindowList.WindowListElem
				{
					Name = "UGC_StringerFilter",
					Desc = "UGC_EquipmentAllowedHint"
				}
			},
			{
				UserCompetitionEquipmentAllowed.Keepnet,
				new WindowList.WindowListElem
				{
					Name = "UGC_FishKeepnetPopup",
					Desc = "UGC_EquipmentAllowedHint"
				}
			},
			{
				UserCompetitionEquipmentAllowed.Kayak,
				new WindowList.WindowListElem
				{
					Name = "UGC_Kayak",
					Desc = "UGC_EquipmentAllowedHint"
				}
			},
			{
				UserCompetitionEquipmentAllowed.MotorBoats_All,
				new WindowList.WindowListElem
				{
					Name = "UGC_MotorBoats_All",
					Desc = "UGC_EquipmentAllowedHint"
				}
			},
			{
				UserCompetitionEquipmentAllowed.RodStand,
				new WindowList.WindowListElem
				{
					Name = "UGC_RodStandsCaption",
					Desc = "UGC_EquipmentAllowedHint"
				}
			}
		};

		private static readonly Dictionary<TournamentScoreType, WindowList.WindowListElem> CompetitionScoreTypeLoc = new Dictionary<TournamentScoreType, WindowList.WindowListElem>
		{
			{
				TournamentScoreType.TotalWeight,
				new WindowList.WindowListElem
				{
					Name = "UGC_TotalWeight",
					Desc = "UGC_TotalWeightDesc"
				}
			},
			{
				TournamentScoreType.TotalScore,
				new WindowList.WindowListElem
				{
					Name = "UGC_TotalScore",
					Desc = "UGC_TotalScoreDesc"
				}
			},
			{
				TournamentScoreType.TotalWeightByLineMaxLoad,
				new WindowList.WindowListElem
				{
					Name = "UGC_TotalWeightByLineMaxLoad",
					Desc = "UGC_TotalWeightByLineMaxLoadDesc"
				}
			},
			{
				TournamentScoreType.TotalFishTypeCount,
				new WindowList.WindowListElem
				{
					Name = "UGC_TotalFishTypeCount",
					Desc = "UGC_TotalFishTypeCountDesc"
				}
			},
			{
				TournamentScoreType.BestWeightMatch,
				new WindowList.WindowListElem
				{
					Name = "UGC_BestWeightMatch",
					Desc = "UGC_BestWeightMatchDesc"
				}
			},
			{
				TournamentScoreType.BiggestFish,
				new WindowList.WindowListElem
				{
					Name = "UGC_BiggestFish",
					Desc = "UGC_BiggestFishDesc"
				}
			},
			{
				TournamentScoreType.SmallestFish,
				new WindowList.WindowListElem
				{
					Name = "UGC_SmallestFish",
					Desc = "UGC_SmallestFishDesc"
				}
			},
			{
				TournamentScoreType.BiggestSizeDiff,
				new WindowList.WindowListElem
				{
					Name = "UGC_BiggestSizeDiff",
					Desc = "UGC_BiggestSizeDiffDesc"
				}
			},
			{
				TournamentScoreType.LongestFish,
				new WindowList.WindowListElem
				{
					Name = "UGC_LongestFish",
					Desc = "UGC_LongestFishDesc"
				}
			},
			{
				TournamentScoreType.TotalLength,
				new WindowList.WindowListElem
				{
					Name = "UGC_TotalLength",
					Desc = "UGC_TotalLengthDesc"
				}
			},
			{
				TournamentScoreType.TotalLengthTop3,
				new WindowList.WindowListElem
				{
					Name = "UGC_TotalLengthTop3",
					Desc = "UGC_TotalLengthTop3Desc"
				}
			},
			{
				TournamentScoreType.TotalFishCount,
				new WindowList.WindowListElem
				{
					Name = "UGC_TotalFishCount",
					Desc = "UGC_TotalFishCountDesc"
				}
			}
		};

		private static readonly Dictionary<RodTemplate, WindowList.WindowListElem> CompetitionRodTemplateLoc = new Dictionary<RodTemplate, WindowList.WindowListElem>
		{
			{
				RodTemplate.Float,
				new WindowList.WindowListElem
				{
					Name = "UGC_Float",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				RodTemplate.Jig,
				new WindowList.WindowListElem
				{
					Name = "UGC_Jig",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				RodTemplate.Lure,
				new WindowList.WindowListElem
				{
					Name = "UGC_Lure",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				RodTemplate.Bottom,
				new WindowList.WindowListElem
				{
					Name = "UGC_Bottom",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				RodTemplate.ClassicCarp,
				new WindowList.WindowListElem
				{
					Name = "UGC_ClassicCarp",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				RodTemplate.MethodCarp,
				new WindowList.WindowListElem
				{
					Name = "UGC_MethodCarp",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				RodTemplate.PVACarp,
				new WindowList.WindowListElem
				{
					Name = "UGC_PVACarp",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				RodTemplate.Spod,
				new WindowList.WindowListElem
				{
					Name = "UGC_Spod",
					Desc = "UGC_SetupsAllowedHint"
				}
			}
		};

		private static readonly Dictionary<UserCompetitionRodEquipmentAllowed, WindowList.WindowListElem> UserCompetitionRodEquipmentAllowedLoc = new Dictionary<UserCompetitionRodEquipmentAllowed, WindowList.WindowListElem>
		{
			{
				UserCompetitionRodEquipmentAllowed.Crankbaits,
				new WindowList.WindowListElem
				{
					Name = "UGC_CrankbaitsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.TopwaterLures,
				new WindowList.WindowListElem
				{
					Name = "UGC_TopwaterLuresCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.Jerkbaits,
				new WindowList.WindowListElem
				{
					Name = "UGC_JerkbaitsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.Minnows,
				new WindowList.WindowListElem
				{
					Name = "UGC_MinnowsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.Spoons,
				new WindowList.WindowListElem
				{
					Name = "UGC_SpoonsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.Spinners,
				new WindowList.WindowListElem
				{
					Name = "UGC_SpinnersCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.Spinnerbaits,
				new WindowList.WindowListElem
				{
					Name = "UGC_SpinnerbaitsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.SpinnerbaitsBuzzbaits,
				new WindowList.WindowListElem
				{
					Name = "UGC_SpinnerbaitsBuzzbaitsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.CommonBaits_Float,
				new WindowList.WindowListElem
				{
					Name = "UGC_CommonBaitsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.InsectWormBaits_Float,
				new WindowList.WindowListElem
				{
					Name = "UGC_InsectWormBaitsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.FreshBaits_Float,
				new WindowList.WindowListElem
				{
					Name = "UGC_FreshBaitsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.CommonBaits_BottomOrFeeder,
				new WindowList.WindowListElem
				{
					Name = "UGC_CommonBaitsCaptionFeeder",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.InsectWormBaits_BottomOrFeeder,
				new WindowList.WindowListElem
				{
					Name = "UGC_InsectWormBaitsCaptionFeeder",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.FreshBaits_BottomOrFeeder,
				new WindowList.WindowListElem
				{
					Name = "UGC_FreshBaitsCaptionFeeder",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.BoilesAndPellets,
				new WindowList.WindowListElem
				{
					Name = "UGC_BoilesPelletsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.JigHeadsAndSoftBaits,
				new WindowList.WindowListElem
				{
					Name = "UGC_JigHeadsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.BassJigs,
				new WindowList.WindowListElem
				{
					Name = "UGC_BassJigsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.CarolinaRigs,
				new WindowList.WindowListElem
				{
					Name = "UGC_CarolinaRigsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.TexasRigs,
				new WindowList.WindowListElem
				{
					Name = "UGC_TexasRigsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.ThreewayRigs,
				new WindowList.WindowListElem
				{
					Name = "UGC_ThreewayRigsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.OffsetHookAndSoftBaits,
				new WindowList.WindowListElem
				{
					Name = "UGC_OffsetHookAndSoftBaitsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.BassJigsSpinnerbaitsAndSoftBaits,
				new WindowList.WindowListElem
				{
					Name = "UGC_BassJigsSpinnerbaitsAndSoftBaitsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.SpinnersSpinnerbaitsAndTails,
				new WindowList.WindowListElem
				{
					Name = "UGC_SpinnersSpinnerbaitsAndTailsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.SpinnersSpinnerbaitsBuzzbaitsAndTails,
				new WindowList.WindowListElem
				{
					Name = "UGC_SpinnersSpinnerbaitsBuzzbaitsAndTailsCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			},
			{
				UserCompetitionRodEquipmentAllowed.Swimbait,
				new WindowList.WindowListElem
				{
					Name = "UGC_SwimbaitCaption",
					Desc = "UGC_SetupsAllowedHint"
				}
			}
		};

		public enum Components
		{
			Template,
			Name,
			Location,
			EntryFee,
			InitialPrizePool,
			Level,
			Duration,
			TimeAdnWeather,
			Fish,
			Scoring,
			Setups,
			Equipment,
			Rulesets,
			Privacy,
			FormatTypes,
			MaxPlayers
		}
	}
}
