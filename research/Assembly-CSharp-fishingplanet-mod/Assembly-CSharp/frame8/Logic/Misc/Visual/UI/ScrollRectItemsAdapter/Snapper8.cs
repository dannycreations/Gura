using System;
using System.Diagnostics;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public class Snapper8 : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action SnappingStarted;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action SnappingEndedOrCancelled;

		public ISRIA Adapter
		{
			set
			{
				if (this._Adapter == value)
				{
					return;
				}
				if (this._Adapter != null)
				{
					this._Adapter.ScrollPositionChanged -= this.OnScrolled;
					this._Adapter.ItemsRefreshed -= this.OnItemsRefreshed;
				}
				this._Adapter = value;
				if (this._Adapter != null)
				{
					this._Adapter.ScrollPositionChanged += this.OnScrolled;
					this._Adapter.ItemsRefreshed += this.OnItemsRefreshed;
				}
			}
		}

		public bool SnappingInProgress { get; private set; }

		private bool IsPointerDraggingOnScrollRect
		{
			get
			{
				return this._Adapter != null && this._Adapter.IsDragging;
			}
		}

		private bool IsPointerDraggingOnScrollbar
		{
			get
			{
				return this._ScrollbarFixer != null && this._ScrollbarFixer.IsDragging;
			}
		}

		private void Start()
		{
			if (this.minSpeedToAllowSnapToNext < 0f)
			{
				this.minSpeedToAllowSnapToNext = float.MaxValue;
			}
			this._SnapToNextOnlyEnabled = this.minSpeedToAllowSnapToNext != float.MaxValue;
			if (this.scrollbar)
			{
				this._ScrollbarFixer = this.scrollbar.GetComponent<ScrollbarFixer8>();
				if (!this._ScrollbarFixer)
				{
					throw new UnityException("ScrollbarFixer8 should be attached to Scrollbar");
				}
			}
			this._ScrollRect = base.GetComponent<ScrollRect>();
			if (this._ScrollRect.horizontal)
			{
				this._GetSignedAbstractSpeed = () => this._ScrollRect.velocity[0];
			}
			else
			{
				this._GetSignedAbstractSpeed = () => -this._ScrollRect.velocity[1];
			}
			if (this.scrollbar)
			{
				this.scrollbar.onValueChanged.AddListener(new UnityAction<float>(this.OnScrollbarValueChanged));
			}
			this._StartCalled = true;
		}

		private void OnDisable()
		{
			this.CancelSnappingIfInProgress();
		}

		private void OnDestroy()
		{
			if (this.scrollbar)
			{
				this.scrollbar.onValueChanged.RemoveListener(new UnityAction<float>(this.OnScrollbarValueChanged));
			}
			this.Adapter = null;
			this.SnappingStarted = null;
			this.SnappingEndedOrCancelled = null;
		}

		internal void CancelSnappingIfInProgress()
		{
			this._SnappingDoneAndEndSnappingEventPending = false;
			this._SnapNeeded = false;
			if (!this.SnappingInProgress)
			{
				return;
			}
			this._SnappingCancelled = true;
			this.SnappingInProgress = false;
		}

		internal void StartSnappingIfNeeded()
		{
			if (!this._StartCalled)
			{
				return;
			}
			if (!base.enabled)
			{
				return;
			}
			if (this._SnappingDoneAndEndSnappingEventPending)
			{
				this.OnSnappingEndedOrCancelled();
				return;
			}
			if (this._Adapter == null)
			{
				return;
			}
			float num = this._GetSignedAbstractSpeed();
			float num2 = Mathf.Abs(num);
			if (this.SnappingInProgress || !this._SnapNeeded || num2 >= this.snapWhenSpeedFallsBelow || this.IsPointerDraggingOnScrollRect || this.IsPointerDraggingOnScrollbar)
			{
				return;
			}
			if (this._Adapter.ContentVirtualInsetFromViewportStart >= 0.0 || this._Adapter.ContentVirtualInsetFromViewportEnd >= 0.0)
			{
				return;
			}
			float num3;
			AbstractViewsHolder middleVH = this.GetMiddleVH(out num3);
			if (middleVH == null)
			{
				return;
			}
			this._SnapNeeded = false;
			if (num3 <= this.snapAllowedError)
			{
				return;
			}
			int indexToSnapTo = middleVH.ItemIndex;
			bool flag = num2 >= this.minSpeedToAllowSnapToNext && indexToSnapTo == this._LastSnappedItemIndex;
			if (flag)
			{
				bool loopItems = this._Adapter.BaseParameters.loopItems;
				int itemsCount = this._Adapter.GetItemsCount();
				if (num < 0f)
				{
					if (indexToSnapTo == itemsCount - 1 && !loopItems)
					{
						return;
					}
					indexToSnapTo = (indexToSnapTo + 1) % itemsCount;
				}
				else
				{
					if (indexToSnapTo == 0 && !loopItems)
					{
						return;
					}
					indexToSnapTo = (indexToSnapTo + itemsCount - 1) % itemsCount;
				}
			}
			else
			{
				indexToSnapTo = middleVH.ItemIndex;
			}
			this._SnappingCancelled = false;
			bool cancelledOrEnded = false;
			this._Adapter.SmoothScrollTo(indexToSnapTo, this.snapDuration, this.viewportSnapPivot01, this.itemSnapPivot01, delegate(float progress)
			{
				bool flag2 = true;
				if (progress == 1f || this._SnappingCancelled || this.IsPointerDraggingOnScrollRect || this.IsPointerDraggingOnScrollbar)
				{
					cancelledOrEnded = true;
					flag2 = false;
					if (this.SnappingInProgress)
					{
						this._LastSnappedItemIndex = indexToSnapTo;
						this.SnappingInProgress = false;
						this._SnappingDoneAndEndSnappingEventPending = true;
					}
				}
				return flag2;
			}, true);
			if (cancelledOrEnded)
			{
				return;
			}
			this.SnappingInProgress = true;
			if (this.SnappingInProgress)
			{
				this.OnSnappingStarted();
			}
		}

		private AbstractViewsHolder GetMiddleVH(out float distanceToTarget)
		{
			return this._Adapter.GetViewsHolderOfClosestItemToViewportPoint(this.viewportSnapPivot01, this.itemSnapPivot01, out distanceToTarget);
		}

		private void OnScrolled(float _)
		{
			if (!this.SnappingInProgress)
			{
				this._SnapNeeded = true;
				if (this._SnapToNextOnlyEnabled && !this.IsPointerDraggingOnScrollbar && !this.IsPointerDraggingOnScrollRect)
				{
					this.UpdateLastSnappedIndexFromMiddleVH();
				}
			}
		}

		private void OnScrollbarValueChanged(float _)
		{
			if (this.IsPointerDraggingOnScrollbar)
			{
				this.CancelSnappingIfInProgress();
			}
		}

		private void OnItemsRefreshed(int newCount, int prevCount)
		{
			if (newCount == prevCount)
			{
				return;
			}
			if (this._SnapToNextOnlyEnabled)
			{
				this.UpdateLastSnappedIndexFromMiddleVH();
			}
		}

		private void UpdateLastSnappedIndexFromMiddleVH()
		{
			float num;
			AbstractViewsHolder middleVH = this.GetMiddleVH(out num);
			this._LastSnappedItemIndex = ((middleVH != null) ? middleVH.ItemIndex : (-1));
		}

		private void OnSnappingStarted()
		{
			if (this.SnappingStarted != null)
			{
				this.SnappingStarted();
			}
		}

		private void OnSnappingEndedOrCancelled()
		{
			this._SnappingDoneAndEndSnappingEventPending = false;
			if (this.SnappingEndedOrCancelled != null)
			{
				this.SnappingEndedOrCancelled();
			}
		}

		public float snapWhenSpeedFallsBelow = 50f;

		public float viewportSnapPivot01 = 0.5f;

		public float itemSnapPivot01 = 0.5f;

		public float snapDuration = 0.3f;

		public float snapAllowedError = 1f;

		public Scrollbar scrollbar;

		[Tooltip("If the current drag distance is not enough to change the currently centered item, snapping to the next item will still occur if the current speed is bigger than this. Set to a negative value to disable (default). This was initially useful for things like page views")]
		public float minSpeedToAllowSnapToNext = -1f;

		private ISRIA _Adapter;

		private ScrollRect _ScrollRect;

		private ScrollbarFixer8 _ScrollbarFixer;

		private bool _SnappingDoneAndEndSnappingEventPending;

		private bool _SnapNeeded;

		private bool _SnappingCancelled;

		private Func<float> _GetSignedAbstractSpeed;

		private int _LastSnappedItemIndex = -1;

		private bool _SnapToNextOnlyEnabled;

		private bool _StartCalled;
	}
}
