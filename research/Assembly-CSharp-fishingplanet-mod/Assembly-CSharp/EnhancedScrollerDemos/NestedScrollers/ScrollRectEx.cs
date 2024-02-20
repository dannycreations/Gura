using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.NestedScrollers
{
	public class ScrollRectEx : ScrollRect
	{
		private void DoForParents<T>(Action<T> action) where T : IEventSystemHandler
		{
			Transform transform = base.transform.parent;
			while (transform != null)
			{
				foreach (Component component in transform.GetComponents<Component>())
				{
					if (component is T)
					{
						action((T)((object)((IEventSystemHandler)component)));
					}
				}
				transform = transform.parent;
			}
		}

		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
			this.DoForParents<IInitializePotentialDragHandler>(delegate(IInitializePotentialDragHandler parent)
			{
				parent.OnInitializePotentialDrag(eventData);
			});
			base.OnInitializePotentialDrag(eventData);
		}

		public override void OnDrag(PointerEventData eventData)
		{
			if (this.routeToParent)
			{
				this.DoForParents<IDragHandler>(delegate(IDragHandler parent)
				{
					parent.OnDrag(eventData);
				});
			}
			else
			{
				base.OnDrag(eventData);
			}
		}

		public override void OnBeginDrag(PointerEventData eventData)
		{
			if (!base.horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
			{
				this.routeToParent = true;
			}
			else if (!base.vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
			{
				this.routeToParent = true;
			}
			else
			{
				this.routeToParent = false;
			}
			if (this.routeToParent)
			{
				this.DoForParents<IBeginDragHandler>(delegate(IBeginDragHandler parent)
				{
					parent.OnBeginDrag(eventData);
				});
			}
			else
			{
				base.OnBeginDrag(eventData);
			}
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			if (this.routeToParent)
			{
				this.DoForParents<IEndDragHandler>(delegate(IEndDragHandler parent)
				{
					parent.OnEndDrag(eventData);
				});
			}
			else
			{
				base.OnEndDrag(eventData);
			}
			this.routeToParent = false;
		}

		private bool routeToParent;
	}
}
