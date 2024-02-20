using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EventTriggerCustom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		this.PointerEnter.Invoke(eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.PointerExit.Invoke(eventData);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		this.PointerClick.Invoke(eventData);
	}

	public void OnSelect(BaseEventData eventData)
	{
		this.OnSelected.Invoke(eventData);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		this.OnDeselected.Invoke(eventData);
	}

	private void OnDisable()
	{
		this.OnDisabled.Invoke();
	}

	public EventTrigger.TriggerEvent PointerEnter;

	public EventTrigger.TriggerEvent PointerExit;

	public EventTrigger.TriggerEvent PointerClick;

	public EventTrigger.TriggerEvent OnSelected;

	public EventTrigger.TriggerEvent OnDeselected;

	public UnityEvent OnDisabled;
}
