using System;

namespace ObjectModel
{
	public class Offer
	{
		public int OfferId { get; set; }

		public string Name { get; set; }

		public int OrderId { get; set; }

		public int ProductId { get; set; }

		public override string ToString()
		{
			return string.Format("Offer: {0}, Title: {1}", this.OfferId, this.Name);
		}
	}
}
