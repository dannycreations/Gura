using System;
using Assets.Scripts.Common.Managers.Helpers;

public class EndStep12Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (this._helper.MenuPrefabsList != null && this._helper.MenuPrefabsList.shopForm.activeSelf && this._helper.MenuPrefabsList.shopFormAS.isActive && this._helper.MenuPrefabsList.inventoryForm.activeSelf && !this._helper.MenuPrefabsList.inventoryFormAS.isActive)
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
