using System;
using Assets.Scripts.Common.Managers.Helpers;

public class StartStep25Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (this._helper.MenuPrefabsList != null && this._helper.MenuPrefabsList.globalMapForm.activeSelf && this._helper.MenuPrefabsList.globalMapFormAS.CanRun)
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
