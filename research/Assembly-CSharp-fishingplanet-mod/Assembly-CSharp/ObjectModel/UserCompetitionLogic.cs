using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel.Tournaments;

namespace ObjectModel
{
	public static class UserCompetitionLogic
	{
		public static bool IsCompetition(this TournamentBase t)
		{
			return t != null && (t.KindId == 3 || t.IsUgc());
		}

		public static bool IsUgc(this TournamentBase t)
		{
			return t.KindId == 4;
		}

		public static bool CanCreateUserCompetition(this Profile profile)
		{
			return profile.IsUgcHost != false;
		}

		public static bool CanCreateUserCompetitionSponsored(this Profile profile)
		{
			return profile.IsInfluencer == true;
		}

		public static bool IsUgcChatChannelName(string channel)
		{
			return channel != null && channel.StartsWith("ugc");
		}

		public static bool IsUgcChatChannelName(string channel, int tournamentId)
		{
			return channel == "ugc" + tournamentId;
		}

		public static string ChatChannelName(int tournamentId)
		{
			return "ugc" + tournamentId;
		}

		public static string ChatChannelName(this UserCompetitionPublic competition)
		{
			return "ugc" + competition.TournamentId;
		}

		public static double PrizePoolWithoutComission(this UserCompetitionPublic competition, int playersParticipated)
		{
			double? entranceFee = competition.EntranceFee;
			int num = playersParticipated * (int)((entranceFee == null) ? 0.0 : entranceFee.Value);
			double? hostEntranceFee = competition.HostEntranceFee;
			int num2 = num + (int)((hostEntranceFee == null) ? 0.0 : hostEntranceFee.Value);
			return (double)((int)((double)num2 * (double)(100 - competition.ComissionPct) / 100.0));
		}

		public static double HostEntranceFeeWithComission(this UserCompetitionPublic competition)
		{
			double? hostEntranceFee = competition.HostEntranceFee;
			return (hostEntranceFee == null) ? 0.0 : hostEntranceFee.Value;
		}

		public static double EntranceFeeWithComission(this UserCompetitionPublic competition)
		{
			double? entranceFee = competition.EntranceFee;
			return (entranceFee == null) ? 0.0 : entranceFee.Value;
		}

