using System;
using Assets.Scripts.Common.Managers.Helpers;

public class EndStep15Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (this._helper.MenuPrefabsList != null && this._helper.MenuPrefabsList.globalMapForm.activeSelf && this._helper.MenuPrefabsList.globalMapFormAS.isActive && this._helper.MenuPrefabsList.inventoryForm.activeSelf && !this._helper.MenuPrefabsList.inventoryFormAS.isActive)
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
