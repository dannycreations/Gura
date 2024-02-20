using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class Hat : OutfitItem
	{
		[JsonConfig]
		public int BaitKitCount { get; set; }

		[JsonConfig]
		public int TackleKitCount { get; set; }

		[JsonConfig]
		public bool IsLighted { get; set; }

		[JsonIgnore]
		protected override bool IsSellable
		{
			get
			{
				return base.Rent == null;
			}
		}

		[JsonConfig]
		public Flashlight Flashlight { get; set; }
	}
}
