using System;

public class StartStep8Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish == null && !CursorManager.IsModalWindow())
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
