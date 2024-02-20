using System;

namespace ObjectModel.Tournaments
{
	public class TournamentSecondaryResult
	{
		public int TournamentId { get; set; }

		public Guid UserId { get; set; }

		public int RewardType { get; set; }

		public double Score { get; set; }

		public int? SecondaryRewardFishId;

		public string SecondaryRewardFishName;
	}
}
