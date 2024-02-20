using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectModel;

public class ChumCarpbaitsFilter : ConcreteBaitFilter
{
	internal override void Init()
	{
		base.Init<ChumCarpbaits>(false);
		base.AddSingle<ChumCarpbaitsForm>(this.SubTypes, "Form", "TypeBoatsFilter", true);
	}

	protected override List<InventoryItem> GetFilteredIiByType(BinaryExpression groupCondition, ParameterExpression param, List<InventoryItem> items)
	{
		bool flag = false;
		return items.Cast<ChumCarpbaits>().Where(Expression.Lambda<Func<ChumCarpbaits, bool>>(groupCondition, new ParameterExpression[] { param }).CompileAot(flag)).Cast<InventoryItem>()
			.ToList<InventoryItem>();
	}

	protected readonly Dictionary<string, ChumCarpbaitsForm> SubTypes = new Dictionary<string, ChumCarpbaitsForm>
	{
		{
			"BoilBaitsFilter",
			ChumCarpbaitsForm.ChumBoils
		},
		{
			"PelletsBaitsFilter",
			ChumCarpbaitsForm.ChumPellets
		},
		{
			"SpodMixCaption",
			ChumCarpbaitsForm.ChumSpodMix
		}
	};
}
