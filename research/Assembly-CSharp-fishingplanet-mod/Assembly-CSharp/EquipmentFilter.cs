using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;

public class EquipmentFilter : BaseFilter
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
					Caption = ScriptLocalization.Get("RodCaseFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.RodCase
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("WaistcoatFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Waistcoat
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("TackleBoxFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.LuresBox
				}
			}
		});
	}
}
