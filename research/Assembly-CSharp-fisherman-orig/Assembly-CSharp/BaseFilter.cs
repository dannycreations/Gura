using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;
using UnityEngine;

public class BaseFilter : MonoBehaviour
{
	private void OnEnable()
	{
		this.Init();
	}

	internal virtual void Init()
	{
		this.CreateCommonFilter();
	}

	protected virtual void Init<T>(bool isClear = false)
	{
		this.CreateCommonFilter();
		this.FilterType = typeof(T);
		if (isClear)
		{
			this.FilterCategories.Clear();
		}
	}

	public virtual List<InventoryItem> GetFilteredIi(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		return this.GetFilteredIiByType(groupCondition, param, items);
	}

	public virtual List<StoreProduct> GetFiltered(BinaryExpression groupCondition, ParameterExpression param, List<StoreProduct> items)
	{
		bool flag = false;
		return items.Where(Expression.Lambda<Func<StoreProduct, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).ToList<StoreProduct>();
	}

	public virtual List<ShopLicenseContainer> GetFiltered(BinaryExpression groupCondition, ParameterExpression param, List<ShopLicenseContainer> items)
	{
		bool flag = false;
		return items.Where(Expression.Lambda<Func<ShopLicenseContainer, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).ToList<ShopLicenseContainer>();
	}

	public virtual List<InventoryItem> GetFiltered(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return items.Where(Expression.Lambda<Func<InventoryItem, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).ToList<InventoryItem>();
	}

	protected virtual List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		return this.GetFiltered(groupCondition, param, items);
	}

	protected virtual List<InventoryItem> FilteredIiByType<T>(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items) where T : InventoryItem
	{
		bool isAot = false;
		return items.Where((InventoryItem p) => (from s in items.Cast<T>().Where(Expression.Lambda<Func<T, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(isAot))
			select (s)).ToList<T>().Any((T e) => e.ItemId == p.ItemId)).ToList<InventoryItem>();
	}

	protected void AddSingle<T>(Dictionary<string, T> data, string filterFieldName, string categoryNameLoc, bool isLocalizeKey = true)
	{
		CategoryFilter categoryFilter = new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get(categoryNameLoc),
			Filters = new List<ISelectionFilterBase>()
		};
		foreach (KeyValuePair<string, T> keyValuePair in data)
		{
			categoryFilter.Filters.Add(new SingleSelectionFilter
			{
				Caption = ((!isLocalizeKey) ? keyValuePair.Key : ScriptLocalization.Get(keyValuePair.Key)),
				FilterFieldName = filterFieldName,
				FilterFieldType = typeof(T),
				Value = keyValuePair.Value
			});
		}
		this.FilterCategories.Add((short)this.FilterCategories.Count, categoryFilter);
	}

	protected void AddSingle<T>(short pos, Dictionary<string, T> data, string filterFieldName, string categoryNameLoc, bool isLocalizeKey = true)
	{
		CategoryFilter categoryFilter = new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get(categoryNameLoc),
			Filters = new List<ISelectionFilterBase>()
		};
		foreach (KeyValuePair<string, T> keyValuePair in data)
		{
			categoryFilter.Filters.Add(new SingleSelectionFilter
			{
				Caption = ((!isLocalizeKey) ? keyValuePair.Key : ScriptLocalization.Get(keyValuePair.Key)),
				FilterFieldName = filterFieldName,
				FilterFieldType = typeof(T),
				Value = keyValuePair.Value
			});
		}
		if (this.FilterCategories.ContainsKey(pos))
		{
			CategoryFilter categoryFilter2 = this.FilterCategories[pos];
			int num = this.FilterCategories.Count + 1;
			short num2 = pos;
			while ((int)num2 < num)
			{
				if (this.FilterCategories.ContainsKey(num2))
				{
					categoryFilter2 = this.FilterCategories[num2];
				}
				this.FilterCategories[num2] = categoryFilter;
				categoryFilter = categoryFilter2;
				num2 += 1;
			}
		}
		else
		{
			this.FilterCategories.Add(pos, categoryFilter);
		}
	}

	protected void AddRange<T>(Dictionary<string, T[]> data, string filterFieldName, string categoryNameLoc, bool isLocalizeKey = true)
	{
		CategoryFilter categoryFilter = new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get(categoryNameLoc),
			Filters = new List<ISelectionFilterBase>()
		};
		foreach (KeyValuePair<string, T[]> keyValuePair in data)
		{
			categoryFilter.Filters.Add(new RangeSelectionFilter
			{
				Caption = ((!isLocalizeKey) ? keyValuePair.Key : ScriptLocalization.Get(keyValuePair.Key)),
				FilterFieldName = filterFieldName,
				FilterFieldType = typeof(T),
				MinValue = keyValuePair.Value[0],
				MaxValue = keyValuePair.Value[1]
			});
		}
		this.FilterCategories.Add((short)this.FilterCategories.Count, categoryFilter);
	}

	protected virtual void CreateCommonFilter()
	{
		this.FilterCategories = new SortedDictionary<short, CategoryFilter> { 
		{
			0,
			new CategoryFilter
			{
				CategoryName = ScriptLocalization.Get("CommonFilter"),
				Filters = new List<ISelectionFilterBase>
				{
					new SingleSelectionFilter
					{
						Caption = ScriptLocalization.Get("SaleFilter"),
						FilterFieldName = "IsDiscount",
						FilterFieldType = typeof(bool),
						Value = true
					},
					new SingleSelectionFilter
					{
						Caption = ScriptLocalization.Get("NewFilter"),
						FilterFieldName = "IsNew",
						FilterFieldType = typeof(bool),
						Value = true
					}
				}
			}
		} };
	}

	public Type FilterType = typeof(InventoryItem);

	public SortedDictionary<short, CategoryFilter> FilterCategories = new SortedDictionary<short, CategoryFilter>();
}
