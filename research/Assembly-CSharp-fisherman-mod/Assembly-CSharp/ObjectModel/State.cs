using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject]
	public class State
	{
		public int StateId { get; set; }

		public int CountryId { get; set; }

		public string Name { get; set; }

		public double HomePrice { get; set; }

		public string HomeCurrency { get; set; }
	}
}
