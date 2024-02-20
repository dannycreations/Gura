using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class SpodRod : Rod
	{
		[JsonIgnore]
		public override AllowedEquipment[] AllowedEquipment
		{
			get
			{
				return new AllowedEquipment[]
				{
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Reel),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Line),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Feeder),
						MaxItems = 1
					},
					new AllowedEquipment
					{
						ItemType = new ItemTypes?(ItemTypes.Chum),
						MaxItems = 1
					}
				};
			}
		}

		public override int GetMaxAggregatesCount(InventoryItem item, Inventory inventory)
		{
			if (!(item is Chum))
			{
				return base.GetMaxAggregatesCount(item, inventory);
			}
			List<InventoryItem> rodEquipment = inventory.GetRodEquipment(this);
			SpodFeeder spodFeeder = rodEquipment.OfType<SpodFeeder>().FirstOrDefault<SpodFeeder>();
			if (spodFeeder == null)
			{
				return 1;
			}
			return spodFeeder.SlotCount;
		}
	}
}
