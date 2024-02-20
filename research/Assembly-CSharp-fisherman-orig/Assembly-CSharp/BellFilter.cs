using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ObjectModel;

public class BellFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init<Bell>(false);
		base.AddRange<float>(this.Sensitivity, "Sensitivity", "Sensitivity", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		return this.FilteredIiByType<Bell>(groupCondition, param, items);
	}

	protected readonly Dictionary<string, float[]> Sensitivity = new Dictionary<string, float[]>
	{
		{
			"FloatingWeight2",
			new float[] { 0f, 0.1f }
		},
		{
			"FloatingWeight3",
			new float[] { 0.11f, 0.3f }
		},
		{
			"FloatingWeight4",
			new float[] { 0.31f, 0.5f }
		},
		{
			"FloatingWeight5",
			new float[] { 0.51f, 0.7f }
		}
	};
}
