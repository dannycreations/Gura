using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using TMPro;

namespace ScrollSelectorSRIA
{
	public class MyItemViewsHolder : BaseItemViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("Name", out this.Text);
			this.root.GetComponentAtPath("Ticker", out this.SelectorIcon);
		}

		public TextMeshProUGUI Text;

		public TextMeshProUGUI SelectorIcon;
	}
}
