using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using ObjectModel;
using ObjectModel.Tournaments;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI._2D.Tournaments.UGC
{
	public class UserCompetitionHelper
	{
		public static List<UserCompetitionEquipmentAllowed> GetNotAllowedEquipment(Inventory i, List<UserCompetitionEquipmentAllowed> eqAllowed)
		{
			List<UserCompetitionEquipmentAllowed> list = new List<UserCompetitionEquipmentAllowed>();
			List<UserCompetitionEquipmentAllowed> list2 = UserCompetitionHelper.EnumToList<UserCompetitionEquipmentAllowed>(UserCompetitionEquipmentAllowed.None);
			List<UserCompetitionEquipmentAllowed> list3 = list2.Where((UserCompetitionEquipmentAllowed p) => !eqAllowed.Contains(p)).ToList<UserCompetitionEquipmentAllowed>();
			for (int j = 0; j < list3.Count; j++)
			{
				UserCompetitionEquipmentAllowed userCompetitionEquipmentAllowed = list3[j];
				switch (userCompetitionEquipmentAllowed)
				{
				case UserCompetitionEquipmentAllowed.Keepnet:
					if (i.Any((InventoryItem p) => p.ItemSubType == ItemSubTypes.Keepnet && UserCompetitionHelper.StoragePlaces4Check.Contains(p.Storage)))
					{
						list.Add(list3[j]);
					}
					break;
				default:
					switch (userCompetitionEquipmentAllowed)
					{
					case UserCompetitionEquipmentAllowed.Stringer:
						if (i.Any((InventoryItem p) => p.ItemSubType == ItemSubTypes.Stringer && UserCompetitionHelper.StoragePlaces4Check.Contains(p.Storage)))
						{
							list.Add(list3[j]);
						}
						break;
					default:
						if (userCompetitionEquipmentAllowed == UserCompetitionEquipmentAllowed.MotorBoats_All)
						{
							if (i.OfType<MotorBoat>().Any((MotorBoat p) => UserCompetitionHelper.StoragePlaces4Check.Contains(p.Storage)))
							{
								list.Add(list3[j]);
							}
						}
						break;
					case UserCompetitionEquipmentAllowed.Kayak:
						if (i.OfType<Kayak>().Any((Kayak p) => UserCompetitionHelper.StoragePlaces4Check.Contains(p.Storage)))
						{
							list.Add(list3[j]);
						}
						break;
					}
					break;
				case UserCompetitionEquipmentAllowed.RodStand:
					if (i.OfType<RodStand>().Any((RodStand p) => UserCompetitionHelper.StoragePlaces4Check.Contains(p.Storage)))
					{
						list.Add(list3[j]);
					}
					break;
				}
			}
			return list;
		}

		public static List<ItemSubTypes> GetNotEquippedSetups(Inventory i, List<UserCompetitionRodEquipmentAllowed> eqAllowed)
		{
			List<ItemSubTypes> itemsAll = new List<ItemSubTypes>();
			eqAllowed.ForEach(delegate(UserCompetitionRodEquipmentAllowed p)
			{
				TournamentEquipment tournamentEquipment = new TournamentEquipment();
				p.TranslateTournamentEquipment(tournamentEquipment);
				itemsAll.AddRange(UserCompetitionHelper.TournamentEquipment2List(tournamentEquipment));
			});
			return (from p in i
				where !UserCompetitionHelper.StoragePlaces4Check.Contains(p.Storage) && itemsAll.Contains(p.ItemSubType)
				select p.ItemSubType).ToList<ItemSubTypes>();
		}

		public static bool IsUgcEnabled
		{
			get
			{
				return true;
			}
		}

		public static Tournament Ugc2Tournament(UserCompetitionPublic ugc)
		{
			Tournament tournament = new Tournament();
			tournament.TournamentId = ugc.TournamentId;
			tournament.LogoBID = ugc.LogoBID;
			tournament.ImageBID = ugc.ImageBID;
			tournament.PondId = ugc.PondId;
			tournament.Rules = ugc.Rules;
			tournament.NameCustom = ugc.NameCustom;
			tournament.KindId = 4;
			tournament.StartDate = ((ugc.SortType != UserCompetitionSortType.Manual || ugc.IsStarted) ? ugc.StartDate : ugc.StartDate.AddYears(100));
			tournament.EndDate = ((ugc.SortType != UserCompetitionSortType.Manual || ugc.IsStarted) ? ugc.EndDate : ugc.EndDate.AddYears(100));
			Tournament tournament2 = tournament;
			DateTime? publishDate = ugc.PublishDate;
			tournament2.RegistrationStart = ((publishDate == null) ? TimeHelper.UtcTime().AddYears(100) : publishDate.Value);
			tournament.IsSponsored = ugc.IsSponsored;
			tournament.IsStarted = ugc.IsStarted;
			tournament.IsRegistered = ugc.IsRegistered;
			tournament.IsEnded = ugc.IsEnded;
			tournament.IsDone = ugc.IsDone;
			tournament.IsActive = ugc.IsActive;
			tournament.IsCanceled = ugc.IsCanceled || ugc.IsDeleted;
			tournament.IsDisqualified = ugc.IsDisqualified;
			tournament.InGameStartHour = ugc.InGameStartHour;
			tournament.EntranceFee = ugc.EntranceFee;
			tournament.Currency = ugc.Currency;
			tournament.MaxParticipants = ugc.MaxParticipants;
			tournament.MinParticipants = ugc.MinParticipants;
			tournament.EquipmentAllowed = ugc.TournamentEquipment;
			return tournament;
		}

		public static Range GetFee(string currency, Range minMaxRange, int maxAdditional)
		{
			return new Range(Mathf.Max(0, minMaxRange.Min), Mathf.Min(minMaxRange.Max, (int)MeasuringSystemManager.GetMaxCoins(currency) - maxAdditional));
		}

		public static double PrizePoolWithComission(UserCompetitionPublic t)
		{
			return t.PrizePoolWithoutComission(t.RegistrationsCount);
		}

		public static string FeeCommissionLoc
		{
			get
			{
				return string.Format(ScriptLocalization.Get("UGC_IncludingTax"), string.Format("{0}%", UserCompetitionPublic.UgcCompetitionFeeCommission));
			}
		}

		public static string FeeCommissionExtLoc
		{
			get
			{
				return string.Format(ScriptLocalization.Get("UGC_IncludingTaxExt"), string.Format("{0}%", UserCompetitionPublic.UgcCompetitionFeeCommission));
			}
		}

		public static void LoadSponsoredMaterials()
		{
			if (!UserCompetitionHelper._sponsoredInited)
			{
				UserCompetitionHelper._sponsoredInited = true;
				for (int i = 0; i < UserCompetitionHelper._fontSponsoredData.Count; i++)
				{
					UserCompetitionHelper.SponsoredMaterialsData fd = UserCompetitionHelper._fontSponsoredData[i];
					ResourcesHelpers.LoadResource(string.Format("TMP_Fonts/{0}", fd.MaterialName), delegate(Object o)
					{
						fd.FontMaterial = o as Material;
						for (int j = 0; j < fd.FontToSet.Count; j++)
						{
							if (fd.FontToSet[j] != null)
							{
								fd.FontToSet[j].fontMaterial = fd.FontMaterial;
							}
						}
					});
				}
			}
		}

		public static void SetSponsoredMaterial(TextMeshProUGUI text, string materialName)
		{
			UserCompetitionHelper.SponsoredMaterialsData sponsoredMaterialsData = UserCompetitionHelper._fontSponsoredData.FirstOrDefault((UserCompetitionHelper.SponsoredMaterialsData p) => p.MaterialName == materialName);
			if (sponsoredMaterialsData != null)
			{
				if (sponsoredMaterialsData.FontMaterial != null)
				{
					text.fontSharedMaterial = sponsoredMaterialsData.FontMaterial;
				}
				else
				{
					if (!sponsoredMaterialsData.FontToSet.Contains(text))
					{
						sponsoredMaterialsData.FontToSet.Add(text);
					}
					if (!UserCompetitionHelper._sponsoredInited)
					{
						UserCompetitionHelper.LoadSponsoredMaterials();
					}
				}
				text.color = Color.white;
			}
		}

		public static string GetScoreString(TournamentScoreType scoringType, double? score, TournamentTotalScoreKind totalScoreKind)
		{
			return MeasuringSystemManager.GetTournamentScoreValueToString(scoringType, (score == null) ? null : new float?((float)score.Value), totalScoreKind, "3");
		}

		public static string GetDefaultName(UserCompetitionPublic competition)
		{
			string text = competition.NameCustom;
			if (string.IsNullOrEmpty(text) || !competition.IsSponsored)
			{
				text = string.Format("{0}'s {1}", competition.HostName, (competition.Type != UserCompetitionType.Custom) ? competition.TemplateName : UgcConsts.GetCompetitionFormatLoc(competition.Format).Name);
			}
			if (competition.IsSponsored)
			{
				return string.Format("{0}", text);
			}
			return text;
		}

		public static void GetDefaultName(TextMeshProUGUI text, UserCompetitionPublic competition)
		{
			if (competition.IsSponsored)
			{
				UserCompetitionHelper.SetSponsoredMaterial(text, "Roboto-Regular_SDF_Sponsored");
			}
			text.text = UserCompetitionHelper.GetDefaultName(competition);
		}

		public static bool IsOwnerHost(UserCompetitionPublic c)
		{
			return PhotonConnectionFactory.Instance.Profile != null && UserCompetitionHelper.IsPlayerHost(c, PhotonConnectionFactory.Instance.Profile.UserId);
		}

		public static bool IsPlayerHost(UserCompetitionPublic c, Guid userId)
		{
			return c.HostUserId == userId;
		}

		public static string GetLevel(UserCompetitionPublic c)
		{
			int? minLevel = c.MinLevel;
			int valueOrDefault = minLevel.GetValueOrDefault();
			int? maxLevel = c.MaxLevel;
			if (valueOrDefault == maxLevel.GetValueOrDefault() && minLevel != null == (maxLevel != null))
			{
				return c.MinLevel.ToString();
			}
			if (c.MaxLevel == null || c.MaxLevel.Value == 0)
			{
				return string.Format("{0}+", c.MinLevel);
			}
			return string.Format("{0} - {1}", c.MinLevel, c.MaxLevel);
		}

		public static string GetTypeIco(UserCompetitionPublic c)
		{
			return (!UserCompetitionHelper._typeIcos.ContainsKey(c.Format)) ? string.Empty : UserCompetitionHelper._typeIcos[c.Format];
		}

		public static string GetJoined(UserCompetitionPublic c)
		{
			return string.Format("{0}/{1}", c.RegistrationsCount, c.MaxParticipants);
		}

		public static string GetDurationText(UserCompetitionPublic t, bool isGrey = true)
		{
			string text = ((t.FixedStartDate == null) ? ScriptLocalization.Get("UGC_ManualStart") : t.FixedStartDate.Value.ToLocalTime().ToString(CultureInfo.InvariantCulture));
			if (isGrey)
			{
				text = UgcConsts.GetGrey(string.Format("({0})", text));
			}
			return string.Format("{0} {1}", UgcConsts.GetDurationLoc(t.Duration).Name, text);
		}

		public static string GetLocNameTotalScoreKind(TournamentTotalScoreKind kind)
		{
			string name = UgcConsts.GetTotalScoreKindLoc(kind).Name;
			if (kind == TournamentTotalScoreKind.ScorePerKg)
			{
				return string.Format(name, MeasuringSystemManager.FishWeightSufix());
			}
			return name;
		}

		public static string GetScoringTypeLoc(UserCompetitionPublic t)
		{
			string text = UgcConsts.GetScoreTypeLoc(t.ScoringType).Name;
			if (t.ScoringType == TournamentScoreType.BestWeightMatch)
			{
				if (t.ReferenceWeight != null)
				{
					string text2 = string.Format("{0} {1}", MeasuringSystemManager.FishWeight(t.ReferenceWeight.Value).ToString("N3"), MeasuringSystemManager.FishWeightSufix());
					text += string.Format("({0})", text2);
				}
			}
			else if (t.ScoringType == TournamentScoreType.TotalScore && t.Type == UserCompetitionType.Custom)
			{
				text += string.Format("({0})", UserCompetitionHelper.GetLocNameTotalScoreKind(t.TotalScoreKind));
			}
			return text;
		}

		public static FishBrief[] GetFishBriefs(string[] fishScore)
		{
			return CacheLibrary.MapCache.FishesLight.Where((FishBrief p) => fishScore.Contains(p.CodeName)).ToArray<FishBrief>();
		}

		public static string[] GetFishScore(int[] fishIds)
		{
			return (from p in CacheLibrary.MapCache.FishesLight
				where fishIds.Contains(p.FishId)
				select p.CodeName).ToArray<string>();
		}

		public static List<ItemSubTypes> TournamentEquipment2List(TournamentEquipment eq)
		{
			List<ItemSubTypes> list = new List<ItemSubTypes>();
			if (eq.RodTypes != null)
			{
				list.AddRange(eq.RodTypes);
			}
			if (eq.LineTypes != null)
			{
				list.AddRange(eq.LineTypes);
			}
			if (eq.TerminalTackleTypes != null)
			{
				list.AddRange(eq.TerminalTackleTypes);
			}
			if (eq.SinkerTypes != null)
			{
				list.AddRange(eq.SinkerTypes);
			}
			if (eq.FeederTypes != null)
			{
				list.AddRange(eq.FeederTypes);
			}
			if (eq.LeaderTypes != null)
			{
				list.AddRange(eq.LeaderTypes);
			}
			if (eq.BaitTypes != null)
			{
				list.AddRange(eq.BaitTypes);
			}
			if (eq.ChumTypes != null)
			{
				list.AddRange(eq.ChumTypes);
			}
			return list;
		}

		public static T[] EnumToArray<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>().ToArray<T>();
		}

		public static List<T> EnumToList<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>().ToList<T>();
		}

		public static List<T> EnumToList<T>(T skipValue)
		{
			return (from T p in Enum.GetValues(typeof(T))
				where !p.Equals(skipValue)
				select p).ToList<T>();
		}

		public static List<T> EnumToList<T>(List<T> skipValues)
		{
			return (from T p in Enum.GetValues(typeof(T))
				where !skipValues.Contains(p)
				select p).ToList<T>();
		}

		public static string JoinArrayAndLocalize<T>(T[] data, Func<T, string> funcLoc, bool addNewLine = false)
		{
			string text = string.Empty;
			if (data != null)
			{
				if (funcLoc == null)
				{
					funcLoc = (T arg1) => arg1 as string;
				}
				text = string.Join((!addNewLine) ? ", " : ",\n", data.Select((T p) => funcLoc(p)).ToArray<string>());
			}
			return text;
		}

		public static string GetLoc<T>(T[] d, Func<T, string> funcLoc)
		{
			if (d == null)
			{
				return ScriptLocalization.Get("NoRestriction");
			}
			return UserCompetitionHelper.JoinArrayAndLocalize<T>(d, funcLoc, false);
		}

		public static List<WindowList.WindowListElem> CreateWindowList<T>(out int index, WindowList.WindowListDataGetter<T> dataGetter, List<T> enumList, T curValue)
		{
			index = 0;
			List<WindowList.WindowListElem> list = new List<WindowList.WindowListElem>();
			for (int i = 0; i < enumList.Count; i++)
			{
				list.Add(new WindowList.WindowListElem
				{
					RadioId = ((dataGetter.RadioId == null) ? 0 : dataGetter.RadioId(enumList[i])),
					Interactable = (dataGetter.Interactable == null || dataGetter.Interactable(enumList[i])),
					Name = dataGetter.LocName(enumList[i]),
					Desc = ((dataGetter.LocDesc == null) ? null : dataGetter.LocDesc(enumList[i])),
					ImgPath = ((dataGetter.GetImgPath == null) ? null : dataGetter.GetImgPath(enumList[i]))
				});
				T t = enumList[i];
				if (t.Equals(curValue))
				{
					index = i;
				}
			}
			return list;
		}

		public static List<WindowList.WindowListElem> CreateWindowList<T>(out List<int> indexes, WindowList.WindowListDataGetter<T> dataGetter, List<T> enumList, T[] curValues)
		{
			indexes = new List<int>();
			List<WindowList.WindowListElem> list = new List<WindowList.WindowListElem>();
			for (int i = 0; i < enumList.Count; i++)
			{
				list.Add(new WindowList.WindowListElem
				{
					RadioId = ((dataGetter.RadioId == null) ? 0 : dataGetter.RadioId(enumList[i])),
					Interactable = (dataGetter.Interactable == null || dataGetter.Interactable(enumList[i])),
					Name = dataGetter.LocName(enumList[i]),
					Desc = ((dataGetter.LocDesc == null) ? null : dataGetter.LocDesc(enumList[i])),
					ImgPath = ((dataGetter.GetImgPath == null) ? null : dataGetter.GetImgPath(enumList[i]))
				});
				if (curValues != null && curValues.Contains(enumList[i]))
				{
					indexes.Add(i);
				}
			}
			return list;
		}

		public static void OpenWindowListMultiselect<T>(T skipValue, WindowList.WindowListDataGetter<T> dataGetter, T[] src, Action<List<T>> funcCallback, WindowList.Titles titles, bool useSkipValue = true, string allDescCaption = null)
		{
			List<T> list = ((!useSkipValue) ? UserCompetitionHelper.EnumToList<T>() : UserCompetitionHelper.EnumToList<T>(skipValue));
			UserCompetitionHelper.OpenWindowListMultiselect<T>(list, dataGetter, src, funcCallback, titles, allDescCaption);
		}

		public static void OpenWindowListMultiselect<T>(List<T> sc, WindowList.WindowListDataGetter<T> dataGetter, T[] src, Action<List<T>> funcCallback, WindowList.Titles titles, string allDescCaption = null)
		{
			List<int> list2;
			List<WindowList.WindowListElem> list = UserCompetitionHelper.CreateWindowList<T>(out list2, dataGetter, sc, src);
			GameObject gameObject = TournamentHelper.ShowWindowList(new WindowList.WindowListContainer
			{
				Data = list,
				Title = titles.Title,
				DataTitle = titles.DataTitle,
				DescTitle = titles.DescTitle,
				Index = 0,
				Indexes = list2,
				AllDescCaption = allDescCaption
			});
			gameObject.GetComponent<WindowList>().OnMultiSelected += delegate(List<int> idxs)
			{
				List<T> list3 = new List<T>();
				for (int i = 0; i < idxs.Count; i++)
				{
					list3.Add(sc[idxs[i]]);
				}
				funcCallback(list3);
			};
		}

		public static void OpenWindowList<T>(T skipValue, WindowList.WindowListDataGetter<T> dataGetter, T src, Action<T> funcCallback, WindowList.Titles titles, bool useSkipValue = true)
		{
			List<T> list = ((!useSkipValue) ? UserCompetitionHelper.EnumToList<T>() : UserCompetitionHelper.EnumToList<T>(skipValue));
			UserCompetitionHelper.OpenWindowList<T>(list, dataGetter, src, funcCallback, titles, null);
		}

		public static void OpenWindowList<T>(List<T> skipValues, WindowList.WindowListDataGetter<T> dataGetter, T src, Action<T> funcCallback, WindowList.Titles titles, bool useSkipValue = true)
		{
			List<T> list = ((!useSkipValue) ? UserCompetitionHelper.EnumToList<T>() : UserCompetitionHelper.EnumToList<T>(skipValues));
			UserCompetitionHelper.OpenWindowList<T>(list, dataGetter, src, funcCallback, titles, null);
		}

		public static void OpenWindowList<T>(List<T> sc, WindowList.WindowListDataGetter<T> dataGetter, T src, Action<T> funcCallback, WindowList.Titles titles, string allDescCaption = null)
		{
			int num;
			List<WindowList.WindowListElem> list = UserCompetitionHelper.CreateWindowList<T>(out num, dataGetter, sc, src);
			GameObject gameObject = TournamentHelper.ShowWindowList(new WindowList.WindowListContainer
			{
				Data = list,
				Title = titles.Title,
				DataTitle = titles.DataTitle,
				DescTitle = titles.DescTitle,
				Index = num,
				AllDescCaption = allDescCaption
			});
			gameObject.GetComponent<WindowList>().OnSelected += delegate(int i)
			{
				funcCallback(sc[i]);
			};
		}

		public const string SponsoredMaterialRR = "Roboto-Regular_SDF_Sponsored";

		public const string SponsoredMaterialA3 = "Angler_icon_v3 SDF_Sponsored";

		public const string SponsoredCompetitionPrefix = "{0}";

		private static bool _sponsoredInited;

		private static readonly List<UserCompetitionHelper.SponsoredMaterialsData> _fontSponsoredData = new List<UserCompetitionHelper.SponsoredMaterialsData>
		{
			new UserCompetitionHelper.SponsoredMaterialsData
			{
				MaterialName = "Roboto-Regular_SDF_Sponsored",
				FontToSet = new List<TextMeshProUGUI>()
			},
			new UserCompetitionHelper.SponsoredMaterialsData
			{
				MaterialName = "Angler_icon_v3 SDF_Sponsored",
				FontToSet = new List<TextMeshProUGUI>()
			}
		};

		private static readonly Dictionary<UserCompetitionFormat, string> _typeIcos = new Dictionary<UserCompetitionFormat, string>
		{
			{
				UserCompetitionFormat.Individual,
				"\ue001"
			},
			{
				UserCompetitionFormat.Duel,
				"\ue002"
			},
			{
				UserCompetitionFormat.Team,
				"\ue000"
			}
		};

		public static readonly IList<StoragePlaces> StoragePlaces4Check = new ReadOnlyCollection<StoragePlaces>(new List<StoragePlaces>
		{
			StoragePlaces.Doll,
			StoragePlaces.Hands
		});

		private class SponsoredMaterialsData
		{
			public string MaterialName { get; set; }

			public Material FontMaterial { get; set; }

			public List<TextMeshProUGUI> FontToSet { get; set; }
		}
	}
}
