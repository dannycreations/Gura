using System;
using UnityEngine;

public class FeederLoadingOut : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		this._waitTill = Time.time + 0.25f;
	}

	protected override Type onUpdate()
	{
		if (this._waitTill < Time.time)
		{
			return typeof(PlayerDrawIn);
		}
		return null;
	}

	protected override void onExit()
	{
		base.onExit();
	}

	private float _waitTill = -1f;
}
