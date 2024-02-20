using System;
using UnityEngine;

public class RodPodOut : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		this._waitTill = Time.time + 0.5f;
		base.Player.DestroyRodPod();
	}

	protected override Type onUpdate()
	{
		if (this._waitTill < Time.time)
		{
			return (!(base.Player.RodPodToPickUp != null)) ? typeof(PlayerEmpty) : typeof(PickUpRodPodIn);
		}
		return null;
	}

	private float _waitTill = -1f;
}
