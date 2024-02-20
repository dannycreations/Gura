using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class BoatFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(Boat);
		this.FilterCategories.Clear();
		this.FilterCategories.Add(0, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeBoatsFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("KayaksShopLabel"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.Kayak
				},
				new TypesSelectionFilter
				{
					Caption = ScriptLocalization.Get("MotorBoatsLabel"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Values = new object[]
					{
						ItemSubTypes.MotorBoat,
						ItemSubTypes.Zodiak
					}
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("BassBoatsLabel"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.BassBoat
				}
			}
		});
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<Boat>().Where(Expression.Lambda<Func<Boat, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}
}
