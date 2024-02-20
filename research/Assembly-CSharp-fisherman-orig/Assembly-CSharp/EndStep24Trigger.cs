using System;
using Assets.Scripts.Common.Managers.Helpers;

public class EndStep24Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (this._helper.MenuPrefabsList != null && GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish == null && TutorialController.FishCatchedCount >= 1 && this._helper.MenuPrefabsList.globalMapForm.activeSelf && !base.IsHudGameObjectActive)
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
