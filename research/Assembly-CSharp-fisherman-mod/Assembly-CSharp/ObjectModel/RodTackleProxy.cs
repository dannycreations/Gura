using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectModel
{
	public class RodTackleProxy
	{
		public RodTackleProxy(RodTemplate rodTemplate, IEnumerable<InventoryItem> rodInventory)
		{
			this.RodTemplate = rodTemplate;
			this.rodInventory = rodInventory;
		}

		public RodTemplate RodTemplate { get; private set; }

		public Chum[] Chum
		{
			get
			{
				return this.rodInventory.OfType<Chum>().ToArray<Chum>();
			}
		}

		public Bait Bait
		{
			get
			{
				return (Bait)this.rodInventory.FirstOrDefault((InventoryItem i) => i is Bait);
			}
		}

		public Hook Hook
		{
			get
			{
				return (Hook)this.rodInventory.FirstOrDefault((InventoryItem i) => i is Hook);
			}
		}

		public Bobber Bobber
		{
			get
			{
				return (Bobber)this.rodInventory.FirstOrDefault((InventoryItem i) => i is Bobber);
			}
		}

		public Sinker Sinker
		{
			get
			{
				return (Sinker)this.rodInventory.FirstOrDefault((InventoryItem i) => i.ItemType == ItemTypes.Sinker);
			}
		}

		public Feeder Feeder
		{
			get
			{
				return (Feeder)this.rodInventory.FirstOrDefault((InventoryItem i) => i.ItemType == ItemTypes.Feeder);
			}
		}

		public TerminalTackleItem Lure
		{
			get
			{
				return this.rodInventory.FirstOrDefault((InventoryItem i) => i is Lure) as TerminalTackleItem;
			}
		}

		public TerminalTackleItem BaitOrLure
		{
			get
			{
				InventoryItem inventoryItem = this.rodInventory.FirstOrDefault((InventoryItem i) => i is Bait);
				if (inventoryItem == null)
				{
					inventoryItem = this.rodInventory.FirstOrDefault((InventoryItem i) => i is Lure);
				}
				if (inventoryItem == null)
				{
					inventoryItem = this.rodInventory.FirstOrDefault((InventoryItem i) => i is JigHead);
				}
				return inventoryItem as TerminalTackleItem;
			}
		}

		public bool HasBait
		{
			get
			{
				return this.RodTemplate.IsBaitFishingTemplate() && this.Bait != null && this.Bait.Count > 0;
			}
		}

		private readonly IEnumerable<InventoryItem> rodInventory;
	}
}
