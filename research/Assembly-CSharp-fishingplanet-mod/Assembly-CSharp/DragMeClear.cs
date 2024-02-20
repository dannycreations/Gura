using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragMeClear : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler, IEventSystemHandler
{
	public void OnBeginDrag(PointerEventData eventData)
	{
		Canvas canvas = DragMeClear.FindInParents<Canvas>(base.gameObject);
		if (canvas == null)
		{
			return;
		}
		this.m_DraggingIcon = new GameObject("icon");
		Image image = this.m_DraggingIcon.AddComponent<Image>();
		this.m_DraggingIcon.transform.SetParent(canvas.transform, false);
		this.m_DraggingIcon.transform.SetAsLastSibling();
		RectTransform component = this.m_DraggingIcon.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(base.GetComponent<RectTransform>().rect.width, base.GetComponent<RectTransform>().rect.height);
		this.m_DraggingIcon.AddComponent<IgnoreRaycast>();
		image.overrideSprite = base.GetComponent<Image>().sprite;
		this.m_DraggingIcon.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
		base.GetComponent<Image>().color = new Color(base.GetComponent<Image>().color.r, base.GetComponent<Image>().color.g, base.GetComponent<Image>().color.b, 0.3f);
		if (this.dragOnSurfaces)
		{
			this.m_DraggingPlane = base.transform as RectTransform;
		}
		else
		{
			this.m_DraggingPlane = canvas.transform as RectTransform;
		}
		this.SetDraggedPosition(eventData);
	}

	public void OnDrag(PointerEventData data)
	{
		if (this.m_DraggingIcon != null)
		{
			this.SetDraggedPosition(data);
		}
	}

	public void OnPointerEnter(PointerEventData data)
	{
		this.clearing = false;
	}

	public void OnPointerExit(PointerEventData data)
	{
		this.clearing = true;
	}

	public void Clear()
	{
		if (this.DropMeContent == null)
		{
			return;
		}
		this.DropMeContent.receivingImage.overrideSprite = this.BaseImage;
	}

	private void SetDraggedPosition(PointerEventData data)
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
			component.rotation = this.m_DraggingPlane.rotation;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (this.m_DraggingIcon != null)
		{
			Object.Destroy(this.m_DraggingIcon);
		}
		if (this.clearing)
		{
			this.Clear();
		}
		base.GetComponent<Image>().color = new Color(base.GetComponent<Image>().color.r, base.GetComponent<Image>().color.g, base.GetComponent<Image>().color.b, 1f);
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

	public DropMe DropMeContent;

	public Sprite BaseImage;

	private GameObject m_DraggingIcon;

	private RectTransform m_DraggingPlane;

	private bool clearing;
}
