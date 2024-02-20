using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class ConcreteRigFilter : ConcreteLineFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(Leader);
		base.AddSingle<float>(this.LeaderLength, "LeaderLength", "ChangeLeashLength", false);
	}

	protected override bool IsAddBrands()
	{
		return false;
	}

	protected override bool IsAddCount()
	{
		return false;
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return items.Cast<Leader>().Where(Expression.Lambda<Func<Leader, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<InventoryItem>()
			.ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, float> LeaderLength = new Dictionary<string, float>
	{
		{ "0.5", 0.5f },
		{ "1.0", 1f },
		{ "2.0", 2f }
	};
}
