using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseScrolling : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	public void OnScroll(PointerEventData eventData)
	{
		this.ScrollBar.GetComponent<Scrollbar>().value += eventData.scrollDelta.y / (float)base.GetComponent<ScrollRect>().content.transform.childCount;
	}

	public GameObject ScrollBar;
}
