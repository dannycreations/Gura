using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class RodStand : ToolItem
	{
		[JsonConfig]
		public int RodCount { get; set; }

		[JsonConfig]
		public int StandCount { get; set; }

		[JsonConfig]
		public float Force { get; set; }

		[JsonConfig]
		public float Angle { get; set; }

		[JsonConfig]
		public Bell Bell { get; set; }

		[JsonIgnore]
		protected override bool IsSellable
		{
			get
			{
				return base.Rent == null;
			}
		}
	}
}
