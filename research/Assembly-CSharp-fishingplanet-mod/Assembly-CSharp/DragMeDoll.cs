using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragMeDoll : DragMe, IBeginDragHandler, IDragHandler, IEndDragHandler, IEventSystemHandler
{
	public virtual void Clear()
	{
		DropMe component = base.GetComponent<DropMe>();
		if (component == null)
		{
			return;
		}
		component.receivingImage.overrideSprite = this.BaseImage;
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		this.DragNDropContent = base.GetComponent<InventoryItemDollComponent>();
		base.OnBeginDrag(eventData);
	}

	protected override void SetDraggingIcon(Image image)
	{
		this._imageLdbl.Image = image;
		this._imageLdbl.Load((this.DragNDropContent.InventoryItem == null || this.DragNDropContent.InventoryItem.ThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", this.DragNDropContent.InventoryItem.ThumbnailBID.ToString()));
	}

	public Sprite BaseImage;
}
