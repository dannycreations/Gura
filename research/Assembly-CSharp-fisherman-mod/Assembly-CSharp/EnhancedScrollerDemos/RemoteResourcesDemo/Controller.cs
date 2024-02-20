using System;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EnhancedScrollerDemos.RemoteResourcesDemo
{
	public class Controller : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private void Start()
		{
			this.scroller.Delegate = this;
			this.scroller.cellViewVisibilityChanged = new CellViewVisibilityChangedDelegate(this.CellViewVisibilityChanged);
			this._data = new SmallList<Data>();
			for (int i = 0; i <= 12; i++)
			{
				this._data.Add(new Data
				{
					imageUrl = string.Format("http://echo17.com/support/enhancedscroller/{0}.jpg", i),
					imageDimensions = new Vector2(200f, 200f)
				});
			}
			this.scroller.ReloadData(0f);
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return 260f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellView cellView = scroller.GetCellView(this.cellViewPrefab) as CellView;
			cellView.name = "Cell " + dataIndex.ToString();
			return cellView;
		}

		private void CellViewVisibilityChanged(EnhancedScrollerCellView cellView)
		{
			CellView cellView2 = cellView as CellView;
			if (cellView.active)
			{
				cellView2.SetData(this._data[cellView.dataIndex]);
			}
			else
			{
				cellView2.ClearImage();
			}
		}

		private SmallList<Data> _data;

		public EnhancedScroller scroller;

		public EnhancedScrollerCellView cellViewPrefab;
	}
}
