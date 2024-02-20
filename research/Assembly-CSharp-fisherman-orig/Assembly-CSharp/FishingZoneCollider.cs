using System;
using UnityEngine;

public class FishingZoneCollider : MonoBehaviour
{
	private void Awake()
	{
		base.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
	}

	internal void OnTriggerEnter(Collider visitor)
	{
		if (GameFactory.FishingZonesPlayerCollider == visitor)
		{
			GameFactory.Player.OnEnterFishingZone(base.gameObject);
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
		}
		else if (GameFactory.Player != null)
		{
			GameFactory.Player.OnColliderExitFishZone(visitor);
		}
	}
}
