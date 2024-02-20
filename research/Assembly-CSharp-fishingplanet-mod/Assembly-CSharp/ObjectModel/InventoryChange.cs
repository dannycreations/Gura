using System;

namespace ObjectModel
{
	public class InventoryChange
	{
		public int? FromItemId
		{
			get
			{
				return (this.FromItem == null) ? null : new int?(this.FromItem.ItemId);
			}
		}

		public int? ToItemId
		{
			get
			{
				return (this.ToItem == null) ? null : new int?(this.ToItem.ItemId);
			}
		}

		public InventoryItem Item;

		public StoragePlaces From;

		public InventoryItem FromItem;

		public StoragePlaces To;

		public InventoryItem ToItem;

		public bool New;

		public bool Destroyed;

		public bool Updated;

		public bool Repaired;

		public bool Weared;

		public bool Broken;

		public string Op;
	}
}
