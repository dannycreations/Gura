using System;

public class EndStep2_2Trigger : EndTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		return GameFactory.Player != null && GameFactory.Player.State == typeof(PlayerIdlePitch);
	}
}
