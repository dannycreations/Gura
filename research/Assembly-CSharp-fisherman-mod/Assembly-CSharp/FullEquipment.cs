using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;
using UnityEngine;

[TriggerName(Name = "Equipment is full")]
[Serializable]
public class FullEquipment : TutorialTrigger
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
			this.inventoryCount = PhotonConnectionFactory.Instance.Profile.Inventory.Count;
			this.result = this.CheckEquipment();
		}
		return this.result;
	}

	private bool CheckEquipment()
	{
		bool flag = false;
		int num = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.Reel).Count<InventoryItem>();
		int num2 = PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Reel].Count;
		flag = flag || num > num2;
		num = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.Rod).Count<InventoryItem>();
		num2 = PhotonConnectionFactory.Instance.Profile.Inventory.DollConstraintsCache()[ItemSubTypes.Rod].Count;
		flag = flag || num > num2;
		num = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.Bait).Count<InventoryItem>();
		num2 = PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Bait].Count;
		flag = flag || num > num2;
		num = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.Line).Count<InventoryItem>();
		num2 = PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Line].Count;
		flag = flag || num > num2;
		num = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.Lure).Count<InventoryItem>();
		num2 = PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Lure].Count;
		flag = flag || num > num2;
		num = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.Hook).Count<InventoryItem>();
		num2 = PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Hook].Count;
		flag = flag || num > num2;
		num = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.Bobber).Count<InventoryItem>();
		num2 = PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.Bobber].Count;
		flag = flag || num > num2;
		num = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.JigHead).Count<InventoryItem>();
		num2 = PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.JigHead].Count;
		flag = flag || num > num2;
		num = PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem item) => item.ItemType == ItemTypes.JigBait).Count<InventoryItem>();
		num2 = PhotonConnectionFactory.Instance.Profile.Inventory.EquipConstraintsCache()[ItemSubTypes.JigBait].Count;
		flag = flag || num > num2;
		Debug.LogWarning("CheckEquipment");
		return flag;
	}

	private static MenuHelpers _helpers = new MenuHelpers();

	private int inventoryCount;

	private bool result;
}
