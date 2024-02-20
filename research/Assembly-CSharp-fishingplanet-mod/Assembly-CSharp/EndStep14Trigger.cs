using System;
using System.Linq;
using ObjectModel;

public class EndStep14Trigger : EndTutorialTriggerContainer
{
	private void Update()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		bool flag;
		if (profile != null && profile.Inventory != null)
		{
			flag = profile.Inventory.Any((InventoryItem x) => x.ItemId == 1426);
		}
		else
		{
			flag = false;
		}
		this._isTriggering = flag;
	}
}
