using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class Boat : InventoryItem
	{
		[JsonConfig]
		public int SitsCount { get; set; }

		[JsonConfig]
		public string Material { get; set; }

		[JsonConfig]
		public float Velocity { get; set; }

		[JsonConfig]
		public float AccelertatedVelocity { get; set; }

		[JsonConfig]
		public float WeightModifier { get; set; }

		[JsonConfig]
		public float LongitudalResistanceMultiplier { get; set; }

		[JsonConfig]
		public float LateralResistanceMultiplier { get; set; }

		[JsonIgnore]
		protected override bool IsSellable
		{
			get
			{
				return base.Rent == null;
			}
		}
	}
}
