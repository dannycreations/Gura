using System;

namespace ObjectModel
{
	public class TopTournamentPlayers : TopPlayerBase
	{
		public int Title { get; set; }

		public int Rating { get; set; }

		public int CompetitionRank { get; set; }

		public int CompetitionRankChange { get; set; }

		public int Played { get; set; }

		public int Won { get; set; }
	}
}
