using System;

public class EndStep4Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if ((GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.State == typeof(FishSwimAway)) || GameFactory.Player.State == typeof(PlayerIdlePitch) || GameFactory.Player.Tackle == null || GameFactory.Player.Tackle.Fish == null)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
