using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiteEditor
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum AttractorResultType
	{
		Add,
		Mul,
		Override,
		None
	}
}
