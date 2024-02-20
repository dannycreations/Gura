using System;

public class StartStep21Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (GameFactory.Player.State == typeof(PlayerIdlePitch) && StaticUserData.RodInHand.Rod.LeaderLength > 0.5f)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
