using System;
using System.Collections.Generic;
using I2.Loc;
using ObjectModel;

public class LureFilter : ConcreteCranckbaitFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterCategories.Add(1, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeLureFilter"),
			Filters = LureFilter.GetItemSubTypesFilters()
		});
	}

	public static List<ISelectionFilterBase> GetItemSubTypesFilters()
	{
		List<ISelectionFilterBase> jerkbaitMinnowFilters = LureFilter.GetJerkbaitMinnowFilters();
		jerkbaitMinnowFilters.AddRange(new List<ISelectionFilterBase>
		{
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("CranckbaitFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Cranckbait
			},
			new TypesSelectionFilter
			{
				Caption = ScriptLocalization.Get("TopWaterLuresMenu"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Values = new object[]
				{
					ItemSubTypes.Frog,
					ItemSubTypes.Popper,
					ItemSubTypes.Walker
				}
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("SwimbaitFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Swimbait
			}
		});
		return jerkbaitMinnowFilters;
	}

	public static List<ISelectionFilterBase> GetJerkbaitMinnowFilters()
	{
		return new List<ISelectionFilterBase>
		{
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("JerkbaitMenuFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Jerkbait
			},
			new SingleSelectionFilter
			{
				Caption = ScriptLocalization.Get("MinnowMenuFilter"),
				FilterFieldName = "ItemSubType",
				FilterFieldType = typeof(ItemSubTypes),
				Value = ItemSubTypes.Minnow
			}
		};
	}
}
