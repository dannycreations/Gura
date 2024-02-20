using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public class EndTournamentSellFishInfo
	{
		public int FishInCageCount { get; set; }

		public bool FishWasSold { get; set; }

		public int? FishGold { get; set; }

		public int? FishSilver { get; set; }

		public List<CaughtFish> FishList { get; set; }
	}
}
