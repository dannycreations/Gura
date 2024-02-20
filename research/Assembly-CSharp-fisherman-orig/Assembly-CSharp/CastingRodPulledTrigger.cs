using System;
using ObjectModel;

[TriggerName(Name = "Casting rod fishing")]
[Serializable]
public class CastingRodPulledTrigger : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return GameFactory.Player != null && GameFactory.Player.InFishingZone && !this.IsFloatRod() && !this.IsFishing();
	}

	private bool IsFishing()
	{
		return GameFactory.Player != null && GameFactory.Player.State != typeof(PlayerIdle) && GameFactory.Player.State != typeof(PlayerIdlePitch) && GameFactory.Player.State != typeof(PlayerDrawIn) && GameFactory.Player.State != typeof(PlayerEmpty) && GameFactory.Player.State != typeof(ToolIdle);
	}

	private bool IsFloatRod()
	{
		return StaticUserData.RodInHand.Rod != null && StaticUserData.RodInHand.Rod.ItemSubType != ItemSubTypes.CastingRod && StaticUserData.RodInHand.Rod.ItemSubType != ItemSubTypes.SpinningRod;
	}

	private static bool _wasInWater;

	private static bool _pulled;
}
