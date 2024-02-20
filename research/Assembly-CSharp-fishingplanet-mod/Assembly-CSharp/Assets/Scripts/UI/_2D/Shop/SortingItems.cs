using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;

namespace Assets.Scripts.UI._2D.Shop
{
	public class SortingItems
	{
		public static List<ShopLicenseContainer> Sort(SortType sortType, List<ShopLicenseContainer> data)
		{
			switch (sortType)
			{
			case SortType.PriceLow:
				return data.OrderBy(delegate(ShopLicenseContainer p)
				{
					float? discountNonResidentCost = p.Costs[0].DiscountNonResidentCost;
					return (discountNonResidentCost == null) ? p.Costs[0].NotResidentCost : discountNonResidentCost.Value;
				}).ToList<ShopLicenseContainer>();
			case SortType.PriceHigh:
				return data.OrderByDescending(delegate(ShopLicenseContainer p)
				{
					float? discountNonResidentCost2 = p.Costs[0].DiscountNonResidentCost;
					return (discountNonResidentCost2 == null) ? p.Costs[0].NotResidentCost : discountNonResidentCost2.Value;
				}).ToList<ShopLicenseContainer>();
			case SortType.LevelLow:
				return data.OrderBy((ShopLicenseContainer p) => p.MinLevel).ToList<ShopLicenseContainer>();
			case SortType.LevelHigh:
				return data.OrderByDescending((ShopLicenseContainer p) => p.MinLevel).ToList<ShopLicenseContainer>();
			case SortType.Name:
				return data.OrderBy((ShopLicenseContainer p) => p.Name).ToList<ShopLicenseContainer>();
			default:
				return data;
			}
		}

		public static List<StoreProduct> Sort(SortType sortType, List<StoreProduct> data)
		{
			if (sortType == SortType.PriceLow)
			{
				return data.OrderBy((StoreProduct x) => x.Price).ToList<StoreProduct>();
			}
			if (sortType != SortType.PriceHigh)
			{
				return data.OrderBy((StoreProduct x) => x.Name).ToList<StoreProduct>();
			}
			return data.OrderByDescending((StoreProduct x) => x.Price).ToList<StoreProduct>();
		}

		public static List<InventoryItem> Sort(SortType sortType, List<InventoryItem> data)
		{
			List<InventoryItem> list = new List<InventoryItem>();
			switch (sortType)
			{
			case SortType.PriceLow:
			{
				IEnumerable<InventoryItem> enumerable = data.Where((InventoryItem x) => x.MinLevel <= PhotonConnectionFactory.Instance.Profile.Level);
				list.AddRange(from x in enumerable
					where x.PriceGold == null || x.PriceGold == 0.0
					orderby x.PriceSilver
					select x);
				list.AddRange(from x in enumerable
					where x.PriceGold != null && x.PriceGold > 0.0
					orderby x.PriceGold
					select x);
				enumerable = data.Where((InventoryItem x) => x.MinLevel > PhotonConnectionFactory.Instance.Profile.Level);
				list.AddRange(from x in enumerable
					where x.PriceGold == null || x.PriceGold == 0.0
					orderby x.PriceSilver
					select x);
				list.AddRange(from x in enumerable
					where x.PriceGold != null && x.PriceGold > 0.0
					orderby x.PriceGold
					select x);
				break;
			}
			case SortType.PriceHigh:
			{
				IEnumerable<InventoryItem> enumerable = data.Where((InventoryItem x) => x.MinLevel <= PhotonConnectionFactory.Instance.Profile.Level);
				list.AddRange(from x in enumerable
					where x.PriceGold != null && x.PriceGold > 0.0
					orderby x.PriceGold descending
					select x);
				list.AddRange(from x in enumerable
					where x.PriceGold == null || x.PriceGold == 0.0
					orderby x.PriceSilver descending
					select x);
				enumerable = data.Where((InventoryItem x) => x.MinLevel > PhotonConnectionFactory.Instance.Profile.Level);
				list.AddRange(from x in enumerable
					where x.PriceGold != null && x.PriceGold > 0.0
					orderby x.PriceGold descending
					select x);
				list.AddRange(from x in enumerable
					where x.PriceGold == null || x.PriceGold == 0.0
					orderby x.PriceSilver descending
					select x);
				break;
			}
			case SortType.LevelLow:
				return SortingItems.Sort(from x in data
					group x by x.OriginalMinLevel into x
					orderby x.Key
					select x);
			case SortType.LevelHigh:
				return SortingItems.Sort(from x in data
					group x by x.OriginalMinLevel into x
					orderby x.Key descending
					select x);
			case SortType.Name:
				return data.OrderBy((InventoryItem x) => x.Name).ToList<InventoryItem>();
			}
			return list;
		}

		private static List<InventoryItem> Sort(IOrderedEnumerable<IGrouping<int, InventoryItem>> data)
		{
			List<InventoryItem> list = new List<InventoryItem>();
			foreach (IGrouping<int, InventoryItem> grouping in data)
			{
				list.AddRange(from x in grouping
					where x.PriceGold == null || x.PriceGold == 0.0
					orderby x.PriceSilver
					select x);
				list.AddRange(from x in grouping
					where x.PriceGold != null && x.PriceGold > 0.0
					orderby x.PriceGold
					select x);
			}
			return list;
		}
	}
}
