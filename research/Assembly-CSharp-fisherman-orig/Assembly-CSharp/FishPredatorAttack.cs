using System;
using ObjectModel;
using UnityEngine;

public class FishPredatorAttack : FishStateBase
{
	protected override void onEnter()
	{
		this._startSwallowAt = Time.realtimeSinceStartup + base.Fish.PredatorHoldDelay;
		base.Fish.PredatorAttack();
		if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(true);
			base.RodSlot.Bell.HighSensitivity(true);
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
		if (this._startSwallowAt < Time.realtimeSinceStartup)
		{
			return typeof(FishSwimAway);
		}
		return null;
	}

	protected override void onExit()
	{
		if (base.RodSlot.Bell != null)
		{
			base.RodSlot.Bell.Voice(false);
		}
	}

	private float _startSwallowAt;
}
