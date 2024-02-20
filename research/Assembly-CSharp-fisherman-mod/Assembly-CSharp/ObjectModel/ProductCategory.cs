using System;

namespace ObjectModel
{
	public class ProductCategory
	{
		public int CategoryId { get; set; }

		public string Code { get; set; }

		public string Name { get; set; }

		public int OrderId { get; set; }

		public int[] ProductIds { get; set; }
	}
}
