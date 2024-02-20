using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class ChumFilter : BaseFilter
{
	internal override void Init()
	{
		base.Init<ChumIngredient>(false);
		base.AddSingle<ItemSubTypes>(this.SubTypes, "ItemSubType", "TypeBoatsFilter", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return (from s in items.Cast<ChumIngredient>().Where(Expression.Lambda<Func<ChumIngredient, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag))
			select (s)).ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, ItemSubTypes> SubTypes = new Dictionary<string, ItemSubTypes>
	{
		{
			"CarpbaitsMenu",
			ItemSubTypes.ChumCarpbaits
		},
		{
			"FeederBasesCaption",
			ItemSubTypes.ChumGroundbaits
		},
		{
			"MethodBasesCaption",
			ItemSubTypes.ChumMethodMix
		},
		{
			"ChumParticlesCaption",
			ItemSubTypes.ChumParticle
		},
		{
			"ChumAromasCaption",
			ItemSubTypes.ChumAroma
		}
	};
}
