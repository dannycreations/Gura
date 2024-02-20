using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class FishBait
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public FishBiteType BiteType { get; set; }

		public float Probability { get; set; }
	}
}
