using System;
using UnityEngine;

public class StartStep15_1Trigger : StartTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.transform.position.y < 0f && GameFactory.Player != null && GameFactory.Player.IsTackleThrown && !this.PlaceForCastIllumination.bounds.Contains(GameFactory.Player.Tackle.transform.position) && (GameFactory.Player.Tackle.Fish == null || (GameFactory.Player.Tackle.Fish.State != typeof(FishShowBig) && GameFactory.Player.Tackle.Fish.State != typeof(FishShowSmall) && GameFactory.Player.Tackle.Fish.State != typeof(FishHooked) && GameFactory.Player.Tackle.Fish.State != typeof(FishSwimAway))))
		{
			this._isTriggering = true;
			return true;
		}
		this._isTriggering = false;
		return false;
	}

	public BoxCollider PlaceForCastIllumination;
}
