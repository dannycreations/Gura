using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EnhancedUI.EnhancedScroller
{
	[RequireComponent(typeof(ScrollRect))]
	public class EnhancedScroller : MonoBehaviour
	{
		public IEnhancedScrollerDelegate Delegate
		{
			get
			{
				return this._delegate;
			}
			set
			{
				this._delegate = value;
				this._reloadData = true;
			}
		}

		public float ScrollPosition
		{
			get
			{
				return this._scrollPosition;
			}
			set
			{
				value = Mathf.Clamp(value, 0f, this.GetScrollPositionForCellViewIndex(this._cellViewSizeArray.Count - 1, EnhancedScroller.CellViewPositionEnum.Before));
				if (this._scrollPosition != value)
				{
					this._scrollPosition = value;
					if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
					{
						this._scrollRect.verticalNormalizedPosition = 1f - this._scrollPosition / this.ScrollSize;
					}
					else
					{
						this._scrollRect.horizontalNormalizedPosition = this._scrollPosition / this.ScrollSize;
					}
				}
			}
		}

		public float ScrollSize
		{
			get
			{
				if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
				{
					return this._container.rect.height - this._scrollRectTransform.rect.height;
				}
				return this._container.rect.width - this._scrollRectTransform.rect.width;
			}
		}

		public float NormalizedScrollPosition
		{
			get
			{
				return this._scrollPosition / this.ScrollSize;
			}
		}

		public bool Loop
		{
			get
			{
				return this.loop;
			}
			set
			{
				if (this.loop != value)
				{
					float scrollPosition = this._scrollPosition;
					this.loop = value;
					this._Resize(false);
					if (this.loop)
					{
						this.ScrollPosition = this._loopFirstScrollPosition + scrollPosition;
					}
					else
					{
						this.ScrollPosition = scrollPosition - this._loopFirstScrollPosition;
					}
					this.ScrollbarVisibility = this.scrollbarVisibility;
				}
			}
		}

		public EnhancedScroller.ScrollbarVisibilityEnum ScrollbarVisibility
		{
			get
			{
				return this.scrollbarVisibility;
			}
			set
			{
				this.scrollbarVisibility = value;
				if (this._scrollbar != null && this._cellViewOffsetArray != null && this._cellViewOffsetArray.Count > 0)
				{
					if (this._cellViewOffsetArray.Last() < this.ScrollRectSize || this.loop)
					{
						this._scrollbar.gameObject.SetActive(this.scrollbarVisibility == EnhancedScroller.ScrollbarVisibilityEnum.Always);
					}
					else
					{
						this._scrollbar.gameObject.SetActive(this.scrollbarVisibility != EnhancedScroller.ScrollbarVisibilityEnum.Never);
					}
				}
			}
		}

		public Vector2 Velocity
		{
			get
			{
				return this._scrollRect.velocity;
			}
			set
			{
				this._scrollRect.velocity = value;
			}
		}

		public float LinearVelocity
		{
			get
			{
				return (this.scrollDirection != EnhancedScroller.ScrollDirectionEnum.Vertical) ? this._scrollRect.velocity.x : this._scrollRect.velocity.y;
			}
			set
			{
				if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
				{
					this._scrollRect.velocity = new Vector2(0f, value);
				}
				else
				{
					this._scrollRect.velocity = new Vector2(value, 0f);
				}
			}
		}

		public bool IsScrolling { get; private set; }

		public bool IsTweening { get; private set; }

		public int StartCellViewIndex
		{
			get
			{
				return this._activeCellViewsStartIndex;
			}
		}

		public int EndCellViewIndex
		{
			get
			{
				return this._activeCellViewsEndIndex;
			}
		}

		public int StartDataIndex
		{
			get
			{
				return this._activeCellViewsStartIndex % this.NumberOfCells;
			}
		}

		public int EndDataIndex
		{
			get
			{
				return this._activeCellViewsEndIndex % this.NumberOfCells;
			}
		}

		public int NumberOfCells
		{
			get
			{
				return (this._delegate == null) ? 0 : this._delegate.GetNumberOfCells(this);
			}
		}

		public ScrollRect ScrollRect
		{
			get
			{
				return this._scrollRect;
			}
		}

		public float ScrollRectSize
		{
			get
			{
				if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
				{
					return this._scrollRectTransform.rect.height;
				}
				return this._scrollRectTransform.rect.width;
			}
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScrollerCellView cellPrefab)
		{
			EnhancedScrollerCellView enhancedScrollerCellView = this._GetRecycledCellView(cellPrefab);
			if (enhancedScrollerCellView == null)
			{
				GameObject gameObject = Object.Instantiate<GameObject>(cellPrefab.gameObject);
				enhancedScrollerCellView = gameObject.GetComponent<EnhancedScrollerCellView>();
				enhancedScrollerCellView.transform.SetParent(this._container);
				enhancedScrollerCellView.transform.localPosition = Vector3.zero;
				enhancedScrollerCellView.transform.localRotation = Quaternion.identity;
			}
			return enhancedScrollerCellView;
		}

		public void ReloadData(float scrollPositionFactor = 0f)
		{
			this._reloadData = false;
			this._RecycleAllCells();
			if (this._delegate != null)
			{
				this._Resize(false);
			}
			if (this._scrollRect == null || this._scrollRectTransform == null || this._container == null)
			{
				this._scrollPosition = 0f;
				return;
			}
			this._scrollPosition = scrollPositionFactor * this.ScrollSize;
			if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
			{
				this._scrollRect.verticalNormalizedPosition = 1f - scrollPositionFactor;
			}
			else
			{
				this._scrollRect.horizontalNormalizedPosition = scrollPositionFactor;
			}
		}

		public void RefreshActiveCellViews()
		{
			for (int i = 0; i < this._activeCellViews.Count; i++)
			{
				this._activeCellViews[i].RefreshCellView();
			}
		}

		public void ClearAll()
		{
			this.ClearActive();
			this.ClearRecycled();
		}

		public void ClearActive()
		{
			for (int i = 0; i < this._activeCellViews.Count; i++)
			{
				Object.DestroyImmediate(this._activeCellViews[i].gameObject);
			}
			this._activeCellViews.Clear();
		}

		public void ClearRecycled()
		{
			for (int i = 0; i < this._recycledCellViews.Count; i++)
			{
				Object.DestroyImmediate(this._recycledCellViews[i].gameObject);
			}
			this._recycledCellViews.Clear();
		}

		public void ToggleLoop()
		{
			this.Loop = !this.loop;
		}

		public void JumpToDataIndex(int dataIndex, float scrollerOffset = 0f, float cellOffset = 0f, bool useSpacing = true, EnhancedScroller.TweenType tweenType = EnhancedScroller.TweenType.immediate, float tweenTime = 0f, Action jumpComplete = null, EnhancedScroller.LoopJumpDirectionEnum loopJumpDirection = EnhancedScroller.LoopJumpDirectionEnum.Closest)
		{
			if (this.StartDataIndex == 0 && this.EndDataIndex == this.NumberOfCells - 1)
			{
				if (jumpComplete != null)
				{
					jumpComplete();
				}
				return;
			}
			float num = 0f;
			if (cellOffset != 0f)
			{
				float num2 = ((this._delegate == null) ? 0f : this._delegate.GetCellViewSize(this, dataIndex));
				if (useSpacing)
				{
					num2 += this.spacing;
					if (dataIndex > 0 && dataIndex < this.NumberOfCells - 1)
					{
						num2 += this.spacing;
					}
				}
				num = num2 * cellOffset;
			}
			float num3 = 0f;
			float num4 = -(scrollerOffset * this.ScrollRectSize) + num;
			if (this.loop)
			{
				float num5 = this.GetScrollPositionForCellViewIndex(dataIndex, EnhancedScroller.CellViewPositionEnum.Before) + num4;
				float num6 = this.GetScrollPositionForCellViewIndex(dataIndex + this.NumberOfCells, EnhancedScroller.CellViewPositionEnum.Before) + num4;
				float num7 = this.GetScrollPositionForCellViewIndex(dataIndex + this.NumberOfCells * 2, EnhancedScroller.CellViewPositionEnum.Before) + num4;
				float num8 = Mathf.Abs(this._scrollPosition - num5);
				float num9 = Mathf.Abs(this._scrollPosition - num6);
				float num10 = Mathf.Abs(this._scrollPosition - num7);
				if (loopJumpDirection != EnhancedScroller.LoopJumpDirectionEnum.Closest)
				{
					if (loopJumpDirection != EnhancedScroller.LoopJumpDirectionEnum.Up)
					{
						if (loopJumpDirection == EnhancedScroller.LoopJumpDirectionEnum.Down)
						{
							num3 = num7;
						}
					}
					else
					{
						num3 = num5;
					}
				}
				else if (num8 < num9)
				{
					if (num8 < num10)
					{
						num3 = num5;
					}
					else
					{
						num3 = num7;
					}
				}
				else if (num9 < num10)
				{
					num3 = num6;
				}
				else
				{
					num3 = num7;
				}
			}
			else
			{
				num3 = this.GetScrollPositionForDataIndex(dataIndex, EnhancedScroller.CellViewPositionEnum.Before) + num4;
			}
			num3 = Mathf.Clamp(num3, 0f, this.GetScrollPositionForCellViewIndex(this._cellViewSizeArray.Count - 1, EnhancedScroller.CellViewPositionEnum.Before));
			if (useSpacing)
			{
				num3 = Mathf.Clamp(num3 - this.spacing, 0f, this.GetScrollPositionForCellViewIndex(this._cellViewSizeArray.Count - 1, EnhancedScroller.CellViewPositionEnum.Before));
			}
			base.StartCoroutine(this.TweenPosition(tweenType, tweenTime, this.ScrollPosition, num3, jumpComplete));
		}

		public void Snap()
		{
			if (this.NumberOfCells == 0)
			{
				return;
			}
			this._snapJumping = true;
			this.LinearVelocity = 0f;
			this._snapInertia = this._scrollRect.inertia;
			this._scrollRect.inertia = false;
			float num = this.ScrollPosition + this.ScrollRectSize * Mathf.Clamp01(this.snapWatchOffset);
			this._snapCellViewIndex = this.GetCellViewIndexAtPosition(num);
			this._snapDataIndex = this._snapCellViewIndex % this.NumberOfCells;
			this.JumpToDataIndex(this._snapDataIndex, this.snapJumpToOffset, this.snapCellCenterOffset, this.snapUseCellSpacing, this.snapTweenType, this.snapTweenTime, new Action(this.SnapJumpComplete), EnhancedScroller.LoopJumpDirectionEnum.Closest);
		}

		public float GetScrollPositionForCellViewIndex(int cellViewIndex, EnhancedScroller.CellViewPositionEnum insertPosition)
		{
			if (this.NumberOfCells == 0)
			{
				return 0f;
			}
			if (cellViewIndex == 0 && insertPosition == EnhancedScroller.CellViewPositionEnum.Before)
			{
				return 0f;
			}
			if (cellViewIndex >= this._cellViewOffsetArray.Count)
			{
				return this._cellViewOffsetArray[this._cellViewOffsetArray.Count - 2];
			}
			if (insertPosition == EnhancedScroller.CellViewPositionEnum.Before)
			{
				return this._cellViewOffsetArray[cellViewIndex - 1] + this.spacing + (float)((this.scrollDirection != EnhancedScroller.ScrollDirectionEnum.Vertical) ? this.padding.left : this.padding.top);
			}
			return this._cellViewOffsetArray[cellViewIndex] + (float)((this.scrollDirection != EnhancedScroller.ScrollDirectionEnum.Vertical) ? this.padding.left : this.padding.top);
		}

		public float GetScrollPositionForDataIndex(int dataIndex, EnhancedScroller.CellViewPositionEnum insertPosition)
		{
			return this.GetScrollPositionForCellViewIndex((!this.loop) ? dataIndex : (this._delegate.GetNumberOfCells(this) + dataIndex), insertPosition);
		}

		public int GetCellViewIndexAtPosition(float position)
		{
			return this._GetCellIndexAtPosition(position, 0, this._cellViewOffsetArray.Count - 1);
		}

		private void _Resize(bool keepPosition)
		{
			float scrollPosition = this._scrollPosition;
			this._cellViewSizeArray.Clear();
			float num = this._AddCellViewSizes();
			if (this.loop)
			{
				if (num < this.ScrollRectSize)
				{
					int num2 = Mathf.CeilToInt(this.ScrollRectSize / num);
					this._DuplicateCellViewSizes(num2, this._cellViewSizeArray.Count);
				}
				this._loopFirstCellIndex = this._cellViewSizeArray.Count;
				this._loopLastCellIndex = this._loopFirstCellIndex + this._cellViewSizeArray.Count - 1;
				this._DuplicateCellViewSizes(2, this._cellViewSizeArray.Count);
			}
			this._CalculateCellViewOffsets();
			if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
			{
				this._container.sizeDelta = new Vector2(this._container.sizeDelta.x, this._cellViewOffsetArray.Last() + (float)this.padding.top + (float)this.padding.bottom);
			}
			else
			{
				this._container.sizeDelta = new Vector2(this._cellViewOffsetArray.Last() + (float)this.padding.left + (float)this.padding.right, this._container.sizeDelta.y);
			}
			if (this.loop)
			{
				this._loopFirstScrollPosition = this.GetScrollPositionForCellViewIndex(this._loopFirstCellIndex, EnhancedScroller.CellViewPositionEnum.Before) + this.spacing * 0.5f;
				this._loopLastScrollPosition = this.GetScrollPositionForCellViewIndex(this._loopLastCellIndex, EnhancedScroller.CellViewPositionEnum.After) - this.ScrollRectSize + this.spacing * 0.5f;
				this._loopFirstJumpTrigger = this._loopFirstScrollPosition - this.ScrollRectSize;
				this._loopLastJumpTrigger = this._loopLastScrollPosition + this.ScrollRectSize;
			}
			this._ResetVisibleCellViews();
			if (keepPosition)
			{
				this.ScrollPosition = scrollPosition;
			}
			else if (this.loop)
			{
				this.ScrollPosition = this._loopFirstScrollPosition;
			}
			else
			{
				this.ScrollPosition = 0f;
			}
			this.ScrollbarVisibility = this.scrollbarVisibility;
		}

		private float _AddCellViewSizes()
		{
			float num = 0f;
			for (int i = 0; i < this.NumberOfCells; i++)
			{
				this._cellViewSizeArray.Add(this._delegate.GetCellViewSize(this, i) + ((i != 0) ? this._layoutGroup.spacing : 0f));
				num += this._cellViewSizeArray[this._cellViewSizeArray.Count - 1];
			}
			return num;
		}

		private void _DuplicateCellViewSizes(int numberOfTimes, int cellCount)
		{
			for (int i = 0; i < numberOfTimes; i++)
			{
				for (int j = 0; j < cellCount; j++)
				{
					this._cellViewSizeArray.Add(this._cellViewSizeArray[j] + ((j != 0) ? 0f : this._layoutGroup.spacing));
				}
			}
		}

		private void _CalculateCellViewOffsets()
		{
			this._cellViewOffsetArray.Clear();
			float num = 0f;
			for (int i = 0; i < this._cellViewSizeArray.Count; i++)
			{
				num += this._cellViewSizeArray[i];
				this._cellViewOffsetArray.Add(num);
			}
		}

		private EnhancedScrollerCellView _GetRecycledCellView(EnhancedScrollerCellView cellPrefab)
		{
			for (int i = 0; i < this._recycledCellViews.Count; i++)
			{
				if (this._recycledCellViews[i].cellIdentifier == cellPrefab.cellIdentifier)
				{
					return this._recycledCellViews.RemoveAt(i);
				}
			}
			return null;
		}

		private void _ResetVisibleCellViews()
		{
			int num;
			int num2;
			this._CalculateCurrentActiveCellRange(out num, out num2);
			int i = 0;
			SmallList<int> smallList = new SmallList<int>();
			while (i < this._activeCellViews.Count)
			{
				if (this._activeCellViews[i].cellIndex < num || this._activeCellViews[i].cellIndex > num2)
				{
					this._RecycleCell(this._activeCellViews[i]);
				}
				else
				{
					smallList.Add(this._activeCellViews[i].cellIndex);
					i++;
				}
			}
			if (smallList.Count == 0)
			{
				for (i = num; i <= num2; i++)
				{
					this._AddCellView(i, EnhancedScroller.ListPositionEnum.Last);
				}
			}
			else
			{
				for (i = num2; i >= num; i--)
				{
					if (i < smallList.First())
					{
						this._AddCellView(i, EnhancedScroller.ListPositionEnum.First);
					}
				}
				for (i = num; i <= num2; i++)
				{
					if (i > smallList.Last())
					{
						this._AddCellView(i, EnhancedScroller.ListPositionEnum.Last);
					}
				}
			}
			this._activeCellViewsStartIndex = num;
			this._activeCellViewsEndIndex = num2;
			this._SetPadders();
		}

		private void _RecycleAllCells()
		{
			while (this._activeCellViews.Count > 0)
			{
				this._RecycleCell(this._activeCellViews[0]);
			}
			this._activeCellViewsStartIndex = 0;
			this._activeCellViewsEndIndex = 0;
		}

		private void _RecycleCell(EnhancedScrollerCellView cellView)
		{
			if (this.cellViewWillRecycle != null)
			{
				this.cellViewWillRecycle(cellView);
			}
			this._activeCellViews.Remove(cellView);
			this._recycledCellViews.Add(cellView);
			cellView.transform.SetParent(this._recycledCellViewContainer);
			cellView.dataIndex = 0;
			cellView.cellIndex = 0;
			cellView.active = false;
			if (this.cellViewVisibilityChanged != null)
			{
				this.cellViewVisibilityChanged(cellView);
			}
		}

		private void _AddCellView(int cellIndex, EnhancedScroller.ListPositionEnum listPosition)
		{
			if (this.NumberOfCells == 0)
			{
				return;
			}
			int num = cellIndex % this.NumberOfCells;
			EnhancedScrollerCellView cellView = this._delegate.GetCellView(this, num, cellIndex);
			cellView.cellIndex = cellIndex;
			cellView.dataIndex = num;
			cellView.active = true;
			cellView.transform.SetParent(this._container, false);
			cellView.transform.localScale = Vector3.one;
			LayoutElement layoutElement = cellView.GetComponent<LayoutElement>();
			if (layoutElement == null)
			{
				layoutElement = cellView.gameObject.AddComponent<LayoutElement>();
			}
			if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
			{
				layoutElement.minHeight = this._cellViewSizeArray[cellIndex] - ((cellIndex <= 0) ? 0f : this._layoutGroup.spacing);
			}
			else
			{
				layoutElement.minWidth = this._cellViewSizeArray[cellIndex] - ((cellIndex <= 0) ? 0f : this._layoutGroup.spacing);
			}
			if (listPosition == EnhancedScroller.ListPositionEnum.First)
			{
				this._activeCellViews.AddStart(cellView);
			}
			else
			{
				this._activeCellViews.Add(cellView);
			}
			if (listPosition == EnhancedScroller.ListPositionEnum.Last)
			{
				cellView.transform.SetSiblingIndex(this._container.childCount - 2);
			}
			else if (listPosition == EnhancedScroller.ListPositionEnum.First)
			{
				cellView.transform.SetSiblingIndex(1);
			}
			if (this.cellViewVisibilityChanged != null)
			{
				this.cellViewVisibilityChanged(cellView);
			}
		}

		private void _SetPadders()
		{
			if (this.NumberOfCells == 0)
			{
				return;
			}
			float num = this._cellViewOffsetArray[this._activeCellViewsStartIndex] - this._cellViewSizeArray[this._activeCellViewsStartIndex];
			float num2 = this._cellViewOffsetArray.Last() - this._cellViewOffsetArray[this._activeCellViewsEndIndex];
			if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
			{
				this._firstPadder.minHeight = num;
				this._firstPadder.gameObject.SetActive(this._firstPadder.minHeight > 0f);
				this._lastPadder.minHeight = num2;
				this._lastPadder.gameObject.SetActive(this._lastPadder.minHeight > 0f);
			}
			else
			{
				this._firstPadder.minWidth = num;
				this._firstPadder.gameObject.SetActive(this._firstPadder.minWidth > 0f);
				this._lastPadder.minWidth = num2;
				this._lastPadder.gameObject.SetActive(this._lastPadder.minWidth > 0f);
			}
		}

		private void _RefreshActive()
		{
			Vector2 vector = Vector2.zero;
			if (this.loop)
			{
				if (this._scrollPosition < this._loopFirstJumpTrigger)
				{
					vector = this._scrollRect.velocity;
					this.ScrollPosition = this._loopLastScrollPosition - (this._loopFirstJumpTrigger - this._scrollPosition) + this.spacing;
					this._scrollRect.velocity = vector;
				}
				else if (this._scrollPosition > this._loopLastJumpTrigger)
				{
					vector = this._scrollRect.velocity;
					this.ScrollPosition = this._loopFirstScrollPosition + (this._scrollPosition - this._loopLastJumpTrigger) - this.spacing;
					this._scrollRect.velocity = vector;
				}
			}
			int num;
			int num2;
			this._CalculateCurrentActiveCellRange(out num, out num2);
			if (num == this._activeCellViewsStartIndex && num2 == this._activeCellViewsEndIndex)
			{
				return;
			}
			this._ResetVisibleCellViews();
		}

		private void _CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
		{
			startIndex = 0;
			endIndex = 0;
			float scrollPosition = this._scrollPosition;
			float num = this._scrollPosition + ((this.scrollDirection != EnhancedScroller.ScrollDirectionEnum.Vertical) ? this._scrollRectTransform.rect.width : this._scrollRectTransform.rect.height);
			startIndex = this.GetCellViewIndexAtPosition(scrollPosition);
			endIndex = this.GetCellViewIndexAtPosition(num);
		}

		private int _GetCellIndexAtPosition(float position, int startIndex, int endIndex)
		{
			if (startIndex >= endIndex)
			{
				return startIndex;
			}
			int num = (startIndex + endIndex) / 2;
			if (this._cellViewOffsetArray[num] + (float)((this.scrollDirection != EnhancedScroller.ScrollDirectionEnum.Vertical) ? this.padding.left : this.padding.top) >= position)
			{
				return this._GetCellIndexAtPosition(position, startIndex, num);
			}
			return this._GetCellIndexAtPosition(position, num + 1, endIndex);
		}

		private void Awake()
		{
			this._scrollRect = base.GetComponent<ScrollRect>();
			this._scrollRectTransform = this._scrollRect.GetComponent<RectTransform>();
			if (this._scrollRect.content != null)
			{
				Object.DestroyImmediate(this._scrollRect.content.gameObject);
			}
			GameObject gameObject = new GameObject("Container", new Type[] { typeof(RectTransform) });
			gameObject.transform.SetParent(this._scrollRectTransform);
			if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
			{
				gameObject.AddComponent<VerticalLayoutGroup>();
			}
			else
			{
				gameObject.AddComponent<HorizontalLayoutGroup>();
			}
			this._container = gameObject.GetComponent<RectTransform>();
			if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
			{
				this._container.anchorMin = new Vector2(0f, 1f);
				this._container.anchorMax = Vector2.one;
				this._container.pivot = new Vector2(0.5f, 1f);
			}
			else
			{
				this._container.anchorMin = Vector2.zero;
				this._container.anchorMax = new Vector2(0f, 1f);
				this._container.pivot = new Vector2(0f, 0.5f);
			}
			this._container.offsetMax = Vector2.zero;
			this._container.offsetMin = Vector2.zero;
			this._container.localPosition = Vector3.zero;
			this._container.localRotation = Quaternion.identity;
			this._container.localScale = Vector3.one;
			this._scrollRect.content = this._container;
			if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
			{
				this._scrollbar = this._scrollRect.verticalScrollbar;
			}
			else
			{
				this._scrollbar = this._scrollRect.horizontalScrollbar;
			}
			this._layoutGroup = this._container.GetComponent<HorizontalOrVerticalLayoutGroup>();
			this._layoutGroup.spacing = this.spacing;
			this._layoutGroup.padding = this.padding;
			this._layoutGroup.childAlignment = 0;
			this._layoutGroup.childForceExpandHeight = true;
			this._layoutGroup.childForceExpandWidth = true;
			this._scrollRect.horizontal = this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Horizontal;
			this._scrollRect.vertical = this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical;
			gameObject = new GameObject("First Padder", new Type[]
			{
				typeof(RectTransform),
				typeof(LayoutElement)
			});
			gameObject.transform.SetParent(this._container, false);
			this._firstPadder = gameObject.GetComponent<LayoutElement>();
			gameObject = new GameObject("Last Padder", new Type[]
			{
				typeof(RectTransform),
				typeof(LayoutElement)
			});
			gameObject.transform.SetParent(this._container, false);
			this._lastPadder = gameObject.GetComponent<LayoutElement>();
			gameObject = new GameObject("Recycled Cells", new Type[] { typeof(RectTransform) });
			gameObject.transform.SetParent(this._scrollRect.transform, false);
			this._recycledCellViewContainer = gameObject.GetComponent<RectTransform>();
			this._recycledCellViewContainer.gameObject.SetActive(false);
			this._lastScrollRectSize = this.ScrollRectSize;
			this._lastLoop = this.loop;
			this._lastScrollbarVisibility = this.scrollbarVisibility;
		}

		private void Update()
		{
			if (this._reloadData)
			{
				this.ReloadData(0f);
			}
			if ((this.loop && this._lastScrollRectSize != this.ScrollRectSize) || this.loop != this._lastLoop)
			{
				this._Resize(true);
				this._lastScrollRectSize = this.ScrollRectSize;
				this._lastLoop = this.loop;
			}
			if (this._lastScrollbarVisibility != this.scrollbarVisibility)
			{
				this.ScrollbarVisibility = this.scrollbarVisibility;
				this._lastScrollbarVisibility = this.scrollbarVisibility;
			}
			if (this.LinearVelocity != 0f && !this.IsScrolling)
			{
				this.IsScrolling = true;
				if (this.scrollerScrollingChanged != null)
				{
					this.scrollerScrollingChanged(this, true);
				}
			}
			else if (this.LinearVelocity == 0f && this.IsScrolling)
			{
				this.IsScrolling = false;
				if (this.scrollerScrollingChanged != null)
				{
					this.scrollerScrollingChanged(this, false);
				}
			}
		}

		private void OnEnable()
		{
			this._scrollRect.onValueChanged.AddListener(new UnityAction<Vector2>(this._ScrollRect_OnValueChanged));
		}

		private void OnDisable()
		{
			this._scrollRect.onValueChanged.RemoveListener(new UnityAction<Vector2>(this._ScrollRect_OnValueChanged));
		}

		private void _ScrollRect_OnValueChanged(Vector2 val)
		{
			if (this.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical)
			{
				this._scrollPosition = (1f - val.y) * this.ScrollSize;
			}
			else
			{
				this._scrollPosition = val.x * this.ScrollSize;
			}
			if (this.scrollerScrolled != null)
			{
				this.scrollerScrolled(this, val, this._scrollPosition);
			}
			if (this.snapping && !this._snapJumping && Mathf.Abs(this.LinearVelocity) <= this.snapVelocityThreshold && this.LinearVelocity != 0f)
			{
				this.Snap();
			}
			this._RefreshActive();
		}

		private void SnapJumpComplete()
		{
			this._snapJumping = false;
			this._scrollRect.inertia = this._snapInertia;
			EnhancedScrollerCellView enhancedScrollerCellView = null;
			for (int i = 0; i < this._activeCellViews.Count; i++)
			{
				if (this._activeCellViews[i].dataIndex == this._snapDataIndex)
				{
					enhancedScrollerCellView = this._activeCellViews[i];
					break;
				}
			}
			if (this.scrollerSnapped != null)
			{
				this.scrollerSnapped(this, this._snapCellViewIndex, this._snapDataIndex, enhancedScrollerCellView);
			}
		}

		private IEnumerator TweenPosition(EnhancedScroller.TweenType tweenType, float time, float start, float end, Action tweenComplete)
		{
			if (tweenType == EnhancedScroller.TweenType.immediate || time == 0f)
			{
				this.ScrollPosition = end;
			}
			else
			{
				this._scrollRect.velocity = Vector2.zero;
				this.IsTweening = true;
				if (this.scrollerTweeningChanged != null)
				{
					this.scrollerTweeningChanged(this, true);
				}
				this._tweenTimeLeft = 0f;
				float newPosition = 0f;
				while (this._tweenTimeLeft < time)
				{
					switch (tweenType)
					{
					case EnhancedScroller.TweenType.linear:
						newPosition = this.linear(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.spring:
						newPosition = EnhancedScroller.spring(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInQuad:
						newPosition = EnhancedScroller.easeInQuad(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutQuad:
						newPosition = EnhancedScroller.easeOutQuad(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutQuad:
						newPosition = EnhancedScroller.easeInOutQuad(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInCubic:
						newPosition = EnhancedScroller.easeInCubic(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutCubic:
						newPosition = EnhancedScroller.easeOutCubic(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutCubic:
						newPosition = EnhancedScroller.easeInOutCubic(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInQuart:
						newPosition = EnhancedScroller.easeInQuart(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutQuart:
						newPosition = EnhancedScroller.easeOutQuart(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutQuart:
						newPosition = EnhancedScroller.easeInOutQuart(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInQuint:
						newPosition = EnhancedScroller.easeInQuint(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutQuint:
						newPosition = EnhancedScroller.easeOutQuint(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutQuint:
						newPosition = EnhancedScroller.easeInOutQuint(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInSine:
						newPosition = EnhancedScroller.easeInSine(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutSine:
						newPosition = EnhancedScroller.easeOutSine(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutSine:
						newPosition = EnhancedScroller.easeInOutSine(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInExpo:
						newPosition = EnhancedScroller.easeInExpo(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutExpo:
						newPosition = EnhancedScroller.easeOutExpo(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutExpo:
						newPosition = EnhancedScroller.easeInOutExpo(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInCirc:
						newPosition = EnhancedScroller.easeInCirc(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutCirc:
						newPosition = EnhancedScroller.easeOutCirc(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutCirc:
						newPosition = EnhancedScroller.easeInOutCirc(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInBounce:
						newPosition = EnhancedScroller.easeInBounce(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutBounce:
						newPosition = EnhancedScroller.easeOutBounce(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutBounce:
						newPosition = EnhancedScroller.easeInOutBounce(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInBack:
						newPosition = EnhancedScroller.easeInBack(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutBack:
						newPosition = EnhancedScroller.easeOutBack(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutBack:
						newPosition = EnhancedScroller.easeInOutBack(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInElastic:
						newPosition = EnhancedScroller.easeInElastic(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeOutElastic:
						newPosition = EnhancedScroller.easeOutElastic(start, end, this._tweenTimeLeft / time);
						break;
					case EnhancedScroller.TweenType.easeInOutElastic:
						newPosition = EnhancedScroller.easeInOutElastic(start, end, this._tweenTimeLeft / time);
						break;
					}
					if (this.loop)
					{
						if (end > start && newPosition > this._loopLastJumpTrigger)
						{
							newPosition = this._loopFirstScrollPosition + (newPosition - this._loopLastJumpTrigger);
						}
						else if (start > end && newPosition < this._loopFirstJumpTrigger)
						{
							newPosition = this._loopLastScrollPosition - (this._loopFirstJumpTrigger - newPosition);
						}
					}
					this.ScrollPosition = newPosition;
					this._tweenTimeLeft += Time.unscaledDeltaTime;
					yield return null;
				}
				this.ScrollPosition = end;
			}
			if (tweenComplete != null)
			{
				tweenComplete();
			}
			this.IsTweening = false;
			if (this.scrollerTweeningChanged != null)
			{
				this.scrollerTweeningChanged(this, false);
			}
			yield break;
		}

		private float linear(float start, float end, float val)
		{
			return Mathf.Lerp(start, end, val);
		}

		private static float spring(float start, float end, float val)
		{
			val = Mathf.Clamp01(val);
			val = (Mathf.Sin(val * 3.1415927f * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + 1.2f * (1f - val));
			return start + (end - start) * val;
		}

		private static float easeInQuad(float start, float end, float val)
		{
			end -= start;
			return end * val * val + start;
		}

		private static float easeOutQuad(float start, float end, float val)
		{
			end -= start;
			return -end * val * (val - 2f) + start;
		}

		private static float easeInOutQuad(float start, float end, float val)
		{
			val /= 0.5f;
			end -= start;
			if (val < 1f)
			{
				return end / 2f * val * val + start;
			}
			val -= 1f;
			return -end / 2f * (val * (val - 2f) - 1f) + start;
		}

		private static float easeInCubic(float start, float end, float val)
		{
			end -= start;
			return end * val * val * val + start;
		}

		private static float easeOutCubic(float start, float end, float val)
		{
			val -= 1f;
			end -= start;
			return end * (val * val * val + 1f) + start;
		}

		private static float easeInOutCubic(float start, float end, float val)
		{
			val /= 0.5f;
			end -= start;
			if (val < 1f)
			{
				return end / 2f * val * val * val + start;
			}
			val -= 2f;
			return end / 2f * (val * val * val + 2f) + start;
		}

		private static float easeInQuart(float start, float end, float val)
		{
			end -= start;
			return end * val * val * val * val + start;
		}

		private static float easeOutQuart(float start, float end, float val)
		{
			val -= 1f;
			end -= start;
			return -end * (val * val * val * val - 1f) + start;
		}

		private static float easeInOutQuart(float start, float end, float val)
		{
			val /= 0.5f;
			end -= start;
			if (val < 1f)
			{
				return end / 2f * val * val * val * val + start;
			}
			val -= 2f;
			return -end / 2f * (val * val * val * val - 2f) + start;
		}

		private static float easeInQuint(float start, float end, float val)
		{
			end -= start;
			return end * val * val * val * val * val + start;
		}

		private static float easeOutQuint(float start, float end, float val)
		{
			val -= 1f;
			end -= start;
			return end * (val * val * val * val * val + 1f) + start;
		}

		private static float easeInOutQuint(float start, float end, float val)
		{
			val /= 0.5f;
			end -= start;
			if (val < 1f)
			{
				return end / 2f * val * val * val * val * val + start;
			}
			val -= 2f;
			return end / 2f * (val * val * val * val * val + 2f) + start;
		}

		private static float easeInSine(float start, float end, float val)
		{
			end -= start;
			return -end * Mathf.Cos(val / 1f * 1.5707964f) + end + start;
		}

		private static float easeOutSine(float start, float end, float val)
		{
			end -= start;
			return end * Mathf.Sin(val / 1f * 1.5707964f) + start;
		}

		private static float easeInOutSine(float start, float end, float val)
		{
			end -= start;
			return -end / 2f * (Mathf.Cos(3.1415927f * val / 1f) - 1f) + start;
		}

		private static float easeInExpo(float start, float end, float val)
		{
			end -= start;
			return end * Mathf.Pow(2f, 10f * (val / 1f - 1f)) + start;
		}

		private static float easeOutExpo(float start, float end, float val)
		{
			end -= start;
			return end * (-Mathf.Pow(2f, -10f * val / 1f) + 1f) + start;
		}

		private static float easeInOutExpo(float start, float end, float val)
		{
			val /= 0.5f;
			end -= start;
			if (val < 1f)
			{
				return end / 2f * Mathf.Pow(2f, 10f * (val - 1f)) + start;
			}
			val -= 1f;
			return end / 2f * (-Mathf.Pow(2f, -10f * val) + 2f) + start;
		}

		private static float easeInCirc(float start, float end, float val)
		{
			end -= start;
			return -end * (Mathf.Sqrt(1f - val * val) - 1f) + start;
		}

		private static float easeOutCirc(float start, float end, float val)
		{
			val -= 1f;
			end -= start;
			return end * Mathf.Sqrt(1f - val * val) + start;
		}

		private static float easeInOutCirc(float start, float end, float val)
		{
			val /= 0.5f;
			end -= start;
			if (val < 1f)
			{
				return -end / 2f * (Mathf.Sqrt(1f - val * val) - 1f) + start;
			}
			val -= 2f;
			return end / 2f * (Mathf.Sqrt(1f - val * val) + 1f) + start;
		}

		private static float easeInBounce(float start, float end, float val)
		{
			end -= start;
			float num = 1f;
			return end - EnhancedScroller.easeOutBounce(0f, end, num - val) + start;
		}

		private static float easeOutBounce(float start, float end, float val)
		{
			val /= 1f;
			end -= start;
			if (val < 0.36363637f)
			{
				return end * (7.5625f * val * val) + start;
			}
			if (val < 0.72727275f)
			{
				val -= 0.54545456f;
				return end * (7.5625f * val * val + 0.75f) + start;
			}
			if ((double)val < 0.9090909090909091)
			{
				val -= 0.8181818f;
				return end * (7.5625f * val * val + 0.9375f) + start;
			}
			val -= 0.95454544f;
			return end * (7.5625f * val * val + 0.984375f) + start;
		}

		private static float easeInOutBounce(float start, float end, float val)
		{
			end -= start;
			float num = 1f;
			if (val < num / 2f)
			{
				return EnhancedScroller.easeInBounce(0f, end, val * 2f) * 0.5f + start;
			}
			return EnhancedScroller.easeOutBounce(0f, end, val * 2f - num) * 0.5f + end * 0.5f + start;
		}

		private static float easeInBack(float start, float end, float val)
		{
			end -= start;
			val /= 1f;
			float num = 1.70158f;
			return end * val * val * ((num + 1f) * val - num) + start;
		}

		private static float easeOutBack(float start, float end, float val)
		{
			float num = 1.70158f;
			end -= start;
			val = val / 1f - 1f;
			return end * (val * val * ((num + 1f) * val + num) + 1f) + start;
		}

		private static float easeInOutBack(float start, float end, float val)
		{
			float num = 1.70158f;
			end -= start;
			val /= 0.5f;
			if (val < 1f)
			{
				num *= 1.525f;
				return end / 2f * (val * val * ((num + 1f) * val - num)) + start;
			}
			val -= 2f;
			num *= 1.525f;
			return end / 2f * (val * val * ((num + 1f) * val + num) + 2f) + start;
		}

		private static float easeInElastic(float start, float end, float val)
		{
			end -= start;
			float num = 1f;
			float num2 = num * 0.3f;
			float num3 = 0f;
			if (val == 0f)
			{
				return start;
			}
			val /= num;
			if (val == 1f)
			{
				return start + end;
			}
			float num4;
			if (num3 == 0f || num3 < Mathf.Abs(end))
			{
				num3 = end;
				num4 = num2 / 4f;
			}
			else
			{
				num4 = num2 / 6.2831855f * Mathf.Asin(end / num3);
			}
			val -= 1f;
			return -(num3 * Mathf.Pow(2f, 10f * val) * Mathf.Sin((val * num - num4) * 6.2831855f / num2)) + start;
		}

		private static float easeOutElastic(float start, float end, float val)
		{
			end -= start;
			float num = 1f;
			float num2 = num * 0.3f;
			float num3 = 0f;
			if (val == 0f)
			{
				return start;
			}
			val /= num;
			if (val == 1f)
			{
				return start + end;
			}
			float num4;
			if (num3 == 0f || num3 < Mathf.Abs(end))
			{
				num3 = end;
				num4 = num2 / 4f;
			}
			else
			{
				num4 = num2 / 6.2831855f * Mathf.Asin(end / num3);
			}
			return num3 * Mathf.Pow(2f, -10f * val) * Mathf.Sin((val * num - num4) * 6.2831855f / num2) + end + start;
		}

		private static float easeInOutElastic(float start, float end, float val)
		{
			end -= start;
			float num = 1f;
			float num2 = num * 0.3f;
			float num3 = 0f;
			if (val == 0f)
			{
				return start;
			}
			val /= num / 2f;
			if (val == 2f)
			{
				return start + end;
			}
			float num4;
			if (num3 == 0f || num3 < Mathf.Abs(end))
			{
				num3 = end;
				num4 = num2 / 4f;
			}
			else
			{
				num4 = num2 / 6.2831855f * Mathf.Asin(end / num3);
			}
			if (val < 1f)
			{
				val -= 1f;
				return -0.5f * (num3 * Mathf.Pow(2f, 10f * val) * Mathf.Sin((val * num - num4) * 6.2831855f / num2)) + start;
			}
			val -= 1f;
			return num3 * Mathf.Pow(2f, -10f * val) * Mathf.Sin((val * num - num4) * 6.2831855f / num2) * 0.5f + end + start;
		}

		public EnhancedScroller.ScrollDirectionEnum scrollDirection;

		public float spacing;

		public RectOffset padding;

		[SerializeField]
		private bool loop;

		[SerializeField]
		private EnhancedScroller.ScrollbarVisibilityEnum scrollbarVisibility;

		public bool snapping;

		public float snapVelocityThreshold;

		public float snapWatchOffset;

		public float snapJumpToOffset;

		public float snapCellCenterOffset;

		public bool snapUseCellSpacing;

		public EnhancedScroller.TweenType snapTweenType;

		public float snapTweenTime;

		public CellViewVisibilityChangedDelegate cellViewVisibilityChanged;

		public CellViewWillRecycleDelegate cellViewWillRecycle;

		public ScrollerScrolledDelegate scrollerScrolled;

		public ScrollerSnappedDelegate scrollerSnapped;

		public ScrollerScrollingChangedDelegate scrollerScrollingChanged;

		public ScrollerTweeningChangedDelegate scrollerTweeningChanged;

		private ScrollRect _scrollRect;

		private RectTransform _scrollRectTransform;

		private Scrollbar _scrollbar;

		private RectTransform _container;

		private HorizontalOrVerticalLayoutGroup _layoutGroup;

		private IEnhancedScrollerDelegate _delegate;

		private bool _reloadData;

		private bool _refreshActive;

		private SmallList<EnhancedScrollerCellView> _recycledCellViews = new SmallList<EnhancedScrollerCellView>();

		private LayoutElement _firstPadder;

		private LayoutElement _lastPadder;

		private RectTransform _recycledCellViewContainer;

		private SmallList<float> _cellViewSizeArray = new SmallList<float>();

		private SmallList<float> _cellViewOffsetArray = new SmallList<float>();

		private float _scrollPosition;

		private SmallList<EnhancedScrollerCellView> _activeCellViews = new SmallList<EnhancedScrollerCellView>();

		private int _activeCellViewsStartIndex;

		private int _activeCellViewsEndIndex;

		private int _loopFirstCellIndex;

		private int _loopLastCellIndex;

		private float _loopFirstScrollPosition;

		private float _loopLastScrollPosition;

		private float _loopFirstJumpTrigger;

		private float _loopLastJumpTrigger;

		private float _lastScrollRectSize;

		private bool _lastLoop;

		private int _snapCellViewIndex;

		private int _snapDataIndex;

		private bool _snapJumping;

		private bool _snapInertia;

		private EnhancedScroller.ScrollbarVisibilityEnum _lastScrollbarVisibility;

		private float _tweenTimeLeft;

		public enum ScrollDirectionEnum
		{
			Vertical,
			Horizontal
		}

		public enum CellViewPositionEnum
		{
			Before,
			After
		}

		public enum ScrollbarVisibilityEnum
		{
			OnlyIfNeeded,
			Always,
			Never
		}

		public enum LoopJumpDirectionEnum
		{
			Closest,
			Up,
			Down
		}

		private enum ListPositionEnum
		{
			First,
			Last
		}

		public enum TweenType
		{
			immediate,
			linear,
			spring,
			easeInQuad,
			easeOutQuad,
			easeInOutQuad,
			easeInCubic,
			easeOutCubic,
			easeInOutCubic,
			easeInQuart,
			easeOutQuart,
			easeInOutQuart,
			easeInQuint,
			easeOutQuint,
			easeInOutQuint,
			easeInSine,
			easeOutSine,
			easeInOutSine,
			easeInExpo,
			easeOutExpo,
			easeInOutExpo,
			easeInCirc,
			easeOutCirc,
			easeInOutCirc,
			easeInBounce,
			easeOutBounce,
			easeInOutBounce,
			easeInBack,
			easeOutBack,
			easeInOutBack,
			easeInElastic,
			easeOutElastic,
			easeInOutElastic
		}
	}
}
