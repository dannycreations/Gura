using System;

public class StartStep4Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.State == typeof(FishBite))
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
