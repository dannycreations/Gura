using System;

namespace ObjectModel
{
	public class Attractant : InventoryItem
	{
		[JsonConfig]
		public int Capacity { get; set; }

		[JsonConfig]
		public BaitAttraction Attraction { get; set; }
	}
}
