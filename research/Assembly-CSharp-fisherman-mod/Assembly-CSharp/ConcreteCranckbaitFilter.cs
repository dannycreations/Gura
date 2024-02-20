using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class ConcreteCranckbaitFilter : JigHeadFilter
{
	internal override void Init()
	{
		base.Init();
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
		this.FilterCategories.Add(5, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("WorkingDepthFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("CranckbaitsWorkingDepth1"),
					FilterFieldName = "Wobbler.WorkingDepth",
					FilterFieldType = typeof(float),
					MinValue = 0f,
					MaxValue = 1f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("CranckbaitsWorkingDepth2"),
					FilterFieldName = "Wobbler.WorkingDepth",
					FilterFieldType = typeof(float),
					MinValue = 1f,
					MaxValue = 2f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("CranckbaitsWorkingDepth3"),
					FilterFieldName = "Wobbler.WorkingDepth",
					FilterFieldType = typeof(float),
					MinValue = 2f,
					MaxValue = 3f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("CranckbaitsWorkingDepth4"),
					FilterFieldName = "Wobbler.WorkingDepth",
					FilterFieldType = typeof(float),
					MinValue = 3f,
					MaxValue = 4f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("CranckbaitsWorkingDepth5"),
					FilterFieldName = "Wobbler.WorkingDepth",
					FilterFieldType = typeof(float),
					MinValue = 4f,
					MaxValue = 6f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("CranckbaitsWorkingDepth6"),
					FilterFieldName = "Wobbler.WorkingDepth",
					FilterFieldType = typeof(float),
					MinValue = 6f,
					MaxValue = 8f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("CranckbaitsWorkingDepth7"),
					FilterFieldName = "Wobbler.WorkingDepth",
					FilterFieldType = typeof(float),
					MinValue = 8f,
					MaxValue = 10f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("CranckbaitsWorkingDepth8"),
					FilterFieldName = "Wobbler.WorkingDepth",
					FilterFieldType = typeof(float),
					MinValue = 10f,
					MaxValue = 1000f
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
