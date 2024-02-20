using Gura.Patchs;
using ObjectModel;
using System;

namespace Gura.Utils;

public static class ItemUtil
{
    private static long ChangeItemCooldown;

    public static void MakeSplit(InventoryItem parent, InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously, int count)
    {
        if (Date.Now < ChangeItemCooldown) return;

        if (dragNDropContentPreviously == null &&
            PhotonConnectionFactory.Instance.Profile.Inventory.CanSplit(dragNDropContent, count) &&
            PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(dragNDropContent, parent, StoragePlaces.ParentItem, false))
        {
            ChangeItemCooldown = Date.Now + 10_000;
            Plugin.Log.LogMessage($"Item split {dragNDropContent.Name.Humanize()} ({dragNDropContent.Count})");
            PhotonConnectionFactory.Instance.SplitItem(dragNDropContent, parent, count, StoragePlaces.ParentItem);
            PlayerIdleThrownPatch.RequestDrawOut = true;
        }

        if (dragNDropContentPreviously != null &&
            PhotonConnectionFactory.Instance.Profile.Inventory.CanSplit(dragNDropContent, count) &&
            PhotonConnectionFactory.Instance.Profile.Inventory.CanReplace(dragNDropContentPreviously, dragNDropContent))
        {
            ChangeItemCooldown = Date.Now + 10_000;
            Plugin.Log.LogMessage($"Item replace {dragNDropContent.Name.Humanize()} ({dragNDropContent.Count})");
            PhotonConnectionFactory.Instance.SplitItemAndReplace(dragNDropContentPreviously, dragNDropContent, count);
            PlayerIdleThrownPatch.RequestDrawOut = true;
        }
    }

    public static InventoryItemDollComponent ActiveRod =>
        InitRods.Instance.ActiveRod.Rod;
}