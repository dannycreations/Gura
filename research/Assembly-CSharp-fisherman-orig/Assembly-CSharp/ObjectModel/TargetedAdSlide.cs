using System;

namespace ObjectModel
{
	public class TargetedAdSlide
	{
		public int ItemId { get; set; }

		public int DesignId { get; set; }

		public string Title { get; set; }

		public string Text { get; set; }

		public int? BackgroundBID { get; set; }

		public int? OrderId { get; set; }

		public DateTime? End { get; set; }

		public string OfferTitle { get; set; }

		public int? OfferImageBID { get; set; }

		public string OfferLinkText { get; set; }

		public int? ProductId { get; set; }

		public int? OfferHoverImageBID { get; set; }

		public TargetedAdSlideConfig AdConfig { get; set; }

		public AdDesignConfig Design { get; set; }

		public override string ToString()
		{
			return string.Format("TargetedAdSlide: {0}, Title: {1}, {2}", this.ItemId, this.Title, this.AdConfig);
		}
	}
}
