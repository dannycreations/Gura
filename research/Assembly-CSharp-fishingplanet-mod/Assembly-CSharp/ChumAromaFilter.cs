using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class ChumAromaFilter : ChumParticlesFilter
{
	internal override void Init()
	{
		base.Init<ChumAroma>(false);
		base.AddSingle<float>(this.WeightAroma, "Amount", "WeightCaption", false);
		base.AddRange<float>(this.Concentration, "AttractMaxEffectConcentration", "ConcentrationCaption", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return items.Cast<ChumAroma>().Where(Expression.Lambda<Func<ChumAroma, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<InventoryItem>()
			.ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, float> WeightAroma = new Dictionary<string, float>
	{
		{ "0.5", 0.5f },
		{ "1.0", 1f }
	};

	protected readonly Dictionary<string, float[]> Concentration = new Dictionary<string, float[]>
	{
		{
			"FloatingWeight2",
			new float[] { 7f, 10f }
		},
		{
			"FloatingWeight3",
			new float[] { 4f, 6f }
		},
		{
			"FloatingWeight4",
			new float[] { 1f, 3f }
		}
	};
}
