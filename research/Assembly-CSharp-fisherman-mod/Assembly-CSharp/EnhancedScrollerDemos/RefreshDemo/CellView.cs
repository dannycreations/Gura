using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.RefreshDemo
{
	public class CellView : EnhancedScrollerCellView
	{
		public RectTransform RectTransform
		{
			get
			{
				return base.gameObject.GetComponent<RectTransform>();
			}
		}

		public void SetData(Data data)
		{
			this._data = data;
			this.RefreshCellView();
		}

		public override void RefreshCellView()
		{
			this.someTextText.text = this._data.someText;
		}

		private Data _data;

		public Text someTextText;
	}
}
