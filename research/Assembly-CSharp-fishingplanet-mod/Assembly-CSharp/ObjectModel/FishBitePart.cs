using System;

namespace ObjectModel
{
	public class FishBitePart
	{
		public int Amount { get; set; }

		public FishBitePartElement[] Elements { get; set; }

		public float MaxBiteTime { get; set; }
	}
}
