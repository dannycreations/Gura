using ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Gura.Utils;

public static class RodUtil
{
    public static bool GetFreeRodSlot(out List<int> slots)
    {
        var listSlot = new List<int>();
        foreach (var item in FindItemAtBackpack(ItemTypes.Rod))
            if (!PodUtil.FindRodSlotByRodId(item.InstanceId, out _))
                listSlot.Add(item.Slot);

        slots = listSlot;
        return listSlot.Count > 0;
    }

    public static InventoryItem[] FindItemAtBackpack(ItemTypes itemType)
    {
        return (from r in GetItemsAtBackpack()
                where r.ItemType == itemType
                select r).ToArray();
    }

    public static InventoryItem[] GetItemsAtBackpack()
    {
        return (from r in StateUtil.Inventory.Items
                where r.Storage != StoragePlaces.Storage
                select r).ToArray();
    }
}