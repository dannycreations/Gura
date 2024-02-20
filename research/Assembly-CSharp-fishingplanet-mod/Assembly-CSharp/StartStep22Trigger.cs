using System;

public class StartStep22Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (StaticUserData.RodInHand.Rod != null && StaticUserData.RodInHand.Rod.LeaderLength <= 0.5f && GameFactory.Player.State == typeof(PlayerIdlePitch))
		{
			this._isTriggering = true;
		}
	}
}
