using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class BoilBaitFilter : ConcreteBaitFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(BoilBait);
		base.AddSingle<BoilBaitForm>(this.BoilType, "Form", "TypeHookFilter", true);
		base.AddRange<float>(this.Buoyancy, "Buoyancy", "BuoyancyCaption", true);
		base.AddRange<double?>(this.Radius, "Weight", "DiameterBoilsFilter", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<BoilBait>().Where(Expression.Lambda<Func<BoilBait, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, BoilBaitForm> BoilType = new Dictionary<string, BoilBaitForm>
	{
		{
			"BoilBaitsFilter",
			BoilBaitForm.Boils
		},
		{
			"PelletsBaitsFilter",
			BoilBaitForm.Pellets
		}
	};

	protected readonly Dictionary<string, float[]> Buoyancy = new Dictionary<string, float[]>
	{
		{
			"PopUpBuoyancy",
			new float[] { -0.09999f, float.MaxValue }
		},
		{
			"SinkingBuoyancy",
			new float[] { float.MinValue, -0.1f }
		}
	};

	protected readonly Dictionary<string, double?[]> Radius = new Dictionary<string, double?[]>
	{
		{
			"Small",
			new double?[]
			{
				new double?(0.0),
				new double?(0.007000000216066837)
			}
		},
		{
			"Medium",
			new double?[]
			{
				new double?(0.007009999826550484),
				new double?(0.012000000104308128)
			}
		},
		{
			"Big",
			new double?[]
			{
				new double?(0.012009999714791775),
				new double?(3.4028234663852886E+38)
			}
		}
	};
}
