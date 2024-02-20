using System;
using UnityEngine;

public class StartStep19Trigger : StartTutorialTriggerContainer
{
	public override bool IsTriggering()
	{
		if (this.ExitZone != null)
		{
			this.ExitZone.SetActive(false);
		}
		return this.ZoneActivation.InZone || GameFactory.Player.State == typeof(PlayerIdlePitch);
	}

	public ZoneTutorialStep ZoneActivation;

	public GameObject ExitZone;
}
