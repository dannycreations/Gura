using System;

namespace ObjectModel
{
	public class Bell : InventoryItem, IBell
	{
		[JsonConfig]
		public float Sensitivity { get; set; }

		[JsonConfig]
		public float Loudness { get; set; }

		[JsonConfig]
		public string AudioSignal { get; set; }
	}
}
