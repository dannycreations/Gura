using System;
using UnityEngine;

public class ZoneEnabler : MonoBehaviour
{
	internal void OnTriggerEnter(Collider visitor)
	{
		if (GameFactory.FishingZonesPlayerCollider == visitor)
		{
			GameFactory.Player.OnEnterFishingZone(base.gameObject);
			this.Log("entered");
		}
		else if (GameFactory.Player != null)
		{
			GameFactory.Player.OnColliderEnterFishZone(visitor);
		}
	}

	internal void OnTriggerExit(Collider visitor)
	{
		if (GameFactory.FishingZonesPlayerCollider == visitor)
		{
			GameFactory.Player.OnExitFishingZone(base.gameObject);
			this.Log("left");
		}
		else if (GameFactory.Player != null)
		{
			GameFactory.Player.OnColliderExitFishZone(visitor);
		}
	}

	private void Log(string text)
	{
	}
}
