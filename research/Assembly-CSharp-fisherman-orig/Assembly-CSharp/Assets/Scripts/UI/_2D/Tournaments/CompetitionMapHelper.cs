using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using ObjectModel.Tournaments;
using Photon.Interfaces;
using UnityEngine;

namespace Assets.Scripts.UI._2D.Tournaments
{
	public class CompetitionMapHelper
	{
		public static void ShowCompetitionInfo(int multyCount, List<Tournament> ts, Tournament current, Action refreshFn, Action<Tournament> onJoinFn)
		{
			if (multyCount > 1)
			{
				ts = ts.OrderByDescending((Tournament p) => p.IsActive).ToList<Tournament>();
				CompetitionMapHelper.ShowMultiCompetitionInfo(ts, delegate(int tournamentId)
				{
					Tournament t = ts.FirstOrDefault((Tournament p) => p.TournamentId == tournamentId);
					if (t != null)
					{
						bool flag = TournamentHelper.GetCompetitionStatus(t) == TournamentStatus.RegAndStarting;
						if (flag && StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId != t.PondId)
						{
							Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond p) => p.PondId == t.PondId);
							UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("UGC_CompetitionPlayerShouldBeOnPond"), UgcConsts.GetYellowTan(pond.Name)), TournamentCanceledInit.MessageTypes.Warning, null, false);
							return;
						}
						if (flag)
						{
							onJoinFn(t);
						}
					}
				}, delegate(int tournamentId)
				{
					Tournament t = ts.FirstOrDefault((Tournament p) => p.TournamentId == tournamentId);
					if (t != null)
					{
						if (t.IsUgc())
						{
							CompetitionMapHelper.ShowingUgcDetails(TournamentManager.Instance.UserGeneratedCompetitions.FirstOrDefault((UserCompetitionPublic p) => p.TournamentId == t.TournamentId), t);
						}
						else
						{
							CompetitionMapHelper.AddEvents(TournamentHelper.ShowingTournamentDetails(t, false), refreshFn);
						}
					}
				});
			}
			else
			{
				UserCompetitionPublic currentUserGeneratedCompetition = TournamentManager.CurrentUserGeneratedCompetition;
				if ((currentUserGeneratedCompetition != null && (current == null || currentUserGeneratedCompetition.TournamentId == current.TournamentId)) || (current != null && current.IsUgc()))
				{
					CompetitionMapHelper.ShowingUgcDetails(currentUserGeneratedCompetition, current);
					return;
				}
				if (current != null)
				{
					GameObject gameObject = null;
					bool hasSeries = TournamentHelper.HasSeries;
					if (!hasSeries || current.SerieInstanceId == null || current.IsRegistered)
					{
						gameObject = TournamentHelper.ShowingTournamentDetails(current, false);
					}
					else if (hasSeries)
					{
						gameObject = TournamentHelper.ShowingSerieDetails(TournamentHelper.SeriesInstances.FirstOrDefault((TournamentSerieInstance x) => x.SerieInstanceId == current.SerieInstanceId.Value));
					}
					if (gameObject != null)
					{
						CompetitionMapHelper.AddEvents(gameObject, refreshFn);
					}
				}
			}
		}

		public static void ShowMultiCompetitionInfo(List<Tournament> ts, Action<int> onSelectCompetition, Action<int> onDetailsCompetition)
		{
			int num = ts.FindIndex((Tournament t) => TournamentHelper.GetCompetitionStatus(t) == TournamentStatus.RegAndStarting);
			if (num < 0)
			{
				num = 0;
			}
			List<WindowListCompetitions.WindowListElemCompetitions> data = CompetitionMapHelper.PrepareListCompetitions(ts);
			if (data.Count == 0)
			{
				LogHelper.Error("___kocha competitions is empty", new object[0]);
				return;
			}
			WindowListCompetitions component = TournamentHelper.ShowWindowListCompetitions(CompetitionMapHelper.PrepareContainerCompetitions(data, num)).GetComponent<WindowListCompetitions>();
			component.OnSelected += delegate(int i)
			{
				LogHelper.Log("___kocha OnSelected i:{0} TournamentId:{1} Name:{2}", new object[]
				{
					i,
					data[i].Competition.TournamentId,
					data[i].Name
				});
				onSelectCompetition(data[i].Competition.TournamentId);
			};
			component.OnDetails += delegate(int i)
			{
				LogHelper.Log("___kocha OnDetails i:{0} TournamentId:{1} Name:{2}", new object[]
				{
					i,
					data[i].Competition.TournamentId,
					data[i].Name
				});
				onDetailsCompetition(data[i].Competition.TournamentId);
			};
		}

		public static WindowListCompetitions.WindowListContainerCompetitions PrepareContainerCompetitions(List<WindowListCompetitions.WindowListElemCompetitions> data, int index)
		{
			return new WindowListCompetitions.WindowListContainerCompetitions
			{
				Data = data,
				Title = ScriptLocalization.Get("UGC_AllCompetitionsCaption"),
				DataTitle = ScriptLocalization.Get("UGC_Name"),
				DescTitle = ScriptLocalization.Get("SupportFormDescription"),
				Index = index
			};
		}

		public static List<WindowListCompetitions.WindowListElemCompetitions> PrepareListCompetitions(List<Tournament> ts)
		{
			ts = ts.OrderByDescending((Tournament p) => p.IsActive).ToList<Tournament>();
			List<WindowListCompetitions.WindowListElemCompetitions> list = new List<WindowListCompetitions.WindowListElemCompetitions>();
			for (int i = 0; i < ts.Count; i++)
			{
				Tournament t = ts[i];
				bool flag = t.IsUgc();
				UserCompetitionPublic userCompetitionPublic = ((!flag) ? null : TournamentManager.Instance.UserGeneratedCompetitions.FirstOrDefault((UserCompetitionPublic p) => p.TournamentId == t.TournamentId));
				list.Add(new WindowListCompetitions.WindowListElemCompetitions
				{
					Name = ((!flag || userCompetitionPublic == null) ? t.Name : UserCompetitionHelper.GetDefaultName(userCompetitionPublic)),
					Desc = t.Rules,
					ImgPath = string.Format("Textures/Inventory/{0}", t.ImageBID),
					Competition = t,
					Ugc = userCompetitionPublic
				});
			}
			return list;
		}

		public static bool CanJoin(Tournament t)
		{
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			if (profile == null)
			{
				return false;
			}
			ErrorCode errorCode = 0;
			try
			{
				errorCode = profile.CheckTournamentStartPrerequisites(t);
			}
			catch (Exception ex)
			{
				string text = string.Format("CompetitionMapHelper::CanJoin e.Message:{0} TournamentId:{1} PrimaryScoring:{2} Inventory != null:{3}", new object[]
				{
					ex.Message,
					(t != null) ? t.TournamentId : (-1),
					(t != null) ? t.PrimaryScoring : null,
					profile.Inventory != null
				});
				PhotonConnectionFactory.Instance.PinError(text, ex.StackTrace);
				return false;
			}
			if (errorCode == 32638 || errorCode == 32579 || errorCode == 32555 || errorCode == 32581 || errorCode == 32580 || errorCode == 32557)
			{
				string text2 = ScriptLocalization.Get((!t.IsUgc()) ? "EquipmentNotConformCompetition" : "UGC_EquipmentNotConformCompetition");
				UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), text2, true, null, false);
				return false;
			}
			if (errorCode == 32643)
			{
				Pond pond = CacheLibrary.MapCache.CachedPonds.FirstOrDefault((Pond x) => x.PondId == t.PondId);
				UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), string.Format("{0}: {1}", ScriptLocalization.Get("OnPondFailedMesssage"), UgcConsts.GetYellowTan(pond.Name)), TournamentCanceledInit.MessageTypes.Warning, null, false);
				return false;
			}
			if (RodHelper.HasAnyRonOnStand())
			{
				UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("CantMoveRodPodsActive"), true, null, false);
				return false;
			}
			return true;
		}

		public static void EnterToTournament(Tournament t, ShowLocationInfo locationInfo, bool isCompetition, Action onProceed)
		{
			if (t == null || GameFactory.Player == null || locationInfo.CurrentLocation == null)
			{
				return;
			}
			ProfileTournament profileTournament = TournamentHelper.ProfileTournament;
			if (StaticUserData.CurrentLocation == null || StaticUserData.CurrentLocation.LocationId != locationInfo.CurrentLocation.LocationId || profileTournament == null || profileTournament.IsEnded)
			{
				if ((profileTournament == null || profileTournament.IsEnded) && !CompetitionMapHelper.CanJoin(t))
				{
					return;
				}
				bool flag = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemType == ItemTypes.Boat && x.Storage == StoragePlaces.Doll) != null;
				if (t.PrimaryScoring.FishOrigin != TournamentFishOrigin.Boat && (t.EquipmentAllowed == null || t.EquipmentAllowed.BoatTypes == null || t.EquipmentAllowed.BoatTypes.Length == 0) && flag)
				{
					UIHelper.ShowYesNo(ScriptLocalization.Get((!isCompetition) ? "UseKayakMessageTournament" : "UseKayakMessage"), onProceed, null, (!isCompetition) ? "EnterTournamentCaption" : "EnterCompetitionCaption", null, "CancelButton", null, null, null);
				}
				else
				{
					onProceed();
				}
			}
			else
			{
				TransferToLocation.Instance.ActivateGameView();
			}
		}

		public static void OpenRoomUserCompetition(UserCompetitionPublic u)
		{
			if (u == null)
			{
				LogHelper.Error("___kocha OpenRoomUserCompetition u == null", new object[0]);
				return;
			}
			UgcMenuStateManager ugcMenuManager = MenuHelpers.Instance.MenuPrefabsList.UgcMenuManager;
			ugcMenuManager.RoomUserCompetitionCtrl.ClearCurrent();
			ugcMenuManager.SetState(UgcMenuStateManager.UgcStates.Room);
			DashboardTabSetter.SwitchForms(FormsEnum.Sport, true);
			ugcMenuManager.SetActiveFormSport(UgcMenuStateManager.UgcStates.Room, true, false);
			if (ugcMenuManager.RoomUserCompetitionCtrl.TournamentId == null || ugcMenuManager.RoomUserCompetitionCtrl.TournamentId != u.TournamentId)
			{
				ugcMenuManager.RoomUserCompetitionCtrl.Init(u);
			}
		}

		private static void ShowingUgcDetails(UserCompetitionPublic ugc, Tournament t)
		{
			if (ugc != null)
			{
				if (!ugc.IsStarted && UserCompetitionHelper.IsUgcEnabled)
				{
					CompetitionMapHelper.OpenRoomUserCompetition(ugc);
				}
				else
				{
					TournamentHelper.ShowingTournamentDetails(ugc, false, true, null, false);
				}
			}
			else if (t != null)
			{
				TournamentHelper.ShowingTournamentDetails(new UserCompetition
				{
					TournamentId = t.TournamentId
				}, false, true, null, true);
			}
		}

		private static void AddEvents(GameObject messageBox, Action refreshFn)
		{
			ITournamentDetails component = messageBox.GetComponent<ITournamentDetails>();
			component.ApplyOnClick += delegate(object sender, EventArgs args)
			{
				refreshFn();
			};
			component.OnUnregister += delegate
			{
				refreshFn();
			};
		}
	}
}
