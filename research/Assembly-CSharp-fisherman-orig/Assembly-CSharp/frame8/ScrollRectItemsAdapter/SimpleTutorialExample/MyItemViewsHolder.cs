using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.SimpleTutorialExample
{
	public class MyItemViewsHolder : BaseItemViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("TitlePanel/TitleText", out this.titleText);
			this.root.GetComponentAtPath("Background", out this.backgroundImage);
			this.root.GetComponentAtPath("Icon1Image", out this.icon1Image);
			this.root.GetComponentAtPath("Icon2Image", out this.icon2Image);
			this.expandOnCollapseComponent = this.root.GetComponent<ExpandCollapseOnClick>();
		}

		public Text titleText;

		public Image backgroundImage;

		public RawImage icon1Image;

		public RawImage icon2Image;

		internal ExpandCollapseOnClick expandOnCollapseComponent;
	}
}
