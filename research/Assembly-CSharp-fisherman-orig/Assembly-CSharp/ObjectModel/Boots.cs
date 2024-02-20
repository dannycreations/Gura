using System;

namespace ObjectModel
{
	public class Boots : OutfitItem
	{
		[JsonConfig]
		public float Depth { get; set; }
	}
}
