using System;

namespace ObjectModel
{
	public class UserCompetitionRuntime
	{
		public int TournamentId { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public bool IsStartRequested { get; set; }

		public bool IsStarted { get; set; }

		public bool IsEnded { get; set; }

		public bool IsCanceled { get; set; }

		public bool IsDeleted { get; set; }
	}
}
