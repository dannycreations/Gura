using System;

namespace ObjectModel
{
	public class WhatsNewItem
	{
		public int ItemId { get; set; }

		public string Title { get; set; }

		public string Text { get; set; }

		public int? BackgroundBID { get; set; }

		public int? OrderId { get; set; }

		public bool IsEnabled { get; set; }

		public DateTime? Start { get; set; }

		public DateTime? End { get; set; }

		public string OfferTitle { get; set; }

		public int? OfferImageBID { get; set; }

		public string OfferLinkText { get; set; }

		public int? ProductId { get; set; }

		public int? OfferHoverImageBID { get; set; }

		public WhatsNewConfig Config { get; set; }
	}
}
