using System;

namespace Assets.Scripts.UI._2D.Shop.PremiumShop.SRIA.Models
{
	public class PremiumShopItemModel : HeaderPremiumShopGroupModel
	{
		public override int GetHeight()
		{
			return 150;
		}

		public string Description;

		public int? ImageBID;
	}
}
