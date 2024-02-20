using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class FishCage : ToolItem
	{
		[JsonConfig]
		public float MaxFishWeight { get; set; }

		[JsonConfig]
		public float TotalWeight { get; set; }

		[JsonConfig]
		public virtual bool Safety { get; set; }

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
