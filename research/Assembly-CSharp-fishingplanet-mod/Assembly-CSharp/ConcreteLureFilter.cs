using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;

public class ConcreteLureFilter : JigHeadFilter
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
		this.FilterType = typeof(Hook);
	}
}
