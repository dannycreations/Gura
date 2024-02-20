using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class Bait : TerminalTackleItem, IBait
	{
		[JsonConfig]
		public BaitAttraction[] Attraction { get; set; }

		[JsonConfig]
		public bool IsAromatizable { get; set; }

		[JsonConfig]
		public bool IsLive { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public BaitColorPattern Color { get; set; }

		[JsonConfig]
		public float? LiveBaitSpeedModifier { get; set; }

		[JsonConfig]
		public int? MinCutCount { get; set; }

		[JsonConfig]
		public int? MaxCutCount { get; set; }

		[NoClone(true)]
		public int? CutCount { get; set; }
	}
}
