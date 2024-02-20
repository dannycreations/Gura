using System;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.GridView;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.SelectionExample
{
	public class MyCellViewsHolder : CellViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.toggle = this.views.Find("Toggle").GetComponent<Toggle>();
			this.title = this.views.Find("TitleText").GetComponent<Text>();
			this.longClickableComponent = this.views.Find("LongClickableArea").GetComponent<LongClickableItem>();
			this.background = this.views.GetComponent<Image>();
			this.expandCollapseComponent = this.views.GetComponent<ExpandCollapseOnClick>();
			this.expandCollapseComponent.nonExpandedSize = 0.001f;
			this.expandCollapseComponent.expandFactor = 1f / this.expandCollapseComponent.nonExpandedSize;
			this.expandCollapseComponent.expanded = true;
		}

		public void UpdateViews(BasicModel model)
		{
			this.title.text = string.Concat(new object[] { "#", this.ItemIndex, " [id:", model.id, "]" });
			this.background.color = model.color;
		}

		public Text title;

		public Toggle toggle;

		public LongClickableItem longClickableComponent;

		public Image background;

		public ExpandCollapseOnClick expandCollapseComponent;
	}
}
