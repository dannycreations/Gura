using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class JigBait : Bait
	{
		[JsonConfig]
		public float MinHookSize { get; set; }

		[JsonConfig]
		public float MaxHookSize { get; set; }

		[JsonConfig]
		public float SpeedIncBuoyancy { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(ArrayOfEnumsToStringConverterDragStyle))]
		public DragStyle[] IgnoreDrags { get; set; }
	}
}
