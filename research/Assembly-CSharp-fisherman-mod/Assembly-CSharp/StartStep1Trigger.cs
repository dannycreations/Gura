using System;
using UnityEngine;

public class StartStep1Trigger : StartTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		if (this.ExitZone != null)
		{
			this.ExitZone.SetActive(false);
		}
		if (this.ExitZone2 != null)
		{
			this.ExitZone2.SetActive(false);
		}
		if (((this._firstActivated && !this.ZoneActivation.InZone) || this.StartZoneActivation.InZone) && !ManagerScenes.InTransition && !ManagerScenes.Instance.LoadingForm.isActive && GameFactory.SkyControllerInstance != null && GameFactory.SkyControllerInstance.IsFirstSkyInited() && GameFactory.Player.isActiveAndEnabled)
		{
			this._firstActivated = true;
			return true;
		}
		return false;
	}

	private bool _firstActivated;

	public ZoneTutorialStep ZoneActivation;

	public ZoneTutorialStep StartZoneActivation;

	public GameObject ExitZone;

	public GameObject ExitZone2;
}
