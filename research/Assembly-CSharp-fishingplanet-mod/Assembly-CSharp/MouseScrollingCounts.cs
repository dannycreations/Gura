using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseScrollingCounts : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnScrolled;

	public void OnScroll(PointerEventData eventData)
	{
		this.value += (int)eventData.scrollDelta.y;
		if (this.OnScrolled != null)
		{
			this.OnScrolled(this, new EventArgs());
		}
	}

	public int value;
}
