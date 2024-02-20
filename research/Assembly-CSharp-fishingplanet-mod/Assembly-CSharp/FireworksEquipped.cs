using System;
using System.Linq;
using ObjectModel;

[TriggerName(Name = "Fireworks equipped")]
[Serializable]
public class FireworksEquipped : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		if (PhotonConnectionFactory.Instance.Profile == null || PhotonConnectionFactory.Instance.Profile.Inventory == null)
		{
			return false;
		}
		InventoryItem[] items = PhotonConnectionFactory.Instance.Profile.Inventory.Items;
		return PhotonConnectionFactory.Instance.Profile.Inventory.Items.Any((InventoryItem i) => i.ItemSubType == ItemSubTypes.Firework && i.Storage == StoragePlaces.Equipment);
	}
}
