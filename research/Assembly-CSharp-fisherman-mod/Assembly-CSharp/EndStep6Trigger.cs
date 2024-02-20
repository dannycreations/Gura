using System;

public class EndStep6Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if ((GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && (GameFactory.Player.Tackle.Fish.State == typeof(FishShowBig) || GameFactory.Player.Tackle.Fish.State == typeof(FishShowSmall))) || GameFactory.Player.State == typeof(PlayerIdlePitch) || GameFactory.Player.Tackle == null || !GameFactory.Player.Tackle.Hook.IsBaitShown)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
