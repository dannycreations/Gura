using System;
using ObjectModel;
using UnityEngine;

public class FishPredatorSwim : FishStateBase
{
	protected override void onEnter()
	{
		this._checkBiteAt = Time.realtimeSinceStartup + base.Fish.PredatorAttackDelay / 2f;
		this._startAttackAt = Time.realtimeSinceStartup + base.Fish.PredatorAttackDelay;
		base.Fish.PredatorSwim();
	}

	protected override Type onUpdate()
	{
		if (base.Fish.Behavior == FishBehavior.Go)
		{
			return typeof(FishEscape);
		}
		if (this._checkBiteAt > 0f && this._checkBiteAt < Time.realtimeSinceStartup)
		{
			this._checkBiteAt = -1f;
			base.Adapter.ConfirmFishBite(base.Fish.InstanceGuid);
			if (SettingsManager.BobberBiteSound && base.IsBellActive && base.IsInHands)
			{
				RandomSounds.PlaySoundAtPoint("Sounds/incoming_bite3", GameFactory.Player.transform.position, 10f, false);
			}
		}
		if (this._startAttackAt < Time.realtimeSinceStartup)
		{
			return typeof(FishPredatorAttack);
		}
		return null;
	}

	private float _checkBiteAt;

	private float _startAttackAt;
}
