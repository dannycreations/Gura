using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util
{
	[RequireComponent(typeof(Graphic))]
	public class LongClickableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ICancelHandler, IEventSystemHandler
	{
		private void Update()
		{
			if (this._Pressing && Time.time - this._PressedTime >= this.longClickTime)
			{
				this._Pressing = false;
				if (this.longClickListener != null)
				{
					this.longClickListener.OnItemLongClicked(this);
				}
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			this._Pressing = true;
			this._PressedTime = Time.time;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			this._Pressing = false;
		}

		public void OnCancel(BaseEventData eventData)
		{
			this._Pressing = false;
		}

		public float longClickTime = 0.7f;

		public LongClickableItem.IItemLongClickListener longClickListener;

		private float _PressedTime;

		private bool _Pressing;

		public interface IItemLongClickListener
		{
			void OnItemLongClicked(LongClickableItem longClickedItem);
		}
	}
}
