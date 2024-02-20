using System;
using ObjectModel;

public class FishSwimAway : FishStateBase
{
	protected override void onEnter()
	{
		if (base.Fish.Tackle.RodTemplate == RodTemplate.Float)
		{
			base.Fish.KeepHookInMouth(FloatSwallowed.ActiveStrikeTimeout(base.Fish.FishTemplate, !base.IsInHands));
		}
		else if (base.Fish.Tackle.RodTemplate.IsBottomFishingTemplate())
		{
			base.Fish.KeepHookInMouth(FeederSwallowed.ActiveStrikeTimeout(base.Fish.FishTemplate, base.Fish.Tackle.IsBottom, !base.IsInHands));
		}
		base.Fish.Tackle.Fish = base.Fish;
		base.Adapter.ResetPeriod();
		if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(true);
		}
	}

	protected override Type onUpdate()
	{
		if (base.Fish.Behavior == FishBehavior.Go)
		{
			return typeof(FishEscape);
		}
		if (base.Fish.Behavior == FishBehavior.Hook)
		{
			return typeof(FishHooked);
		}
		return null;
	}

	protected override void onExit()
	{
		if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(false);
		}
		if (base.IsInHands && base.RodSlot.Rod.IsQuiver && GameFactory.QuiverIndicator != null)
		{
			GameFactory.QuiverIndicator.HighSensitivity(false);
		}
	}
}
