using System;

namespace ObjectModel
{
	public class PondLicenseInfo
	{
		[JsonConfig]
		public int FishCategoryId { get; set; }

		[JsonConfig]
		public int[] TakeLicenses { get; set; }

		[JsonConfig]
		public int[] ReleaseLicenses { get; set; }
	}
}
