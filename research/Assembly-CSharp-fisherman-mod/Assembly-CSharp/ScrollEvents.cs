using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollEvents : MonoBehaviour, IDragHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnDragEvent;

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if (this.OnDragEvent != null)
		{
			this.OnDragEvent(this, new EventArgs());
		}
	}
}
