using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;

public class ReelFilter : ConcreteReelFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterCategories.Add(1, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeReelFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("SpinningReelFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.SpinReel
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("CastingReelFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.CastReel
				}
			}
		});
	}
}
