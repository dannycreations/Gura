using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class QuiverTip : IQuiverTip
	{
		[NoClone(true)]
		public int ItemId { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public QuiverTipColor Color { get; set; }

		[JsonConfig]
		public float Test { get; set; }

		[JsonConfig]
		public float MaxCastWeight { get; set; }

		[JsonConfig]
		public float RodDurabilityLoss { get; set; }

		[JsonConfig]
		[NoClone(true)]
		public bool IsBroken { get; set; }

		public int GetHashCode(QuiverTip obj)
		{
			return this.ItemId;
		}
	}
}
