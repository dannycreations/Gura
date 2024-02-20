using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.NestedScrollers
{
	public class DetailCellView : EnhancedScrollerCellView
	{
		public void SetData(DetailData data)
		{
			this.someTextText.text = data.someText;
		}

		public Text someTextText;
	}
}
