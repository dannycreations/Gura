using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class FlatFeederFilter : SinkerFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(Feeder);
		base.AddRange<float>(this.Capacity, "Capacity", "CapacityCaption", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return items.Cast<Feeder>().Where(Expression.Lambda<Func<Feeder, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<InventoryItem>()
			.ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, float[]> Capacity = new Dictionary<string, float[]>
	{
		{
			"FloatingWeight3",
			new float[] { 0f, 0.019999f }
		},
		{
			"FloatingWeight4",
			new float[] { 0.02f, 0.03f }
		},
		{
			"FloatingWeight5",
			new float[] { 0.03001f, float.MaxValue }
		}
	};
}
