using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel.Tournaments
{
	public class SecondaryReward
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public SecondaryRewardType RewardType { get; set; }

		public bool ScoredFishOnly { get; set; }

		public string RewardName { get; set; }
	}
}
