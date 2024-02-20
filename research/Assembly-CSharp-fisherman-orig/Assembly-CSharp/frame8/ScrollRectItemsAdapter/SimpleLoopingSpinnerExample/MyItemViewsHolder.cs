using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.SimpleLoopingSpinnerExample
{
	public class MyItemViewsHolder : BaseItemViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.background = this.root.GetComponent<Image>();
			this.titleText = this.root.GetComponentInChildren<Text>();
		}

		public Image background;

		public Text titleText;
	}
}
