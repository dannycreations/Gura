using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class AllowedEquipment
	{
		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public ItemTypes? ItemType { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public ItemSubTypes? ItemSubType { get; set; }

		[JsonConfig]
		public int MaxItems { get; set; }
	}
}
