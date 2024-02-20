using System;
using System.Collections.Generic;
using ObjectModel;

public class NameResolvingHelpers
{
	public static string GetStorageName(StoragePlaces storage)
	{
		return NameResolvingHelpers._storagesNames[storage];
	}

	public static string GetStorageName(InventoryItem item)
	{
		if (PhotonConnectionFactory.Instance.Profile.Inventory.StorageExceededInventory.Contains(item))
		{
			return "ItemPlacedToEscessTab";
		}
		return NameResolvingHelpers._storagesNames[item.Storage];
	}

	private static Dictionary<StoragePlaces, string> _storagesNames = new Dictionary<StoragePlaces, string>
	{
		{
			StoragePlaces.Equipment,
			"Equipment"
		},
		{
			StoragePlaces.Storage,
			"HomeStorage"
		},
		{
			StoragePlaces.ParentItem,
			"Parent"
		},
		{
			StoragePlaces.Hands,
			"Hands"
		},
		{
			StoragePlaces.Doll,
			"Doll"
		},
		{
			StoragePlaces.Shore,
			"Shore"
		},
		{
			StoragePlaces.CarEquipment,
			"Car"
		},
		{
			StoragePlaces.LodgeEquipment,
			"Lodge"
		}
	};
}
