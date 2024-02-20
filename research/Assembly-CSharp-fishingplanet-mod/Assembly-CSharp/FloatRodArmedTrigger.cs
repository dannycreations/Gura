using System;
using ObjectModel;

[TriggerName(Name = "Armed with float rod")]
[Serializable]
public class FloatRodArmedTrigger : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return GameFactory.Player != null && GameFactory.Player.InFishingZone && !this.IsFishing() && StaticUserData.RodInHand.Rod != null && StaticUserData.RodInHand.Rod.ItemSubType != ItemSubTypes.CastingRod && StaticUserData.RodInHand.Rod.ItemSubType != ItemSubTypes.SpinningRod;
	}

	private bool IsFishing()
	{
		return GameFactory.Player != null && GameFactory.Player.State != typeof(PlayerIdle) && GameFactory.Player.State != typeof(PlayerIdlePitch) && GameFactory.Player.State != typeof(PlayerDrawIn) && GameFactory.Player.State != typeof(PlayerEmpty) && GameFactory.Player.State != typeof(ToolIdle);
	}
}
