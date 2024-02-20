using System;
using Assets.Scripts.Common.Managers.Helpers;

public class StartStep16Trigger : StartTutorialTriggerContainer
{
	private void Update()
	{
		if (this._helper.MenuPrefabsList != null && this._helper.MenuPrefabsList.globalMapForm.activeSelf && this._helper.MenuPrefabsList.globalMapFormAS.isActive && this.MapPanel == null && ShowLocationInfo.Instance != null)
		{
			this.MapPanel = ShowLocationInfo.Instance.GetComponentInChildren<SetLocationsOnGlobalMap>();
		}
		if (this.MapPanel != null && this._helper.MenuPrefabsList != null && this._helper.MenuPrefabsList.globalMapForm.activeSelf && this._helper.MenuPrefabsList.globalMapFormAS.isActive && this.MapPanel.Inited)
		{
			this._isTriggering = true;
		}
		else
		{
			this._isTriggering = false;
		}
	}

	public SetLocationsOnGlobalMap MapPanel;

	private MenuHelpers _helper = new MenuHelpers();
}
