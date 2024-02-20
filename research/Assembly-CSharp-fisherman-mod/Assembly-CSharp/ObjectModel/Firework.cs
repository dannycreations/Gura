using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class Firework : InventoryItem
	{
		[JsonIgnore]
		public override bool IsConsumable
		{
			get
			{
				return true;
			}
		}

		[JsonConfig]
		public string LaunchAsset { get; set; }
	}
}
