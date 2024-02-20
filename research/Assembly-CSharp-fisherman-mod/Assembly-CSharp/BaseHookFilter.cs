using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class BaseHookFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterCategories.Add(2, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("HookSizeFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = "16 - 12",
					FilterFieldName = "HookSize",
					FilterFieldType = typeof(float),
					MinValue = 3f,
					MaxValue = 4.5f
				},
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
		this.FilterType = typeof(Hook);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<Hook>().Where(Expression.Lambda<Func<Hook, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}
}
