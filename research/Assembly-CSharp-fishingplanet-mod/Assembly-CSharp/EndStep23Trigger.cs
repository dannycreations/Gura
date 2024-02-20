using System;
using Assets.Scripts.Common.Managers.Helpers;

public class EndStep23Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (this._helper.MenuPrefabsList != null && TutorialController.FishCatchedCount >= 1 && GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish == null)
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
