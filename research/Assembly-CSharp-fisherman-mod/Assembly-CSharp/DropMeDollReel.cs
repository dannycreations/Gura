using System;
using Assets.Scripts.UI._2D.Inventory;
using ObjectModel;
using UnityEngine.EventSystems;

public class DropMeDollReel : DropMeDollTackle, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	protected override bool TransferItem(InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously)
	{
		if (!this.CanEquipNow(dragNDropContent))
		{
			return false;
		}
		if (!(this.rod != null) || this.rod.InventoryItem == null)
		{
			return false;
		}
		if (InventoryHelper.IsBlocked2Equip(this.rod.InventoryItem, dragNDropContent, false))
		{
			return false;
		}
		if (dragNDropContentPreviously != null)
		{
			if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanReplace(dragNDropContentPreviously, dragNDropContent))
			{
				GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
				return false;
			}
			PhotonConnectionFactory.Instance.ReplaceItem(dragNDropContentPreviously, dragNDropContent);
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
		base.GetComponent<ChangeHandler>().Refresh();
		GameFactory.Message.HideMessage();
		return true;
	}

	protected override void Highlight()
	{
		if (this.containerImage != null)
		{
			if (this.DragNDropTypeInst.IsActive && this.rod.InventoryItem != null && ((this.rod.InventoryItem.ItemSubType == ItemSubTypes.CastingRod && this.DragNDropTypeInst.CurrentActiveTypeId == 32) || (this.rod.InventoryItem.ItemSubType != ItemSubTypes.CastingRod && this.DragNDropTypeInst.CurrentActiveTypeId == 30)))
			{
				this.containerImage.color = this.highlightColor;
			}
			else
			{
				this.containerImage.color = this.normalColor;
			}
		}
	}

	public override bool CanEquipNow(InventoryItem itemToEquip)
	{
		if ((Rod)base.GetComponent<ChangeHandler>().InitRod.Rod.InventoryItem == null)
		{
			GameFactory.Message.ShowRodMustBeSetup(base.transform.root.gameObject);
			return false;
		}
		return true;
	}
}
