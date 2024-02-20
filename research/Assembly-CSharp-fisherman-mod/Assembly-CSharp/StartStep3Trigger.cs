using System;
using UnityEngine;

public class StartStep3Trigger : StartTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		DateTime? hookIdleTime = base.GetHookIdleTime();
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.transform.position.y < 0f && GameFactory.Player != null && GameFactory.Player.IsTackleThrown && hookIdleTime != null && !base.IsHookIdleTimeout() && GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Hook.IsBaitShown)
		{
			this._isTriggering = true;
			return true;
		}
		this._isTriggering = false;
		return false;
	}

	public BoxCollider PlaceForCastIllumination;
}
