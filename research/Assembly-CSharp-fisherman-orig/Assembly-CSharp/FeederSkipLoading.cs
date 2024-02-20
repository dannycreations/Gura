using System;
using UnityEngine;

public class FeederSkipLoading : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		base.Player.CancelFeederLoading();
		this._waitTill = Time.time + 0.1f;
	}

	protected override Type onUpdate()
	{
		if (this._waitTill < Time.time)
		{
			base.Player.IsEmptyHandsMode = true;
			base.Player.OnHandMode(false);
			return typeof(PlayerDrawIn);
		}
		return null;
	}

	private float _waitTill = -1f;
}