		public static void TranslateTournamentEquipment(this UserCompetitionRodEquipmentAllowed rodEquipment, TournamentRodEquipment tournamentEquipment)
		{
			switch (rodEquipment)
			{
			case UserCompetitionRodEquipmentAllowed.JigHeadsAndSoftBaits:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[]
				{
					ItemSubTypes.CommonJigHeads,
					ItemSubTypes.BarblessJigHeads
				};
				break;
			case UserCompetitionRodEquipmentAllowed.BassJigs:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[] { ItemSubTypes.BassJig };
				break;
			case UserCompetitionRodEquipmentAllowed.Crankbaits:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[] { ItemSubTypes.Cranckbait };
				break;
			case UserCompetitionRodEquipmentAllowed.TopwaterLures:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[]
				{
					ItemSubTypes.Frog,
					ItemSubTypes.Walker,
					ItemSubTypes.Popper
				};
				break;
			case UserCompetitionRodEquipmentAllowed.Jerkbaits:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[] { ItemSubTypes.Jerkbait };
				break;
			case UserCompetitionRodEquipmentAllowed.Minnows:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[] { ItemSubTypes.Minnow };
				break;
			case UserCompetitionRodEquipmentAllowed.Spoons:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[]
				{
					ItemSubTypes.Spoon,
					ItemSubTypes.BarblessSpoons
				};
				break;
			case UserCompetitionRodEquipmentAllowed.Spinners:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[]
				{
					ItemSubTypes.Spinner,
					ItemSubTypes.BarblessSpinners
				};
				break;
			case UserCompetitionRodEquipmentAllowed.Spinnerbaits:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[] { ItemSubTypes.Spinnerbait };
				break;
			case UserCompetitionRodEquipmentAllowed.SpinnerbaitsBuzzbaits:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[]
				{
					ItemSubTypes.Spinnerbait,
					ItemSubTypes.BuzzBait
				};
				break;
			case UserCompetitionRodEquipmentAllowed.Swimbait:
				tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[] { ItemSubTypes.Swimbait };
				break;
			default:
				switch (rodEquipment)
				{
				case UserCompetitionRodEquipmentAllowed.CommonBaits_Float:
					tournamentEquipment.RodTypes = new ItemSubTypes[]
					{
						ItemSubTypes.TelescopicRod,
						ItemSubTypes.MatchRod
					};
					tournamentEquipment.BaitTypes = new ItemSubTypes[] { ItemSubTypes.CommonBait };
					break;
				case UserCompetitionRodEquipmentAllowed.InsectWormBaits_Float:
					tournamentEquipment.RodTypes = new ItemSubTypes[]
					{
						ItemSubTypes.TelescopicRod,
						ItemSubTypes.MatchRod
					};
					tournamentEquipment.BaitTypes = new ItemSubTypes[] { ItemSubTypes.InsectsWormBait };
					break;
				case UserCompetitionRodEquipmentAllowed.FreshBaits_Float:
					tournamentEquipment.RodTypes = new ItemSubTypes[]
					{
						ItemSubTypes.TelescopicRod,
						ItemSubTypes.MatchRod
					};
					tournamentEquipment.BaitTypes = new ItemSubTypes[] { ItemSubTypes.FreshBait };
					break;
				case UserCompetitionRodEquipmentAllowed.CommonBaits_BottomOrFeeder:
					tournamentEquipment.RodTypes = new ItemSubTypes[]
					{
						ItemSubTypes.FeederRod,
						ItemSubTypes.BottomRod
					};
					tournamentEquipment.BaitTypes = new ItemSubTypes[] { ItemSubTypes.CommonBait };
					break;
				case UserCompetitionRodEquipmentAllowed.InsectWormBaits_BottomOrFeeder:
					tournamentEquipment.RodTypes = new ItemSubTypes[]
					{
						ItemSubTypes.FeederRod,
						ItemSubTypes.BottomRod
					};
					tournamentEquipment.BaitTypes = new ItemSubTypes[] { ItemSubTypes.InsectsWormBait };
					break;
				case UserCompetitionRodEquipmentAllowed.FreshBaits_BottomOrFeeder:
					tournamentEquipment.RodTypes = new ItemSubTypes[]
					{
						ItemSubTypes.FeederRod,
						ItemSubTypes.BottomRod
					};
					tournamentEquipment.BaitTypes = new ItemSubTypes[] { ItemSubTypes.FreshBait };
					break;
				case UserCompetitionRodEquipmentAllowed.BoilesAndPellets:
					tournamentEquipment.BaitTypes = new ItemSubTypes[] { ItemSubTypes.BoilBait };
					break;
				default:
					switch (rodEquipment)
					{
					case UserCompetitionRodEquipmentAllowed.CarolinaRigs:
						tournamentEquipment.LeaderTypes = new ItemSubTypes[] { ItemSubTypes.CarolinaRig };
						break;
					case UserCompetitionRodEquipmentAllowed.TexasRigs:
						tournamentEquipment.LeaderTypes = new ItemSubTypes[] { ItemSubTypes.TexasRig };
						break;
					case UserCompetitionRodEquipmentAllowed.ThreewayRigs:
						tournamentEquipment.LeaderTypes = new ItemSubTypes[] { ItemSubTypes.ThreewayRig };
						break;
					case UserCompetitionRodEquipmentAllowed.OffsetHookAndSoftBaits:
						tournamentEquipment.RodBuilds = new RodTemplate[] { RodTemplate.OffsetJig };
						tournamentEquipment.HookTypes = new ItemSubTypes[] { ItemSubTypes.OffsetHook };
						tournamentEquipment.BaitTypes = new ItemSubTypes[]
						{
							ItemSubTypes.Craw,
							ItemSubTypes.Worm,
							ItemSubTypes.Grub,
							ItemSubTypes.Shad,
							ItemSubTypes.Tube,
							ItemSubTypes.Frog,
							ItemSubTypes.Slug
						};
						break;
					case UserCompetitionRodEquipmentAllowed.BassJigsSpinnerbaitsAndSoftBaits:
						tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[]
						{
							ItemSubTypes.BassJig,
							ItemSubTypes.Spinnerbait
						};
						tournamentEquipment.BaitTypes = new ItemSubTypes[]
						{
							ItemSubTypes.Craw,
							ItemSubTypes.Worm,
							ItemSubTypes.Grub,
							ItemSubTypes.Shad,
							ItemSubTypes.Tube,
							ItemSubTypes.Frog,
							ItemSubTypes.Slug
						};
						break;
					case UserCompetitionRodEquipmentAllowed.SpinnersSpinnerbaitsAndTails:
						tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[]
						{
							ItemSubTypes.Spinner,
							ItemSubTypes.BarblessSpinners,
							ItemSubTypes.Spinnerbait
						};
						tournamentEquipment.BaitTypes = new ItemSubTypes[] { ItemSubTypes.Tail };
						break;
					case UserCompetitionRodEquipmentAllowed.SpinnersSpinnerbaitsBuzzbaitsAndTails:
						tournamentEquipment.TerminalTackleTypes = new ItemSubTypes[]
						{
							ItemSubTypes.Spinner,
							ItemSubTypes.BarblessSpinners,
							ItemSubTypes.Spinnerbait,
							ItemSubTypes.BuzzBait
						};
						tournamentEquipment.BaitTypes = new ItemSubTypes[] { ItemSubTypes.Tail };
						break;
					}
					break;
				}
				break;
			}
		}

