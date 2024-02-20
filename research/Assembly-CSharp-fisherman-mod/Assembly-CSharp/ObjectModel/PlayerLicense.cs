using System;

namespace ObjectModel
{
	public class PlayerLicense
	{
		[JsonConfig]
		[NoClone(true)]
		public int LicenseId { get; set; }

		[JsonConfig]
		[NoClone(true)]
		public Guid InstanceId { get; set; }

		[JsonConfig]
		public int StateId { get; set; }

		[JsonConfig]
		public int? PondId { get; set; }

		[JsonConfig]
		public int? MinLevel { get; set; }

		[JsonConfig]
		public string Name { get; set; }

		[JsonConfig]
		public string Desc { get; set; }

		[JsonConfig]
		public string Currency { get; set; }

		[JsonConfig]
		public double? Cost { get; set; }

		[JsonConfig]
		public string PenaltyCurrency { get; set; }

		[JsonConfig]
		public double? Penalty { get; set; }

		[JsonConfig]
		public int[] LocationIds { get; set; }

		[JsonConfig]
		public FishLicenseConstraint[] IllegalFish { get; set; }

		[JsonConfig]
		public FishLicenseConstraint[] FreeFish { get; set; }

		[JsonConfig]
		public FishLicenseConstraint[] TakeFish { get; set; }

		[JsonConfig]
		[NoClone(true)]
		public int Term { get; set; }

		[JsonConfig]
		[NoClone(true)]
		public DateTime? Start { get; set; }

		[JsonConfig]
		[NoClone(true)]
		public DateTime? End { get; set; }

		[JsonConfig]
		[NoClone(true)]
		public bool CanExpire { get; set; }

		[JsonConfig]
		public bool IsAdvanced { get; set; }
	}
}
