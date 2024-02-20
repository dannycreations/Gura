using System;
using ObjectModel;

public class LeaderBehavior : ChangeHandler
{
	public override void OnChange()
	{
		InventoryItem inventoryItem = InitRods.Instance.ActiveRod.Rod.InventoryItem;
		this.LeashLength.Initialize(inventoryItem);
	}

	public LeashLineController LeashLength;
}
