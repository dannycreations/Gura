using System;

namespace ObjectModel
{
	public class TournamentTeamResult
	{
		public string Team { get; set; }

		public double? Score { get; set; }

		public double? SecondaryScore { get; set; }

		public int? Place { get; set; }

		public int FishCount { get; set; }

		public TournamentIndividualResults[] IndividualResults { get; set; }
	}
}
