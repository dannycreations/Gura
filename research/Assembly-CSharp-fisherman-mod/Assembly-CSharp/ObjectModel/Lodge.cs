using System;

namespace ObjectModel
{
	public class Lodge : InventoryItem
	{
		[JsonConfig]
		public int PondId { get; set; }

		[JsonConfig]
		public float StayCost { get; set; }

		[JsonConfig]
		public int Capacity { get; set; }

		[JsonConfig]
		public float MaxWeight { get; set; }

		[JsonConfig]
		public float ModeModifier { get; set; }

		[JsonConfig]
		public bool HasBerth { get; set; }
	}
}
