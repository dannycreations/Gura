using System;

public class EndStep1Trigger : EndTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		return this.ZoneActivation.InZone;
	}

	public ZoneTutorialStep ZoneActivation;
}
