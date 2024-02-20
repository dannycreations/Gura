using System;
using ObjectModel;

public class StartStep5_1Trigger : StartTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		return GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Hook != null && !GameFactory.Player.Tackle.Hook.IsBaitShown && (GameFactory.Player.Tackle.Fish == null || GameFactory.Player.Tackle.Fish.Behavior != FishBehavior.Hook);
	}
}
