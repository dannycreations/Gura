using System;
using Phy;
using UnityEngine;

public class EndStep8Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		this._isTriggering = GameFactory.Player.Tackle != null && ((GameFactory.Player.Tackle.Fish != null && GameFactory.Player.Tackle.Fish.State == typeof(FishSwimAway)) || (GameFactory.Player.Tackle.IsInWater && this.IsLying));
	}

	private bool IsLying
	{
		get
		{
			Mass tackleTipMass = GameFactory.Player.RodSlot.Sim.TackleTipMass;
			return tackleTipMass.Position.y < tackleTipMass.GroundHeight || Mathf.Abs(tackleTipMass.Position.y) - Mathf.Abs(tackleTipMass.GroundHeight) > 0f;
		}
	}
}
