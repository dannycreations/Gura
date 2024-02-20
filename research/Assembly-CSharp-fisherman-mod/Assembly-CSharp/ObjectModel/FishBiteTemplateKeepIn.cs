using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class FishBiteTemplateKeepIn
	{
		public float MinPeriod { get; set; }

		public float MaxPeriod { get; set; }

		public float MinFactorForce { get; set; }

		public float MaxFactorForce { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public FishBiteDirection[] Directions { get; set; }

		public int[] DirectionsWeights { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public FishBiteDirection[] BottomDirections { get; set; }

		public int[] BottomDirectionsWeights { get; set; }

		public bool IsShaking { get; set; }

		public float MinShakePeriod { get; set; }

		public float MaxShakePeriod { get; set; }

		public float MinShakeFactorForce { get; set; }

		public float MaxShakeFactorForce { get; set; }
	}
}
