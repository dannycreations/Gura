using System;
using ObjectModel;
using UnityEngine;

public class EndStep22Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle != null && ((GameFactory.Player.Tackle.Fish != null && (GameFactory.Player.Tackle.Fish.State == typeof(FishShowBig) || GameFactory.Player.Tackle.Fish.State == typeof(FishShowSmall))) || (GameFactory.Player.Tackle.transform.position.y < 0f && GameFactory.Player != null && GameFactory.Player.IsTackleThrown && !this.PlaceForCastIllumination.bounds.Contains(GameFactory.Player.Tackle.transform.position) && !GameFactory.Player.Tackle.IsFishHooked) || (!GameFactory.Player.Tackle.Hook.IsBaitShown && (GameFactory.Player.Tackle.Fish == null || GameFactory.Player.Tackle.Fish.Behavior != FishBehavior.Hook))))
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
