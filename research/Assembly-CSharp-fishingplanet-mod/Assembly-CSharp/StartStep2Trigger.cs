using System;

public class StartStep2Trigger : StartTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		return (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish == null && GameFactory.Player.State == typeof(PlayerIdlePitch)) || GameFactory.Player.State == typeof(PlayerDrawIn);
	}
}
