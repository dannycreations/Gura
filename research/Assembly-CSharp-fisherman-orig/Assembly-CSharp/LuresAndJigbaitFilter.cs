using System;
using System.Collections.Generic;
using System.Globalization;
using I2.Loc;
using ObjectModel;

public class LuresAndJigbaitFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		List<ISelectionFilterBase> itemSubTypesFilters = LureFilter.GetItemSubTypesFilters();
		itemSubTypesFilters.AddRange(new List<ISelectionFilterBase>
		{
			new SingleSelectionFilter
			{
				Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ScriptLocalization.Get("CommonJigHeadsMenu").ToLower()),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.CommonJigHeads
			},
			new SingleSelectionFilter
			{
				Caption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ScriptLocalization.Get("BarblessJigHeadsMenu").ToLower()),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.BarblessJigHeads
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("WormFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Worm
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("GrubFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Grub
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("ShadFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Shad
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("TubeFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Tube
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("CrawFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Craw
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("SlugMenu"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Slug
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("BassJigFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.BassJig
			},
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
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("TailsCaption"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Tail
			}
		});
		this.FilterCategories.Add(1, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeJigBaitFilter"),
			Filters = itemSubTypesFilters
		});
		this.FilterType = typeof(InventoryItem);
	}
}
