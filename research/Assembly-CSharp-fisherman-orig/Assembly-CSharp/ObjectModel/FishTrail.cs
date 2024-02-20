using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class FishTrail
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public FishAiBehavior Behavior { get; set; }

		public float Probability { get; set; }

		public float? Duration { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public FishBehaviorDirection? Direction { get; set; }
	}
}
