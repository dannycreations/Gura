using System;
using System.Collections.Generic;
using System.Globalization;
using I2.Loc;
using ObjectModel;

public class JigHeadTopFilter : JigHeadFilter
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
				}
			}
		});
	}
}
