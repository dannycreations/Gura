using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class SpodFeeder : Feeder
	{
		public int SlotCount { get; set; }

		[JsonIgnore]
		public override float ChumCapacity
		{
			get
			{
				return (float)Math.Round((double)(base.Capacity / (float)this.SlotCount), 3, MidpointRounding.AwayFromZero);
			}
		}
	}
}
