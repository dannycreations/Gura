using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class TopwaterFilter : JigHeadFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterCategories.Add(1, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeLureFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("PopperFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Popper
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("WalkersFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Walker
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("FrogFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Frog
				}
			}
		});
		this.FilterCategories.Add(3, new CategoryFilter
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
		this.FilterType = typeof(Lure);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return items.Cast<Lure>().Where(Expression.Lambda<Func<Lure, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<InventoryItem>()
			.ToList<InventoryItem>();
	}
}
