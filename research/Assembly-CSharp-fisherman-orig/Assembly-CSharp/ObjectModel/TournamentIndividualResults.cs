using System;

namespace ObjectModel
{
	public class TournamentIndividualResults : TournamentResult
	{
		public int TournamentId { get; set; }

		public DateTime? Date { get; set; }

		public Guid UserId { get; set; }

		public string Name { get; set; }

		public int? AvatarBID { get; set; }

		public int LanguageId { get; set; }

		public int? ClubId { get; set; }

		public Guid? GroupId { get; set; }

		public double? PredictedScore { get; set; }

		public int? Rank { get; set; }

		public int? Rating { get; set; }

		public bool IsDisqualified { get; set; }

		public string Team { get; set; }

		public TournamentTeamResult TeamResult { get; set; }
	}
}
