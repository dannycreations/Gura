using System;

namespace ObjectModel
{
	public class StoreProduct
	{
		public int ProductId { get; set; }

		public int TypeId { get; set; }

		public int? Term { get; set; }

		public string Name { get; set; }

		public string Desc { get; set; }

		public string PromoText { get; set; }

		public string ProductCurrency { get; set; }

		public double Price { get; set; }

		public int? Silver { get; set; }

		public int? Gold { get; set; }

		public double? ExpMultiplier { get; set; }

		public double? MoneyMultiplier { get; set; }

		public int? ImageBID { get; set; }

		public int? PremShopImageBID { get; set; }

		public int? ItemListImageBID { get; set; }

		public int? PromotionImageBID { get; set; }

		public DateTime? DiscountStart { get; set; }

		public DateTime? DiscountEnd { get; set; }

		public double? DiscountPrice { get; set; }

		public int? DiscountImageBID { get; set; }

		public string ForeignProductId { get; set; }

		public int[] PondsUnlocked { get; set; }

		public int[] PaidPondsUnlocked { get; set; }

		public ProductInventoryItemBrief[] Items { get; set; }

		public LicenseRef[] Licenses { get; set; }

		public int? AccessibleLevel { get; set; }

		public int? InventoryExt { get; set; }

		public int? RodSetupExt { get; set; }

		public int? BuoyExt { get; set; }

		public int? ChumRecipesExt { get; set; }

		public string ExternalShopLink { get; set; }

		public bool IsPopular { get; set; }

		public bool IsNew { get; set; }

		public string PriceString { get; set; }

		public string ProductSkuId { get; set; }

		public string ProductXBId { get; set; }

		public bool HasActiveDiscount(DateTime serverUtcNow)
		{
			return this.DiscountStart != null && this.DiscountEnd != null && serverUtcNow >= this.DiscountStart && serverUtcNow <= this.DiscountEnd;
		}

		public double GetPrice(DateTime serverUtcNow)
		{
			if (this.HasActiveDiscount(serverUtcNow))
			{
				double? discountPrice = this.DiscountPrice;
				return (discountPrice == null) ? this.Price : discountPrice.Value;
			}
			return this.Price;
		}

		public override string ToString()
		{
			return string.Format("#{0} {1} ", this.ProductId, this.Name);
		}
	}
}
