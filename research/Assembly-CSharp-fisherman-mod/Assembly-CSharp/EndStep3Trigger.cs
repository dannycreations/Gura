using System;

public class EndStep3Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if ((GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.State == typeof(FishSwimAway)) || GameFactory.Player.State == typeof(PlayerIdlePitch))
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
