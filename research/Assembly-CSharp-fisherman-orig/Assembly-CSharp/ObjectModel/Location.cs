using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(0)]
	public class Location
	{
		public int LocationId { get; set; }

		public Region Region { get; set; }

		public Country Country { get; set; }

		public State State { get; set; }

		public Pond Pond { get; set; }

		public string Name { get; set; }

		public string Desc { get; set; }

		public int PhotoBID { get; set; }

		public string Asset { get; set; }

		public double MapX { get; set; }

		public double MapY { get; set; }
	}
}
