using System;

namespace ObjectModel
{
	public class BoatPriceDesc
	{
		public int PondId { get; set; }

		public string Currency { get; set; }

		public int PricePerDay { get; set; }

		public int? PricePerWeek { get; set; }

		public int? PricePerHour { get; set; }
	}
}
