using System;
using ObjectModel;
using UnityEngine;

public class HandSkipLoading : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		Chum chum = base.Player.CancelHandLoading();
		this._waitTill = Time.time + 0.1f;
	}

	protected override Type onUpdate()
	{
		if (this._waitTill < Time.time)
		{
			base.Player.IsHandThrowMode = false;
			base.Player.IsEmptyHandsMode = true;
			base.Player.OnHandMode(false);
			return typeof(PlayerEmpty);
		}
		return null;
	}

	private float _waitTill = -1f;
}
