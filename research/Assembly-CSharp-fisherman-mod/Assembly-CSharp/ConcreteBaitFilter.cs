using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class ConcreteBaitFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(Bait);
		base.AddRange<double?>(this.Weight, "Weight", "WeightCaption", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<Bait>().Where(Expression.Lambda<Func<Bait, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, double?[]> Weight = new Dictionary<string, double?[]>
	{
		{
			"BaitWeight1",
			new double?[]
			{
				new double?(5E-05),
				new double?(0.003)
			}
		},
		{
			"BaitWeight2",
			new double?[]
			{
				new double?(0.0031),
				new double?(0.006)
			}
		},
		{
			"BaitWeight3",
			new double?[]
			{
				new double?(0.0061),
				new double?(0.009)
			}
		},
		{
			"BaitWeight4",
			new double?[]
			{
				new double?(0.0091),
				new double?(0.01999)
			}
		},
		{
			"BaitWeight5",
			new double?[]
			{
				new double?(0.02),
				new double?(0.0299999)
			}
		},
		{
			"BaitWeight6",
			new double?[]
			{
				new double?(0.03),
				new double?(1.9)
			}
		}
	};
}
