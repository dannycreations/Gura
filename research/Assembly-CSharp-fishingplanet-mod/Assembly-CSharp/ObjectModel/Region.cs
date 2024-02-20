using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject]
	public class Region
	{
		public int RegionId { get; set; }

		public string Name { get; set; }
	}
}
