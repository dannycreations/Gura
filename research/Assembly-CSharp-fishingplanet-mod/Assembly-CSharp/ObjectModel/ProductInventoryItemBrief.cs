using System;

namespace ObjectModel
{
	public class ProductInventoryItemBrief
	{
		public int ItemId { get; set; }

		public string Storage { get; set; }

		public int Count { get; set; }

		public double? Length { get; set; }

		public float? Amount { get; set; }
	}
}
