using System;
using Assets.Scripts.Common.Managers.Helpers;

public class EndStep10Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (this._helper.MenuPrefabsList != null && this._helper.MenuPrefabsList.inventoryForm.activeSelf && !base.IsHudGameObjectActive)
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
