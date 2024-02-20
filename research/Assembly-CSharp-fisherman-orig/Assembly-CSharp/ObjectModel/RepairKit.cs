using System;

namespace ObjectModel
{
	public class RepairKit : InventoryItem
	{
		[JsonConfig]
		public bool FullRepair { get; set; }

		[JsonConfig]
		public int DamageRestored { get; set; }
	}
}
