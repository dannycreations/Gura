using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class StatsCounter
{
	[JsonConverter(typeof(StringEnumConverter))]
	public StatsCounterType Type { get; set; }

	public int Count { get; set; }
}
