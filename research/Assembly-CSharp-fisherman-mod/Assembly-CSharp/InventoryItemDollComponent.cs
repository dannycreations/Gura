using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine.UI;

public class InventoryItemDollComponent : InventoryItemComponent
{
	public Image InventoryImage
	{
		get
		{
			if (this._invImage == null)
			{
				this._invImage = base.GetComponent<Image>();
			}
			return this._invImage;
		}
	}

	public override void Set(InventoryItem inventoryItem, bool shouldCallChangeHandler = true)
	{
		base.StopAllCoroutines();
		this.ClearSlot();
		HintItemId hintItemId = base.GetComponent<HintItemId>();
		if (hintItemId == null && !this.isOtherPlayer)
		{
			hintItemId = base.gameObject.AddComponent<HintItemId>();
		}
		Chum chum = inventoryItem as Chum;
		bool flag = chum != null;
		if (this._damageIcoChum != null)
		{
			this._damageIcoChum.gameObject.SetActive(flag);
			if (flag)
			{
				this._damageIcoChum.Init(chum);
			}
		}
		if (this._damageIco != null)
		{
			this._damageIco.gameObject.SetActive(!flag);
			if (!flag)
			{
				this._damageIco.Init(inventoryItem);
			}
		}
		if (inventoryItem != null)
		{
			if (inventoryItem.Equals(this.InventoryItem))
			{
				return;
			}
			this.InventoryItem = inventoryItem;
			if (!this.isOtherPlayer)
			{
				hintItemId.SetItemId(inventoryItem, new List<string>
				{
					inventoryItem.ItemType.ToString(),
					inventoryItem.ItemSubType.ToString()
				});
				hintItemId.SetRemoveOnDisable(true);
			}
			if (this.InventoryImage != null)
			{
				this._imageLoadable.Image = this.InventoryImage;
				this._imageLoadable.Load((this.InventoryItem.DollThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", this.InventoryItem.DollThumbnailBID));
			}
		}
		else
		{
			this.InventoryItem = null;
			if (!this.isOtherPlayer)
			{
				hintItemId.Remove();
			}
			DropMe component = base.GetComponent<DropMe>();
			if (component != null)
			{
				component.receivingImage.overrideSprite = base.GetComponent<DragMeDoll>().BaseImage;
			}
			else if (this.InventoryImage != null)
			{
				this.InventoryImage.overrideSprite = ResourcesHelpers.GetTransparentSprite();
			}
		}
		if (this.ChangeHandler != null && shouldCallChangeHandler)
		{
			this.ChangeHandler.OnChange();
		}
	}

	public void ClearSlot()
	{
		if (base.GetComponent<DropMeDoll>() != null)
		{
			base.GetComponent<DropMeDoll>().InventoryItemView = this;
		}
		if (!this.isOtherPlayer)
		{
			HintItemId hintItemId = base.GetComponent<HintItemId>() ?? base.gameObject.AddComponent<HintItemId>();
			hintItemId.Remove();
		}
		this.InventoryItem = null;
		DropMe component = base.GetComponent<DropMe>();
		if (component != null)
		{
			component.receivingImage.overrideSprite = base.GetComponent<DragMeDoll>().BaseImage;
		}
		else if (this.InventoryImage != null)
		{
			this.InventoryImage.overrideSprite = null;
		}
		if (this._damageIcoChum != null)
		{
			this._damageIcoChum.Init(null);
		}
		if (this._damageIco != null)
		{
			this._damageIco.Init(null);
		}
	}

	public bool isOtherPlayer;

	private Image _invImage;

	private ResourcesHelpers.AsyncLoadableImage _imageLoadable = new ResourcesHelpers.AsyncLoadableImage();
}
