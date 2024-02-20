using System;

namespace ObjectModel
{
	public class FishNet : ToolItem
	{
		[JsonConfig]
		public float MaxFishWeight { get; set; }

		[JsonConfig]
		public float MaxFishLegth { get; set; }

		[JsonConfig]
		public float Convenience { get; set; }
	}
}
