using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropMeStorage : DropMe
{
	public override void OnDrop(PointerEventData data)
	{
		if (this.containerImage != null)
		{
			this.containerImage.color = this.normalColor;
		}
		if (this.DragNDropTypeInst == null || !Array.Exists<int>(this.typeId, (int x) => x == this.DragNDropTypeInst.CurrentActiveTypeId))
		{
			return;
		}
		DragMe dragObject = this.GetDragObject(data);
		if (dragObject == null || dragObject.DranNDropType.CurrentActiveStorage == this.storage)
		{
			return;
		}
		InventoryItemComponent dragNDropContent = dragObject.DragNDropContent;
		if (this.TransferItem(dragNDropContent))
		{
			DragMeDoll component = dragObject.GetComponent<DragMeDoll>();
			if (component != null)
			{
				component.Clear();
			}
		}
	}

	protected virtual bool TransferItem(InventoryItemComponent dragNDropContent)
	{
		if (dragNDropContent.InventoryItem != null)
		{
			InventoryItem inventoryItem = dragNDropContent.InventoryItem;
			if (PhotonConnectionFactory.Instance.Profile.Inventory.CanMoveOrCombineItem(inventoryItem, null, this.storage))
			{
				dragNDropContent.Set(null, true);
				this.MoveItemOrCombine(inventoryItem, null, this.storage, !(inventoryItem is Rod));
				return true;
			}
			if (!string.IsNullOrEmpty(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError))
			{
				GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, dragNDropContent.transform.root.gameObject);
			}
		}
		return false;
	}

	public override void OnPointerEnter(PointerEventData data)
	{
		if (this.containerImage == null)
		{
			return;
		}
		if (data.pointerDrag != null)
		{
			ChumComponent component = data.pointerDrag.GetComponent<ChumComponent>();
			if (component != null)
			{
				component.PointerOnStorage(true);
			}
		}
		Sprite dropSprite = base.GetDropSprite(data);
		if (dropSprite != null)
		{
			this.containerImage.color = this.highlightColor;
			this._isHovered = true;
		}
	}

	public override void OnPointerExit(PointerEventData data)
	{
		if (this.containerImage == null)
		{
			return;
		}
		if (data.pointerDrag != null)
		{
			ChumComponent component = data.pointerDrag.GetComponent<ChumComponent>();
			if (component != null)
			{
				component.PointerOnStorage(false);
			}
		}
		this._isHovered = false;
		this.containerImage.color = this.normalColor;
	}

	protected override void Highlight()
	{
		if (this.containerImage != null)
		{
			if (this.DragNDropTypeInst.IsActive && Array.Exists<int>(this.typeId, (int x) => x == this.DragNDropTypeInst.CurrentActiveTypeId) && this.DragNDropTypeInst.CurrentActiveStorage != this.storage)
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

	protected DragMe GetDragObject(PointerEventData data)
	{
		GameObject pointerDrag = data.pointerDrag;
		if (pointerDrag == null)
		{
			return null;
		}
		return pointerDrag.GetComponent<DragMe>();
	}

	public Color HoverringColor = Color.green;

	private bool _isHovered;

	public StoragePlaces storage;
}
