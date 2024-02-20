using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class PvaFeederFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(PvaFeeder);
		base.AddSingle<PvaFeederForm>(this.PvaType, "Form", "TypeFilter", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<PvaFeeder>().Where(Expression.Lambda<Func<PvaFeeder, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, PvaFeederForm> PvaType = new Dictionary<string, PvaFeederForm>
	{
		{
			"PvaBagFeeder",
			PvaFeederForm.Bag
		},
		{
			"PvaMeshFeeder",
			PvaFeederForm.Stick
		}
	};
}