		public static bool HasCorrespondingRod(this Profile profile, UserCompetitionRodEquipmentAllowed rodEquipment)
		{
			TournamentRodEquipment tournamentRodEquipment = new TournamentRodEquipment();
			rodEquipment.TranslateTournamentEquipment(tournamentRodEquipment);
			foreach (Rod rod in from r in profile.Inventory.OfType<Rod>()
				where r.IsRodOnDoll
				select r)
			{
				RodTemplate rodTemplate = profile.Inventory.GetRodTemplate(rod);
				if (profile.RodMatchesTournamentSingleRod(rod, rodTemplate, tournamentRodEquipment) == null)
				{
					return true;
				}
			}
			return false;
		}

		public static string GetTournamentType(this UserCompetitionPublic competition)
		{
			string text = "-";
			UserCompetitionFormat format = competition.Format;
			if (format != UserCompetitionFormat.Individual)
			{
				if (format != UserCompetitionFormat.Duel)
				{
					if (format == UserCompetitionFormat.Team)
					{
						text = "T";
					}
				}
				else
				{
					text = "D";
				}
			}
			else
			{
				text = "I";
			}
			return string.Format("{0}{1}{2}", text, (!competition.IsSponsored) ? "-" : "S", (competition.Type != UserCompetitionType.Predefined) ? "C" : "T");
		}

		public static bool IsCustomCompetition(string tournamentType)
		{
			return tournamentType != null && tournamentType.Length >= 3 && tournamentType[2] == 'C';
		}

		public static bool IsTeamCompetition(string tournamentType)
		{
			return tournamentType != null && tournamentType.Length >= 3 && tournamentType[0] == 'T';
		}

		public static bool IsDuelCompetition(string tournamentType)
		{
			return tournamentType != null && tournamentType.Length >= 3 && tournamentType[0] == 'D';
		}

