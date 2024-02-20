using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class FeederRodFilter : ConcreteRodFilter
{
	internal override void Init()
	{
		base.Init();
		this.FilterType = typeof(FeederRod);
		base.AddSingle<QuiverTip[]>(this.QuiverTips, "QuiverTips", "QuiverTipLabel", false);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		if (groupCondition.Left.ToString() == "param.QuiverTips")
		{
			ConstantExpression constantExpression = groupCondition.Right as ConstantExpression;
			if (constantExpression != null)
			{
				QuiverTip[] array = constantExpression.Value as QuiverTip[];
				if (array != null)
				{
					float oz = 35.274f;
					float min = array[0].Test;
					float max = array[1].Test;
					IEnumerable<FeederRod> enumerable = items.Cast<FeederRod>();
					List<int> qtItems = (from p in enumerable
						where p.QuiverTips != null && p.QuiverTips.ToList<QuiverTip>().Any((QuiverTip el) => el.Test * oz >= min && el.Test * oz <= max)
						select p.ItemId).ToList<int>();
					return items.Where((InventoryItem p) => qtItems.Contains(p.ItemId)).ToList<InventoryItem>();
				}
			}
			return new List<InventoryItem>();
		}
		bool flag = false;
		return items.Cast<FeederRod>().Where(Expression.Lambda<Func<FeederRod, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<InventoryItem>()
			.ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, QuiverTip[]> QuiverTips = new Dictionary<string, QuiverTip[]>
	{
		{
			"0.5 Oz-1.5 Oz",
			new QuiverTip[]
			{
				new QuiverTip
				{
					Test = 0.5f
				},
				new QuiverTip
				{
					Test = 1.5f
				}
			}
		},
		{
			"1.5 Oz-3.0 Oz",
			new QuiverTip[]
			{
				new QuiverTip
				{
					Test = 1.51f
				},
				new QuiverTip
				{
					Test = 3f
				}
			}
		},
		{
			"3.0 Oz-5.0 Oz",
			new QuiverTip[]
			{
				new QuiverTip
				{
					Test = 3.1f
				},
				new QuiverTip
				{
					Test = 5f
				}
			}
		},
		{
			"5.0 Oz+",
			new QuiverTip[]
			{
				new QuiverTip
				{
					Test = 5.1f
				},
				new QuiverTip
				{
					Test = float.MaxValue
				}
			}
		}
	};
}
