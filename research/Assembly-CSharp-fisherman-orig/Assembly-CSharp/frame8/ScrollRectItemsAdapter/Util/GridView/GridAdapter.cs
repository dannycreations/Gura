using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;

namespace frame8.ScrollRectItemsAdapter.Util.GridView
{
	public abstract class GridAdapter<TParams, TCellVH> : SRIA<TParams, CellGroupViewsHolder<TCellVH>> where TParams : GridParams where TCellVH : CellViewsHolder, new()
	{
		public int CellsCount
		{
			get
			{
				return this._CellsCount;
			}
		}

		public sealed override void InsertItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			throw new InvalidOperationException("Cannot use InsertItems() with a GridAdapter yet. Use ResetItems() instead.");
		}

		public sealed override void RemoveItems(int index, int itemsCount, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			throw new InvalidOperationException("Cannot use RemoveItems() with a GridAdapter yet. Use ResetItems() instead.");
		}

		public override void ChangeItemsCount(ItemCountChangeMode changeMode, int cellsCount, int indexIfAppendingOrRemoving = -1, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			if (changeMode != ItemCountChangeMode.RESET)
			{
				throw new InvalidOperationException("Only ItemCountChangeMode.RESET is supported with a GridAdapter for now");
			}
			this._CellsCount = cellsCount;
			int numberOfRequiredGroups = this._Params.GetNumberOfRequiredGroups(this._CellsCount);
			base.ChangeItemsCount(changeMode, numberOfRequiredGroups, indexIfAppendingOrRemoving, contentPanelEndEdgeStationary, keepVelocity);
		}

		public sealed override AbstractViewsHolder GetViewsHolderOfClosestItemToViewportPoint(float viewportPoint01, float itemPoint01, out float distance)
		{
			CellGroupViewsHolder<TCellVH> cellGroupViewsHolder = base.GetViewsHolderOfClosestItemToViewportPoint(viewportPoint01, itemPoint01, out distance) as CellGroupViewsHolder<TCellVH>;
			if (cellGroupViewsHolder == null || cellGroupViewsHolder.NumActiveCells == 0)
			{
				return null;
			}
			return cellGroupViewsHolder.ContainingCellViewsHolders[cellGroupViewsHolder.NumActiveCells / 2];
		}

		public sealed override void ScrollTo(int cellIndex, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f)
		{
			this.ScrollToGroup(this._Params.GetGroupIndex(cellIndex), normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
		}

		public sealed override bool SmoothScrollTo(int cellIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null, bool overrideAnyCurrentScrollingAnimation = false)
		{
			return this.SmoothScrollToGroup(this._Params.GetGroupIndex(cellIndex), duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, onProgress, overrideAnyCurrentScrollingAnimation);
		}

		public sealed override void Refresh()
		{
			this.Refresh(false, false);
		}

		public virtual void Refresh(bool contentPanelEndEdgeStationary, bool keepVelocity = false)
		{
			this.ChangeItemsCount(ItemCountChangeMode.RESET, this._CellsCount, -1, contentPanelEndEdgeStationary, keepVelocity);
		}

		public sealed override int GetItemsCount()
		{
			return this._CellsCount;
		}

		public virtual int GetCellGroupsCount()
		{
			return base.GetItemsCount();
		}

		public virtual int GetNumVisibleCells()
		{
			if (this._VisibleItemsCount == 0)
			{
				return 0;
			}
			return (this._VisibleItemsCount - 1) * this._Params.numCellsPerGroup + this._VisibleItems[this._VisibleItemsCount - 1].NumActiveCells;
		}

		public virtual TCellVH GetCellViewsHolder(int cellViewsHolderIndex)
		{
			if (this._VisibleItemsCount == 0)
			{
				return (TCellVH)((object)null);
			}
			if (cellViewsHolderIndex > this.GetNumVisibleCells() - 1)
			{
				return (TCellVH)((object)null);
			}
			return this._VisibleItems[this._Params.GetGroupIndex(cellViewsHolderIndex)].ContainingCellViewsHolders[cellViewsHolderIndex % this._Params.numCellsPerGroup];
		}

		public virtual TCellVH GetCellViewsHolderIfVisible(int withCellItemIndex)
		{
			CellGroupViewsHolder<TCellVH> itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(this._Params.GetGroupIndex(withCellItemIndex));
			if (itemViewsHolderIfVisible == null)
			{
				return (TCellVH)((object)null);
			}
			int num = itemViewsHolderIfVisible.ItemIndex * this._Params.numCellsPerGroup;
			if (withCellItemIndex < num + itemViewsHolderIfVisible.NumActiveCells)
			{
				return itemViewsHolderIfVisible.ContainingCellViewsHolders[withCellItemIndex - num];
			}
			return (TCellVH)((object)null);
		}

		public virtual void ScrollToGroup(int groupIndex, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f)
		{
			base.ScrollTo(groupIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
		}

		public virtual bool SmoothScrollToGroup(int groupIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null, bool overrideAnyCurrentScrollingAnimation = false)
		{
			return base.SmoothScrollTo(groupIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, onProgress, overrideAnyCurrentScrollingAnimation);
		}

		protected override CellGroupViewsHolder<TCellVH> CreateViewsHolder(int itemIndex)
		{
			CellGroupViewsHolder<TCellVH> cellGroupViewsHolder = new CellGroupViewsHolder<TCellVH>();
			cellGroupViewsHolder.Init(this._Params.GetGroupPrefab(itemIndex).gameObject, itemIndex, this._Params.cellPrefab, this._Params.numCellsPerGroup);
			return cellGroupViewsHolder;
		}

		protected sealed override void UpdateViewsHolder(CellGroupViewsHolder<TCellVH> newOrRecycled)
		{
			int num2;
			if (newOrRecycled.ItemIndex + 1 == this.GetCellGroupsCount())
			{
				int num = 0;
				if (newOrRecycled.ItemIndex > 0)
				{
					num = newOrRecycled.ItemIndex * this._Params.numCellsPerGroup;
				}
				num2 = this._CellsCount - num;
			}
			else
			{
				num2 = this._Params.numCellsPerGroup;
			}
			newOrRecycled.NumActiveCells = num2;
			for (int i = 0; i < num2; i++)
			{
				this.UpdateCellViewsHolder(newOrRecycled.ContainingCellViewsHolders[i]);
			}
		}

		protected abstract void UpdateCellViewsHolder(TCellVH viewsHolder);

		protected sealed override void OnBeforeRecycleOrDisableViewsHolder(CellGroupViewsHolder<TCellVH> inRecycleBinOrVisible, int newItemIndex)
		{
			base.OnBeforeRecycleOrDisableViewsHolder(inRecycleBinOrVisible, newItemIndex);
			if (newItemIndex == -1)
			{
				for (int i = 0; i < inRecycleBinOrVisible.NumActiveCells; i++)
				{
					this.OnBeforeRecycleOrDisableCellViewsHolder(inRecycleBinOrVisible.ContainingCellViewsHolders[i], -1);
				}
			}
			else
			{
				for (int j = 0; j < inRecycleBinOrVisible.NumActiveCells; j++)
				{
					this.OnBeforeRecycleOrDisableCellViewsHolder(inRecycleBinOrVisible.ContainingCellViewsHolders[j], newItemIndex * this._Params.numCellsPerGroup + j);
				}
			}
		}

		protected virtual void OnBeforeRecycleOrDisableCellViewsHolder(TCellVH viewsHolder, int newItemIndex)
		{
		}

		protected int _CellsCount;
	}
}
