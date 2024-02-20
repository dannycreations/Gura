using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject]
	public class PondBrief
	{
		public int PondId { get; set; }

		public string Name { get; set; }

		public int MinLevel { get; set; }

		public int PhotoBID { get; set; }

		public State State { get; set; }

		public double MapX { get; set; }

		public double MapY { get; set; }

		public double MapZ { get; set; }

		public WeatherIcon WeatherIcon { get; set; }

		public bool IsActive { get; set; }

		public bool HasAction { get; set; }

		public int PlayersCount { get; set; }
	}
}
