using System;

public class EndStep2Trigger : EndTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		return (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.IsInitialized && GameFactory.Player.Tackle.transform.position.y < 0f && GameFactory.Player != null && GameFactory.Player.IsTackleThrown) || !this.ZoneActivation.InZone;
	}

	public ZoneTutorialStep ZoneActivation;
}
