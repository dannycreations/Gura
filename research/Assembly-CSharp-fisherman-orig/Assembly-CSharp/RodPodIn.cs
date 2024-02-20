using System;
using UnityEngine;

public class RodPodIn : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.Player.ShowSleeves = false;
		this._waitTill = Time.time + 0.5f;
		base.Player.CreateRodPod();
		base.Player.CameraController.CameraMouseLook.SetTargetPitch(-45f);
	}

	protected override Type onUpdate()
	{
		if (this._waitTill < Time.time)
		{
			return typeof(RodPodIdle);
		}
		return null;
	}

	protected override void onExit()
	{
		base.Player.CameraController.CameraMouseLook.ClearTargetPitch();
	}

	private float _waitTill = -1f;
}
