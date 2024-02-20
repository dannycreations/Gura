using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.CellEvents
{
	public class CellView : EnhancedScrollerCellView
	{
		public void SetData(Data data)
		{
			this._data = data;
			this.someTextText.text = ((this._data.hour != 0) ? string.Format("{0} 'o clock", this._data.hour.ToString()) : "Midnight");
		}

		public void CellButtonText_OnClick(string value)
		{
			if (this.cellButtonTextClicked != null)
			{
				this.cellButtonTextClicked(value);
			}
		}

		public void CellButtonFixedInteger_OnClick(int value)
		{
			if (this.cellButtonFixedIntegerClicked != null)
			{
				this.cellButtonFixedIntegerClicked(value);
			}
		}

		public void CellButtonDataInteger_OnClick()
		{
			if (this.cellButtonDataIntegerClicked != null)
			{
				this.cellButtonDataIntegerClicked(this._data.hour);
			}
		}

		private Data _data;

		public Text someTextText;

		public CellButtonTextClickedDelegate cellButtonTextClicked;

		public CellButtonIntegerClickedDelegate cellButtonFixedIntegerClicked;

		public CellButtonIntegerClickedDelegate cellButtonDataIntegerClicked;
	}
}
