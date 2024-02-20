using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class FishBiteKeepIn
	{
		public float Period { get; set; }

		public float FactorForce { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public FishBiteDirection Direction { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public FishBiteDirection BottomDirection { get; set; }

		public bool IsShaking { get; set; }

		public float ShakePeriod { get; set; }

		public float ShakeFactorForce { get; set; }
	}
}
