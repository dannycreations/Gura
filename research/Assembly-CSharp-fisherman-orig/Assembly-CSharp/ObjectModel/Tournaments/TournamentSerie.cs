using System;

namespace ObjectModel.Tournaments
{
	public class TournamentSerie
	{
		public int SerieId { get; set; }

		public string Name { get; set; }

		public int? ImageBID { get; set; }

		public int? LogoBID { get; set; }

		public string Desc { get; set; }

		public string Rules { get; set; }

		public string Terms { get; set; }

		public double? EntranceFee { get; set; }

		public string Currency { get; set; }

		public string Reward { get; set; }
	}
}
