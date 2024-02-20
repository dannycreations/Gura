using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(1)]
	public class MissionGroupOnClient
	{
		[JsonProperty]
		public int GroupId { get; set; }

		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public int Priority { get; set; }
	}
}
