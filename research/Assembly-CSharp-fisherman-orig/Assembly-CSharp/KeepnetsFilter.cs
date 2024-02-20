using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;

public class KeepnetsFilter : ConcreteKeepnetFilter
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
					Caption = ScriptLocalization.Get("KeepnetFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Keepnet
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("StringerFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Stringer
				}
			}
		});
	}
}
