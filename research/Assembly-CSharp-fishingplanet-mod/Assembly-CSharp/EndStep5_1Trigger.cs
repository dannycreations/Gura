using System;

public class EndStep5_1Trigger : EndTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		return GameFactory.Player != null && GameFactory.Player.State == typeof(PlayerIdlePitch);
	}
}
