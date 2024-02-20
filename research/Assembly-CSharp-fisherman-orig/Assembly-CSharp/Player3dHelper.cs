using System;
using UnityEngine;

public static class Player3dHelper
{
	public static PlayerController GetPlayerController()
	{
		GameObject gameObject = GameObject.Find("3D/HUD/Collider/Player/HandsRoot/FakeHandsRoot");
		return (!(gameObject != null)) ? null : gameObject.GetComponent<PlayerController>();
	}
}
