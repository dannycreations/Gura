using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class ChumParticlesFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init<ChumParticle>(false);
		base.AddSingle<float>(this.Weight, "Amount", "WeightCaption", false);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<ChumParticle>().Where(Expression.Lambda<Func<ChumParticle, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, float> Weight = new Dictionary<string, float>
	{
		{ "0.250", 0.25f },
		{ "0.500", 0.5f }
	};
}
