using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollForwarding : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	private ScrollRect parentScrollRect
	{
		get
		{
			if (this._parentScrollRect == null)
			{
				this._parentScrollRect = base.GetComponentInParent<ScrollRect>();
			}
			return this._parentScrollRect;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<ScrollEventArgs> ScrollCalled;

	public void OnScroll(PointerEventData eventData)
	{
		if (this.parentScrollRect != null)
		{
			this.parentScrollRect.OnScroll(eventData);
		}
		if (this.ScrollCalled != null)
		{
			this.ScrollCalled(this, new ScrollEventArgs
			{
				PointerEventData = eventData
			});
		}
	}

	private ScrollRect _parentScrollRect;
}
