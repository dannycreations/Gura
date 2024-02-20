using System;

namespace EnhancedScrollerDemos.SelectionDemo
{
	public class InventoryData
	{
		public bool Selected
		{
			get
			{
				return this._selected;
			}
			set
			{
				if (this._selected != value)
				{
					this._selected = value;
					if (this.selectedChanged != null)
					{
						this.selectedChanged(this._selected);
					}
				}
			}
		}

		public string itemName;

		public int itemCost;

		public int itemDamage;

		public int itemDefense;

		public int itemWeight;

		public string itemDescription;

		public string spritePath;

		public SelectedChangedDelegate selectedChanged;

		private bool _selected;
	}
}
