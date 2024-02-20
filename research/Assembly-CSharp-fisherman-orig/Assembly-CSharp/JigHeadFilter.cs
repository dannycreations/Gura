using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class JigHeadFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(Hook);
		this.FilterCategories.Add(2, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("WeightJigHeadFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("WeightJigHead1"),
					FilterFieldName = "Weight",
					FilterFieldType = typeof(double?),
					MinValue = 0.0005,
					MaxValue = 0.005
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("WeightJigHead2"),
					FilterFieldName = "Weight",
					FilterFieldType = typeof(double?),
					MinValue = 0.005,
					MaxValue = 0.015
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("WeightJigHead3"),
					FilterFieldName = "Weight",
					FilterFieldType = typeof(double?),
					MinValue = 0.015,
					MaxValue = 0.03
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("WeightJigHead4"),
					FilterFieldName = "Weight",
					FilterFieldType = typeof(double?),
					MinValue = 0.03,
					MaxValue = 0.045
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("WeightJigHead5"),
					FilterFieldName = "Weight",
					FilterFieldType = typeof(double?),
					MinValue = 0.045,
					MaxValue = 0.06
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("WeightJigHead6"),
					FilterFieldName = "Weight",
					FilterFieldType = typeof(double?),
					MinValue = 0.06,
					MaxValue = 1.9
				}
			}
		});
		this.FilterCategories.Add(4, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("HookSizeFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = "10 - 6",
					FilterFieldName = "HookSize",
					FilterFieldType = typeof(float),
					MinValue = 5f,
					MaxValue = 7f
				},
				new RangeSelectionFilter
				{
					Caption = "4 - 1",
					FilterFieldName = "HookSize",
					FilterFieldType = typeof(float),
					MinValue = 8f,
					MaxValue = 10f
				},
				new RangeSelectionFilter
				{
					Caption = "1/0 - 3/0",
					FilterFieldName = "HookSize",
					FilterFieldType = typeof(float),
					MinValue = 12f,
					MaxValue = 16f
				},
				new RangeSelectionFilter
				{
					Caption = "4/0 - 6/0",
					FilterFieldName = "HookSize",
					FilterFieldType = typeof(float),
					MinValue = 18f,
					MaxValue = 22f
				},
				new RangeSelectionFilter
				{
					Caption = "7/0 +",
					FilterFieldName = "HookSize",
					FilterFieldType = typeof(float),
					MinValue = 25f,
					MaxValue = 1000f
				}
			}
		});
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<Hook>().Where(Expression.Lambda<Func<Hook, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}
}
