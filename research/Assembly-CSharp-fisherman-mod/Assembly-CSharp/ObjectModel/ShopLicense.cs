using System;

namespace ObjectModel
{
	public class ShopLicense
	{
		public int LicenseId { get; set; }

		public int StateId { get; set; }

		public int? PondId { get; set; }

		public int? OriginalMinLevel { get; set; }

		public int? MinLevel { get; set; }

		public string Name { get; set; }

		public string Desc { get; set; }

		public string Currency { get; set; }

		public double? Penalty { get; set; }

		public int? LogoBID { get; set; }

		public bool IsAdvanced { get; set; }

		[JsonConfig]
		public int[] LocationIds { get; set; }

		[JsonConfig]
		public FishLicenseConstraint[] IllegalFish { get; set; }

		[JsonConfig]
		public FishLicenseConstraint[] FreeFish { get; set; }

		[JsonConfig]
		public FishLicenseConstraint[] TakeFish { get; set; }

		public ShopLicenseCost[] Costs { get; set; }
	}
}
