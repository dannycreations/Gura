using System;
using ObjectModel;

public class RodBehavior : ChangeHandler
{
	public override void OnChange()
	{
		if (base.InventoryItemComponent == null || base.InventoryItemComponent.InventoryItem == null)
		{
			this.LeashLength.gameObject.SetActive(false);
			this.QuiverTips.Initialize(null);
			return;
		}
		InventoryItem inventoryItem = base.InventoryItemComponent.InventoryItem;
		this.LeashLength.Initialize(inventoryItem);
		this.QuiverTips.Initialize(inventoryItem as FeederRod);
	}

	internal override void Refresh()
	{
		this.OnChange();
	}

	internal void Clear()
	{
	}

	private void Awake()
	{
		DropMe component = base.GetComponent<DropMe>();
		component.typeId = new int[9];
		component.typeId[0] = 23;
		component.typeId[1] = 24;
		component.typeId[2] = 25;
		component.typeId[3] = 26;
		component.typeId[4] = 27;
		component.typeId[5] = 28;
		component.typeId[6] = 129;
		component.typeId[7] = 130;
		component.typeId[8] = 29;
	}

	public LeashLineController LeashLength;

	public QuiverTipController QuiverTips;
}
