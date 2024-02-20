using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class ConcreteLineFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init<Line>(false);
		if (this.IsAddBrands())
		{
			base.AddSingle<string>(this.Brands, "BrandName", "BrandFilter", false);
		}
		base.AddRange<float>(this.Diameters, "Thickness", "DiameterLineFilter", true);
		base.AddRange<float>(this.MaxLoads, "MaxLoad", "PoundTestLineFilter", true);
		if (this.IsAddCount())
		{
			base.AddSingle<int>(this.Counts, "Count", "TrophyStatLengthCaption", false);
		}
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<Line>().Where(Expression.Lambda<Func<Line, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected virtual bool IsAddBrands()
	{
		return true;
	}

	protected virtual bool IsAddCount()
	{
		return true;
	}

	protected readonly Dictionary<string, string> Brands = new Dictionary<string, string>
	{
		{ "Garry Scott", "Garry Scott" },
		{ "MagFin", "MagFin" },
		{ "UL-CHUBER", "UL-CHUBER" }
	};

	protected readonly Dictionary<string, float[]> Diameters = new Dictionary<string, float[]>
	{
		{
			"DiameterLine1",
			new float[] { 0.05f, 0.12f }
		},
		{
			"DiameterLine2",
			new float[] { 0.12f, 0.18f }
		},
		{
			"DiameterLine3",
			new float[] { 0.18f, 0.25f }
		},
		{
			"DiameterLine4",
			new float[] { 0.25f, 0.35f }
		},
		{
			"DiameterLine5",
			new float[] { 0.35f, 0.45f }
		},
		{
			"DiameterLine6",
			new float[] { 0.45f, 0.55f }
		},
		{
			"DiameterLine7",
			new float[] { 0.55f, 10f }
		}
	};

	protected readonly Dictionary<string, float[]> MaxLoads = new Dictionary<string, float[]>
	{
		{
			"PoundTestLine1",
			new float[] { 0f, 1.5f }
		},
		{
			"PoundTestLine2",
			new float[] { 1.5f, 3f }
		},
		{
			"PoundTestLine3",
			new float[] { 3f, 4.5f }
		},
		{
			"PoundTestLine4",
			new float[] { 4.5f, 7.5f }
		},
		{
			"PoundTestLine5",
			new float[] { 7.5f, 11f }
		},
		{
			"PoundTestLine6",
			new float[] { 11f, 15f }
		},
		{
			"PoundTestLine7",
			new float[] { 15f, 1000f }
		}
	};

	protected readonly Dictionary<string, int> Counts = new Dictionary<string, int>
	{
		{ "150", 150 },
		{ "300", 300 },
		{ "500", 500 },
		{ "1000", 1000 }
	};
}
