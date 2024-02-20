using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class FishBitePartElement
	{
		public float Period { get; set; }

		public float FactorForce { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public FishBiteDirection Direction { get; set; }
	}
}
