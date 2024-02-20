using System;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class InitLocalStorage : InitStorages
{
	protected override void OnInventoryUpdated()
	{
		if (PhotonConnectionFactory.Instance.Profile.Inventory.Any((InventoryItem x) => x.ItemSubType == ItemSubTypes.Lodge && ((Lodge)x).PondId == StaticUserData.CurrentPond.PondId))
		{
			this.ToggleLodge.GetComponent<Toggle>().enabled = true;
			this.ToggleLodge.GetComponent<ToggleStateChanges>().Enable();
		}
		else
		{
			this.ToggleLodge.GetComponent<ToggleStateChanges>().Disable();
			this.ToggleLodge.GetComponent<Toggle>().enabled = false;
		}
	}

	public GameObject ToggleLodge;
}
