using System;
using UnityEngine;

public class FishEscape : FishStateBase
{
	protected override void onEnter()
	{
		if (base.Fish.Tackle != null)
		{
			base.Fish.Tackle.EscapeFish();
		}
		this.escapeTime = 0f;
	}

	protected override Type onUpdate()
	{
		this.escapeTime += Time.deltaTime;
		if (this.escapeTime > 3f || base.Fish.IsPathCompleted)
		{
			return typeof(FishDestroy);
		}
		return null;
	}

	private const float DestroyTimeout = 3f;

	private float escapeTime;
}
