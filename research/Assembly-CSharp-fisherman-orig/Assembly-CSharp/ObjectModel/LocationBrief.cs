using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject]
	public class LocationBrief
	{
		public int LocationId { get; set; }

		public string Name { get; set; }

		public double MapX { get; set; }

		public double MapY { get; set; }

		public string Asset { get; set; }
	}
}
