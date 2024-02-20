using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using I2.Loc;
using ObjectModel;

public class RodStandFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init<RodStand>(false);
		base.AddSingle<int>(this.StandsCount, "StandCount", "RodStandsCountCaption", false);
		base.AddSingle<int>(this.RodSlots, "RodCount", "RodSlotsCaption", false);
		base.AddSingle<string>(this.Brands, "BrandName", "BrandFilter", false);
		this.FilterCategories.Add((short)this.FilterCategories.Count, new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("BiteAlarmCaption"),
			Filters = new List<ISelectionFilterBase>
			{
				new SingleSelectionFilter
				{
					Caption = ScriptLocalization.Get("HasBiteAlarmCaption"),
					FilterFieldName = "Bell",
					FilterFieldType = typeof(Bell),
					Value = null
				}
			}
		});
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		if (groupCondition.Left.ToString() == "param.Bell")
		{
			return items.Where((InventoryItem p) => (from RodStand rs in items
				where rs.Bell != null
				select rs).ToList<RodStand>().Any((RodStand e) => e.ItemId == p.ItemId)).ToList<InventoryItem>();
		}
		bool flag = false;
		return (from s in items.Cast<RodStand>().Where(Expression.Lambda<Func<RodStand, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, int> StandsCount = new Dictionary<string, int>
	{
		{ "1", 1 },
		{ "2", 2 },
		{ "3", 3 }
	};

	protected readonly Dictionary<string, int> RodSlots = new Dictionary<string, int>
	{
		{ "1", 1 },
		{ "2", 2 },
		{ "3", 3 },
		{ "4", 4 }
	};

	protected readonly Dictionary<string, string> Brands = new Dictionary<string, string>
	{
		{ "Flaggmann", "Flaggmann" },
		{ "UL-CHUBER", "UL-CHUBER" }
	};
}
