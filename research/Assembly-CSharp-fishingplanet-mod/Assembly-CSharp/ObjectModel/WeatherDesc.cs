using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject]
	public class WeatherDesc
	{
		public int PondId { get; set; }

		[JsonConfig]
		public DateTime Date { get; set; }

		[JsonConfig]
		public string TimeOfDay { get; set; }

		[JsonConfig]
		public int Icon { get; set; }

		[JsonConfig]
		public string Name { get; set; }

		[JsonConfig]
		public int AirTemperature { get; set; }

		[JsonConfig]
		public int WaterTemperature { get; set; }

		[JsonConfig]
		public string Pressure { get; set; }

		[JsonConfig]
		public float WindSpeed { get; set; }

		[JsonConfig]
		public string WindDirection { get; set; }

		[JsonConfig]
		public string Precipitation { get; set; }

		[JsonConfig]
		public string FishPlayFreq { get; set; }

		[JsonConfig]
		public string Sky { get; set; }

		[JsonConfig]
		public int? FishingDiagramImageId { get; set; }

		[JsonConfig]
		public string FishingDiagramDesc { get; set; }
	}
}
