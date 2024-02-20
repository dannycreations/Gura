using System;
using Assets.Scripts.Common.Managers.Helpers;

[TriggerName(Name = "Inventory entered")]
[Serializable]
public class InventoryEntered : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return StaticUserData.CurrentPond == null && InventoryEntered._helpers.MenuPrefabsList != null && InventoryEntered._helpers.MenuPrefabsList.inventoryFormAS != null && InventoryEntered._helpers.MenuPrefabsList.inventoryFormAS.isActive;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
