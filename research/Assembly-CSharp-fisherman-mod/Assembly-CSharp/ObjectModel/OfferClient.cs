using System;

namespace ObjectModel
{
	public class OfferClient
	{
		public int OfferId { get; set; }

		public string Name { get; set; }

		public int OrderId { get; set; }

		public int ProductId { get; set; }

		public DateTime? LifetimeRemain { get; set; }
	}
}
