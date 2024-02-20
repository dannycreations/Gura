using System;
using UnityEngine;

public class EndStep19Trigger : EndTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		return (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.transform.position.y < 0f && GameFactory.Player != null && GameFactory.Player.IsTackleThrown && StaticUserData.RodInHand.Rod.LeaderLength > 0.5f) || (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.Fish != null && (GameFactory.Player.Tackle.Fish.State == typeof(FishShowBig) || GameFactory.Player.Tackle.Fish.State == typeof(FishShowSmall))) || (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.transform.position.y < 0f && GameFactory.Player != null && GameFactory.Player.IsTackleThrown && !this.PlaceForCastIllumination.bounds.Contains(GameFactory.Player.Tackle.transform.position) && !GameFactory.Player.Tackle.IsFishHooked);
	}

	public BoxCollider PlaceForCastIllumination;
}
