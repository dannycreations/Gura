using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

public class StartStep18Trigger : StartTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		if (this.ExitZone != null)
		{
			this.ExitZone.SetActive(false);
		}
		if (this.FishingZoneDisable1 != null)
		{
			this.FishingZoneDisable1.SetActive(false);
		}
		if (this.FishingZoneDisable2 != null)
		{
			this.FishingZoneDisable2.SetActive(false);
		}
		return (this.ZoneActivation.InZone || GameFactory.Player.State == typeof(PlayerIdlePitch)) && !PondControllers.Instance.IsInMenu && !MenuHelpers.Instance.MenuPrefabsList.IsLoadingOrTransfer();
	}

	public ZoneTutorialStep ZoneActivation;

	public GameObject ExitZone;

	public GameObject FishingZoneDisable1;

	public GameObject FishingZoneDisable2;
}
