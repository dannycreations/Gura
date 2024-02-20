using System;

namespace ObjectModel
{
	public class UserCompetition : UserCompetitionPublic
	{
		public int? TournamentTemplateId { get; set; }

		public int? RealDurationInSeconds { get; set; }

		public string Password { get; set; }

		public DateTime RegistrationStart { get; set; }

		public int? PromotionInterval { get; set; }

		public string PromotionType { get; set; }

		public string PromotionMessage { get; set; }

		public bool? IsPromotionDisabled { get; set; }

		public DateTime? LastPromotionTime { get; set; }

		public const string TeamRed = "Red";

		public const string TeamBlue = "Blue";
	}
}
