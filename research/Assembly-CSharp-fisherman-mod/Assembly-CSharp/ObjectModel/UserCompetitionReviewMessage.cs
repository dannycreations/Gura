using System;

namespace ObjectModel
{
	public class UserCompetitionReviewMessage : UserCompetitionMessageBase
	{
		public DateTime? ReviewDate { get; set; }

		public string ReviewComment { get; set; }
	}
}
