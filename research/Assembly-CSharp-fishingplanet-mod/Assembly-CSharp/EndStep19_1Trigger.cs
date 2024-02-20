using System;

public class EndStep19_1Trigger : EndTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		return GameFactory.Player != null && GameFactory.Player.State == typeof(PlayerIdlePitch);
	}
}
