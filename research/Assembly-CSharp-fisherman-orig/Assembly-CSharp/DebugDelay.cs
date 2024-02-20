using System;
using UnityEngine;

public class DebugDelay : PlayerStateBase
{
	protected override void onEnter()
	{
		this._stopAt = Time.time + 10f;
	}

	protected override Type onUpdate()
	{
		if (this._stopAt < Time.time)
		{
			return typeof(TakeRodFromPodOut);
		}
		return null;
	}

	private float _stopAt;
}
