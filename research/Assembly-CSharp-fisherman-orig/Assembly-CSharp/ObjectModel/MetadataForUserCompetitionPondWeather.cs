using System;

namespace ObjectModel
{
	public class MetadataForUserCompetitionPondWeather
	{
		public int PondId { get; set; }

		public string WeatherName { get; set; }

		public MetadataForUserCompetitionPondWeather.DayWeather[] DayWeathers { get; set; }

		public class DayWeather : WeatherDesc
		{
			public int Hour { get; set; }
		}
	}
}
