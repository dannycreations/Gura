using System;
using Assets.Scripts.UI._2D.Inventory;
using ObjectModel;

public class DropMeDollLure : DropMeDollTackle
{
	protected override bool TransferItem(InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously)
	{
		if (!this.CanEquipNow(dragNDropContent))
		{
			return false;
		}
		if (this.rod.InventoryItem != null)
		{
			if (InventoryHelper.IsBlocked2Equip(this.rod.InventoryItem, dragNDropContent, false))
			{
				return false;
			}
			Reel reel = (Reel)base.GetComponent<ChangeHandler>().InitRod.Reel.InventoryItem;
			base.CheckWeightsAndSpawnHelpMessages(dragNDropContent);
			if (dragNDropContent.IsUnstockable)
			{
				if (dragNDropContentPreviously != null)
				{
					if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanReplace(dragNDropContentPreviously, dragNDropContent))
					{
						GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
						return false;
					}
					this.ReplaceItem(dragNDropContentPreviously, dragNDropContent);
				}
				else
				{
					if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(dragNDropContent, this.rod.InventoryItem, StoragePlaces.ParentItem, true))
					{
						GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
						return false;
					}
					this.MoveItemOrCombine(dragNDropContent, this.rod.InventoryItem, StoragePlaces.ParentItem, true);
				}
			}
			else if (dragNDropContent is Line)
			{
				int capacityLineOnReel = DropMeDollTackle.GetCapacityLineOnReel(dragNDropContent, reel);
				if (!base.MakeSplit(this.rod.InventoryItem, dragNDropContent, dragNDropContentPreviously, (float)capacityLineOnReel))
				{
					return false;
				}
			}
			else if (!base.MakeSplit(this.rod.InventoryItem, dragNDropContent, dragNDropContentPreviously, 1))
			{
				return false;
			}
			base.GetComponent<ChangeHandler>().Refresh();
		}
		return true;
	}

	public override bool CanEquipNow(InventoryItem itemToEquip)
	{
		DropMeDollHook component = base.GetComponent<ChangeHandler>().InitRod.LureHook.GetComponent<DropMeDollHook>();
		if (!component.CanEquipNow(itemToEquip))
		{
			return false;
		}
		Rod rod = (Rod)base.GetComponent<ChangeHandler>().InitRod.Rod.InventoryItem;
		Hook hook = (Hook)base.GetComponent<ChangeHandler>().InitRod.LureHook.InventoryItem;
		Bobber bobber = (Bobber)base.GetComponent<ChangeHandler>().InitRod.Tackle.InventoryItem;
		if (rod.ItemSubType.IsRodWithLure())
		{
			RodTemplate closestTemplateWith = InventoryHelper.GetClosestTemplateWith(rod, itemToEquip);
			if (!closestTemplateWith.IsSpinningFishingTemplate())
			{
				GameFactory.Message.ShowHookMustBeSetup(base.transform.root.gameObject);
				return false;
			}
		}
		if (rod.ItemSubType.IsRodWithBobber())
		{
			if (bobber == null)
			{
				GameFactory.Message.ShowTackleMustBeSetup(base.transform.root.gameObject);
				return false;
			}
			if (hook == null)
			{
				GameFactory.Message.ShowHookMustBeSetup(base.transform.root.gameObject);
				return false;
			}
		}
		return true;
	}
}
