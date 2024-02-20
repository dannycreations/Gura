using System;
using UnityEngine;

public class PlayerSimpleThrow : PlayerStateBase
{
	protected override void onEnter()
	{
		this.temps = Time.time;
	}

	protected override Type onUpdate()
	{
		if (ControlsController.ControlsActions.Fire1.WasReleased && Time.time - this.temps < 0.15f)
		{
			return typeof(PlayerIdle);
		}
		if (ControlsController.ControlsActions.Fire1.IsPressed && Time.time - this.temps > 0.15f)
		{
			return typeof(PlayerThrowInTwoHands);
		}
		return null;
	}

	private float temps;
}
