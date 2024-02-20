using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public class Tournament : TournamentBase
	{
		public int? SerieId { get; set; }

		public int? MinParticipants { get; set; }

		public int? MaxParticipants { get; set; }

		public DateTime RegistrationStart { get; set; }

		public double? EntranceFee { get; set; }

		public string Currency { get; set; }

		public int Rating { get; set; }

		public bool IsCanceled { get; set; }

		public bool IsRegistered { get; set; }

		public bool IsApproved { get; set; }

		public bool IsDone { get; set; }

		public bool IsDisqualified { get; set; }

		public bool IsActive { get; set; }

		public bool FreeForPremium { get; set; }

		public int RegistrationsCount { get; set; }

		public int PlayingCount { get; set; }

		public int FinishedCount { get; set; }

		public int? Place { get; set; }

		public List<TournamentPrecondition> Preconditions { get; set; }

		public List<TournamentPlace> Places { get; set; }

		public bool IsSponsored { get; set; }

		public string NameCustom { get; set; }
	}
}
