using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class BoilBait : Bait
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public BoilBaitForm Form { get; set; }

		public BoilBaitBottomTypeAttraction BottomTypeAttraction { get; set; }
	}
}
