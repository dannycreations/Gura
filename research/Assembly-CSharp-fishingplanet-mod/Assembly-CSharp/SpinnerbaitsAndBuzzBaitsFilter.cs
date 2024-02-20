using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;

public class SpinnerbaitsAndBuzzBaitsFilter : ConcreteLureFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterCategories.Add(1, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeJigBaitFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("SpinnerbaitFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Spinnerbait
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("BuzzbaitsCaption"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.BuzzBait
				}
			}
		});
	}
}
