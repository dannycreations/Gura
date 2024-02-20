using System;

namespace ObjectModel
{
	public class Clothing : OutfitItem
	{
		[JsonConfig]
		public ClothingKind Kind { get; set; }

		[JsonConfig]
		public float CastingDistanceDecrease { get; set; }
	}
}
