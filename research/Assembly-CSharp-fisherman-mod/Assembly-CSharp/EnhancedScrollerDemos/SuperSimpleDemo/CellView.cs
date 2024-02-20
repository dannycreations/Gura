using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.SuperSimpleDemo
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
