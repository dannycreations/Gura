using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class BobberFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(Bobber);
		this.FilterCategories.Add(1, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("MaxFloatingWeightFilter"),
			Filters = new List<ISelectionFilterBase>
			{
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("FloatingWeight1"),
					FilterFieldName = "Buoyancy",
					FilterFieldType = typeof(float),
					MinValue = 0f,
					MaxValue = 7f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("FloatingWeight2"),
					FilterFieldName = "Buoyancy",
					FilterFieldType = typeof(float),
					MinValue = 7.01f,
					MaxValue = 14.99f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("FloatingWeight3"),
					FilterFieldName = "Buoyancy",
					FilterFieldType = typeof(float),
					MinValue = 15f,
					MaxValue = 30f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("FloatingWeight4"),
					FilterFieldName = "Buoyancy",
					FilterFieldType = typeof(float),
					MinValue = 30f,
					MaxValue = 100f
				},
				new RangeSelectionFilter
				{
					Caption = ScriptLocalization.Get("FloatingWeight5"),
					FilterFieldName = "Buoyancy",
					FilterFieldType = typeof(float),
					MinValue = 100f,
					MaxValue = 300f
				}
			}
		});
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<Bobber>().Where(Expression.Lambda<Func<Bobber, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}
}
