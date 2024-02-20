using System;

namespace ObjectModel
{
	public class UserCompetitionStartMessage : UserCompetitionMessageBase
	{
		public int? ImageBID { get; set; }

		public DateTime EndDate { get; set; }
	}
}
