using System;
using EnhancedUI.EnhancedScroller;

namespace EnhancedScrollerDemos.MultipleCellTypesDemo
{
	public class CellView : EnhancedScrollerCellView
	{
		public virtual void SetData(Data data)
		{
			this._data = data;
		}

		protected Data _data;
	}
}
