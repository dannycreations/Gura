using System;

namespace ObjectModel
{
	public class ShopLicenseCost
	{
		public int Term { get; set; }

		public string Currency { get; set; }

		public float ResidentCost { get; set; }

		public float NotResidentCost { get; set; }

		public float? DiscountResidentCost { get; set; }

		public float? DiscountNonResidentCost { get; set; }
	}
}
