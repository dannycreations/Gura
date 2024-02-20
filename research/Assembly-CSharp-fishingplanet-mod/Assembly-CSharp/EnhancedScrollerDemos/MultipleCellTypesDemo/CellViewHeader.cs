using System;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.MultipleCellTypesDemo
{
	public class CellViewHeader : CellView
	{
		public override void SetData(Data data)
		{
			base.SetData(data);
			this._headerData = data as HeaderData;
			this.categoryText.text = this._headerData.category;
		}

		private HeaderData _headerData;

		public Text categoryText;
	}
}
