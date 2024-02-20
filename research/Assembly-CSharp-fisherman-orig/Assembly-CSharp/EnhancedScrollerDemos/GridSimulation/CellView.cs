using System;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;

namespace EnhancedScrollerDemos.GridSimulation
{
	public class CellView : EnhancedScrollerCellView
	{
		public void SetData(ref SmallList<Data> data, int startingIndex)
		{
			for (int i = 0; i < this.rowCellViews.Length; i++)
			{
				this.rowCellViews[i].SetData((startingIndex + i >= data.Count) ? null : data[startingIndex + i]);
			}
		}

		public RowCellView[] rowCellViews;
	}
}
