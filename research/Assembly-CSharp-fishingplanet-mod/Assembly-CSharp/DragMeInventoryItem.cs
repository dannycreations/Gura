using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragMeInventoryItem : DragMe
{
	public void Init(InventoryItem item, DragNDropType typeObjectOfDragItem)
	{
		if (this.m_DraggingIcon != null)
		{
			this.OnEndDrag(null);
		}
		this._item = item;
		this.DranNDropType = typeObjectOfDragItem;
		this.DragNDropContent = base.GetComponent<InventoryItemComponent>();
		if (this.IsChumExpired)
		{
			this.ImageLoadable.Load(string.Format("Textures/Inventory/{0}", "sour-foodball"), this.Image);
		}
		else
		{
			this.ImageLoadable.Load(this._item.ThumbnailBID, this.Image, "Textures/Inventory/{0}");
		}
		this.typeId = (int)item.ItemSubType;
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		base.OnBeginDrag(eventData);
		this.Image.color = new Color(this.Image.color.r, this.Image.color.g, this.Image.color.b, 0.3f);
	}

	protected override void SetDraggingIcon(Image image)
	{
		image.overrideSprite = this.Image.overrideSprite;
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (this.m_DraggingIcon != null)
		{
			Object.Destroy(this.m_DraggingIcon);
		}
		this.DranNDropType.IsActive = false;
		this.Image.color = new Color(this.Image.color.r, this.Image.color.g, this.Image.color.b, 1f);
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.DropClip, SettingsManager.InterfaceVolume);
	}

	protected bool IsChumExpired
	{
		get
		{
			Chum chum = this._item as Chum;
			return chum != null && chum.IsExpired;
		}
	}

	public Image Image;

	private ResourcesHelpers.AsyncLoadableImage ImageLoadable = new ResourcesHelpers.AsyncLoadableImage();

	private InventoryItem _item;
}
