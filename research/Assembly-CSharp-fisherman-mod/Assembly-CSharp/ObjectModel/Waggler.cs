using System;

namespace ObjectModel
{
	public class Waggler : Bobber
	{
		[JsonConfig]
		public float DelaySinking { get; set; }

		[JsonConfig]
		public float WindageMultiplier { get; set; }

		[JsonConfig]
		public int ShimsCount { get; set; }

		[JsonConfig]
		public int ShimsQuantity { get; set; }

		[JsonConfig]
		public Shim[] Shims { get; set; }
	}
}
