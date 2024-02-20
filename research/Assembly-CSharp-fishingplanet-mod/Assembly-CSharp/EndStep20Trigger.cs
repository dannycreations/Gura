using System;

public class EndStep20Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (StaticUserData.RodInHand.Rod != null && (GameFactory.Player == null || GameFactory.Player.State == typeof(PlayerIdlePitch)))
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
