using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class LineFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		base.AddSingle<ItemSubTypes>(this.LineTypes, "ItemSubType", "TypeLineFilter", true);
		this.FilterType = typeof(InventoryItem);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Where(Expression.Lambda<Func<InventoryItem, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, ItemSubTypes> LineTypes = new Dictionary<string, ItemSubTypes>
	{
		{
			"MonoLineFilter",
			ItemSubTypes.MonoLine
		},
		{
			"FlurLineFilter",
			ItemSubTypes.FlurLine
		},
		{
			"BraidLineFilter",
			ItemSubTypes.BraidLine
		}
	};
}
