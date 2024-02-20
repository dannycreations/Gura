using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FilterHandler : MonoBehaviour
{
	public void SetupFilters(BaseFilter filter)
	{
		this.ScrollBar.value = 0f;
		this._filter = filter;
		foreach (GameObject gameObject in this.filters)
		{
			Object.Destroy(gameObject);
		}
		this.filters.Clear();
		ushort num = 0;
		foreach (KeyValuePair<short, CategoryFilter> keyValuePair in filter.FilterCategories)
		{
			num = this.InsertCategory(keyValuePair.Value, num);
			num += 1;
		}
	}

	private ushort InsertCategory(CategoryFilter category, ushort index)
	{
		GameObject gameObject = GUITools.AddChild(base.transform.gameObject, this.CategoryCaptionPrefab);
		this.filters.Add(gameObject);
		gameObject.name = "Filter" + index;
		gameObject.transform.Find("Value").GetComponent<Text>().text = category.CategoryName;
		index += 1;
		foreach (ISelectionFilterBase selectionFilterBase in category.Filters)
		{
			this.InsertFilter(selectionFilterBase, category, index);
			index += 1;
		}
		return index;
	}

	private void InsertFilter(ISelectionFilterBase filter, CategoryFilter category, ushort index)
	{
		GameObject gameObject = GUITools.AddChild(base.transform.gameObject, this.SingleSelectionPrefab);
		this.filters.Add(gameObject);
		gameObject.name = "Filter" + index;
		filter.CategoryFilter = category;
		gameObject.AddComponent<FilterContent>().SelectionFilter = (SingleSelectionFilter)filter;
		gameObject.transform.Find("Toggle").transform.Find("Label").GetComponent<Text>().text = filter.Caption;
		gameObject.GetComponent<EventAction>().ActionCalled += this.ApplyFilter_Action;
	}

	private void ApplyFilter_Action(object sender, EventArgs e)
	{
		this.ApplyFilters();
	}

	public void ApplyFilters()
	{
		IList<SingleSelectionFilter> list = new List<SingleSelectionFilter>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.GetComponentInChildren<Toggle>() != null && child.GetComponentInChildren<Toggle>().isOn)
			{
				list.Add(child.GetComponent<FilterContent>().SelectionFilter);
			}
		}
		IEnumerable<IGrouping<CategoryFilter, SingleSelectionFilter>> enumerable = from x in list
			group x by x.CategoryFilter;
		ParameterExpression parameterExpression = Expression.Parameter(this._filter.FilterType, "param");
		BinaryExpression binaryExpression = null;
		foreach (IGrouping<CategoryFilter, SingleSelectionFilter> grouping in enumerable)
		{
			BinaryExpression binaryExpression2 = null;
			foreach (SingleSelectionFilter singleSelectionFilter in grouping)
			{
				if (binaryExpression2 == null)
				{
					binaryExpression2 = singleSelectionFilter.GetExpression(parameterExpression);
				}
				else
				{
					binaryExpression2 = Expression.Or(binaryExpression2, singleSelectionFilter.GetExpression(parameterExpression));
				}
			}
			if (binaryExpression == null)
			{
				binaryExpression = binaryExpression2;
			}
			else
			{
				binaryExpression = Expression.And(binaryExpression, binaryExpression2);
			}
		}
		if (this._filter.FilterType == typeof(ShopLicenseContainer))
		{
			List<ShopLicenseContainer> list2 = ((binaryExpression != null) ? this._filter.GetFiltered(binaryExpression, parameterExpression, this.updateContentItems.FullShopLicense) : this.updateContentItems.FullShopLicense);
			this.updateContentItems.FilterCurrentContent(list2);
		}
		else if (this._filter.FilterType == typeof(StoreProduct))
		{
			List<StoreProduct> list3 = this.updateContentItems.FullProductItems;
			if (binaryExpression != null)
			{
				list3 = this._filter.GetFiltered(binaryExpression, parameterExpression, list3);
			}
			this.updateContentItems.FilterCurrentContent(list3);
		}
		else
		{
			List<InventoryItem> list4 = this.updateContentItems.FullInventoryItems;
			if (binaryExpression != null)
			{
				list4 = this._filter.GetFilteredIi(binaryExpression, parameterExpression, list4);
			}
			this.updateContentItems.FilterCurrentContent(list4);
		}
	}

	public void ClearFilters()
	{
		this.ScrollBar.value = 0f;
		foreach (GameObject gameObject in this.filters)
		{
			Transform transform = gameObject.transform.Find("Toggle");
			if (transform != null)
			{
				transform.GetComponent<Toggle>().isOn = false;
			}
		}
		this.updateContentItems.ClearFiltersEndSearch();
	}

	public GameObject CategoryCaptionPrefab;

	public GameObject SingleSelectionPrefab;

	public Scrollbar ScrollBar;

	private BaseFilter _filter;

	private List<GameObject> filters = new List<GameObject>();

	public UpdateContentItems updateContentItems;
}
