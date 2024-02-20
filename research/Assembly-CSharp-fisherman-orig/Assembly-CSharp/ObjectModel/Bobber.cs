using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class Bobber : TerminalTackleItem, IBobber
	{
		[JsonConfig]
		public float Sensitivity { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public BobberMount Mount { get; set; }

		[JsonConfig]
		public float SinkerMass { get; set; }
	}
}
