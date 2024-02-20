using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiteEditor
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Depth
	{
		Bottom,
		All,
		Top,
		Middle,
		Invalid
	}
}
