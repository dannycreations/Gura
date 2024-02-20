using System;

public class EndStep21Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (StaticUserData.RodInHand.Rod != null && StaticUserData.RodInHand.Rod.LeaderLength <= 0.5f)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
