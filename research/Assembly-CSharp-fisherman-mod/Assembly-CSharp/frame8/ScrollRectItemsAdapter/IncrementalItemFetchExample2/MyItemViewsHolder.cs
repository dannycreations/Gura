using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.IncrementalItemFetchExample2
{
	public class MyItemViewsHolder : BaseItemViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.titleText = this.root.Find("TitlePanel/TitleText").GetComponent<Text>();
		}

		public Text titleText;
	}
}
