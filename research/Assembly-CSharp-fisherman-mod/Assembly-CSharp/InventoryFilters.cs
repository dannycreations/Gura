using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;

public static class InventoryFilters
{
	public static Func<StoragePlaces, InventoryItem> GetItemByStorage(Guid instanceId, HintMessage message, bool hasInstanceId)
	{
		return (StoragePlaces storage) => PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => ((hasInstanceId && x.InstanceId != null && x.InstanceId.Value == instanceId) || (!hasInstanceId && x.ItemId == message.ItemId)) && x.Storage == storage && (message.DisplayStorage == (StoragePlaces)0 || x.Storage == message.DisplayStorage));
	}

	public static Func<StoragePlaces, IEnumerable<InventoryItem>> GetItemsByStorage(Guid instanceId, HintMessage message, bool hasInstanceId)
	{
		return (StoragePlaces storage) => PhotonConnectionFactory.Instance.Profile.Inventory.Where((InventoryItem x) => (x.ItemSubType == message.ItemSubType || x.ItemType == (ItemTypes)message.ItemSubType) && x.Storage == storage && (message.DisplayStorage == (StoragePlaces)0 || x.Storage == message.DisplayStorage));
	}

	public static Func<StoragePlaces, List<InventoryItem>> GetListItemsByStorage(Guid instanceId, HintMessage message, bool hasInstanceId, bool hasItemId)
	{
		return delegate(StoragePlaces storage)
		{
			if (!hasItemId)
			{
				return InventoryFilters.GetItemsByStorage(instanceId, message, hasInstanceId)(storage).ToList<InventoryItem>();
			}
			InventoryItem inventoryItem = InventoryFilters.GetItemByStorage(instanceId, message, hasInstanceId)(storage);
			return (inventoryItem == null) ? new List<InventoryItem>() : new List<InventoryItem> { inventoryItem };
		};
	}

	public static ManagedHint.FilterFunction CorrectSlot(bool isRod, HintMessage message)
	{
		return () => (isRod && InitRods.Instance != null && (InitRods.Instance.ActiveRod.Rod.InventoryItem == null || RodHelper.GetSlotCount() == RodHelper.FindAllUsedRods().Count)) || (InitRods.Instance != null && InitRods.Instance.ActiveRod.SlotId == message.Slot);
	}
}
