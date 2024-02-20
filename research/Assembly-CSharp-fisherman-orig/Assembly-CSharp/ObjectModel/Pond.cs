using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(0)]
	public class Pond
	{
		public int PondId { get; set; }

		public Region Region { get; set; }

		public Country Country { get; set; }

		public State State { get; set; }

		public string Name { get; set; }

		public string Desc { get; set; }

		public int OriginalMinLevel { get; set; }

		public int MinLevel { get; set; }

		public DateTime? MinLevelExpiration { get; set; }

		public int MapBID { get; set; }

		public int PhotoBID { get; set; }

		public string Currency { get; set; }

		public float? TravelCost { get; set; }

		public float? StayFee { get; set; }

		public float? DiscountTravelCost { get; set; }

		public float? DiscountStayFee { get; set; }

		public DateTime? DiscountStart { get; set; }

		public DateTime? DiscountEnd { get; set; }

		public WeatherDesc[] Weather { get; set; }

		public PlayerLicense[] PlayerLicenses { get; set; }

		public double MapX { get; set; }

		public double MapY { get; set; }

		public double MapZ { get; set; }

		public string Asset { get; set; }

		public PondLicenseInfo[] PondLicenseBrief { get; set; }

		public bool IsActive { get; set; }

		public bool IsVisible { get; set; }

		public bool IsPaid { get; set; }

		public bool HasAction { get; set; }

		public BoxInfo[] HitchBoxes { get; set; }

		public int[] FishIds { get; set; }
	}
}
