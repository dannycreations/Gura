using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EnhancedScrollerDemos.NestedScrollers
{
	public class MasterCellView : EnhancedScrollerCellView, IEnhancedScrollerDelegate
	{
		public void SetData(MasterData data)
		{
			this.detailScroller.Delegate = this;
			this.detailScroller.scrollerScrolled = new ScrollerScrolledDelegate(this.ScrollerScrolled);
			this._data = data;
			this.reloadDataNextFrame = true;
		}

		private void Update()
		{
			if (this.reloadDataNextFrame)
			{
				this.reloadDataNextFrame = false;
				this.detailScroller.ReloadData(this._data.normalizedScrollPosition);
			}
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.childData.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return 100f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			DetailCellView detailCellView = scroller.GetCellView(this.detailCellViewPrefab) as DetailCellView;
			detailCellView.name = "Detail Cell " + dataIndex.ToString();
			detailCellView.SetData(this._data.childData[dataIndex]);
			return detailCellView;
		}

		private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
		{
			this._data.normalizedScrollPosition = scroller.NormalizedScrollPosition;
		}

		private bool reloadDataNextFrame;

		public EnhancedScroller detailScroller;

		private MasterData _data;

		public EnhancedScrollerCellView detailCellViewPrefab;
	}
}
