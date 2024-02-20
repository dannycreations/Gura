using System;
using System.Linq;
using ObjectModel;
using TMPro;

public class RodStandBehavior : ChangeHandler
{
	private void Awake()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnChange;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnChange;
	}

	private void OnEnable()
	{
		this.OnChange();
	}

	protected override void Unequip()
	{
		this.OnChange();
	}

	public override void OnChange()
	{
		if (PhotonConnectionFactory.Instance.Profile == null)
		{
			return;
		}
		InventoryItem inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.Storage == StoragePlaces.Doll && x.ItemSubType == ItemSubTypes.RodStand);
		if (inventoryItem == null)
		{
			this.Counter.text = string.Empty;
			return;
		}
		int standCount = (inventoryItem as RodStand).StandCount;
		this.Counter.text = string.Format("{0} / {1}", RodPodHelper.GetUnusedCount(), standCount);
	}

	public TextMeshProUGUI Counter;
}
