using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;
using Photon.Interfaces;

public class ProductTypeFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(StoreProduct);
		this.FilterCategories.Clear();
		CategoryFilter categoryFilter = new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeServicesFilter"),
			Filters = new List<ISelectionFilterBase>()
		};
		foreach (KeyValuePair<int, string> keyValuePair in ProductTypeFilter._products)
		{
			categoryFilter.Filters.Add(new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get(keyValuePair.Value),
				FilterFieldName = "TypeId",
				FilterFieldType = typeof(int),
				Value = keyValuePair.Key
			});
		}
		this.FilterCategories.Add((short)this.FilterCategories.Count, categoryFilter);
	}

	public new List<StoreProduct> GetFiltered(BinaryExpression groupCondition, ParameterExpression param, List<StoreProduct> items)
	{
		bool flag = false;
		return items.Cast<StoreProduct>().Where(Expression.Lambda<Func<StoreProduct, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<StoreProduct>()
			.ToList<StoreProduct>();
	}

	public static string GetLocForProductType(ProductTypes pt)
	{
		return (!ProductTypeFilter._products.ContainsKey(pt)) ? string.Empty : ProductTypeFilter._products[pt];
	}

	private static readonly Dictionary<int, string> _products = new Dictionary<int, string>
	{
		{ 5, "StorageBoxShopLabel" },
		{ 6, "RodSetupShopLabel" },
		{ 7, "BuoysShopLabel" },
		{ 8, "RecipesCaption" }
	};
}
