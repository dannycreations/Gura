using System;

namespace ObjectModel
{
	public class MarketingEvent
	{
		public int EventId { get; set; }

		public string Name { get; set; }

		public DateTime Start { get; set; }

		public DateTime Finish { get; set; }

		public MarketingEventConfig Config;
	}
}
