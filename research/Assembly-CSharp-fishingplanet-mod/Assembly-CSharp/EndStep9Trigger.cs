using System;

public class EndStep9Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		this._isTriggering = GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && (GameFactory.Player.Tackle.Fish.State == typeof(FishSwimAway) || GameFactory.Player.Tackle.Fish.State == typeof(FishHooked));
	}
}
