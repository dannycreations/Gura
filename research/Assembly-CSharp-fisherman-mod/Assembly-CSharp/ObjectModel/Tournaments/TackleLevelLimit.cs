using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel.Tournaments
{
	public class TackleLevelLimit
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public ItemSubTypes TackleType { get; set; }

		public int? MinLevel { get; set; }

		public int? MaxLevel { get; set; }
	}
}
