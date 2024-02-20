using System;
using UnityEngine;

public class StartStep8_5Trigger : StartTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		if (GameFactory.Player.Tackle != null && GameFactory.Player.Tackle.IsInWater)
		{
			if (this._currentFloatingTime > this._floatingTime && this.IsLying)
			{
				return true;
			}
			this._currentFloatingTime += Time.deltaTime;
		}
		else
		{
			this._currentFloatingTime = 0f;
		}
		return false;
	}

	private bool IsLying
	{
		get
		{
			return false;
		}
	}

	private float _floatingTime = 1f;

	private float _currentFloatingTime;

	private float _desiredHitDistance = 0.6f;
}
