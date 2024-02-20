using System;
using Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.Models;
using frame8.Logic.Misc.Other.Extensions;
using TMPro;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.ViewHolders
{
	public class PremiumShopItemIcoSmallVH : HeaderPremiumShopGroupVH
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("Ico", out this.Ico);
		}

		internal override void UpdateViews(PremiumShopBaseModel model)
		{
			base.UpdateViews(model);
			PremiumShopItemModelIcoSmall premiumShopItemModelIcoSmall = model as PremiumShopItemModelIcoSmall;
			this.Ico.text = premiumShopItemModelIcoSmall.Ico;
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(PremiumShopItemModelIcoSmall);
		}

		protected TextMeshProUGUI Ico;
	}
}
