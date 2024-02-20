using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace InControl
{
	public class TouchPool
	{
		public TouchPool(int capacity)
		{
			this.freeTouches = new List<Touch>(capacity);
			for (int i = 0; i < capacity; i++)
			{
				this.freeTouches.Add(new Touch());
			}
			this.usedTouches = new List<Touch>(capacity);
			this.Touches = new ReadOnlyCollection<Touch>(this.usedTouches);
		}

		public TouchPool()
			: this(16)
		{
		}

		public Touch FindOrCreateTouch(int fingerId)
		{
			int count = this.usedTouches.Count;
			Touch touch;
			for (int i = 0; i < count; i++)
			{
				touch = this.usedTouches[i];
				if (touch.fingerId == fingerId)
				{
					return touch;
				}
			}
			touch = this.NewTouch();
			touch.fingerId = fingerId;
			this.usedTouches.Add(touch);
			return touch;
		}

		public Touch FindTouch(int fingerId)
		{
			int count = this.usedTouches.Count;
			for (int i = 0; i < count; i++)
			{
				Touch touch = this.usedTouches[i];
				if (touch.fingerId == fingerId)
				{
					return touch;
				}
			}
			return null;
		}

		private Touch NewTouch()
		{
			int count = this.freeTouches.Count;
			if (count > 0)
			{
				Touch touch = this.freeTouches[count - 1];
				this.freeTouches.RemoveAt(count - 1);
				return touch;
			}
			return new Touch();
		}

		public void FreeTouch(Touch touch)
		{
			touch.Reset();
			this.freeTouches.Add(touch);
		}

		public void FreeEndedTouches()
		{
			int count = this.usedTouches.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				Touch touch = this.usedTouches[i];
				if (touch.phase == 3)
				{
					this.usedTouches.RemoveAt(i);
				}
			}
		}

		public readonly ReadOnlyCollection<Touch> Touches;

		private List<Touch> usedTouches;

		private List<Touch> freeTouches;
	}
}
