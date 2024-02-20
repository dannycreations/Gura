using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BiteEditor
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Operation
	{
		Add,
		Sub,
		Mul,
		InvAdd,
		InvSub
	}
}
