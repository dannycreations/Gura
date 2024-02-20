using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class SpodFeederFilter : SinkerFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(SpodFeeder);
		base.AddRange<float>(this.Capacity, "Capacity", "CapacityCaption", true);
		base.AddSingle<int>(this.SlotCount, "SlotCount", "SpodSlotCountCaption", false);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return items.Cast<SpodFeeder>().Where(Expression.Lambda<Func<SpodFeeder, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<InventoryItem>()
			.ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, float[]> Capacity = new Dictionary<string, float[]>
	{
		{
			"FloatingWeight2",
			new float[] { 0f, 0.07f }
		},
		{
			"FloatingWeight3",
			new float[] { 0.0700001f, 0.109999f }
		},
		{
			"FloatingWeight4",
			new float[] { 0.11f, 0.15f }
		},
		{
			"FloatingWeight5",
			new float[] { 0.15001f, float.MaxValue }
		}
	};

	protected readonly Dictionary<string, int> SlotCount = new Dictionary<string, int>
	{
		{ "1", 1 },
		{ "2", 2 }
	};
}
