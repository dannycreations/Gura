using System;
using System.Collections.Generic;
using System.Globalization;
using I2.Loc;
using ObjectModel;

public class SpoonAndSpinnerFilter : ConcreteLureFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterCategories.Add(1, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeSpoonFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("SpoonFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Spoon
				},
				new SingleSelectionFilter
				{
					Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ScriptLocalization.Get("BarblessSpoonMenu").ToLower()),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.BarblessSpoons
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("SpinnerFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Spinner
				},
				new SingleSelectionFilter
				{
					Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ScriptLocalization.Get("BarblessSpinnerMenu").ToLower()),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.BarblessSpinners
				},
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
