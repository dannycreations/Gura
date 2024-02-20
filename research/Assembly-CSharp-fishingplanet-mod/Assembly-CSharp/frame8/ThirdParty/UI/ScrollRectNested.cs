using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace frame8.ThirdParty.UI
{
	public class ScrollRectNested : ScrollRect
	{
		protected override void Start()
		{
			base.Start();
			Transform transform = base.transform;
			while (!this._ParentScrollRect && (transform = transform.parent))
			{
				this._ParentScrollRect = transform.GetComponent<ScrollRect>();
			}
		}

		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
			if (this._ParentScrollRect)
			{
				this._ParentScrollRect.OnInitializePotentialDrag(eventData);
			}
			base.OnInitializePotentialDrag(eventData);
		}

		public override void OnBeginDrag(PointerEventData eventData)
		{
			if (!base.horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
			{
				this._RouteToParent = true;
			}
			else if (!base.vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
			{
				this._RouteToParent = true;
			}
			else
			{
				this._RouteToParent = false;
			}
			if (this._RouteToParent)
			{
				if (this._ParentScrollRect)
				{
					this._ParentScrollRect.OnBeginDrag(eventData);
				}
			}
			else
			{
				base.OnBeginDrag(eventData);
			}
		}

		public override void OnDrag(PointerEventData eventData)
		{
			if (this._RouteToParent)
			{
				if (this._ParentScrollRect)
				{
					this._ParentScrollRect.OnDrag(eventData);
				}
			}
			else
			{
				base.OnDrag(eventData);
			}
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			if (this._RouteToParent)
			{
				if (this._ParentScrollRect)
				{
					this._ParentScrollRect.OnEndDrag(eventData);
				}
			}
			else
			{
				base.OnEndDrag(eventData);
			}
			this._RouteToParent = false;
		}

		private ScrollRect _ParentScrollRect;

		private bool _RouteToParent;
	}
}
