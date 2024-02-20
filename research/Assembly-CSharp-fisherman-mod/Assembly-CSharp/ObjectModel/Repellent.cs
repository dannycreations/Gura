using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class Repellent : InventoryItem
	{
		[JsonConfig]
		public int Capacity { get; set; }

		[JsonConfig]
		public int Duration { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public TemperatureType Temperature { get; set; }
	}
}
