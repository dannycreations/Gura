using System;
using System.Linq;
using ObjectModel;

[TriggerName(Name = "Rod bought")]
[Serializable]
public class RodBought : TutorialTrigger
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
		if (this.inventoryCount != PhotonConnectionFactory.Instance.Profile.Inventory.Count)
		{
			this.rodCount = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.Rod).Count<InventoryItem>();
			this.inventoryCount = PhotonConnectionFactory.Instance.Profile.Inventory.Count;
		}
		return this.rodCount >= 2;
	}

	private int inventoryCount;

	private int rodCount;
}
