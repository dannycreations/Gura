using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class ConcreteJigBaitFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(JigBait);
		this.FilterCategories.Add(2, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("JigBaitLengthFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("JigBaitLength1"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 0.025,
					MaxValue = 0.05
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("JigBaitLength2"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 0.05,
					MaxValue = 0.075
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("JigBaitLength3"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 0.075,
					MaxValue = 0.1
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("JigBaitLength4"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 0.1,
					MaxValue = 0.125
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("JigBaitLength5"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 0.125,
					MaxValue = 0.15
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("JigBaitLength6"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 0.15,
					MaxValue = 0.175
				}
			}
		});
		this.FilterCategories.Add(4, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("FittingHookFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleInRangeSelectionFiter
				{
					Caption = "10",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 5f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "8",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 6f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "6",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 7f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "4",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 8f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "2",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 9f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "1",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 10f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "1/0",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 12f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "2/0",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 14f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "3/0",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 16f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "4/0",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 18f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "6/0",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 22f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "8/0",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 28f
				},
				new SingleInRangeSelectionFiter
				{
					Caption = "10/0",
					FilterFieldType = typeof(float),
					FilterFieldNameMin = "MinHookSize",
					FilterFieldNameMax = "MaxHookSize",
					Value = 32f
				}
			}
		});
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<JigBait>().Where(Expression.Lambda<Func<JigBait, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}
}
