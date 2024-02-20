using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using InventorySRIA;
using InventorySRIA.Models;
using ObjectModel;
using UnityEngine;

public class StartStep11Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		int num = 0;
		if (this.Content == null)
		{
			num = this._helper.MenuPrefabsList.inventoryForm.GetComponentInChildren<global::InventorySRIA.InventorySRIA>().Parameters.data.Count((BaseModel x) => x.CachedType == typeof(ExpandableInventoryItemModel) && (x as ExpandableInventoryItemModel).item.ItemType == ItemTypes.Bait);
		}
		if (this._helper.MenuPrefabsList != null && this._helper.MenuPrefabsList.inventoryForm.activeSelf && this._helper.MenuPrefabsList.inventoryFormAS.CanRun && num > 0)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}

	public GameObject Content;

	private MenuHelpers _helper = new MenuHelpers();
}
