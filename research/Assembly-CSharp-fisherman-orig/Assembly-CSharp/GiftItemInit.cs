using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class GiftItemInit : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event GiftItemInit.SelectedItemHandler OnSelectedItemChange;

	public void Init(InventoryItem giftItem, GameObject parent)
	{
		this._giftItem = giftItem;
		this._toggle = base.gameObject.GetComponent<Toggle>();
		this._parentContent = parent;
		this.TitleText.text = giftItem.Name;
		this.thumbImage.Image = this.ThumbImage;
		this.thumbImage.Load(string.Format("Textures/Inventory/{0}", (giftItem.ThumbnailBID == null) ? null : giftItem.ThumbnailBID.ToString()));
		this.CountText.text = "x" + giftItem.Count;
		this._toggle.group = this._parentContent.GetComponent<ToggleGroup>();
	}

	public void SelectItem()
	{
		if (this._toggle.isOn)
		{
			this.Background.color = new Color(0.3137255f, 0.3137255f, 0.3137255f, 1f);
			this.OnSelectedItemChange(this._giftItem);
		}
		else
		{
			this.Background.color = new Color(0.23529412f, 0.23529412f, 0.23529412f, 1f);
		}
	}

	public Text TitleText;

	public Text CountText;

	public Image ThumbImage;

	private ResourcesHelpers.AsyncLoadableImage thumbImage = new ResourcesHelpers.AsyncLoadableImage();

	private GameObject _parentContent;

	private Toggle _toggle;

	private InventoryItem _giftItem;

	public Image Background;

	public delegate void SelectedItemHandler(InventoryItem i);
}
