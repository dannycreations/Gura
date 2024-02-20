using System;
using Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.Models;
using frame8.Logic.Misc.Other.Extensions;
using TMPro;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.ViewHolders
{
	public class HeaderPremiumShopGroupVH : PremiumShopBaseVH
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("Name", out this.Name);
		}

		internal override void UpdateViews(PremiumShopBaseModel model)
		{
			base.UpdateViews(model);
			HeaderPremiumShopGroupModel headerPremiumShopGroupModel = model as HeaderPremiumShopGroupModel;
			this.Name.text = headerPremiumShopGroupModel.Name;
			this.Name.margin = model.Margin;
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(HeaderPremiumShopGroupModel);
		}

		protected TextMeshProUGUI Name;
	}
}
