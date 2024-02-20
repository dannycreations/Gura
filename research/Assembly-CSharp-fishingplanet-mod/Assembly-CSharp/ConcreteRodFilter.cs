using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class ConcreteRodFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(Rod);
		this.FilterCategories.Add((short)this.FilterCategories.Count, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("BrandFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = "Flaggmann",
					FilterFieldName = "BrandName",
					FilterFieldType = typeof(string),
					Value = "Flaggmann"
				},
				new SingleSelectionFilter
				{
					Caption = "Garry Scott",
					FilterFieldName = "BrandName",
					FilterFieldType = typeof(string),
					Value = "Garry Scott"
				},
				new SingleSelectionFilter
				{
					Caption = "MagFin",
					FilterFieldName = "BrandName",
					FilterFieldType = typeof(string),
					Value = "MagFin"
				},
				new SingleSelectionFilter
				{
					Caption = "RIVERTEX",
					FilterFieldName = "BrandName",
					FilterFieldType = typeof(string),
					Value = "RIVERTEX"
				},
				new SingleSelectionFilter
				{
					Caption = "UL-CHUBER",
					FilterFieldName = "BrandName",
					FilterFieldType = typeof(string),
					Value = "UL-CHUBER"
				}
			}
		});
		this.FilterCategories.Add((short)this.FilterCategories.Count, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("RodActionFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("RodAction2"),
					FilterFieldName = "ExtendedAction",
					FilterFieldType = typeof(RodAction),
					Value = RodAction.Fast
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("RodAction3"),
					FilterFieldName = "ExtendedAction",
					FilterFieldType = typeof(RodAction),
					Value = RodAction.ModFast
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("RodAction4"),
					FilterFieldName = "ExtendedAction",
					FilterFieldType = typeof(RodAction),
					Value = RodAction.Moderate
				},
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("RodAction5"),
					FilterFieldName = "ExtendedAction",
					FilterFieldType = typeof(RodAction),
					Value = RodAction.Slow
				}
			}
		});
		this.FilterCategories.Add((short)this.FilterCategories.Count, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("LengthRodFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LengthRod1"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 1.4,
					MaxValue = 1.9
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LengthRod2"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 1.9,
					MaxValue = 2.2
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LengthRod3"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 2.2,
					MaxValue = 2.5
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LengthRod4"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 2.5,
					MaxValue = 2.9
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LengthRod5"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 3.0,
					MaxValue = 4.0
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LengthRod6"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 4.0,
					MaxValue = 5.0
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LengthRod7"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 5.0,
					MaxValue = 6.0
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LengthRod8"),
					FilterFieldName = "Length",
					FilterFieldType = typeof(double?),
					MinValue = 6.0,
					MaxValue = 7.0
				}
			}
		});
		this.FilterCategories.Add((short)this.FilterCategories.Count, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("LureWeightRodFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeForRangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LureWeightRod1"),
					FilterFieldNameMin = "CastWeightMin",
					FilterFieldNameMax = "CastWeightMax",
					FilterFieldType = typeof(float),
					MinValue = 0f,
					MaxValue = 0.015f
				},
				new RangeForRangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LureWeightRod2"),
					FilterFieldNameMin = "CastWeightMin",
					FilterFieldNameMax = "CastWeightMax",
					FilterFieldType = typeof(float),
					MinValue = 0.004f,
					MaxValue = 0.025f
				},
				new RangeForRangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LureWeightRod3"),
					FilterFieldNameMin = "CastWeightMin",
					FilterFieldNameMax = "CastWeightMax",
					FilterFieldType = typeof(float),
					MinValue = 0.006f,
					MaxValue = 0.03f
				},
				new RangeForRangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LureWeightRod4"),
					FilterFieldNameMin = "CastWeightMin",
					FilterFieldNameMax = "CastWeightMax",
					FilterFieldType = typeof(float),
					MinValue = 0.008f,
					MaxValue = 0.04f
				},
				new RangeForRangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LureWeightRod5"),
					FilterFieldNameMin = "CastWeightMin",
					FilterFieldNameMax = "CastWeightMax",
					FilterFieldType = typeof(float),
					MinValue = 0.01f,
					MaxValue = 0.05f
				},
				new RangeForRangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LureWeightRod6"),
					FilterFieldNameMin = "CastWeightMin",
					FilterFieldNameMax = "CastWeightMax",
					FilterFieldType = typeof(float),
					MinValue = 0.015f,
					MaxValue = 0.06f
				},
				new RangeForRangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LureWeightRod7"),
					FilterFieldNameMin = "CastWeightMin",
					FilterFieldNameMax = "CastWeightMax",
					FilterFieldType = typeof(float),
					MinValue = 0.02f,
					MaxValue = 0.08f
				},
				new RangeForRangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LureWeightRod8"),
					FilterFieldNameMin = "CastWeightMin",
					FilterFieldNameMax = "CastWeightMax",
					FilterFieldType = typeof(float),
					MinValue = 0.08f,
					MaxValue = 1000f
				}
			}
		});
		this.FilterCategories.Add((short)this.FilterCategories.Count, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("LineWeightRodFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LineWeightRod1"),
					FilterFieldName = "LineWeight",
					FilterFieldType = typeof(float),
					MinValue = 1f,
					MaxValue = 3f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LineWeightRod2"),
					FilterFieldName = "LineWeight",
					FilterFieldType = typeof(float),
					MinValue = 3f,
					MaxValue = 5f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LineWeightRod3"),
					FilterFieldName = "LineWeight",
					FilterFieldType = typeof(float),
					MinValue = 5f,
					MaxValue = 7f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LineWeightRod4"),
					FilterFieldName = "LineWeight",
					FilterFieldType = typeof(float),
					MinValue = 7f,
					MaxValue = 9f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("LineWeightRod5"),
					FilterFieldName = "LineWeight",
					FilterFieldType = typeof(float),
					MinValue = 9f,
					MaxValue = 1000f
				}
			}
		});
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<Rod>().Where(Expression.Lambda<Func<Rod, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}
}
