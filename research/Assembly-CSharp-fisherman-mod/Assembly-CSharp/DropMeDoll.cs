using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropMeDoll : DropMe
{
	public virtual InventoryItemComponent InventoryItemView
	{
		get
		{
			return this._inventoryItemViewComponent;
		}
		set
		{
			this._inventoryItemViewComponent = value;
		}
	}

	public void VerifyRegisteredForFastEquip()
	{
		if (!InitRods.DropMeComponents.Contains(this))
		{
			InitRods.DropMeComponents.Add(this);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.VerifyRegisteredForFastEquip();
	}

	protected virtual void OnDisable()
	{
		if (InitRods.DropMeComponents.Contains(this) && (!base.transform.IsChildOf(InitRods.Instance.BodyView.transform) || this is DropMeDollChumHands))
		{
			InitRods.DropMeComponents.Remove(this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (InitRods.DropMeComponents.Contains(this))
		{
			InitRods.DropMeComponents.Remove(this);
		}
	}

	public override void OnDrop(PointerEventData data)
	{
		this.containerImage.color = this.normalColor;
		if (this.receivingImage == null || this.DragNDropTypeInst == null || !Array.Exists<int>(this.typeId, (int x) => x == this.DragNDropTypeInst.CurrentActiveTypeId))
		{
			return;
		}
		InventoryItem dropObject = base.GetDropObject(data);
		if (this.TransferItem(dropObject, this.InventoryItemView.InventoryItem))
		{
			this.InventoryItemView.Set(null, false);
		}
	}

	protected virtual bool TransferItem(InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously)
	{
		if (dragNDropContent is Chum)
		{
			float chumHandCapacity = Inventory.ChumHandCapacity;
			return this.MakeSplit(null, dragNDropContent, dragNDropContentPreviously, chumHandCapacity);
		}
		if (dragNDropContentPreviously != null)
		{
			if (dragNDropContent is Rod)
			{
				if (PhotonConnectionFactory.Instance.Profile.Inventory.CanSubordinate(dragNDropContentPreviously, dragNDropContent))
				{
					this.SubordinateItem(dragNDropContentPreviously, dragNDropContent);
					return true;
				}
				GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
				return false;
			}
			else
			{
				if (PhotonConnectionFactory.Instance.Profile.Inventory.CanReplace(dragNDropContentPreviously, dragNDropContent))
				{
					this.ReplaceItem(dragNDropContentPreviously, dragNDropContent);
					return true;
				}
				GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
				return false;
			}
		}
		else
		{
			if (PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(dragNDropContent, null, StoragePlaces.Doll, true))
			{
				this.MoveItemOrCombine(dragNDropContent, null, StoragePlaces.Doll, true);
				return true;
			}
			GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
			return false;
		}
	}

	public override void OnPointerEnter(PointerEventData data)
	{
		if (this.containerImage == null)
		{
			return;
		}
		Sprite dropSprite = base.GetDropSprite(data);
		if (dropSprite != null)
		{
			if (this.DragNDropTypeInst.IsActive && Array.Exists<int>(this.typeId, (int x) => x == this.DragNDropTypeInst.CurrentActiveTypeId))
			{
				this.containerImage.color = this.highlightColor;
				this._isHovered = true;
			}
			else
			{
				this.containerImage.color = this.normalColor;
				this._isHovered = false;
			}
		}
	}

	public override void OnPointerExit(PointerEventData data)
	{
		if (this.containerImage == null)
		{
			return;
		}
		this._isHovered = false;
		this.containerImage.color = this.normalColor;
	}

	protected override void Highlight()
	{
		if (this.containerImage != null)
		{
			if (this.DragNDropTypeInst.IsActive && Array.Exists<int>(this.typeId, (int x) => x == this.DragNDropTypeInst.CurrentActiveTypeId))
			{
				if (this._isHovered)
				{
					this.containerImage.color = this.HoverringColor;
				}
				else
				{
					this.containerImage.color = this.highlightColor;
				}
			}
			else
			{
				this.containerImage.color = this.normalColor;
			}
		}
	}

	protected bool MakeSplit(InventoryItem parent, InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously, int count)
	{
		if (dragNDropContentPreviously == null && PhotonConnectionFactory.Instance.Profile.Inventory.CanSplit(dragNDropContent, count) && PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(dragNDropContent, parent, StoragePlaces.ParentItem, false))
		{
			this.SplitItem(dragNDropContent, parent, count, StoragePlaces.ParentItem);
			return true;
		}
		if (dragNDropContentPreviously != null && PhotonConnectionFactory.Instance.Profile.Inventory.CanSplit(dragNDropContent, count) && PhotonConnectionFactory.Instance.Profile.Inventory.CanReplace(dragNDropContentPreviously, dragNDropContent))
		{
			this.SplitItemAndReplace(dragNDropContentPreviously, dragNDropContent, count);
			return true;
		}
		GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
		return false;
	}

	protected bool MakeSplit(InventoryItem parent, InventoryItem dragNDropContent, InventoryItem dragNDropContentPreviously, float amount)
	{
		if (dragNDropContentPreviously == null && PhotonConnectionFactory.Instance.Profile.Inventory.CanSplit(dragNDropContent, amount) && PhotonConnectionFactory.Instance.Profile.Inventory.CanMove(dragNDropContent, parent, (parent != null) ? StoragePlaces.ParentItem : StoragePlaces.Doll, false))
		{
			this.SplitItem(dragNDropContent, parent, amount, (parent != null) ? StoragePlaces.ParentItem : StoragePlaces.Doll);
			return true;
		}
		if (dragNDropContentPreviously != null && PhotonConnectionFactory.Instance.Profile.Inventory.CanSplit(dragNDropContent, amount) && PhotonConnectionFactory.Instance.Profile.Inventory.CanReplace(dragNDropContentPreviously, dragNDropContent))
		{
			this.SplitItemAndReplace(dragNDropContentPreviously, dragNDropContent, amount);
			return true;
		}
		GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
		return false;
	}

	public Color HoverringColor = Color.green;

	protected InventoryItemComponent _inventoryItemViewComponent;

	private bool _isHovered;
}
