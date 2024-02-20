using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class PvaFeeder : Feeder
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public PvaFeederForm Form { get; set; }
	}
}
