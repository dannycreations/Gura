using System;
using UnityEngine;

public class StartStep2_2Trigger : StartTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		DateTime? hookIdleTime = base.GetHookIdleTime();
		if (GameFactory.Player.Tackle != null && hookIdleTime != null && ((base.IsHookIdleTimeout() && GameFactory.Player.Tackle.Fish == null) || !GameFactory.Player.Tackle.Hook.IsBaitShown))
		{
			this._isTriggering = true;
			return true;
		}
		this._isTriggering = false;
		return false;
	}

	public BoxCollider PlaceForCastIllumination;
}
