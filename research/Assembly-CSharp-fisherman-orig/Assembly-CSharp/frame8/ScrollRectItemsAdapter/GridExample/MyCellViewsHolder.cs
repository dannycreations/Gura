using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample;
using frame8.ScrollRectItemsAdapter.Util.GridView;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.GridExample
{
	public class MyCellViewsHolder : CellViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.views.GetComponentAtPath("IconRawImage", out this.iconRemoteImageBehaviour);
			this.views.GetComponentAtPath("OverlayImage", out this.overlayImage);
			this.views.GetComponentAtPath("LoadingProgressImage", out this.loadingProgress);
			this.views.GetComponentAtPath("TitleText", out this.title);
		}

		public RemoteImageBehaviour iconRemoteImageBehaviour;

		public Image loadingProgress;

		public Image overlayImage;

		public Text title;
	}
}
