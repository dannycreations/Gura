using System;

public class StartStep5Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && (GameFactory.Player.Tackle.Fish as Fish1stBehaviour).isHooked)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
