using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.SnappingDemo
{
	public class SlotCellView : EnhancedScrollerCellView
	{
		public void SetData(SlotData data)
		{
			if (data.sprite == null)
			{
				this.slotImage.color = new Color(0f, 0f, 0f, 0f);
			}
			else
			{
				this.slotImage.sprite = data.sprite;
				this.slotImage.color = Color.white;
			}
		}

		public Image slotImage;
	}
}
