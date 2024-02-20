using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using InventorySRIA.Models;

namespace InventorySRIA.ViewsHolders
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
		}
	}
}
