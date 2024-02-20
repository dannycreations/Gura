using System;

namespace ObjectModel
{
	public class BaitAttraction
	{
		public int FishId { get; set; }

		[JsonConfig]
		public string FishCode { get; set; }

		[JsonConfig]
		public int Attraction { get; set; }
	}
}
