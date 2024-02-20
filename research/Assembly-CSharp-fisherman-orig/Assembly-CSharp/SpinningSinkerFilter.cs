using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class SpinningSinkerFilter : SinkerFilter
{
	internal override void Init()
	{
		base.Init<Sinker>(false);
		this.FilterCategories.Add((short)this.FilterCategories.Count, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("TypeTerminalTackleFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("BulletSinkersCaption"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.SpinningSinker
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("DropSinkersCaption"),
					FilterFieldName = "ItemSubType",
					FilterFieldType = typeof(ItemSubTypes),
					Value = ItemSubTypes.DropSinker
				}
			}
		});
		this.AddWeight();
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return items.Cast<Sinker>().Where(Expression.Lambda<Func<Sinker, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<InventoryItem>()
			.ToList<InventoryItem>();
	}
}
