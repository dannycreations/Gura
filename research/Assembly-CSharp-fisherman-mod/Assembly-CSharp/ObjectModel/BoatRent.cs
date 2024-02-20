using System;

namespace ObjectModel
{
	public class BoatRent
	{
		public ItemSubTypes BoatType { get; set; }

		public int StartDay { get; set; }

		public int EndDay { get; set; }

		public int Duration { get; set; }
	}
}
