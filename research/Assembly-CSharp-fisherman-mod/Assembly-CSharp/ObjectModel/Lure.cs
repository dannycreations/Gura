using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class Lure : Hook
	{
		[JsonConfig]
		public Bait Bait { get; set; }

		[JsonConfig]
		public float MinReelSpeed { get; set; }

		[JsonConfig]
		public float MaxReelSpeed { get; set; }

		[JsonConfig]
		public float SpeedIncBuoyancy { get; set; }

		[JsonConfig]
		public float MinSpeedBuoyancy { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(ArrayOfEnumsToStringConverterDragStyle))]
		public DragStyle[] IgnoreDrags { get; set; }

		[JsonConfig]
		public Wobbler Wobbler { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public TopWaterType? TopWaterType { get; set; }
	}
}
