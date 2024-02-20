using System;

namespace ObjectModel
{
	public class Belt : OutfitItem
	{
		[JsonConfig]
		public float PowerCompensation { get; set; }

		[JsonConfig]
		public float FatigueReduction { get; set; }

		[JsonConfig]
		public float SpeedLoadIndicator { get; set; }

		[JsonConfig]
		public float ChangeLoad { get; set; }
	}
}
