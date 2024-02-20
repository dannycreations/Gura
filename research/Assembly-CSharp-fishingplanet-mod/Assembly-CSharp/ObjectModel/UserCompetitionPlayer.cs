using System;

namespace ObjectModel
{
	public class UserCompetitionPlayer
	{
		public Guid UserId { get; set; }

		public int LanguageId { get; set; }

		public string Name { get; set; }

		public int Level { get; set; }

		public bool IsApproved { get; set; }

		public string Team { get; set; }

		public bool IsLocked { get; set; }

		public bool IsStarted { get; set; }

		public bool IsDone { get; set; }

		public bool IsDisqualified { get; set; }
	}
}
