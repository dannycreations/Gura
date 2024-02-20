using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;

public class RodFilter : ConcreteRodFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterCategories.Add((short)this.FilterCategories.Count, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeRodFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("SpinningRodsFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.SpinningRod
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("CastingRodsFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.CastingRod
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("TelescopicRodsFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.TelescopicRod
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("MatchRodsFilter"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.MatchRod
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("FeederRodsCaption"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.FeederRod
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("BottomRodsCaption"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.BottomRod
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("CarpRodsCaption"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.CarpRod
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("SpodRodsCaption"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.SpodRod
				}
			}
		});
	}
}
