using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

public class EndStep16Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		if (this.MapPanel == null && ShowLocationInfo.Instance != null)
		{
			this.MapPanel = ShowLocationInfo.Instance.GetComponentInChildren<SetLocationsOnGlobalMap>().MapTexture.transform;
		}
		if (this.MapPanel == null)
		{
			return;
		}
		for (int i = 0; i < this.MapPanel.childCount; i++)
		{
			Transform child = this.MapPanel.GetChild(i);
			LocationPin component = child.GetComponent<LocationPin>();
			if (child.gameObject.activeInHierarchy && component != null && component.RLDA.CurrentLocationBrief.LocationId == 10121 && component.Toggle.isOn)
			{
				this._isTriggering = true;
			}
		}
	}

	public Transform MapPanel;

	private MenuHelpers _helper = new MenuHelpers();
}
