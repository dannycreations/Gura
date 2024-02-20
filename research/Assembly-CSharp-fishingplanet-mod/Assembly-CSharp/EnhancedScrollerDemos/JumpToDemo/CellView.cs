using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.JumpToDemo
{
	public class CellView : EnhancedScrollerCellView
	{
		public void SetData(Data data)
		{
			this.cellText.text = data.cellText;
		}

		public Text cellText;
	}
}