		public static void FillCompetitionPropertiesPlayers(this UserCompetitionPublic competition, Profile profile)
		{
			UserCompetitionPlayer userCompetitionPlayer = competition.Players.FirstOrDefault((UserCompetitionPlayer p) => p.UserId == profile.UserId);
			competition.IsRegistered = userCompetitionPlayer != null;
			competition.IsApproved = userCompetitionPlayer != null && userCompetitionPlayer.IsApproved;
			competition.IsDone = userCompetitionPlayer != null && userCompetitionPlayer.IsDone;
			competition.IsDisqualified = userCompetitionPlayer != null && userCompetitionPlayer.IsDisqualified;
			competition.RegistrationsCount = competition.Players.Count;
			competition.ApprovedCount = competition.Players.Count((UserCompetitionPlayer p) => p.IsApproved);
			int num;
			if (competition.IsStarted)
			{
				num = competition.Players.Count((UserCompetitionPlayer p) => p.IsStarted && !p.IsDone);
			}
			else
			{
				num = 0;
			}
			competition.PlayingCount = num;
			int num2;
			if (competition.IsStarted)
			{
				num2 = competition.Players.Count((UserCompetitionPlayer p) => p.IsDone);
			}
			else
			{
				num2 = 0;
			}
			competition.FinishedCount = num2;
		}

		public static Reward CalculateRewardForPlayer(this UserCompetitionPublic competition, Reward[] rewards, TournamentFinalResult result)
		{
			UserCompetitionFormat format = competition.Format;
			if (format != UserCompetitionFormat.Individual && format != UserCompetitionFormat.Duel)
			{
				if (format != UserCompetitionFormat.Team)
				{
					return null;
				}
				if (result.CurrentPlayerResult.TeamPlace == null)
				{
					return null;
				}
				int teamPlace = result.CurrentPlayerResult.TeamPlace.Value;
				if (teamPlace > rewards.Length)
				{
					return null;
				}
				int num = result.Winners.Count((PlayerFinalResult w) => w.TeamPlace == teamPlace);
				Reward[] array = rewards.Skip(teamPlace - 1).Take(num).ToArray<Reward>();
				return array.FirstOrDefault<Reward>();
			}
			else
			{
				if (result.CurrentPlayerResult.Place == null)
				{
					return null;
				}
				int place = result.CurrentPlayerResult.Place.Value;
				if (place > rewards.Length)
				{
					return null;
				}
				int num2 = result.Winners.Count((PlayerFinalResult w) => w.Place == place);
				Reward[] array2 = rewards.Skip(place - 1).Take(num2).ToArray<Reward>();
				return array2.FirstOrDefault<Reward>();
			}
		}

		private static int[] GenerateRewardDistribution(UserCompetitionPublic competition, double sum)
		{
			int[] array = null;
			UserCompetitionFormat format = competition.Format;
			if (format != UserCompetitionFormat.Individual)
			{
				if (format != UserCompetitionFormat.Team)
				{
					if (format == UserCompetitionFormat.Duel)
					{
						array = new int[1];
						UserCompetitionRewardScheme rewardScheme = competition.RewardScheme;
						if (rewardScheme != UserCompetitionRewardScheme.Duel_AllForWinner)
						{
						}
						array[0] = (int)sum;
					}
				}
				else
				{
					array = new int[1];
					UserCompetitionRewardScheme rewardScheme2 = competition.RewardScheme;
					if (rewardScheme2 != UserCompetitionRewardScheme.Team1_EqualEachOther)
					{
					}
					array[0] = (int)sum;
				}
			}
			else
			{
				array = new int[3];
				switch (competition.RewardScheme)
				{
				default:
					array[0] = (int)sum;
					array[1] = 0;
					array[2] = 0;
					break;
				case UserCompetitionRewardScheme.Individual1_50_35_15:
					array[0] = (int)(0.5 * sum);
					array[1] = (int)(0.35 * sum);
					array[2] = (int)(0.15 * sum);
					break;
				case UserCompetitionRewardScheme.Individual2_60_30_10:
					array[0] = (int)(0.6 * sum);
					array[1] = (int)(0.3 * sum);
					array[2] = (int)(0.1 * sum);
					break;
				case UserCompetitionRewardScheme.Individual3_80_15_5:
					array[0] = (int)(0.8 * sum);
					array[1] = (int)(0.15 * sum);
					array[2] = (int)(0.05 * sum);
					break;
				}
			}
			return array;
		}

