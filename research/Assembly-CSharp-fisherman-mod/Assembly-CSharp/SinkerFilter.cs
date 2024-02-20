using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class SinkerFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init<Sinker>(false);
		this.AddWeight();
	}

	protected virtual void AddWeight()
	{
		base.AddRange<double?>(this.Weight, "Weight", "WeightJigHeadFilter", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<Sinker>().Where(Expression.Lambda<Func<Sinker, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected Dictionary<string, double?[]> Weight = new Dictionary<string, double?[]>
	{
		{
			"WeightSinker1",
			new double?[]
			{
				new double?(0.0),
				new double?(0.04)
			}
		},
		{
			"WeightSinker2",
			new double?[]
			{
				new double?(0.0400099983215332),
				new double?(0.08)
			}
		},
		{
			"WeightSinker3",
			new double?[]
			{
				new double?(0.08001000213623047),
				new double?(0.12)
			}
		},
		{
			"WeightSinker4",
			new double?[]
			{
				new double?(0.12001000213623046),
				new double?(0.16)
			}
		},
		{
			"WeightSinker5",
			new double?[]
			{
				new double?(0.16000999450683595),
				new double?(3.4028234663852886E+35)
			}
		}
	};
}
