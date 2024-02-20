using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class ConcreteReelFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(Reel);
		this.FilterCategories.Add(2, new CategoryFilter
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
		this.FilterCategories.Add(3, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("LineCapacityFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = "800 - 1500",
					FilterFieldName = "LineCapacity",
					FilterFieldType = typeof(int),
					MinValue = 800,
					MaxValue = 1500
				},
				new RangeSelectionFilter
				{
					Caption = "1500 - 2500",
					FilterFieldName = "LineCapacity",
					FilterFieldType = typeof(int),
					MinValue = 1500,
					MaxValue = 2500
				},
				new RangeSelectionFilter
				{
					Caption = "2500 - 3500",
					FilterFieldName = "LineCapacity",
					FilterFieldType = typeof(int),
					MinValue = 2500,
					MaxValue = 3500
				},
				new RangeSelectionFilter
				{
					Caption = "4000 - 6000",
					FilterFieldName = "LineCapacity",
					FilterFieldType = typeof(int),
					MinValue = 4000,
					MaxValue = 6000
				},
				new RangeSelectionFilter
				{
					Caption = "6000 - 8000",
					FilterFieldName = "LineCapacity",
					FilterFieldType = typeof(int),
					MinValue = 6000,
					MaxValue = 8000
				}
			}
		});
		this.FilterCategories.Add(4, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("GearRatioFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = "4.2:1 - 4.7:1",
					FilterFieldName = "Ratio",
					FilterFieldType = typeof(float),
					MinValue = 4.2f,
					MaxValue = 4.7f
				},
				new RangeSelectionFilter
				{
					Caption = "5.0:1 - 5.5:1",
					FilterFieldName = "Ratio",
					FilterFieldType = typeof(float),
					MinValue = 5f,
					MaxValue = 5.5f
				},
				new RangeSelectionFilter
				{
					Caption = "5.8:1 - 6.2:1",
					FilterFieldName = "Ratio",
					FilterFieldType = typeof(float),
					MinValue = 5.8f,
					MaxValue = 6.2f
				},
				new RangeSelectionFilter
				{
					Caption = "6.3:1 - 6.6:1",
					FilterFieldName = "Ratio",
					FilterFieldType = typeof(float),
					MinValue = 6.3f,
					MaxValue = 6.6f
				},
				new RangeSelectionFilter
				{
					Caption = "7.0:1 - 7.6:1",
					FilterFieldName = "Ratio",
					FilterFieldType = typeof(float),
					MinValue = 7f,
					MaxValue = 7.6f
				}
			}
		});
		this.FilterCategories.Add(5, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("RecoveryReelFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("RecoveryReel1"),
					FilterFieldName = "Speed",
					FilterFieldType = typeof(float),
					MinValue = 0.45f,
					MaxValue = 0.55f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("RecoveryReel2"),
					FilterFieldName = "Speed",
					FilterFieldType = typeof(float),
					MinValue = 0.55f,
					MaxValue = 0.65f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("RecoveryReel3"),
					FilterFieldName = "Speed",
					FilterFieldType = typeof(float),
					MinValue = 0.65f,
					MaxValue = 0.75f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("RecoveryReel4"),
					FilterFieldName = "Speed",
					FilterFieldType = typeof(float),
					MinValue = 0.75f,
					MaxValue = 0.85f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("RecoveryReel5"),
					FilterFieldName = "Speed",
					FilterFieldType = typeof(float),
					MinValue = 0.85f,
					MaxValue = 0.95f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("RecoveryReel6"),
					FilterFieldName = "Speed",
					FilterFieldType = typeof(float),
					MinValue = 0.95f,
					MaxValue = 1000f
				}
			}
		});
		this.FilterCategories.Add(6, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("MaxDragReelFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxDragReel1"),
					FilterFieldName = "MaxDrag",
					FilterFieldType = typeof(float),
					MinValue = 0.5f,
					MaxValue = 2f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxDragReel2"),
					FilterFieldName = "MaxDrag",
					FilterFieldType = typeof(float),
					MinValue = 2f,
					MaxValue = 3.5f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxDragReel3"),
					FilterFieldName = "MaxDrag",
					FilterFieldType = typeof(float),
					MinValue = 3.5f,
					MaxValue = 5f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxDragReel4"),
					FilterFieldName = "MaxDrag",
					FilterFieldType = typeof(float),
					MinValue = 5f,
					MaxValue = 7f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxDragReel5"),
					FilterFieldName = "MaxDrag",
					FilterFieldType = typeof(float),
					MinValue = 7f,
					MaxValue = 10f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxDragReel6"),
					FilterFieldName = "MaxDrag",
					FilterFieldType = typeof(float),
					MinValue = 10f,
					MaxValue = 15f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxDragReel7"),
					FilterFieldName = "MaxDrag",
					FilterFieldType = typeof(float),
					MinValue = 15f,
					MaxValue = 20f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("MaxDragReel8"),
					FilterFieldName = "MaxDrag",
					FilterFieldType = typeof(float),
					MinValue = 20f,
					MaxValue = 1000f
				}
			}
		});
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<Reel>().Where(Expression.Lambda<Func<Reel, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}
}
