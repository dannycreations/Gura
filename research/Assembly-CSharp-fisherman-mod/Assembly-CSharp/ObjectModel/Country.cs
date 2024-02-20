using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject]
	public class Country
	{
		public int CountryId { get; set; }

		public string Name { get; set; }

		public int MapBID { get; set; }
	}
}
