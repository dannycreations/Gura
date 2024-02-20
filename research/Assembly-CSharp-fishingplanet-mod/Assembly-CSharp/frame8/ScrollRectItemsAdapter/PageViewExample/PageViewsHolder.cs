using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.PageViewExample
{
	public class PageViewsHolder : BaseItemViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("TitlePanel/TitleText", out this.titleText);
			this.root.GetComponentAtPath("BodyPanel/BodyText", out this.bodyText);
			this.root.GetComponentAtPath("BackgroundMask/BackgroundImage", out this.image);
		}

		public void UpdateViews(PageModel model)
		{
			this.titleText.text = model.title;
			this.bodyText.text = model.body;
			this.image.sprite = model.image;
		}

		public Text titleText;

		public Text bodyText;

		public Image image;
	}
}
