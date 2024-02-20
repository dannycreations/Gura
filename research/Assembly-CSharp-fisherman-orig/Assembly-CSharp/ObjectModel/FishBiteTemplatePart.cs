using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class FishBiteTemplatePart
	{
		public int MinAmount { get; set; }

		public int MaxAmount { get; set; }

		public float MinPeriod { get; set; }

		public float MaxPeriod { get; set; }

		public float MinFactorForce { get; set; }

		public float MaxFactorForce { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public FishBiteDirection[] Directions { get; set; }

		public int[] DirectionsWeights { get; set; }
	}
}
