using System;

namespace ObjectModel
{
	public class FishLicenseConstraint
	{
		[JsonConfig]
		public int FishId { get; set; }

		[JsonConfig]
		public string Name { get; set; }

		[JsonConfig]
		public float? MinWeight { get; set; }

		[JsonConfig]
		public float? MaxWeight { get; set; }
	}
}
