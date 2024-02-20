using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public abstract class SRIA<TParams, TItemViewsHolder> : MonoBehaviour, ISRIA, IBeginDragHandler, IEndDragHandler, IScrollRectProxy, IEventSystemHandler where TParams : BaseParams where TItemViewsHolder : BaseItemViewsHolder
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float> ScrollPositionChanged;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int, int> ItemsRefreshed;

		public bool Initialized { get; private set; }

		public BaseParams BaseParameters
		{
			get
			{
				return this.Parameters;
			}
		}

		public MonoBehaviour AsMonoBehaviour
		{
			get
			{
				return this;
			}
		}

		public double ContentVirtualSizeToViewportRatio
		{
			get
			{
				return this._InternalState.contentPanelVirtualSize / (double)this._InternalState.viewportSize;
			}
		}

		public double ContentVirtualInsetFromViewportStart
		{
			get
			{
				return this._InternalState.ContentPanelVirtualInsetFromViewportStart;
			}
		}

		public double ContentVirtualInsetFromViewportEnd
		{
			get
			{
				return this._InternalState.ContentPanelVirtualInsetFromViewportEnd;
			}
		}

		public int VisibleItemsCount
		{
			get
			{
				return this._VisibleItemsCount;
			}
		}

		public int RecyclableItemsCount
		{
			get
			{
				return this._RecyclableItems.Count;
			}
		}

		public bool IsDragging { get; private set; }

		public TParams Parameters
		{
			get
			{
				return this._Params;
			}
		}

		protected virtual void Awake()
		{
		}

		protected virtual void Start()
		{
			this.Init();
		}

		protected virtual void Update()
		{
			this.MyUpdate();
		}

		protected virtual void OnDestroy()
		{
			this.Dispose();
		}

		public void SetNormalizedPosition(float normalizedPosition)
		{
			float num = ((!this._Params.scrollRect.horizontal) ? normalizedPosition : (1f - normalizedPosition));
			this.SetVirtualAbstractNormalizedScrollPosition((double)num, true);
		}

		public float GetNormalizedPosition()
		{
			float num = (float)this._InternalState.GetVirtualAbstractNormalizedScrollPosition();
			return (!this._Params.scrollRect.horizontal) ? num : (1f - num);
		}

		public float GetContentSize()
		{
			return (float)Math.Min(this._InternalState.contentPanelVirtualSize, 3.4028234663852886E+38);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			this.IsDragging = true;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			this.IsDragging = false;
		}

		public void Init()
		{
			Canvas.ForceUpdateCanvases();
			this._Params.InitIfNeeded(this);
			if (this._Params.Snapper)
			{
				this._Params.Snapper.Adapter = this;
			}
			if (this._Params.scrollRect.horizontalScrollbar != null || this._Params.scrollRect.verticalScrollbar != null)
			{
				throw new UnityException("SRIA only works with a " + typeof(ScrollbarFixer8).Name + " component added to the Scrollbar and the ScrollRect shouldn't have any scrollbar set up in the inspector (it hooks up automatically)");
			}
			this._ItemsDesc = new ItemsDescriptor(this._Params.DefaultItemSize);
			this._InternalState = SRIA<TParams, TItemViewsHolder>.InternalState.CreateFromSourceParamsOrThrow(this._Params, this._ItemsDesc);
			this._VisibleItems = new List<TItemViewsHolder>();
			this._AVGVisibleItemsCount = 0.0;
			this.Refresh();
			this._InternalState.UpdateLastProcessedCTVirtualInsetFromParentStart();
			this.SetVirtualAbstractNormalizedScrollPosition(1.0, false);
			this._Params.scrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this.OnScrollViewValueChanged));
			if (this.ScrollPositionChanged != null)
			{
				this.ScrollPositionChanged(this.GetNormalizedPosition());
			}
			this.Initialized = true;
		}

		public virtual void Refresh()
		{
			this.ChangeItemsCount(ItemCountChangeMode.RESET, this._ItemsDesc.itemsCount, -1, false, false);
		}

		public virtual void ResetItems(int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			this.ChangeItemsCount(ItemCountChangeMode.RESET, itemsCount, -1, contentPanelEndEdgeStationary, keepVelocity);
		}

		public virtual void InsertItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			this.ChangeItemsCount(ItemCountChangeMode.INSERT, itemsCount, index, contentPanelEndEdgeStationary, keepVelocity);
		}

		public virtual void RemoveItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			this.ChangeItemsCount(ItemCountChangeMode.REMOVE, itemsCount, index, contentPanelEndEdgeStationary, keepVelocity);
		}

		public virtual void ChangeItemsCount(ItemCountChangeMode changeMode, int itemsCount, int indexIfInsertingOrRemoving = -1, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			this.ChangeItemsCountInternal(changeMode, itemsCount, indexIfInsertingOrRemoving, contentPanelEndEdgeStationary, keepVelocity);
		}

		public virtual int GetItemsCount()
		{
			return this._ItemsDesc.itemsCount;
		}

		public TItemViewsHolder GetItemViewsHolder(int vhIndex)
		{
			if (vhIndex >= this._VisibleItemsCount)
			{
				return (TItemViewsHolder)((object)null);
			}
			return this._VisibleItems[vhIndex];
		}

		public TItemViewsHolder GetItemViewsHolderIfVisible(int withItemIndex)
		{
			for (int i = 0; i < this._VisibleItemsCount; i++)
			{
				TItemViewsHolder titemViewsHolder = this._VisibleItems[i];
				int itemIndex = titemViewsHolder.ItemIndex;
				if (itemIndex == withItemIndex)
				{
					return titemViewsHolder;
				}
			}
			return (TItemViewsHolder)((object)null);
		}

		public TItemViewsHolder GetItemViewsHolderIfVisible(RectTransform withRoot)
		{
			for (int i = 0; i < this._VisibleItemsCount; i++)
			{
				TItemViewsHolder titemViewsHolder = this._VisibleItems[i];
				if (titemViewsHolder.root == withRoot)
				{
					return titemViewsHolder;
				}
			}
			return (TItemViewsHolder)((object)null);
		}

		public virtual AbstractViewsHolder GetViewsHolderOfClosestItemToViewportPoint(float viewportPoint01, float itemPoint01, out float distance)
		{
			Func<RectTransform, float, RectTransform, float, float> func;
			if (this._Params.scrollRect.horizontal)
			{
				func = new Func<RectTransform, float, RectTransform, float, float>(RectTransformExtensions.GetWorldSignedHorDistanceBetweenCustomPivots);
			}
			else
			{
				func = new Func<RectTransform, float, RectTransform, float, float>(RectTransformExtensions.GetWorldSignedVertDistanceBetweenCustomPivots);
			}
			return this.GetViewsHolderOfClosestItemToViewportPoint(this._VisibleItems, func, viewportPoint01, itemPoint01, out distance);
		}

		public virtual void ScrollTo(int itemIndex, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f)
		{
			this.CancelAnimationsIfAny();
			double num = -this._InternalState.VirtualScrollableArea;
			if (num >= 0.0)
			{
				return;
			}
			this.ScrollToHelper_SetContentVirtualInsetFromViewportStart(this.ScrollToHelper_GetContentStartVirtualInsetFromViewportStart_Clamped(num, itemIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse), false);
			this.ComputeVisibilityForCurrentPosition(false, true, false, -0.1);
			this.ComputeVisibilityForCurrentPosition(true, true, false, 0.1);
		}

		public virtual bool SmoothScrollTo(int itemIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null, bool overrideCurrentScrollingAnimation = false)
		{
			if (this._SmoothScrollCoroutine != null)
			{
				if (!overrideCurrentScrollingAnimation)
				{
					return false;
				}
				this.CancelAnimationsIfAny();
			}
			this._SmoothScrollCoroutine = base.StartCoroutine(this.SmoothScrollProgressCoroutine(itemIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, onProgress));
			return true;
		}

		public virtual void CancelAnimationsIfAny()
		{
			if (this._Params.Snapper)
			{
				this._Params.Snapper.CancelSnappingIfInProgress();
			}
			if (this._SmoothScrollCoroutine != null)
			{
				base.StopCoroutine(this._SmoothScrollCoroutine);
				this._SmoothScrollCoroutine = null;
				this._SkipComputeVisibilityInUpdateOrOnScroll = false;
			}
		}

		public float RequestChangeItemSizeAndUpdateLayout(TItemViewsHolder withVH, float requestedSize, bool itemEndEdgeStationary = false, bool computeVisibility = true)
		{
			return this.RequestChangeItemSizeAndUpdateLayout(withVH.ItemIndex, requestedSize, itemEndEdgeStationary, computeVisibility);
		}

		public float RequestChangeItemSizeAndUpdateLayout(int itemIndex, float requestedSize, bool itemEndEdgeStationary = false, bool computeVisibility = true)
		{
			this.CancelAnimationsIfAny();
			bool skipComputeVisibilityInUpdateOrOnScroll = this._SkipComputeVisibilityInUpdateOrOnScroll;
			this._SkipComputeVisibilityInUpdateOrOnScroll = true;
			this._Params.scrollRect.StopMovement();
			int itemViewIndexFromRealIndex = this._ItemsDesc.GetItemViewIndexFromRealIndex(itemIndex);
			TItemViewsHolder itemViewsHolderIfVisible = this.GetItemViewsHolderIfVisible(itemIndex);
			float num = this._ItemsDesc[itemViewIndexFromRealIndex];
			bool flag = this._InternalState.ContentPanelVirtualInsetFromViewportEnd >= 0.0;
			if (requestedSize <= num)
			{
				if (this._InternalState.ContentPanelVirtualInsetFromViewportStart >= 0.0)
				{
					itemEndEdgeStationary = false;
				}
				else if (flag)
				{
					itemEndEdgeStationary = true;
				}
			}
			float num2 = this._InternalState.ChangeItemSizeAndUpdateContentSizeAccordingly(itemViewsHolderIfVisible, itemViewIndexFromRealIndex, requestedSize, itemEndEdgeStationary, true);
			float num3;
			if (itemEndEdgeStationary)
			{
				num3 = 0.1f;
			}
			else
			{
				num3 = -0.1f;
				if (flag)
				{
					num3 = 0.1f;
				}
			}
			if (computeVisibility)
			{
				this.ComputeVisibilityForCurrentPosition(true, true, false, (double)num3);
			}
			if (!this._CorrectedPositionInCurrentComputeVisibilityPass)
			{
				this.CorrectPositionsOfVisibleItems(false);
			}
			this._SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
			return num2;
		}

		public double GetItemVirtualInsetFromParentStart(int itemIndex)
		{
			return this._InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(this._ItemsDesc.GetItemViewIndexFromRealIndex(itemIndex));
		}

		public double GetVirtualAbstractNormalizedScrollPosition()
		{
			return this._InternalState.GetVirtualAbstractNormalizedScrollPosition();
		}

		public void SetVirtualAbstractNormalizedScrollPosition(double pos, bool computeVisibilityNow)
		{
			this.CancelAnimationsIfAny();
			bool skipComputeVisibilityInUpdateOrOnScroll = this._SkipComputeVisibilityInUpdateOrOnScroll;
			this._SkipComputeVisibilityInUpdateOrOnScroll = true;
			bool flag = this._InternalState.SetVirtualAbstractNormalizedScrollPosition(pos);
			if (computeVisibilityNow && flag)
			{
				this.ComputeVisibilityForCurrentPosition(true, false, false);
				if (!this._CorrectedPositionInCurrentComputeVisibilityPass)
				{
					this.CorrectPositionsOfVisibleItems(false);
				}
			}
			else if (this.ScrollPositionChanged != null)
			{
				this.ScrollPositionChanged(this.GetNormalizedPosition());
			}
			this._SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
		}

		protected virtual void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
		{
			itemsDesc.ReinitializeSizes(changeMode, count, indexIfInsertingOrRemoving, new float?(this._Params.DefaultItemSize));
		}

		protected abstract TItemViewsHolder CreateViewsHolder(int itemIndex);

		protected abstract void UpdateViewsHolder(TItemViewsHolder newOrRecycled);

		protected virtual bool IsRecyclable(TItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float sizeOfItemThatWillBecomeVisible)
		{
			return true;
		}

		protected virtual bool ShouldDestroyRecyclableItem(TItemViewsHolder inRecycleBin, bool isInExcess)
		{
			return isInExcess;
		}

		protected virtual void OnBeforeRecycleOrDisableViewsHolder(TItemViewsHolder inRecycleBinOrVisible, int newItemIndex)
		{
		}

		protected virtual void ClearCachedRecyclableItems()
		{
			if (this._RecyclableItems != null)
			{
				foreach (TItemViewsHolder titemViewsHolder in this._RecyclableItems)
				{
					if (titemViewsHolder != null && titemViewsHolder.root != null)
					{
						try
						{
							Object.Destroy(titemViewsHolder.root.gameObject);
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
						}
					}
				}
				this._RecyclableItems.Clear();
			}
		}

		protected virtual void ClearVisibleItems()
		{
			if (this._VisibleItems != null)
			{
				foreach (TItemViewsHolder titemViewsHolder in this._VisibleItems)
				{
					if (titemViewsHolder != null && titemViewsHolder.root != null)
					{
						try
						{
							Object.Destroy(titemViewsHolder.root.gameObject);
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
						}
					}
				}
				this._VisibleItems.Clear();
				this._VisibleItemsCount = 0;
			}
		}

		protected virtual void OnScrollViewSizeChanged()
		{
		}

		protected virtual void RebuildLayoutDueToScrollViewSizeChange()
		{
			this.MarkViewsHoldersForRebuild(this._VisibleItems);
			this.ClearCachedRecyclableItems();
			Canvas.ForceUpdateCanvases();
			this._Params.InitIfNeeded(this);
			if (this._Params.Snapper)
			{
				this._Params.Snapper.Adapter = this;
			}
			this._InternalState.CacheScrollViewInfo();
			this._ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange = 0;
			this._ItemsDesc.destroyedItemsSinceLastScrollViewSizeChange = 0;
			this.Refresh();
		}

		protected virtual void OnItemHeightChangedPreTwinPass(TItemViewsHolder viewsHolder)
		{
		}

		protected virtual void OnItemWidthChangedPreTwinPass(TItemViewsHolder viewsHolder)
		{
		}

		protected void ScheduleComputeVisibilityTwinPass(bool preferContentEndEdgeStationaryIfSizeChanges = false)
		{
			this._InternalState.computeVisibilityTwinPassScheduled = true;
			this._InternalState.preferKeepingContentEndEdgeStationaryInNextComputeVisibilityTwinPass = preferContentEndEdgeStationaryIfSizeChanges;
		}

		protected AbstractViewsHolder GetViewsHolderOfClosestItemToViewportPoint(ICollection<TItemViewsHolder> viewsHolders, Func<RectTransform, float, RectTransform, float, float> getDistanceFn, float viewportPoint01, float itemPoint01, out float distance)
		{
			TItemViewsHolder titemViewsHolder = (TItemViewsHolder)((object)null);
			float num = float.MaxValue;
			foreach (TItemViewsHolder titemViewsHolder2 in viewsHolders)
			{
				float num2 = Mathf.Abs(getDistanceFn(titemViewsHolder2.root, itemPoint01, this._Params.viewport, viewportPoint01));
				if (num2 < num)
				{
					titemViewsHolder = titemViewsHolder2;
					num = num2;
				}
			}
			distance = num;
			return titemViewsHolder;
		}

		protected virtual void Dispose()
		{
			this.Initialized = false;
			if (this._Params != null && this._Params.scrollRect)
			{
				this._Params.scrollRect.onValueChanged.RemoveListener(new UnityAction<Vector2>(this.OnScrollViewValueChanged));
			}
			if (this._SmoothScrollCoroutine != null)
			{
				try
				{
					base.StopCoroutine(this._SmoothScrollCoroutine);
				}
				catch
				{
				}
				this._SmoothScrollCoroutine = null;
			}
			this.ClearCachedRecyclableItems();
			this._RecyclableItems = null;
			this.ClearVisibleItems();
			this._VisibleItems = null;
			this._Params = (TParams)((object)null);
			this._InternalState = null;
			if (this.ItemsRefreshed != null)
			{
				this.ItemsRefreshed = null;
			}
		}

		private IEnumerator SmoothScrollProgressCoroutine(int itemIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null)
		{
			double minContentVirtualInsetFromVPAllowed = -this._InternalState.VirtualScrollableArea;
			if (minContentVirtualInsetFromVPAllowed >= 0.0)
			{
				this._SmoothScrollCoroutine = null;
				if (onProgress != null)
				{
					onProgress(1f);
				}
				yield break;
			}
			bool ignorOnScroll_lastValue = this._SkipComputeVisibilityInUpdateOrOnScroll;
			this._SkipComputeVisibilityInUpdateOrOnScroll = true;
			this._Params.scrollRect.StopMovement();
			Canvas.ForceUpdateCanvases();
			Func<double> getTargetVrtInset = delegate
			{
				minContentVirtualInsetFromVPAllowed = -this._InternalState.VirtualScrollableArea;
				return this.ScrollToHelper_GetContentStartVirtualInsetFromViewportStart_Clamped(minContentVirtualInsetFromVPAllowed, itemIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
			};
			double initialVrtInsetFromParent = -1.0;
			double targetVrtInsetFromParent = -1.0;
			bool needToCalculateInitialInset = true;
			bool needToCalculateTargetInset = true;
			bool notCanceledByCaller = true;
			float startTime = Time.time;
			WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
			double progress;
			do
			{
				yield return null;
				yield return endOfFrame;
				float elapsedTime = Time.time - startTime;
				if (elapsedTime >= duration)
				{
					progress = 1.0;
				}
				else
				{
					progress = Math.Sin((double)(elapsedTime / duration) * 3.141592653589793 / 2.0);
				}
				if (needToCalculateInitialInset)
				{
					initialVrtInsetFromParent = this._InternalState.ContentPanelVirtualInsetFromViewportStart;
					needToCalculateInitialInset = this._Params.loopItems;
				}
				if (needToCalculateTargetInset || this._InternalState.lastComputeVisibilityHadATwinPass)
				{
					targetVrtInsetFromParent = getTargetVrtInset();
					needToCalculateTargetInset = this._Params.loopItems || this._InternalState.lastComputeVisibilityHadATwinPass;
				}
				double value = initialVrtInsetFromParent * (1.0 - progress) + targetVrtInsetFromParent * progress;
				if (Math.Abs(targetVrtInsetFromParent - value) < 1.0)
				{
					value = targetVrtInsetFromParent;
					progress = 1.0;
				}
				if (value > 0.0)
				{
					progress = 1.0;
					value = 0.0;
				}
				else
				{
					this.ScrollToHelper_SetContentVirtualInsetFromViewportStart(value, false);
				}
			}
			while (progress < 1.0 && (onProgress == null || (notCanceledByCaller = onProgress((float)progress))));
			if (notCanceledByCaller)
			{
				this.ScrollToHelper_SetContentVirtualInsetFromViewportStart(getTargetVrtInset(), false);
				this.ComputeVisibilityForCurrentPosition(false, true, false, -0.1);
				this.ComputeVisibilityForCurrentPosition(true, true, false, 0.1);
				this._SmoothScrollCoroutine = null;
				if (onProgress != null)
				{
					onProgress(1f);
				}
			}
			this._SkipComputeVisibilityInUpdateOrOnScroll = ignorOnScroll_lastValue;
			yield break;
		}

		private void MarkViewsHoldersForRebuild(List<TItemViewsHolder> vhs)
		{
			if (vhs != null)
			{
				foreach (TItemViewsHolder titemViewsHolder in vhs)
				{
					if (titemViewsHolder != null && titemViewsHolder.root != null)
					{
						titemViewsHolder.MarkForRebuild();
					}
				}
			}
		}

		private double ScrollToHelper_GetContentStartVirtualInsetFromViewportStart_Clamped(double minContentVirtualInsetFromVPAllowed, int itemIndex, float normalizedItemOffsetFromStart, float normalizedPositionOfItemPivotToUse)
		{
			float num = ((!this._Params.loopItems) ? 0f : (this._InternalState.viewportSize / 2f));
			minContentVirtualInsetFromVPAllowed -= (double)num;
			int itemViewIndexFromRealIndex = this._ItemsDesc.GetItemViewIndexFromRealIndex(itemIndex);
			float num2 = this._ItemsDesc[itemViewIndexFromRealIndex];
			float num3 = this._InternalState.viewportSize * normalizedItemOffsetFromStart - num2 * normalizedPositionOfItemPivotToUse;
			double itemVirtualInsetFromParentStartUsingItemIndexInView = this._InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(itemViewIndexFromRealIndex);
			return Math.Max(minContentVirtualInsetFromVPAllowed, Math.Min((double)num, -itemVirtualInsetFromParentStartUsingItemIndexInView + (double)num3));
		}

		private void ScrollToHelper_SetContentVirtualInsetFromViewportStart(double virtualInset, bool cancelSnappingIfAny)
		{
			bool skipComputeVisibilityInUpdateOrOnScroll = this._SkipComputeVisibilityInUpdateOrOnScroll;
			this._SkipComputeVisibilityInUpdateOrOnScroll = true;
			if (cancelSnappingIfAny && this._Params.Snapper)
			{
				this._Params.Snapper.CancelSnappingIfInProgress();
			}
			this._Params.scrollRect.StopMovement();
			this._InternalState.SetContentVirtualInsetFromViewportStart(virtualInset);
			this.ComputeVisibilityForCurrentPosition(true, true, false);
			if (!this._CorrectedPositionInCurrentComputeVisibilityPass)
			{
				this.CorrectPositionsOfVisibleItems(false);
			}
			this._SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
		}

		private void ChangeItemsCountInternal(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, bool contentPanelEndEdgeStationary, bool keepVelocity)
		{
			this.CancelAnimationsIfAny();
			bool skipComputeVisibilityInUpdateOrOnScroll = this._SkipComputeVisibilityInUpdateOrOnScroll;
			this._SkipComputeVisibilityInUpdateOrOnScroll = true;
			int itemsCount = this._ItemsDesc.itemsCount;
			Vector2 velocity = this._Params.scrollRect.velocity;
			if (!keepVelocity)
			{
				this._Params.scrollRect.StopMovement();
			}
			double cumulatedSizeOfAllItems = this._ItemsDesc.CumulatedSizeOfAllItems;
			this._ItemsDesc.realIndexOfFirstItemInView = ((count <= 0) ? (-1) : 0);
			this.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, this._ItemsDesc);
			double cumulatedSizeOfAllItems2 = this._ItemsDesc.CumulatedSizeOfAllItems;
			bool flag = this._InternalState.ContentPanelVirtualInsetFromViewportEnd >= 0.0;
			if (cumulatedSizeOfAllItems2 <= cumulatedSizeOfAllItems)
			{
				if (this._InternalState.ContentPanelVirtualInsetFromViewportStart >= 0.0)
				{
					contentPanelEndEdgeStationary = false;
				}
				else if (flag)
				{
					contentPanelEndEdgeStationary = true;
				}
			}
			this._InternalState.OnItemsCountChanged(itemsCount, contentPanelEndEdgeStationary);
			if (this.GetNumExcessObjects() > 0)
			{
				throw new UnityException("ChangeItemsCountInternal: GetNumExcessObjects() > 0 when calling ChangeItemsCountInternal(); this may be due ComputeVisibility not being finished executing yet");
			}
			this._RecyclableItems.AddRange(this._VisibleItems);
			if (count == 0)
			{
				this.ClearCachedRecyclableItems();
			}
			this._VisibleItems.Clear();
			this._VisibleItemsCount = 0;
			double num;
			if (contentPanelEndEdgeStationary)
			{
				num = 0.10000000149011612;
			}
			else
			{
				num = -0.10000000149011612;
				if (flag)
				{
					num = 0.10000000149011612;
				}
			}
			this.ComputeVisibilityForCurrentPosition(true, true, true, num);
			if (!this._CorrectedPositionInCurrentComputeVisibilityPass)
			{
				this.CorrectPositionsOfVisibleItems(true);
			}
			if (keepVelocity)
			{
				this._Params.scrollRect.velocity = velocity;
			}
			if (this.ItemsRefreshed != null)
			{
				this.ItemsRefreshed(itemsCount, count);
			}
			this._SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
		}

		private void MyUpdate()
		{
			if (this._InternalState.computeVisibilityTwinPassScheduled)
			{
				throw new UnityException("ScheduleComputeVisibilityTwinPass() can only be called in UpdateViewsHolder() !!!");
			}
			bool hasScrollViewSizeChanged = this._InternalState.HasScrollViewSizeChanged;
			if (hasScrollViewSizeChanged)
			{
				this.OnScrollViewSizeChanged();
				this.RebuildLayoutDueToScrollViewSizeChange();
				return;
			}
			bool flag = !this.IsDragging && !this._SkipComputeVisibilityInUpdateOrOnScroll && this._Params.Snapper;
			if (this._InternalState.updateRequestPending)
			{
				this._InternalState.updateRequestPending = this._Params.updateMode != BaseParams.UpdateMode.ON_SCROLL;
				if (!this._SkipComputeVisibilityInUpdateOrOnScroll)
				{
					this.ComputeVisibilityForCurrentPosition(false, true, false);
				}
			}
			if (flag)
			{
				this._Params.Snapper.StartSnappingIfNeeded();
			}
			this.UpdateGalleryEffectIfNeeded();
		}

		private void OnScrollViewValueChanged(Vector2 _)
		{
			if (this._SkipComputeVisibilityInUpdateOrOnScroll)
			{
				return;
			}
			if (this._Params.updateMode != BaseParams.UpdateMode.MONOBEHAVIOUR_UPDATE)
			{
				this.ComputeVisibilityForCurrentPosition(false, true, false);
			}
			if (this._Params.updateMode != BaseParams.UpdateMode.ON_SCROLL)
			{
				this._InternalState.updateRequestPending = true;
			}
			if (this.ScrollPositionChanged != null)
			{
				this.ScrollPositionChanged(this.GetNormalizedPosition());
			}
		}

		private void ComputeVisibilityForCurrentPosition(bool forceFireScrollViewPositionChangedEvent, bool virtualizeContentPositionIfNeeded, bool alsoCorrectTransversalPositions, double overrideScrollingDelta)
		{
			double contentPanelVirtualInsetFromViewportStart = this._InternalState.ContentPanelVirtualInsetFromViewportStart;
			this._InternalState.lastProcessedCTVirtualInsetFromParentStart = contentPanelVirtualInsetFromViewportStart - overrideScrollingDelta;
			this.ComputeVisibilityForCurrentPosition(forceFireScrollViewPositionChangedEvent, virtualizeContentPositionIfNeeded, alsoCorrectTransversalPositions);
		}

		private void ComputeVisibilityForCurrentPosition(bool forceFireScrollViewPositionChangedEvent, bool virtualizeContentPositionIfNeeded, bool alsoCorrectTransversalPositions)
		{
			this._CorrectedPositionInCurrentComputeVisibilityPass = false;
			if (this._InternalState.computeVisibilityTwinPassScheduled)
			{
				throw new UnityException("ScheduleComputeVisibilityTwinPass() can only be called in UpdateViewsHolder() !!!");
			}
			double contentPanelVirtualInsetFromViewportStart = this._InternalState.ContentPanelVirtualInsetFromViewportStart;
			double num = contentPanelVirtualInsetFromViewportStart - this._InternalState.lastProcessedCTVirtualInsetFromParentStart;
			Vector2 vector = this._Params.scrollRect.velocity;
			PointerEventData pointerEventData = null;
			bool flag = false;
			if (virtualizeContentPositionIfNeeded && this._VisibleItemsCount > 0)
			{
				flag = this.VirtualizeContentPositionIfNeeded(ref pointerEventData, num, pointerEventData == null, alsoCorrectTransversalPositions);
			}
			this.ComputeVisibility(num);
			if (this._InternalState.computeVisibilityTwinPassScheduled)
			{
				this.ComputeVisibilityTwinPass(ref pointerEventData, num, !flag);
			}
			else
			{
				this._InternalState.lastComputeVisibilityHadATwinPass = false;
			}
			if (pointerEventData != null)
			{
				Utils.ForceSetPointerEventDistanceToZero(pointerEventData);
				vector += pointerEventData.delta * Vector3.Distance(pointerEventData.pressPosition, pointerEventData.position) / 10f;
			}
			if (!this.IsDragging)
			{
				this._Params.scrollRect.velocity = vector;
			}
			this._InternalState.UpdateLastProcessedCTVirtualInsetFromParentStart();
			if ((forceFireScrollViewPositionChangedEvent || num != 0.0) && this.ScrollPositionChanged != null)
			{
				this.ScrollPositionChanged(this.GetNormalizedPosition());
			}
		}

		private void ComputeVisibilityTwinPass(ref PointerEventData pev, double delta, bool returnPointerEventDataIfNeeded)
		{
			this._InternalState.computeVisibilityTwinPassScheduled = false;
			if (this._VisibleItemsCount == 0)
			{
				throw new UnityException("computeVisibilityTwinPassScheduled, but there are no visible items. Only call ScheduleComputeVisibilityTwinPass() in UpdateViewsHolder() !!!");
			}
			bool skipComputeVisibilityInUpdateOrOnScroll = this._SkipComputeVisibilityInUpdateOrOnScroll;
			this._SkipComputeVisibilityInUpdateOrOnScroll = true;
			Canvas.ForceUpdateCanvases();
			float[] array = new float[this._VisibleItemsCount];
			if (this._Params.scrollRect.horizontal)
			{
				for (int i = 0; i < this._VisibleItemsCount; i++)
				{
					TItemViewsHolder titemViewsHolder = this._VisibleItems[i];
					array[i] = titemViewsHolder.root.rect.width;
					this.OnItemWidthChangedPreTwinPass(titemViewsHolder);
				}
			}
			else
			{
				for (int j = 0; j < this._VisibleItemsCount; j++)
				{
					TItemViewsHolder titemViewsHolder = this._VisibleItems[j];
					array[j] = titemViewsHolder.root.rect.height;
					this.OnItemHeightChangedPreTwinPass(titemViewsHolder);
				}
			}
			bool flag = ((delta != 0.0) ? (delta > 0.0) : this._InternalState.preferKeepingContentEndEdgeStationaryInNextComputeVisibilityTwinPass);
			this._InternalState.OnItemsSizesChangedExternally(this._VisibleItems, array, flag);
			this._SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
			if (returnPointerEventDataIfNeeded)
			{
				pev = Utils.GetOriginalPointerEventDataWithPointerDragGO(this._Params.scrollRect.gameObject);
			}
			this._InternalState.lastComputeVisibilityHadATwinPass = true;
		}

		private bool VirtualizeContentPositionIfNeeded(ref PointerEventData pev, double delta, bool returnPointerEventDataIfNeeded, bool alsoCorrectTransversalPositions)
		{
			if (delta == 0.0)
			{
				return false;
			}
			int num = ((delta <= 0.0) ? 1 : 2);
			float num2 = ((!this._Params.scrollRect.horizontal) ? this._Params.scrollRect.verticalNormalizedPosition : (1f - this._Params.scrollRect.horizontalNormalizedPosition));
			int itemIndexInView = this._VisibleItems[0].itemIndexInView;
			int num3 = itemIndexInView + this._VisibleItemsCount - 1;
			bool flag = itemIndexInView == 0;
			bool flag2 = num3 == this._ItemsDesc.itemsCount - 1;
			float insetFromParentEdge = this._Params.content.GetInsetFromParentEdge(this._Params.viewport, this._InternalState.startEdge);
			float insetFromParentEdge2 = this._Params.content.GetInsetFromParentEdge(this._Params.viewport, this._InternalState.endEdge);
			double itemSizeCumulative = this._ItemsDesc.GetItemSizeCumulative(num3, true);
			double itemSizeCumulative2 = this._ItemsDesc.GetItemSizeCumulative(itemIndexInView, true);
			double num4 = itemSizeCumulative;
			if (!flag)
			{
				num4 -= itemSizeCumulative2 - (double)this._ItemsDesc[itemIndexInView];
			}
			num4 += (double)((float)(num3 - itemIndexInView) * this._InternalState.spacing);
			float num5 = -insetFromParentEdge - this._InternalState.GetItemInferredRealInsetFromParentStart(itemIndexInView);
			float num6 = -insetFromParentEdge2 - this._InternalState.GetItemInferredRealInsetFromParentEnd(num3);
			float num11;
			RectTransform.Edge edge;
			if (num == 1)
			{
				int num7 = this._ItemsDesc.itemsCount - num3 - 1;
				double num8 = (double)(this._InternalState.spacing * (float)num7) + (this._ItemsDesc.GetItemSizeCumulative(this._ItemsDesc.itemsCount - 1, true) - itemSizeCumulative) + (double)this._InternalState.paddingContentEnd;
				float num10;
				if (num8 < num4 && (!this._Params.loopItems || !flag2))
				{
					float num9 = (float)num8 + num6;
					num10 = -(this._InternalState.contentPanelSize - this._InternalState.viewportSize - num9);
				}
				else
				{
					if ((!this._Params.loopItems || !flag2) && (double)num2 >= 1.0 - this._InternalState.ProximityToLimitNeeded01ToResetPos)
					{
						return false;
					}
					num10 = -(num5 + this._InternalState.paddingContentStart);
				}
				if (Math.Abs(insetFromParentEdge - num10) < 1f)
				{
					return false;
				}
				num11 = num10;
				edge = this._InternalState.startEdge;
			}
			else
			{
				int num12 = itemIndexInView;
				double num13 = (double)(this._InternalState.spacing * (float)num12) + itemSizeCumulative2 - (double)this._ItemsDesc[itemIndexInView] + (double)this._InternalState.paddingContentStart;
				float num15;
				if (num13 < num4 && (!this._Params.loopItems || !flag))
				{
					float num14 = (float)num13 + num5;
					num15 = -(this._InternalState.contentPanelSize - this._InternalState.viewportSize - num14);
				}
				else
				{
					if ((!this._Params.loopItems || !flag) && (double)num2 <= this._InternalState.ProximityToLimitNeeded01ToResetPos)
					{
						return false;
					}
					num15 = -(num6 + this._InternalState.paddingContentEnd);
				}
				if (Math.Abs(insetFromParentEdge2 - num15) < 1f)
				{
					return false;
				}
				num11 = num15;
				edge = this._InternalState.endEdge;
			}
			bool skipComputeVisibilityInUpdateOrOnScroll = this._SkipComputeVisibilityInUpdateOrOnScroll;
			this._SkipComputeVisibilityInUpdateOrOnScroll = true;
			if (returnPointerEventDataIfNeeded)
			{
				pev = Utils.GetOriginalPointerEventDataWithPointerDragGO(this._Params.scrollRect.gameObject);
			}
			this._Params.content.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this._Params.viewport, edge, num11, this._InternalState.contentPanelSize);
			this._Params.scrollRect.Rebuild(2);
			Canvas.ForceUpdateCanvases();
			this._SkipComputeVisibilityInUpdateOrOnScroll = skipComputeVisibilityInUpdateOrOnScroll;
			bool flag3 = false;
			if (this._Params.loopItems)
			{
				int num16 = -1;
				if (num == 1)
				{
					if (flag2)
					{
						this._InternalState.contentPanelSkippedInsetDueToVirtualization = 0.0;
						TItemViewsHolder titemViewsHolder = this._VisibleItems[0];
						num16 = titemViewsHolder.ItemIndex;
						for (int i = 0; i < this._VisibleItemsCount; i++)
						{
							this._VisibleItems[i].itemIndexInView = i;
						}
					}
				}
				else if (flag)
				{
					this._InternalState.contentPanelSkippedInsetDueToVirtualization = -(this._InternalState.contentPanelVirtualSize - (double)this._InternalState.contentPanelSize);
					num16 = this._ItemsDesc.GetItemRealIndexFromViewIndex(num3 + 1);
					for (int j = 0; j < this._VisibleItemsCount; j++)
					{
						this._VisibleItems[j].itemIndexInView = this._ItemsDesc.itemsCount - this._VisibleItemsCount + j;
					}
				}
				flag3 = num16 != -1;
				if (flag3)
				{
					this._ItemsDesc.RotateItemsSizesOnScrollViewLooped(num16);
				}
			}
			if (!flag3)
			{
				this._InternalState.contentPanelSkippedInsetDueToVirtualization += (double)(insetFromParentEdge - this._Params.content.GetInsetFromParentEdge(this._Params.viewport, this._InternalState.startEdge));
			}
			this.CorrectPositionsOfVisibleItems(alsoCorrectTransversalPositions);
			this._CorrectedPositionInCurrentComputeVisibilityPass = true;
			this._InternalState.UpdateLastProcessedCTVirtualInsetFromParentStart();
			return true;
		}

		private void CorrectPositionsOfVisibleItems(bool alsoCorrectTransversalPositioning)
		{
			if (this._VisibleItemsCount > 0)
			{
				this._InternalState.CorrectPositions(this._VisibleItems, alsoCorrectTransversalPositioning);
			}
		}

		private void ComputeVisibility(double abstractDelta)
		{
			bool flag = abstractDelta <= 0.0;
			float viewportSize = this._InternalState.viewportSize;
			float spacing = this._InternalState.spacing;
			float transversalPaddingContentStart = this._InternalState.transversalPaddingContentStart;
			float itemsConstantTransversalSize = this._ItemsDesc.itemsConstantTransversalSize;
			TItemViewsHolder titemViewsHolder = (TItemViewsHolder)((object)null);
			RectTransform.Edge transvStartEdge = this._InternalState.transvStartEdge;
			int num;
			if (flag)
			{
				num = 1;
			}
			else
			{
				num = -1;
			}
			int num2 = (num + 1) / 2;
			int num3 = 1 - num2;
			int num4 = num3 * (this._ItemsDesc.itemsCount - 1) - num;
			bool flag2 = this._VisibleItemsCount > 0;
			int num5 = num2 * (this._ItemsDesc.itemsCount - 1);
			double contentPanelVirtualInsetFromViewportStart = this._InternalState.ContentPanelVirtualInsetFromViewportStart;
			double num6 = ((!flag) ? (-this._InternalState.contentPanelVirtualSize + (double)this._InternalState.viewportSize - contentPanelVirtualInsetFromViewportStart) : contentPanelVirtualInsetFromViewportStart);
			TItemViewsHolder titemViewsHolder2 = (TItemViewsHolder)((object)null);
			if (flag2)
			{
				int num7 = num2 * (this._VisibleItemsCount - 1);
				titemViewsHolder2 = this._VisibleItems[num7];
				num4 = titemViewsHolder2.itemIndexInView;
				int num8 = num3 * (this._VisibleItemsCount - 1);
				TItemViewsHolder titemViewsHolder3 = this._VisibleItems[num8];
				double num9 = ((!flag) ? this._InternalState.GetItemVirtualInsetFromParentEndUsingItemIndexInView(titemViewsHolder3.itemIndexInView) : this._InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(titemViewsHolder3.itemIndexInView));
				for (;;)
				{
					float num10 = this._ItemsDesc[titemViewsHolder3.itemIndexInView] + spacing;
					bool flag3 = num6 + (num9 + (double)num10) <= 0.0;
					if (!flag3)
					{
						break;
					}
					this._RecyclableItems.Add(titemViewsHolder3);
					this._VisibleItems.RemoveAt(num8);
					this._VisibleItemsCount--;
					if (this._VisibleItemsCount == 0)
					{
						break;
					}
					num8 -= num3;
					num9 += (double)num10;
					titemViewsHolder3 = this._VisibleItems[num8];
				}
			}
			double num11 = double.PositiveInfinity;
			int num12;
			if (Math.Abs(abstractDelta) > 10000.0 && (num12 = (int)Math.Round(Math.Min((double)(this._InternalState.viewportSize / (this._Params.DefaultItemSize + this._InternalState.spacing)), this._AVGVisibleItemsCount))) < this._ItemsDesc.itemsCount)
			{
				int num13 = (int)Math.Round((1.0 - this._InternalState.GetVirtualAbstractNormalizedScrollPosition()) * (double)(this._ItemsDesc.itemsCount - 1 - num2 * num12));
				double num14 = Math.Max(-num6, 0.0);
				int num15 = num13;
				int num16 = num15;
				float num17 = this._ItemsDesc[num16];
				double num18;
				for (num18 = (double)num3 * (this._InternalState.contentPanelVirtualSize - (double)num17) + (double)num * this._InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(num16); num18 <= num14 - (double)num17; num18 += (double)(num17 + this._InternalState.spacing))
				{
					num16 += num;
					num17 = this._ItemsDesc[num16];
				}
				if (num16 == num15)
				{
					while (num18 > num14 - (double)num17)
					{
						num16 -= num;
						num17 = this._ItemsDesc[num16];
						num18 -= (double)(num17 + this._InternalState.spacing);
					}
					num18 += (double)(num17 + this._InternalState.spacing);
				}
				else
				{
					num16 -= num;
				}
				if (!flag2 || (flag && num16 > num4) || (!flag && num16 < num4))
				{
					num4 = num16;
					num11 = num18;
				}
			}
			if (double.IsInfinity(num11) && num4 != num5)
			{
				int num19 = num4 + num;
				if (flag)
				{
					num11 = this._InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(num19);
				}
				else
				{
					num11 = this._InternalState.GetItemVirtualInsetFromParentEndUsingItemIndexInView(num19);
				}
			}
			IL_415:
			while (num4 != num5)
			{
				int num20 = num4;
				bool flag4 = false;
				float num21;
				double num22;
				for (;;)
				{
					num20 += num;
					num21 = this._ItemsDesc[num20];
					num22 = num11;
					bool flag5 = num6 + (num22 + (double)num21) <= 0.0;
					if (!flag5)
					{
						goto IL_47A;
					}
					if (num20 == num5)
					{
						goto Block_20;
					}
					num11 += (double)(num21 + this._InternalState.spacing);
				}
				IL_4AC:
				if (flag4)
				{
					IL_666:
					if (this._VisibleItemsCount > this._ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange)
					{
						this._ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange = this._VisibleItemsCount;
					}
					int i = 0;
					while (i < this._RecyclableItems.Count)
					{
						TItemViewsHolder titemViewsHolder4 = this._RecyclableItems[i];
						GameObject gameObject = titemViewsHolder4.root.gameObject;
						if (gameObject.activeSelf)
						{
							this.OnBeforeRecycleOrDisableViewsHolder(titemViewsHolder4, -1);
						}
						gameObject.SetActive(false);
						if (this.ShouldDestroyRecyclableItem(titemViewsHolder4, this.GetNumExcessObjects() > 0))
						{
							Object.Destroy(gameObject);
							this._RecyclableItems.RemoveAt(i);
							this._ItemsDesc.destroyedItemsSinceLastScrollViewSizeChange++;
						}
						else
						{
							i++;
						}
					}
					this._AVGVisibleItemsCount = this._AVGVisibleItemsCount * 0.9 + (double)this._VisibleItemsCount * 0.1;
					return;
				}
				int itemRealIndexFromViewIndex = this._ItemsDesc.GetItemRealIndexFromViewIndex(num20);
				for (int j = 0; j < this._RecyclableItems.Count; j++)
				{
					TItemViewsHolder titemViewsHolder5 = this._RecyclableItems[j];
					if (this.IsRecyclable(titemViewsHolder5, itemRealIndexFromViewIndex, num21))
					{
						this.OnBeforeRecycleOrDisableViewsHolder(titemViewsHolder5, itemRealIndexFromViewIndex);
						this._RecyclableItems.RemoveAt(j);
						titemViewsHolder = titemViewsHolder5;
						IL_53B:
						this._VisibleItems.Insert(num2 * this._VisibleItemsCount, titemViewsHolder);
						this._VisibleItemsCount++;
						titemViewsHolder.ItemIndex = itemRealIndexFromViewIndex;
						titemViewsHolder.itemIndexInView = num20;
						RectTransform root = titemViewsHolder.root;
						root.SetParent(this._Params.content, false);
						this.UpdateViewsHolder(titemViewsHolder);
						titemViewsHolder.root.gameObject.SetActive(true);
						RectTransform rectTransform = root;
						Vector2 constantAnchorPosForAllItems = this._InternalState.constantAnchorPosForAllItems;
						root.anchorMax = constantAnchorPosForAllItems;
						rectTransform.anchorMin = constantAnchorPosForAllItems;
						double num23 = (double)num3 * (this._InternalState.contentPanelVirtualSize - (double)num21) + (double)num * num22;
						root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this._Params.content, this._InternalState.startEdge, this._InternalState.ConvertItemInsetFromParentStart_FromVirtualToReal(num23), num21);
						root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this._Params.content, transvStartEdge, transversalPaddingContentStart, itemsConstantTransversalSize);
						num4 = num20;
						num11 += (double)(num21 + this._InternalState.spacing);
						goto IL_415;
					}
				}
				titemViewsHolder = this.CreateViewsHolder(itemRealIndexFromViewIndex);
				goto IL_53B;
				Block_20:
				flag4 = true;
				goto IL_4AC;
				IL_47A:
				if (num6 + num22 > (double)viewportSize)
				{
					flag4 = true;
					goto IL_4AC;
				}
				goto IL_4AC;
			}
			goto IL_666;
		}

		private int GetNumExcessObjects()
		{
			if (this._RecyclableItems.Count > 1)
			{
				int num = this._RecyclableItems.Count + this._VisibleItemsCount - this.GetMaxNumObjectsToKeepInMemory();
				if (num > 0)
				{
					return num;
				}
			}
			return 0;
		}

		private int GetMaxNumObjectsToKeepInMemory()
		{
			return (this._Params.recycleBinCapacity <= 0) ? (this._ItemsDesc.maxVisibleItemsSeenSinceLastScrollViewSizeChange + this._ItemsDesc.destroyedItemsSinceLastScrollViewSizeChange + 1) : (this._Params.recycleBinCapacity + this._VisibleItemsCount);
		}

		private void UpdateGalleryEffectIfNeeded()
		{
			if (this._Params.galleryEffectAmount == 0f)
			{
				if (this._PrevGalleryEffectAmount == this._Params.galleryEffectAmount)
				{
					return;
				}
				foreach (TItemViewsHolder titemViewsHolder in this._RecyclableItems)
				{
					if (titemViewsHolder != null && titemViewsHolder.root)
					{
						titemViewsHolder.root.localScale = Vector3.one;
					}
				}
			}
			if (this._VisibleItemsCount == 0)
			{
				return;
			}
			float num = this._Params.galleryEffectViewportPivot * 2f - 1f;
			float num2;
			Func<RectTransform, float> func;
			if (this._Params.scrollRect.horizontal)
			{
				num2 = 1f;
				func = new Func<RectTransform, float>(RectTransformExtensions.GetWorldRight);
			}
			else
			{
				num2 = -1f;
				func = new Func<RectTransform, float>(RectTransformExtensions.GetWorldTop);
			}
			float num3 = this._InternalState.viewportSize / 2f;
			float num4 = func(this._Params.viewport) - num3 + num3 * (num * num2);
			for (int i = 0; i < this._VisibleItemsCount; i++)
			{
				TItemViewsHolder titemViewsHolder2 = this._VisibleItems[i];
				float num5 = func(titemViewsHolder2.root) - this._ItemsDesc[titemViewsHolder2.itemIndexInView] / 2f;
				float num6 = 1f - Mathf.Clamp01(Mathf.Abs(num5 - num4) / num3);
				titemViewsHolder2.root.localScale = Vector3.Lerp(Vector3.one * (1f - this._Params.galleryEffectAmount), Vector3.one, num6);
			}
			this._PrevGalleryEffectAmount = this._Params.galleryEffectAmount;
		}

		[SerializeField]
		protected TParams _Params;

		protected List<TItemViewsHolder> _VisibleItems;

		protected int _VisibleItemsCount;

		protected List<TItemViewsHolder> _RecyclableItems = new List<TItemViewsHolder>();

		private SRIA<TParams, TItemViewsHolder>.InternalState _InternalState;

		private ItemsDescriptor _ItemsDesc;

		private Coroutine _SmoothScrollCoroutine;

		private bool _SkipComputeVisibilityInUpdateOrOnScroll;

		private bool _CorrectedPositionInCurrentComputeVisibilityPass;

		private float _PrevGalleryEffectAmount;

		private double _AVGVisibleItemsCount;

		private class InternalState : InternalStateGeneric<TParams, TItemViewsHolder>
		{
			protected InternalState(TParams sourceParams, ItemsDescriptor itemsDescriptor)
				: base(sourceParams, itemsDescriptor)
			{
			}

			internal static SRIA<TParams, TItemViewsHolder>.InternalState CreateFromSourceParamsOrThrow(TParams sourceParams, ItemsDescriptor itemsDescriptor)
			{
				if (sourceParams.scrollRect.horizontal && sourceParams.scrollRect.vertical)
				{
					throw new UnityException("Can't optimize a ScrollRect with both horizontal and vertical scrolling modes. Disable one of them");
				}
				return new SRIA<TParams, TItemViewsHolder>.InternalState(sourceParams, itemsDescriptor);
			}
		}
	}
}
