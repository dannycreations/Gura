using System;

public class StartStep7Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (GameFactory.Player.Tackle != null && GameFactory.Player != null && GameFactory.Player.Tackle.IsShowing && GameFactory.Player.Tackle.IsShowingComplete && (GameFactory.Player.State == typeof(PlayerShowFishIdle) || GameFactory.Player.State == typeof(PlayerShowFishLineIdle)) && (GameFactory.Player.Tackle.Fish.State == typeof(FishShowSmall) || GameFactory.Player.Tackle.Fish.State == typeof(FishShowBig)))
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}
}
