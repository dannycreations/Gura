using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IEventSystemHandler
{
	public void OnDisable()
	{
		if (this.m_DraggingIcon != null)
		{
			this.OnEndDrag(null);
		}
	}

	protected virtual void Start()
	{
		this._image = base.GetComponent<Image>();
		this._imageLdbl.Image = this._image;
		this.BeginDragColor = new Color(this._image.color.r, this._image.color.g, this._image.color.b, 0.3f);
		this.NormalColor = this._image.color;
	}

	public void DranNDropStartChangeActiveStorage(bool isActive, int activeTypeId)
	{
		this.DranNDropActive(isActive, activeTypeId);
		this.DranNDropType.CurrentActiveStorage = this.GetCurrentActiveStorage();
	}

	public void DranNDropActive(bool isActive, int activeTypeId)
	{
		this.DranNDropType.IsActive = isActive;
		this.DranNDropType.CurrentActiveTypeId = activeTypeId;
	}

	public virtual void OnBeginDrag(PointerEventData eventData)
	{
		Canvas canvas = DragMe.FindInParents<Canvas>(base.gameObject);
		if (canvas == null || this.DranNDropType.IsActive)
		{
			return;
		}
		this.m_DraggingIcon = new GameObject("icon");
		Image image = this.m_DraggingIcon.AddComponent<Image>();
		this.m_DraggingIcon.transform.SetParent(canvas.transform, false);
		this.m_DraggingIcon.transform.SetAsLastSibling();
		image.raycastTarget = false;
		this.SetDraggingIcon(image);
		if (this._image != null)
		{
			this._image.color = this.BeginDragColor;
		}
		if (this.dragOnSurfaces)
		{
			this.m_DraggingPlane = base.transform as RectTransform;
		}
		else
		{
			this.m_DraggingPlane = canvas.transform as RectTransform;
		}
		this.SetDraggedPosition(eventData);
		this.DranNDropStartChangeActiveStorage(true, this.typeId);
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.DragClip, SettingsManager.InterfaceVolume);
	}

	protected virtual void SetDraggingIcon(Image image)
	{
		image.overrideSprite = base.GetComponent<Image>().overrideSprite;
	}

	public void OnDrag(PointerEventData data)
	{
		if (this.m_DraggingIcon != null)
		{
			this.SetDraggedPosition(data);
		}
	}

	protected virtual StoragePlaces GetCurrentActiveStorage()
	{
		return this.DragNDropContent.Storage;
	}

	protected void SetDraggedPosition(PointerEventData data)
	{
		if (this.dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
		{
			this.m_DraggingPlane = data.pointerEnter.transform as RectTransform;
		}
		RectTransform component = this.m_DraggingIcon.GetComponent<RectTransform>();
		Vector3 vector;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(this.m_DraggingPlane, data.position, data.pressEventCamera, ref vector))
		{
			component.position = vector;
		}
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
		if (eventData != null && eventData.pointerDrag != null)
		{
			ChumComponent component = eventData.pointerDrag.GetComponent<ChumComponent>();
			if (component != null && component.Storage == StoragePlaces.Storage && component.OverStorage)
			{
				component.Drop2Inventory();
			}
		}
		if (this.m_DraggingIcon != null)
		{
			Object.Destroy(this.m_DraggingIcon);
		}
		this.DranNDropType.IsActive = false;
		this._image.color = this.NormalColor;
		UIAudioSourceListener.Instance.Audio.PlayOneShot(UIAudioSourceListener.Instance.DropClip, SettingsManager.InterfaceVolume);
	}

	public static T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null)
		{
			return (T)((object)null);
		}
		T t = go.GetComponent<T>();
		if (t != null)
		{
			return t;
		}
		Transform transform = go.transform.parent;
		while (transform != null && t == null)
		{
			t = transform.gameObject.GetComponent<T>();
			transform = transform.parent;
		}
		return t;
	}

	public bool dragOnSurfaces = true;

	public int typeId;

	public DragNDropType DranNDropType;

	public InventoryItemComponent DragNDropContent;

	protected GameObject m_DraggingIcon;

	protected RectTransform m_DraggingPlane;

	protected Image _image;

	protected ResourcesHelpers.AsyncLoadableImage _imageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	protected Color BeginDragColor;

	protected Color NormalColor;
}
