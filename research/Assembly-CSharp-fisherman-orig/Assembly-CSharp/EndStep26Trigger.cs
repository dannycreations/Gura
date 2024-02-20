using System;

public class EndStep26Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (StaticUserData.CurrentPond == null)
		{
			this._isTriggering = true;
		}
	}
}
