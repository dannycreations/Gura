using System;

namespace ObjectModel
{
	public class MissionInventoryFilter
	{
		public ItemTypes ItemType { get; set; }

		public float? MinLoad { get; set; }

		public float? MaxLoad { get; set; }

		public int? LineLength { get; set; }

		public float? LineMinThickness { get; set; }

		public float? LineMaxThickness { get; set; }

		public int? LeaderLength { get; set; }

		public float? LeaderMinThickness { get; set; }

		public float? LeaderMaxThickness { get; set; }

		public float? ChumWeight { get; set; }
	}
}
