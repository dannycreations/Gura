using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.PullDownRefresh
{
	public class CellView : EnhancedScrollerCellView
	{
		public void SetData(Data data)
		{
			this.someTextText.text = data.someText;
		}

		public Text someTextText;
	}
}
