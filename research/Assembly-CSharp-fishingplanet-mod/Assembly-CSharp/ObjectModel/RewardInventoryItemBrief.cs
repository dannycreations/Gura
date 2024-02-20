using System;

namespace ObjectModel
{
	public class RewardInventoryItemBrief
	{
		public int ItemId { get; set; }

		public virtual string Name { get; set; }

		public virtual int? ThumbnailBID { get; set; }
	}
}
