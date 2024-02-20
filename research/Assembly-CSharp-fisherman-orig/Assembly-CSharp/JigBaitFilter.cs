using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;

public class JigBaitFilter : ConcreteJigBaitFilter
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
				}
			}
		});
	}
}
