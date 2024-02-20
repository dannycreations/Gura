using System;

namespace ObjectModel
{
	public class Shim
	{
		[JsonConfig]
		public int ShimsCount { get; set; }

		[JsonConfig]
		public float WindageMultiplier { get; set; }
	}
}
