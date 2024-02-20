using System;
using Assets.Scripts.Common.Managers.Helpers;

public class EndStep17Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (base.IsHudGameObjectActive && !PondControllers.Instance.IsInMenu && !MenuHelpers.Instance.MenuPrefabsList.IsLoadingOrTransfer())
		{
			this._isTriggering = true;
		}
	}
}
