using System;

namespace ObjectModel
{
	public class UserCompetitionRevertMessage : UserCompetitionCancellationMessage
	{
		public Reward Reward { get; set; }
	}
}
