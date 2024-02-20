using System;
using ObjectModel.Tournaments;

namespace ObjectModel
{
	public abstract class TournamentBase
	{
		public int TemplateId { get; set; }

		public int TournamentId { get; set; }

		public int? SerieInstanceId { get; set; }

		public string Name { get; set; }

		public string Desc { get; set; }

		public string Rules { get; set; }

		public int? LogoBID { get; set; }

		public int? ImageBID { get; set; }

		public int KindId { get; set; }

		public string TournamentType { get; set; }

		public int? StageTypeId { get; set; }

		public int? CountryId { get; set; }

		public int PondId { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public int? InGameStartHour { get; set; }

		public int InGameStartMinute { get; set; }

		public int InGameDuration { get; set; }

		public int InGameDurationMinute { get; set; }

		public bool IsStarted { get; set; }

		public bool IsEnded { get; set; }

		public TournamentEquipment EquipmentAllowed { get; set; }

		public TournamentScoring PrimaryScoring { get; set; }

		public TournamentScoring SecondaryScoring { get; set; }

		public int? MaxFishInCage { get; set; }

		public SecondaryReward[] SecondaryRewards { get; set; }

		public bool IsSynchronous { get; set; }

		public int WinnersCountAdmited2NextStage { get; set; }
	}
}
