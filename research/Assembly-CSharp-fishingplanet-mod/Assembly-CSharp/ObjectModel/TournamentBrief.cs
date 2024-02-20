using System;

namespace ObjectModel
{
	public class TournamentBrief
	{
		public int TournamentId { get; set; }

		public int TemplateId { get; set; }

		public string Name { get; set; }

		public int? LogoBID { get; set; }

		public bool IsStarted { get; set; }

		public DateTime EndDate { get; set; }
	}
}
