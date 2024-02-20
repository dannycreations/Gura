using System;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class MissionItemComponent : MonoBehaviour
{
	public void Init(MissionItem item)
	{
		this._rect = base.GetComponent<RectTransform>();
		this._canvas = base.GetComponent<Canvas>();
		this.InventoryItem = item;
		this.ImageLoadable.Image = this.Image;
		this.ImageLoadable.Load((this.InventoryItem.ThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", this.InventoryItem.ThumbnailBID));
		if (this.sdi != null)
		{
			this.sdi.Init(item);
		}
		this.Name.text = this.InventoryItem.Name;
		this.Description.text = InventoryParamsHelper.ParseParamsInfo(this.InventoryItem, false);
		string text = InventoryHelper.ItemCountStr(this.InventoryItem);
		this.Count.GetComponent<Text>().text = text;
		this.Count.gameObject.SetActive(!string.IsNullOrEmpty(text));
	}

	public void Update()
	{
		if (this._rect == null)
		{
			this._rect = base.GetComponent<RectTransform>();
		}
		if (this._canvas == null)
		{
			this._canvas = base.GetComponent<Canvas>();
		}
		if (this.parentMaskRect == null)
		{
			this.parentMaskRect = base.GetComponentInParent<Mask>().rectTransform;
		}
		if (this.AreCoordsWithinUiObject(this._rect, this.parentMaskRect.gameObject))
		{
			this.Name.gameObject.SetActive(true);
			this.Description.gameObject.SetActive(true);
			this._canvas.enabled = true;
		}
		else
		{
			this.Name.gameObject.SetActive(false);
			this.Description.gameObject.SetActive(false);
			this._canvas.enabled = false;
		}
	}

	private bool AreCoordsWithinUiObject(RectTransform rTransform, GameObject gameObj)
	{
		Vector3 vector = rTransform.TransformPoint(rTransform.rect.max);
		Vector3 vector2 = rTransform.TransformPoint(rTransform.rect.min);
		Vector3 vector3 = gameObj.transform.InverseTransformPoint(vector2);
		Vector3 vector4 = gameObj.transform.InverseTransformPoint(vector);
		return ((RectTransform)gameObj.transform).rect.Contains(vector3) || ((RectTransform)gameObj.transform).rect.Contains(vector4);
	}

	public virtual void Set(MissionItem inventoryItem)
	{
		this.InventoryItem = inventoryItem;
		if (this.ChangeHandler != null)
		{
			this.ChangeHandler.OnChange();
		}
	}

	public bool Clear(StoragePlaces storage)
	{
		if (this.InventoryItem != null)
		{
			if (PhotonConnectionFactory.Instance.Profile.Inventory.CanMoveOrCombineItem(this.InventoryItem, null, storage))
			{
				PhotonConnectionFactory.Instance.MoveItemOrCombine(this.InventoryItem, null, storage, true);
				this.Set(null);
				return true;
			}
			GameFactory.Message.ShowCanNotMove(PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError, base.transform.root.gameObject);
		}
		return false;
	}

	public void ToEquipment()
	{
		PhotonConnectionFactory.Instance.MoveItemOrCombine(this.InventoryItem, null, StoragePlaces.Equipment, true);
	}

	public void ToCar()
	{
		PhotonConnectionFactory.Instance.MoveItemOrCombine(this.InventoryItem, null, StoragePlaces.CarEquipment, true);
	}

	public void DeleteItem()
	{
		PhotonConnectionFactory.Instance.DestroyItem(this.InventoryItem);
	}

	public void ToStorage()
	{
		PhotonConnectionFactory.Instance.MoveItemOrCombine(this.InventoryItem, null, StoragePlaces.Storage, true);
	}

	[HideInInspector]
	public MissionItem InventoryItem;

	[HideInInspector]
	public ChangeHandler ChangeHandler;

	public Text Name;

	public Text Description;

	public Toggle Info;

	public Text Count;

	public Image Blocked;

	public Image Image;

	private ResourcesHelpers.AsyncLoadableImage ImageLoadable = new ResourcesHelpers.AsyncLoadableImage();

	[HideInInspector]
	public RectTransform parentMaskRect;

	private RectTransform _rect;

	private Canvas _canvas;

	[SerializeField]
	private ShowDetailedInfo sdi;
}
