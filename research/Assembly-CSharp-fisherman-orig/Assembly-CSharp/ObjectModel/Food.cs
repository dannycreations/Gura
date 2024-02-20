using System;

namespace ObjectModel
{
	public class Food : InventoryItem
	{
		[JsonConfig]
		public int PotionCount { get; set; }

		[JsonConfig]
		public float ForceRestoration { get; set; }

		[JsonConfig]
		public float ModeRestoration { get; set; }
	}
}
