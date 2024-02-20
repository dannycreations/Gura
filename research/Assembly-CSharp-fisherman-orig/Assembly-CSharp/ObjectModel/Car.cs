using System;

namespace ObjectModel
{
	public class Car : InventoryItem
	{
		[JsonConfig]
		public float TravelCost { get; set; }

		[JsonConfig]
		public int Passability { get; set; }

		[JsonConfig]
		public int SitCount { get; set; }

		[JsonConfig]
		public int BoatTransportationClass { get; set; }

		[JsonConfig]
		public float MaxWeight { get; set; }

		[JsonConfig]
		public float ModeModifier { get; set; }
	}
}
