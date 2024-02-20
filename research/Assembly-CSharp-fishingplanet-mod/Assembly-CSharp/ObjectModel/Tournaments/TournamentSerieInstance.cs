using System;

namespace ObjectModel.Tournaments
{
	public class TournamentSerieInstance : TournamentSerie
	{
		public int SerieInstanceId { get; set; }

		public DateTime RegistrationStart { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public Tournament[] Stages { get; set; }
	}
}
