using System;

namespace ObjectModel
{
	public class LevelLockRemoval
	{
		public DateTime StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public int[] Ponds { get; set; }

		public int? AccessibleLevel { get; set; }

		public LevelLockRemovalType Type
		{
			get
			{
				if (this.EndDate == null)
				{
					return LevelLockRemovalType.Unlock;
				}
				if (this.AccessibleLevel == null)
				{
					return LevelLockRemovalType.Tournament;
				}
				return LevelLockRemovalType.Product;
			}
		}
	}
}
