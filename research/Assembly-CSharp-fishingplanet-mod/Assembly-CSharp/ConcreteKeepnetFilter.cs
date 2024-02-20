using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class ConcreteKeepnetFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(FishCage);
		this.FilterCategories.Add(2, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("MaxSingleFishWeightFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxSingleFishWeight1"),
					FilterFieldName = "MaxFishWeight",
					FilterFieldType = typeof(float),
					MinValue = 0f,
					MaxValue = 2f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxSingleFishWeight2"),
					FilterFieldName = "MaxFishWeight",
					FilterFieldType = typeof(float),
					MinValue = 2f,
					MaxValue = 5f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxSingleFishWeight3"),
					FilterFieldName = "MaxFishWeight",
					FilterFieldType = typeof(float),
					MinValue = 5f,
					MaxValue = 10f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxSingleFishWeight4"),
					FilterFieldName = "MaxFishWeight",
					FilterFieldType = typeof(float),
					MinValue = 10f,
					MaxValue = 25f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxSingleFishWeight5"),
					FilterFieldName = "MaxFishWeight",
					FilterFieldType = typeof(float),
					MinValue = 25f,
					MaxValue = 50f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxSingleFishWeight6"),
					FilterFieldName = "MaxFishWeight",
					FilterFieldType = typeof(float),
					MinValue = 50f,
					MaxValue = 100f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxSingleFishWeight7"),
					FilterFieldName = "MaxFishWeight",
					FilterFieldType = typeof(float),
					MinValue = 100f,
					MaxValue = 10000f
				}
			}
		});
		this.FilterCategories.Add(3, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TotalFishWeightFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("TotalFishWeight1"),
					FilterFieldName = "TotalWeight",
					FilterFieldType = typeof(float),
					MinValue = 0f,
					MaxValue = 10f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("TotalFishWeight2"),
					FilterFieldName = "TotalWeight",
					FilterFieldType = typeof(float),
					MinValue = 10f,
					MaxValue = 30f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("TotalFishWeight3"),
					FilterFieldName = "TotalWeight",
					FilterFieldType = typeof(float),
					MinValue = 30f,
					MaxValue = 70f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("TotalFishWeight4"),
					FilterFieldName = "TotalWeight",
					FilterFieldType = typeof(float),
					MinValue = 70f,
					MaxValue = 120f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("TotalFishWeight5"),
					FilterFieldName = "TotalWeight",
					FilterFieldType = typeof(float),
					MinValue = 120f,
					MaxValue = 200f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("TotalFishWeight6"),
					FilterFieldName = "TotalWeight",
					FilterFieldType = typeof(float),
					MinValue = 200f,
					MaxValue = 1000000f
				}
			}
		});
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<FishCage>().Where(Expression.Lambda<Func<FishCage, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}
}
