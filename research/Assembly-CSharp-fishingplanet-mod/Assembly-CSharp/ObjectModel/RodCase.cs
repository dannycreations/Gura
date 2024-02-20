using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class RodCase : OutfitItem
	{
		[JsonConfig]
		public int RodWithReelCount { get; set; }

		[JsonConfig]
		public int ReelCount { get; set; }

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
