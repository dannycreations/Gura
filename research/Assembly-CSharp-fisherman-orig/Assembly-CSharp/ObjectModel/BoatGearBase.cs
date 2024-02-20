using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class BoatGearBase : InventoryItem
	{
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
