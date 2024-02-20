using System;
using Assets.Scripts.Common.Managers.Helpers;

public class EndStep11Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (InitRods.Instance == null)
		{
			return;
		}
		InventoryItemDollComponent bait = InitRods.Instance.ActiveRod.Bait;
		if (this._helper.MenuPrefabsList != null && this._helper.MenuPrefabsList.inventoryForm.activeSelf && bait != null && bait.InventoryItem != null && bait.InventoryItem.ItemId == 434)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}

	private MenuHelpers _helper = new MenuHelpers();
}
