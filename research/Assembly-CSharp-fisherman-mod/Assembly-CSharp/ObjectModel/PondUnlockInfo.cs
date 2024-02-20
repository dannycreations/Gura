using System;

namespace ObjectModel
{
	public class PondUnlockInfo
	{
		public int PondId { get; set; }

		public int MinLevel { get; set; }

		public int GoldCost { get; set; }

		public bool CanUse { get; set; }
	}
}
