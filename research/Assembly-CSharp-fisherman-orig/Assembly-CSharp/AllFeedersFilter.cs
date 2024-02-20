using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class AllFeedersFilter : SinkerFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(Feeder);
		base.AddSingle<ItemSubTypes>(1, this.Types, "ItemSubType", "TypeBoatsFilter", true);
		base.AddRange<float>(this.Capacity, "Capacity", "CapacityCaption", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return items.Cast<Feeder>().Where(Expression.Lambda<Func<Feeder, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<InventoryItem>()
			.ToList<InventoryItem>();
	}

	protected Dictionary<string, float[]> Capacity = new Dictionary<string, float[]>
	{
		{
			"FloatingWeight1",
			new float[] { 0f, 0.04f }
		},
		{
			"FloatingWeight2",
			new float[] { 0.040009998f, 0.08f }
		},
		{
			"FloatingWeight3",
			new float[] { 0.080010004f, 0.12f }
		},
		{
			"FloatingWeight4",
			new float[] { 0.12001f, 0.16f }
		},
		{
			"FloatingWeight5",
			new float[] { 0.16001f, 3.4028234E+35f }
		}
	};

	protected readonly Dictionary<string, ItemSubTypes> Types = new Dictionary<string, ItemSubTypes>
	{
		{
			"CageFeedersCaption",
			ItemSubTypes.CageFeeder
		},
		{
			"FlatFeedersCaption",
			ItemSubTypes.FlatFeeder
		},
		{
			"PVACaption",
			ItemSubTypes.PvaFeeder
		},
		{
			"SpodFeedersCaption",
			ItemSubTypes.SpodFeeder
		}
	};
}
