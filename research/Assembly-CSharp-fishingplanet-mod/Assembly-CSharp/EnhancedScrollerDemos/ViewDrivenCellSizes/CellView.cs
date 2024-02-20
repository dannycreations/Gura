using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.ViewDrivenCellSizes
{
	public class CellView : EnhancedScrollerCellView
	{
		public void SetData(Data data)
		{
			this.someTextText.text = data.someText;
			Canvas.ForceUpdateCanvases();
			data.cellSize = this.textRectTransform.rect.height + (float)this.textBuffer.top + (float)this.textBuffer.bottom;
		}

		public Text someTextText;

		public RectTransform textRectTransform;

		public RectOffset textBuffer;
	}
}
