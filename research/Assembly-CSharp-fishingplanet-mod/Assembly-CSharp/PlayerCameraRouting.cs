using System;
using System.Linq;
using UnityEngine;

public class PlayerCameraRouting : PlayerStateBase
{
	protected override void onEnter()
	{
		this._camera = base.Player.SetCameraRouting().transform;
		RoutingBase[] array = Object.FindObjectsOfType<RoutingBase>();
		this._routing = array.FirstOrDefault((RoutingBase r) => r.Tag == "Untagged");
		if (this._routing != null)
		{
			this._routing.EFade += this.OnFade;
			this._routing.Play();
		}
	}

	private void OnFade(bool flag, float fadeTime)
	{
	}

	protected override Type onUpdate()
	{
		if (base.Player.LeaveCameraRouting)
		{
			base.Player.LeaveCameraRouting = false;
			return base.PrevState;
		}
		if (this._routing != null)
		{
			this._camera.rotation = this._routing.Rotation;
			this._camera.position = this._routing.Position;
		}
		return null;
	}

	protected override void onExit()
	{
		if (this._routing != null)
		{
			this._routing.Stop();
			this._routing.EFade -= this.OnFade;
		}
		base.Player.ClearCameraRouting();
	}

	private RoutingBase _routing;

	private Transform _camera;
}
