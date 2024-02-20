using System;
using ObjectModel;

public class EndStep5Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if ((GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.Behavior == FishBehavior.Hook) || (GameFactory.Player != null && GameFactory.Player.State == typeof(PlayerIdlePitch)) || (GameFactory.Player.Tackle == null || (GameFactory.Player.Tackle.Hook != null && !GameFactory.Player.Tackle.Hook.IsBaitShown)) || GameFactory.Player.Tackle == null || GameFactory.Player.Tackle.Fish == null)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
