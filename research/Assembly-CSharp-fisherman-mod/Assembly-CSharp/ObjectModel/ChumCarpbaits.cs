using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class ChumCarpbaits : ChumBase
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public ChumCarpbaitsForm Form { get; set; }
	}
}
