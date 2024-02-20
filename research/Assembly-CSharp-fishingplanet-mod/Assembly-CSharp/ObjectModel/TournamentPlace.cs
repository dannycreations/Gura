using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class TournamentPlace
	{
		public int PlaceId { get; set; }

		public int Count { get; set; }

		public string RewardName { get; set; }

		[JsonIgnore]
		public Reward Reward { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentTitles TitleGiven { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public TournamentTitles TitleProlonged { get; set; }

		public bool IsRated { get; set; }
	}
}
