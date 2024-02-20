using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointerActionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	private void Start()
	{
		if (this.PointerEnter == null)
		{
			this.PointerEnter = new UnityEvent();
		}
		if (this.OnSelected == null)
		{
			this.OnSelected = new UnityEvent();
		}
		if (this.OnDeselected == null)
		{
			this.OnDeselected = new UnityEvent();
		}
		if (this.PointerExit == null)
		{
			this.PointerExit = new UnityEvent();
		}
		if (this.PointerClick == null)
		{
			this.PointerClick = new UnityEvent();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this.PointerEnter.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.PointerExit.Invoke();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		this.PointerClick.Invoke();
	}

	public void OnSelect(BaseEventData eventData)
	{
		this.OnSelected.Invoke();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		this.OnDeselected.Invoke();
	}

	public UnityEvent PointerEnter;

	public UnityEvent PointerExit;

	public UnityEvent PointerClick;

	public UnityEvent OnSelected;

	public UnityEvent OnDeselected;
}
