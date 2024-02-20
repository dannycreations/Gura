using System;

namespace ObjectModel
{
	public class ProfileTournament : TournamentBase
	{
		public TimeSpan? StartTime { get; set; }

		public TimeSpan? EndTime { get; set; }

		public double? Score { get; set; }

		public double? SecondaryScore { get; set; }

		public int FishCount { get; set; }

		public int TotalFishCount { get; set; }

		public string Team { get; set; }

		public UserCompetitionPublic UserCompetition { get; set; }
	}
}
