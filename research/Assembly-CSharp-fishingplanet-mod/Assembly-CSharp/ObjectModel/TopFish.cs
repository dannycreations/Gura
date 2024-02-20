using System;

namespace ObjectModel
{
	public class TopFish : TopPlayerBase
	{
		public int FishId { get; set; }

		public string FishName { get; set; }

		public int FishCategoryId { get; set; }

		public string FishCategoryName { get; set; }

		public int Time { get; set; }

		public int WeatherIcon { get; set; }

		public int? PondId { get; set; }

		public string PondName { get; set; }

		public int BaitId { get; set; }

		public int BaitThumbnailBID { get; set; }

		public string BaitName { get; set; }

		public double Weight;

		public double Length;
	}
}
