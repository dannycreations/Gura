using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiteEditor
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum FishForm
	{
		Young,
		Common,
		Trophy,
		Unique
	}
}
