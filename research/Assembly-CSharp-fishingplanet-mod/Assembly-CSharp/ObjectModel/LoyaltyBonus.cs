using System;

namespace ObjectModel
{
	public class LoyaltyBonus
	{
		public int BonusId { get; set; }

		public int PlatformId { get; set; }

		public int RegionId { get; set; }

		public string Text { get; set; }

		public int IconBID { get; set; }

		public int OrderId { get; set; }

		public DateTime Start { get; set; }

		public DateTime Finish { get; set; }

		public TargetedAdSlideConfig Config { get; set; }

		public Reward Reward { get; set; }

		public bool IsActive { get; set; }

		public override string ToString()
		{
			return string.Format("LoyaltyBonus #{0} {1}, {2}", this.BonusId, this.Text, this.Config);
		}
	}
}
