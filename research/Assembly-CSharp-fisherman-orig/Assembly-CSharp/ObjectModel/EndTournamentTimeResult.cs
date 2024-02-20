using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public class EndTournamentTimeResult
	{
		public bool Prematurely { get; set; }

		public ProfileTournament Tournament { get; set; }

		public TournamentIndividualResults TournamentResult { get; set; }

		public List<FishRef> CatchStats { get; set; }

		public EndTournamentSellFishInfo SellFishInfo { get; set; }
	}
}
