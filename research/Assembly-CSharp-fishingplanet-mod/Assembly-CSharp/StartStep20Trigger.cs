using System;
using UnityEngine;

public class StartStep20Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.transform.position.y < 0f && GameFactory.Player != null && GameFactory.Player.IsTackleThrown && this.PlaceForCastIllumination.bounds.Contains(GameFactory.Player.Tackle.transform.position) && GameFactory.Player.Tackle.Fish == null && GameFactory.Player.Tackle.Hook.IsBaitShown && StaticUserData.RodInHand.Rod.LeaderLength > 0.5f)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}

	public BoxCollider PlaceForCastIllumination;
}
