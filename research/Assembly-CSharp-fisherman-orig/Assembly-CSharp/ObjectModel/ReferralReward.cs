using System;

namespace ObjectModel
{
	public class ReferralReward
	{
		public Reward StartPlayInvitedReward { get; set; }

		public Reward StartPlayInvitingReward { get; set; }

		public Reward FinalInvitedReward { get; set; }

		public Reward FinalInvitingReward { get; set; }

		public int FinalRewardLevel { get; set; }
	}
}
