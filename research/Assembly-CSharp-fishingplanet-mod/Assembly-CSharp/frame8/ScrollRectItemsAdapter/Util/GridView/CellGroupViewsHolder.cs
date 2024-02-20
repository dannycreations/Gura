using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.GridView
{
	public class CellGroupViewsHolder<TCellVH> : BaseItemViewsHolder where TCellVH : CellViewsHolder, new()
	{
		public override int ItemIndex
		{
			get
			{
				return base.ItemIndex;
			}
			set
			{
				base.ItemIndex = value;
				if (this._Capacity > 0)
				{
					this.OnGroupIndexChanged();
				}
			}
		}

		public int NumActiveCells
		{
			get
			{
				return this._NumActiveCells;
			}
			set
			{
				if (this._NumActiveCells != value)
				{
					this._NumActiveCells = value;
					for (int i = 0; i < this._Capacity; i++)
					{
						this.ContainingCellViewsHolders[i].views.gameObject.SetActive(i < this._NumActiveCells);
					}
				}
			}
		}

		public TCellVH[] ContainingCellViewsHolders { get; private set; }

		public override void CollectViews()
		{
			base.CollectViews();
			this._LayoutGroup = this.root.GetComponent<HorizontalOrVerticalLayoutGroup>();
			this.ContainingCellViewsHolders = new TCellVH[this._Capacity];
			for (int i = 0; i < this._Capacity; i++)
			{
				this.ContainingCellViewsHolders[i] = new TCellVH();
				this.ContainingCellViewsHolders[i].InitWithExistingRootPrefab(this.root.GetChild(i) as RectTransform);
				this.ContainingCellViewsHolders[i].views.gameObject.SetActive(false);
			}
			if (this.ItemIndex != -1 && this._Capacity > 0)
			{
				this.UpdateIndicesOfContainingCells();
			}
		}

		internal void Init(GameObject groupPrefab, int itemIndex, RectTransform cellPrefab, int numCellsPerGroup)
		{
			base.Init(groupPrefab, itemIndex, true, false);
			this._Capacity = numCellsPerGroup;
			for (int i = 0; i < this._Capacity; i++)
			{
				RectTransform rectTransform = Object.Instantiate<GameObject>(cellPrefab.gameObject).transform as RectTransform;
				rectTransform.gameObject.SetActive(true);
				rectTransform.SetParent(this.root);
			}
			this.CollectViews();
		}

		protected virtual void OnGroupIndexChanged()
		{
			if (this._Capacity > 0)
			{
				this.UpdateIndicesOfContainingCells();
			}
		}

		protected virtual void UpdateIndicesOfContainingCells()
		{
			for (int i = 0; i < this._Capacity; i++)
			{
				this.ContainingCellViewsHolders[i].ItemIndex = this.ItemIndex * this._Capacity + i;
			}
		}

		protected HorizontalOrVerticalLayoutGroup _LayoutGroup;

		protected int _Capacity = -1;

		protected int _NumActiveCells;
	}
}