		public static Reward CalculateRewardForPlayer(this UserCompetitionPublic competition, double sum, TournamentFinalResult result)
		{
			UserCompetitionFormat format = competition.Format;
			if (format != UserCompetitionFormat.Individual && format != UserCompetitionFormat.Duel)
			{
				if (format != UserCompetitionFormat.Team)
				{
					return null;
				}
				if (result.CurrentPlayerResult.TeamPlace == null)
				{
					return null;
				}
				int[] array = UserCompetitionLogic.GenerateRewardDistribution(competition, sum);
				int teamPlace = result.CurrentPlayerResult.TeamPlace.Value;
				if (teamPlace > array.Length)
				{
					return null;
				}
				int num = result.Winners.Count((PlayerFinalResult w) => w.TeamPlace == teamPlace);
				int[] array2 = array.Skip(teamPlace - 1).Take(num).ToArray<int>();
				int num2 = array2.Sum();
				return new Reward
				{
					Currency1 = competition.Currency,
					Money1 = new double?((double)((int)((double)num2 / (double)num)))
				};
			}
			else
			{
				if (result.CurrentPlayerResult.Place == null)
				{
					return null;
				}
				int[] array3 = UserCompetitionLogic.GenerateRewardDistribution(competition, sum);
				int place = result.CurrentPlayerResult.Place.Value;
				if (place > array3.Length)
				{
					return null;
				}
				int num3 = result.Winners.Count((PlayerFinalResult w) => w.Place == place);
				int[] array4 = array3.Skip(place - 1).Take(num3).ToArray<int>();
				int num4 = array4.Sum();
				return new Reward
				{
					Currency1 = competition.Currency,
					Money1 = new double?((double)((int)((double)num4 / (double)num3)))
				};
			}
		}

		public static Reward CalculateRewardForPlayer(this UserCompetitionPublic competition, double sum, int? place, int? teamPlace, int? teamWinnersCount)
		{
			TournamentFinalResult tournamentFinalResult = new TournamentFinalResult
			{
				CurrentPlayerResult = new PlayerFinalResult
				{
					Id = Guid.Empty.ToString(),
					Place = place,
					TeamPlace = teamPlace
				}
			};
			List<PlayerFinalResult> list = new List<PlayerFinalResult>();
			for (int i = 0; i < ((teamWinnersCount == null) ? 1 : teamWinnersCount.Value); i++)
			{
				list.Add(tournamentFinalResult.CurrentPlayerResult);
			}
			tournamentFinalResult.Winners = list.ToArray();
			return competition.CalculateRewardForPlayer(sum, tournamentFinalResult);
		}

		public static Reward CalculateRewardForPlayer(this UserCompetitionPublic competition, Reward[] rewards, int? place, int? teamPlace, int? teamWinnersCount)
		{
			TournamentFinalResult tournamentFinalResult = new TournamentFinalResult
			{
				CurrentPlayerResult = new PlayerFinalResult
				{
					Id = Guid.Empty.ToString(),
					Place = place,
					TeamPlace = teamPlace
				}
			};
			List<PlayerFinalResult> list = new List<PlayerFinalResult>();
			for (int i = 0; i < ((teamWinnersCount == null) ? 1 : teamWinnersCount.Value); i++)
			{
				list.Add(tournamentFinalResult.CurrentPlayerResult);
			}
			tournamentFinalResult.Winners = list.ToArray();
			return competition.CalculateRewardForPlayer(rewards, tournamentFinalResult);
		}
	}
}
