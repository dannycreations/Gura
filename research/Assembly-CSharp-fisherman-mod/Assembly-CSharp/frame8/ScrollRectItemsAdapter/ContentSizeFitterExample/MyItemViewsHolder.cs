using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.ContentSizeFitterExample
{
	public class MyItemViewsHolder : BaseItemViewsHolder
	{
		public ContentSizeFitter contentSizeFitter { get; private set; }

		public override void CollectViews()
		{
			base.CollectViews();
			this.contentSizeFitter = this.root.GetComponent<ContentSizeFitter>();
			this.contentSizeFitter.enabled = false;
			this.root.GetComponentAtPath("TitlePanel/TitleText", out this.titleText);
			this.root.GetComponentAtPath("Icon1Image", out this.icon1Image);
		}

		public override void MarkForRebuild()
		{
			base.MarkForRebuild();
			if (this.contentSizeFitter)
			{
				this.contentSizeFitter.enabled = true;
			}
		}

		public void UpdateFromModel(ExampleItemModel model, Texture2D[] availableIcons)
		{
			string text = string.Concat(new object[] { "[#", this.ItemIndex, "] ", model.Title });
			if (this.titleText.text != text)
			{
				this.titleText.text = text;
			}
			Texture2D texture2D = availableIcons[model.IconIndex];
			if (this.icon1Image.texture != texture2D)
			{
				this.icon1Image.texture = texture2D;
			}
		}

		public Text titleText;

		public RawImage icon1Image;
	}
}
