using System;
using Assets.Scripts.Common.Managers;
using UnityEngine;

public class ZoneExitEnabler : MonoBehaviour
{
	private void Start()
	{
		base.gameObject.SetActive(false);
	}

	private void OnTriggerEnter(Collider visitor)
	{
		if (GameFactory.FishingZonesPlayerCollider == visitor)
		{
			KeysHandlerAction.ExitZoneHandler();
		}
	}
}
