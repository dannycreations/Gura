using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.ViewsHolders
{
	public abstract class BaseVH : BaseItemViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
		}

		public abstract bool CanPresentModelType(Type modelType);

		internal virtual void UpdateViews(BaseModel model)
		{
			this.titleText.text = string.Concat(new object[] { "#", this.ItemIndex, " [id:", model.id, "]" });
		}

		public Text titleText;
	}
}
