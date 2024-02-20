using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class InteractiveObjectConfig
	{
		public Point3 Position { get; set; }

		public Point3 Rotation { get; set; }

		public Point3 Scale { get; set; }

		public string InteractSound { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public InteractFreq Frequency { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public InteractAction Action { get; set; }

		public ItemRef[] ItemRefs { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public CustomInteractionRules CustomInteractionRule;
	}
}
