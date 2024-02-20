using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class InventoryItemBrief
	{
		public int ItemId { get; set; }

		public virtual string Name { get; set; }

		public string Params { get; set; }

		public int? ThumbnailBID { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public ItemTypes ItemType { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public ItemSubTypes ItemSubType { get; set; }
	}
}
