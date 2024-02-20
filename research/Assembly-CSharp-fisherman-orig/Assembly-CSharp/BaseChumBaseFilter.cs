using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class BaseChumBaseFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init<ChumBase>(false);
		base.AddSingle<float>(this.Weight, "Amount", "WeightCaption", false);
		base.AddRange<int>(this.Grains, "BaseViscosity", "GrainCaption", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<ChumBase>().Where(Expression.Lambda<Func<ChumBase, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, float> Weight = new Dictionary<string, float>
	{
		{ "1.0", 1f },
		{ "1.5", 1.5f },
		{ "2.0", 2f },
		{ "3.0", 3f }
	};

	protected readonly Dictionary<string, int[]> Grains = new Dictionary<string, int[]>
	{
		{
			"FloatingWeight2",
			new int[] { 0, 50 }
		},
		{
			"FloatingWeight3",
			new int[] { 51, 65 }
		},
		{
			"FloatingWeight4",
			new int[] { 66, 80 }
		},
		{
			"FloatingWeight5",
			new int[] { 81, int.MaxValue }
		}
	};
}
