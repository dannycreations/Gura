using System;

namespace ObjectModel
{
	public class Wobbler
	{
		[JsonConfig]
		public bool IsSelfAnimated { get; set; }

		[JsonConfig]
		public float WorkingDepth { get; set; }

		[JsonConfig]
		public float SinkRate { get; set; }

		[JsonConfig]
		public float WalkingFactor { get; set; }

		[JsonConfig]
		public float ImbalanceRatio { get; set; }
	}
}
