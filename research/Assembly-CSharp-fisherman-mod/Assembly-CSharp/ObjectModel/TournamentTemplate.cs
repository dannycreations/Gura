using System;
using System.Collections.Generic;
using ObjectModel.Tournaments;

namespace ObjectModel
{
	public class TournamentTemplate
	{
		public int TemplateId { get; set; }

		public string Name { get; set; }

		public string Desc { get; set; }

		public string Rules { get; set; }

		public int? LogoBID { get; set; }

		public int? ImageBID { get; set; }

		public int? SerieId { get; set; }

		public int KindId { get; set; }

		public string TournamentType { get; set; }

		public int? StageTypeId { get; set; }

		public int? CountryId { get; set; }

		public int PondId { get; set; }

		public int? MinParticipants { get; set; }

		public int? MaxParticipants { get; set; }

		public string ScheduleType { get; set; }

		public string DaysOfWeek { get; set; }

		public string DaysOfMonth { get; set; }

		public int? Month { get; set; }

		public int RegistrationDayOffset { get; set; }

		public int RegistrationStartHour { get; set; }

		public int RegistrationStartMinute { get; set; }

		public int StartHour { get; set; }

		public int StartMinute { get; set; }

		public int EndHour { get; set; }

		public int EndMinute { get; set; }

		public int? DurationDays { get; set; }

		public int? InGameStartHour { get; set; }

		public int InGameStartMinute { get; set; }

		public int InGameDuration { get; set; }

		public int InGameDurationMinute { get; set; }

		public double? EntranceFee { get; set; }

		public string Currency { get; set; }

		public int Rating { get; set; }

		public bool FreeForPremium { get; set; }

		public TournamentEquipment EquipmentAllowed { get; set; }

		public List<TournamentPrecondition> Preconditions { get; set; }

		public List<TournamentPlace> Places { get; set; }

		public TournamentScoring PrimaryScoring { get; set; }

		public TournamentScoring SecondaryScoring { get; set; }

		public int? MaxFishInCage { get; set; }

		public SecondaryReward[] SecondaryRewards { get; set; }

		public bool IsSynchronous { get; set; }

		public int WinnersCountAdmited2NextStage { get; set; }
	}
}
