using System;

namespace ObjectModel
{
	public class MetadataForUserCompetitionPond : PondBrief
	{
		public int MinEntranceFee { get; set; }

		public int MaxEntranceFee { get; set; }

		public int MinHostEntranceFee { get; set; }

		public int MaxHostEntranceFee { get; set; }

		public float InitPrizePoolToEntranceFeeKoef { get; set; }

		public int MinEntranceFeeGold { get; set; }

		public int MaxEntranceFeeGold { get; set; }

		public int MinHostEntranceFeeGold { get; set; }

		public int MaxHostEntranceFeeGold { get; set; }
	}
}
