using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnScrollEvent : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	private void Start()
	{
		if (this.OnScrolling == null)
		{
			this.OnScrolling = new UnityEvent();
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		this.OnScrolling.Invoke();
	}

	public UnityEvent OnScrolling;
}
