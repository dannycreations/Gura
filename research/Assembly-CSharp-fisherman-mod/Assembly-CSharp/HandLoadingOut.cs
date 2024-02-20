using System;
using UnityEngine;

public class HandLoadingOut : PlayerStateBase
{
	protected override void onEnter()
	{
		base.onEnter();
		this._waitTill = Time.time + 0.25f;
	}

	protected override Type onUpdate()
	{
		if (this._waitTill >= Time.time)
		{
			return null;
		}
		if (base.IsHandChumLoadingRequired)
		{
			return typeof(HandLoadingIdle);
		}
		return typeof(HandDrawIn);
	}

	private float _waitTill = -1f;
}
