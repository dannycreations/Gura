using System;
using Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.Models;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.ViewHolders
{
	public abstract class PremiumShopBaseVH : BaseItemViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
		}

		public abstract bool CanPresentModelType(Type modelType);

		internal virtual void UpdateViews(PremiumShopBaseModel model)
		{
		}
	}
}
