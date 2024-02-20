using System;
using Assets.Scripts.Common.Managers.Helpers;

public class StartStep24Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (this._helper.MenuPrefabsList != null && TutorialController.FishCatchedCount >= 1 && GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish == null && !this.IsFishing() && !CursorManager.IsModalWindow())
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}

	private bool IsFishing()
	{
		return GameFactory.Player != null && GameFactory.Player.State != typeof(PlayerIdle) && GameFactory.Player.State != typeof(PlayerIdlePitch) && GameFactory.Player.State != typeof(PlayerDrawIn) && GameFactory.Player.State != typeof(PlayerEmpty) && GameFactory.Player.State != typeof(ToolIdle);
	}

	private MenuHelpers _helper = new MenuHelpers();
}
