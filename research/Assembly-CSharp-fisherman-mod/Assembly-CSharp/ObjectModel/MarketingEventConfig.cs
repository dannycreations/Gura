using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class MarketingEventConfig
	{
		public int? ActionFishCategory { get; set; }

		public string ActionFishCategoryName { get; set; }

		public bool? IsFireworkLaunchEnabled { get; set; }

		public int[] FireworkLaunchPonds { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public MarketingEventHoliday Holiday { get; set; }

		public int[] PondIds { get; set; }
	}
}
